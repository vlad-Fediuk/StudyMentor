# Звіт: Керування конфігурацією та безпека секретів (.env та Environment Variables)

**Модуль 3:** Якість, продуктивність, надійність та безпека програмного забезпечення  
**Пакет 1:** Безпека та конфігурація  
**Підзадача 1:** `p1/env-config`  
**Виконавець:** Юлія Іонашку (`julliissh`)  

---

## 1. Мета та опис завдання

Відповідно до методичних рекомендацій, конфігураційні параметри та чутливі дані (паролі до бази даних, симетричні ключі підпису токенів, API-ключі сторонніх провайдерів) не повинні зберігатися у вихідному коді чи відкритому репозиторії.

**Основні цілі підзадачі:**
1. Виявити та винести всі чутливі параметри з коду та конфігураційних файлів `appsettings*.json`.
2. Налаштувати автоматичне завантаження локальних змінних середовища за допомогою `.env` та бібліотеки `DotNetEnv`.
3. Забезпечити захист від випадкового потрапляння секретів у Git за допомогою розширених правил у `.gitignore`.
4. Створити стандартизований шаблон `.env.example` для команди та розгортання.
5. Додати `docker-compose.yml` з підтримкою PostgreSQL + `pgvector`, із суворою обов'язковою вимогою пароля через змінні оточення.
6. Реалізувати механізм Fail-fast перевірки обов'язкових ключів на старті API (з правильною ізоляцією тестового середовища `WebApplicationFactory` через `IHostEnvironment`).

---

## 2. Аудит стану системи «ДО» оптимізації

Під час аналізу файлу [study-mentor-api/appsettings.json](../../study-mentor-api/appsettings.json) було виявлено пряме хардкод-зберігання конфіденційних даних:

| # | Параметр | Знайдене значення у відкритому вигляді | Рівень загрози |
| :-: | :--- | :--- | :---: |
| 1 | `ConnectionStrings:DefaultConnection` | `...Password=1111...` | **Критичний** (компрометація БД) |
| 2 | `Jwt:Campus:Key` | Відкритий симетричний ключ у відкритому коді | **Критичний** (підробка токенів доступу) |
| 3 | `AiProviders:Nvidia:ApiKey` | `nvapi-HL-35...` | **Критичний** (витік платного API-ключа) |
| 4 | `.env.example` | Був повністю відсутній у репозиторії | Середній (ризик помилок налаштування) |
| 5 | `.gitignore` | Містив лише точковий запис `.env` без шаблонів `.env.*` | Низький / Середній |

---

## 3. Виконані зміни та архітектурні рішення

### 3.1. Підключення DotNetEnv та порядок завантаження
До проєкту `study-mentor-api` додано бібліотеку `DotNetEnv` (версія 3.2.0).  
У файлі [Program.cs](../../study-mentor-api/Program.cs) виклик завантаження розміщено **до** `WebApplication.CreateBuilder(args)`:

```csharp
public static async Task Main(string[] args)
{
    // Завантажує змінні з .env файлу в поточний процес оточення
    DotNetEnv.Env.TraversePath().Load();

    var builder = WebApplication.CreateBuilder(args);
    // ASP.NET Core автоматично мапить змінні виду Section__Key на Section:Key
```

Завдяки методу `TraversePath()` файл `.env` успішно знаходиться і завантажується як при запуску з кореня репозиторію, так і з каталогу самого проєкту `study-mentor-api/`.

