# Модуль 3 — пакет 2: як показати роботу

1. [SonarQube: аналіз, проблеми й порівняння](sonarqube-report.md).
2. [API Versioning: маршрути, контракти й перевірки](api-versioning.md).
3. [Рефакторинг: зміни, причини й тести](refactoring-report.md).
4. [Окрема Postman-колекція P2](flashcards-versioning.postman_collection.json).

## Що встановлено

Локальний dotnet-sonarscanner 12.0.0 записано в dotnet-tools.json.
Для нового checkout виконайте `dotnet tool restore`.
Пакети versioning, TestHost, EF InMemory та Coverlet відновлюються через `dotnet restore`.
Java 21 для сканера завантажена автоматично; окремий Java installer не потрібний.
Docker Desktop і .NET 10 вже були встановлені.
Для знімків використано headless Chrome та Playwright, встановлений поза репозиторієм
у `$env:USERPROFILE/.codex/studymentor-local/node_modules`.

## Тести

```powershell
dotnet test StudyMentor.sln --settings coverage.runsettings --collect:"XPlat Code Coverage"
```

23 тести: сумісність старого/v1 JSON, CRUD round-trip, валідація й 404,
v2 із метаданими та пагінацією, порожня сторінка, неправильні параметри,
відсутня версія, збереження згенерованих вправ, Swagger/OpenAPI v1/v2.
Тестам не потрібні PostgreSQL, Docker або AI-ключі.
Повний backend для ручної Postman-перевірки потребує власної налаштованої PostgreSQL БД.

Для backend використайте звичайний запуск:

```powershell
dotnet run --project study-mentor-api --launch-profile http
```

Адреса профілю: `http://localhost:5132`. Саме її вкажіть у baseUrl Postman-колекції.
Після інтеграції захисту API налаштуйте Authorization із дійсним токеном.
Стара версія повертає масив, v2 — items/page/pageSize/total; v3 повертає 404.

## Демо Sonar

Після запуску Docker Desktop:

```powershell
docker start studymentor-sonarqube
```

Відкрийте [фінальний проєкт](http://127.0.0.1:9000/dashboard?id=study-mentor).
Логін: `admin`. Пароль змінено з початкового й зашифровано Windows DPAPI поза Git.
Щоб переглянути пароль у власній PowerShell-сесії:

```powershell
$localSonar = Import-Clixml "$env:USERPROFILE/.codex/studymentor-local/sonar-secrets.xml"
[Net.NetworkCredential]::new('', $localSonar.Password).Password
```

У Summary виберіть **Overall Code**, щоб показати загальні метрики; **New Code**
відображає інші показники, лише для зміненого коду. В Issues покажіть залишкові проблеми
та порівняйте із JSON baseline. Знімки й експорти лежать у screenshots/sonar та sonar.

Git-перевірки для демонстрації:

```powershell
git log --oneline origin/p/module-3..HEAD
git diff sonar-baseline..sonar-after -- study-mentor-api/Flashcards
git diff sonar-baseline..sonar-after -- study-mentor-api/LearningContent
```

Зміни локальні; створення командних PR, review і злиття до p/module-3 виконуються
за правилами вашої команди. Після інтеграції інших пакетів потрібно повторити Sonar і регресію.