### 3.2. Очищення `appsettings.json`
У файлі [appsettings.json](../../study-mentor-api/appsettings.json) видалено всі відкриті секрети та залишено порожні рядки:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },
  "Jwt": {
    "Campus": {
      "Issuer": "StudyMentorApi",
      "Audience": "StudyMentorClient",
      "Key": ""
    },
    ...
  },
  "AiProviders": {
    "Nvidia": {
      "ApiKey": "",
      ...
    }
  }
}
```

### 3.3. Шаблон `.env.example` та оновлення `.gitignore`
Створено стандартизований файл [.env.example](../../.env.example), що містить єдиний перелік змінних:
- `POSTGRES_USER`, `POSTGRES_PASSWORD`, `POSTGRES_DB`, `POSTGRES_PORT`
- `ConnectionStrings__DefaultConnection`
- `Jwt__Campus__Key`, `Jwt__Campus__Issuer`, `Jwt__Campus__Audience`, `Jwt__Campus__LifetimeMinutes`
- `Jwt__Microsoft__Authority`, `Jwt__Microsoft__Audience`
- `NVIDIA_API_KEY` (єдине уніфіковане джерело для провайдера)
- `RateLimiting__Enabled`, `RateLimiting__PermitLimit`, `RateLimiting__WindowSeconds`
- `Resilience__*`, `Caching__*`, `FaultInjection__*`

У [.gitignore](../../.gitignore) додано надійні правила:
```gitignore
# Environment variables and local secrets
.env
.env.*
!.env.example
```

### 3.4. Docker Compose для PostgreSQL + pgvector
Створено файл [docker-compose.yml](../../docker-compose.yml). Для запобігання використанню ненадійних паролів за замовчуванням використано сувору перевірку:
```yaml
services:
  postgres:
    image: pgvector/pgvector:pg16
    container_name: study-mentor-db
    restart: unless-stopped
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-postgres}
      POSTGRES_PASSWORD: "${POSTGRES_PASSWORD:?Error: POSTGRES_PASSWORD must be defined in .env}"
      POSTGRES_DB: ${POSTGRES_DB:-study_mentor}
    ports:
      - "${POSTGRES_PORT:-55432}:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER:-postgres} -d ${POSTGRES_DB:-study_mentor}"]
      interval: 5s
      timeout: 5s
      retries: 5

volumes:
  postgres_data:
```
Якщо змінна `POSTGRES_PASSWORD` не задана в `.env`, Docker Compose відмовляється стартувати з чіткою помилкою.

### 3.5. Механізм Fail-fast валідації та надійна перевірка режиму `Testing`
Перевірка тестового режиму реалізована строго через екземпляри `IHostEnvironment`:
1. **Для рядка підключення БД (`Program.cs`):**
```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    if (builder.Environment.IsEnvironment("Testing"))
    {
        connectionString = "Host=localhost;Database=test;Username=postgres;Password=postgres;";
    }
    else
    {
        throw new InvalidOperationException(
            "Connection string 'DefaultConnection' is not configured. " +
            "Please configure ConnectionStrings__DefaultConnection via .env or environment variables.");
    }
}
```
2. **Для ключа підпису JWT (`JwtAuthenticationExtensions.cs`):**
```csharp
public static AuthenticationBuilder AddJwtAuthentication(
    this IServiceCollection services,
    IConfiguration configuration,
    IHostEnvironment? environment = null)
{
    var jwtKey = configuration["Jwt:Campus:Key"];
    if (string.IsNullOrWhiteSpace(jwtKey))
    {
        if (environment?.IsEnvironment("Testing") == true)
        {
            jwtKey = "TestingDefaultSecretKeyMinimum32CharactersLong!";
        }
        else
        {
            throw new InvalidOperationException(
                "Jwt:Campus:Key is not configured. Please configure Jwt__Campus__Key via .env or environment variables.");
        }
    }
...
```
3. **Єдине джерело для NVIDIA API Key (`NvidiaProviderClient.cs`):**
```csharp
private static string GetApiKey(string? environmentVariable)
{
    var variableName = string.IsNullOrWhiteSpace(environmentVariable)
        ? "NVIDIA_API_KEY"
        : environmentVariable;

    return Environment.GetEnvironmentVariable(variableName)
        ?? Environment.GetEnvironmentVariable("NVIDIA_API_KEY")
        ?? string.Empty;
}
```

---

## 4. Результати верифікації (Контрольний чек-лист)

### Крок 1. Перевірка статусу та відстеження Git
```powershell
git status --short
# Вивід:
#  M .gitignore
#  M StudyMentor.sln
#  M study-mentor-api/Extensions/JwtAuthenticationExtensions.cs
#  M study-mentor-api/Extensions/ServiceExtensions.cs
#  M study-mentor-api/Program.cs
#  M study-mentor-api/Services/Ai/NvidiaProviderClient.cs
#  M study-mentor-api/appsettings.json
#  M study-mentor-api/study-mentor-api.csproj
# ?? .env.example
# ?? doc/module-3/
# ?? docker-compose.yml

git check-ignore -v .env
# Вивід: .gitignore:411:.env    .env (успішно ігнорується)

git ls-files .env
# Вивід: порожньо (файл не відстежується індексом Git)

git diff --check
# Вивід: код 0 (жодних помилок форматування чи пробілів)
```

### Крок 2. Перевірка відсутності секретів у файлах репозиторію
```powershell
git grep -n -I -e "nvapi-" -e "Password=1111"
# Вивід: порожньо (exit code 1 - збігів у репозиторії не виявлено)
```

### Крок 3. Перевірка конфігурації Docker Compose
```powershell
docker compose config
# Вивід: синтаксис валідний, обов'язковий пароль успішно зчитаний з .env
```

### Крок 4. Перевірка збірки та тестів
```powershell
dotnet restore StudyMentor.sln
dotnet build StudyMentor.sln
dotnet test StudyMentor.sln
# Вивід: Build succeeded. 0 Error(s). Всі тести проходять успішно.
```

### Крок 5. Перевірка Fail-Fast поведінки та незалежності тестів
1. **Звичайний запуск без `.env` (Development):**
```text
Unhandled exception. System.InvalidOperationException: Connection string 'DefaultConnection' is not configured.
Please configure ConnectionStrings__DefaultConnection via .env or environment variables.
   at StudyMentorApi.Program.Main(String[] args) in D:\campus\StudyMentor\study-mentor-api\Program.cs:line 42
```
2. **Запуск у режимі `Testing` без локального `.env`:**
```text
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Testing
```
*Застосунок успішно запускається в ізольованому тестовому оточенні WebApplicationFactory без фізичного файлу .env.*

3. **Запуск у звичайному режимі з дійсним `.env`:**
```text
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5132
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Development
```

---

## 5. Безпека історії репозиторію, ротація та очищення рішення

### 5.1. Ротація скомпрометованих ключів
- **NVIDIA API Key:** Старий навчальний ключ підлягає анулюванню (revoke) у кабінеті розробника NVIDIA NIM. Для локальної розробки використовується змінна `NVIDIA_API_KEY`.
- **JWT Signing Key:** Згенеровано новий 48-байтний криптографічний секрет за допомогою `RandomNumberGenerator`. Значення збережено суто локально в `.env` і ніколи не публікується у відкритому вигляді.
- **Пароль БД:** Згенеровано випадковий надійний пароль для контейнера PostgreSQL, який передається через змінну `POSTGRES_PASSWORD`.

### 5.2. Обґрунтування очищення `StudyMentor.sln`
У початковому файлі `StudyMentor.sln` були присутні застарілі прив'язки до зовнішніх проєктів:
```text
"..\..\..\source\repos\dotnetlab5\dotnetlab5.csproj"
"..\..\..\source\repos\dotnetlab7chat\dotnetlab7chat.csproj"
```
Оскільки ці шляхи посилалися на абсолютні каталоги на диску `D:\source\repos\...` автора одного з попередніх комітів, команда `dotnet build StudyMentor.sln` падала з помилками `MSB3202: The project file was not found`. Видалення цих неіснуючих посилань відновило працездатність збірки розв'язку для всіх учасників команди.
