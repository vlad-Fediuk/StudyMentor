# SonarQube — пакет 2, модуль 3

## Умови аналізу

Дата виконання: 30 жовтня 2026 року, Europe/Kyiv.
SonarQube Community Build 26.10.0.133078 у локальному Docker-контейнері
`studymentor-sonarqube`; SonarScanner for .NET 12.0.0; .NET SDK 10.0.402.
Дашборд: http://127.0.0.1:9000/dashboard?id=study-mentor.

Для C# вибрано профіль **Sonar way comprehensive** (321 активне правило).
Перший пробний запуск мав core-профіль на етапі Roslyn-компіляції; він не використовується
як основа порівняння. Експорт baseline зроблено після повторного повного аналізу
незмінного початкового коміту. Обидва порівнювані аналізи мають comprehensive-профіль.

Обсяг: backend C# і включені в .NET-проєкти конфігураційні файли; `scanAll=false`.
Це не аудит обох frontend-клієнтів. Метрики включають зовнішні Roslyn-діагностики,
імпортовані Sonar; `external_roslyn:*` відрізняються від власних `csharpsquid:*` правил.

`Migrations/**`, `bin/**`, `obj/**` виключені з джерел аналізу.
Міграції є згенерованим кодом і спотворювали б метрики підтримуваності.
Вони можуть з'являтися серед попереджень компілятора, але не входять до підсумкових issues.
Із coverage також виключено Program.cs — wiring хоста не перевіряється поточними
ізольованими HTTP-тестами. Це явне обмеження метрики, а не доказ покриття всього застосунку.

Baseline: `840e563` / `sonar-baseline`, 3 тести.
Після змін: `ea59120` / `sonar-after`, 23 тести.
Підвищення coverage включає додані тести версіювання й збереження вправ;
його не можна приписувати лише рефакторингу.

## Початкові категорії та пріоритети

| Категорія | Кількість | Файли з найбільшою кількістю / пріоритет |
| --- | ---: | --- |
| Code Smells | 22 | UserService — 4, ChatSessionService — 4, LearningContentGenerationService — 3; спершу контрактні/структурні проблеми, далі INFO-поради |
| Vulnerabilities | 2 | appsettings.json — 1 (найвищий), LectureChunkService — 1 (оцінити експлуатаційний ризик) |
| Bugs | 0 | Файлів немає; це результат даного набору правил |
| Security Hotspots | 0 | Файлів немає; окремий список hotspots порожній |

Критичність Code Smells: CRITICAL — 8, MAJOR — 4, MINOR — 4, INFO — 6.
Критичність Vulnerabilities: BLOCKER — 1, MINOR — 1.
Severity інструмента оцінюється разом із контекстом, а не використовується як готова оцінка бізнес-ризику.

До: [метрики](sonar/baseline/metrics.json), [issues](sonar/baseline/issues.json), [hotspots](sonar/baseline/hotspots.json).
Після: [метрики](sonar/after/metrics.json), [issues](sonar/after/issues.json), [hotspots](sonar/after/hotspots.json).
Початковий стан заархівовано JSON-експортом. Для знімків і подальшого перегляду його
також повторно проаналізовано окремим локальним проєктом `study-mentor-baseline`
з того самого коміту та comprehensive-профілю. Його метрики звірено з первинним baseline;
це не порівняння різних ревізій. Фінальний проєкт — `study-mentor`.
Знімки: [до](screenshots/sonar/baseline-dashboard.png), [після](screenshots/sonar/after-dashboard.png),
[список проблем до](screenshots/sonar/baseline-issues.png), [після](screenshots/sonar/after-issues.png).

## Розбір реальних проблем

1. **S6418, appsettings.json:19 — BLOCKER, Vulnerability.** Аналіз знайшов значення ApiKey
   у конфігурації. Реальне значення не відтворюється у звіті. Сам факт наявності схожого
   на ключ рядка потребує перевірки власником; якщо він дійсний, його слід відкликати,
   замінити й винести в секрети/змінні середовища. Видалення поточного рядка не відкликає
   ключ і не прибирає його з Git-історії. Передано P1; у цьому пакеті конфігурація не змінювалася.
2. **S6444, LectureChunkService.cs:110 — MINOR, Vulnerability.** Regex без явного таймауту
   може надовго зайняти потік на невдалому вході. Потрібно оцінити походження/розмір тексту,
   задати обмеження часу та перевірити звичайні й патологічні входи. Не закривати як false positive
   без цієї перевірки. Передано P3 для узгодження змін у LectureChunks.
3. **S3358, LearningContentGenerationService — MAJOR, Code Smell.** Вкладені тернарні вирази
   у виборі кількості вправ і побудові повідомлення ускладнювали читання. Винесено defaultCount
   і використано послідовні умови; значення 5/8 і тексти повідомлень збережені. Виправлено P2.
4. **S1192, PromptComposer/AiChatService — MINOR, Code Smell.** Повторення fallback-контексту
   та completed створювало ризик розходження копій при майбутній зміні.
   Винесено константи без зміни їх значень. Виправлено P2.
5. **S927, UserService/ChatSessionService — CRITICAL, Code Smell.** Назви параметрів override
   відрізняються від базового declaration. Це реальна проблема узгодженості API коду,
   але severity інструмента не означає критичну аварію виконання. Перейменовувати узгоджено,
   враховуючи виклики з іменованими аргументами. Передано P1.

Додатково P2 виправив S6966, S1118 та S8969. Структурні зміни й перевірки контрактів:
[refactoring-report.md](refactoring-report.md).

## Розподіл і пріоритети

| Пакет | Реальні проблеми / дія |
| --- | --- |
| P1 | S6418 у конфігурації — найвищий пріоритет; 4 × S927 у UserService і 4 × S927 у ChatSessionService; ASP0025 у JwtAuthenticationExtensions |
| P2 | S1192 × 2, S3358 × 2, S8969, S6966, S1118 — виправлено; низькопріоритетні Roslyn-поради оцінювати за користю |
| P3 | S6444 і SYSLIB1045 в LectureChunkService; S3267 у TestService; узгодити timeout із сценаріями профілювання |
| P4 | У поточному експорті немає двох підтверджених знахідок у його файлах. Не вигадувати issue IDs; повторити аналіз після інтеграції його змін і переглянути Reliability/Caching |

Це таблиця для передачі команді. Повідомлення іншим учасникам не надсилалися.
Власні коміти інших учасників і по два їхні виправлення ще не перевірені.

## Security Hotspots і межі висновків

Hotspots у цьому запуску: **0**; список перевірено через окремий API hotspots.
Позначок Safe/Reviewed немає, бо переглядати в цій категорії нічого.
S6418/S6444 у цій версії/профілі належать саме до Vulnerabilities, а не Hotspots.
Нуль Bugs/Hotspots не доводить відсутність помилок чи вразливостей.

Окремо `dotnet list study-mentor-api package --vulnerable --include-transitive`
знайшов транзитивний `Microsoft.OpenApi` 2.4.1 (High, GHSA-v5pm-xwqc-g5wc).
Це результат NuGet audit, не додаткова Sonar-знахідка. Під час інтеграції Swagger/OpenAPI
прямий Microsoft.AspNetCore.OpenApi оновлено з 10.0.3 до 10.0.10 для сумісності
з Asp.Versioning.OpenApi; транзитивна вразлива версія також оновилася.
Повторний NuGet audit не знайшов вразливих пакетів у поточних джерелах.
Чотири HTTP-тести описів API пройшли. Це не закриває дві окремі Sonar Vulnerabilities.

## Відтворення

1. Запустити Docker Desktop, потім `docker start studymentor-sonarqube`.
2. Завантажити токен у змінну середовища SONAR_TOKEN, не записуючи його у Git.
3. Виконати `./scripts/Invoke-SonarAnalysis.ps1 -ServerUrl http://127.0.0.1:9000 -Label after`.
   Скрипт збирає solution, запускає тести й покриття, передає звіт та чекає завершення server processing.
4. `./scripts/Export-SonarResults.ps1 -Label after` зберігає JSON метрик, issues і hotspots.

Для локального встановлення облікові дані зашифровані Windows DPAPI у
`$env:USERPROFILE/.codex/studymentor-local/sonar-secrets.xml`, поза репозиторієм.
Токен можна завантажити в поточну PowerShell-сесію:

```powershell
$localSonar = Import-Clixml "$env:USERPROFILE/.codex/studymentor-local/sonar-secrets.xml"
$env:SONAR_TOKEN = [Net.NetworkCredential]::new('', $localSonar.Token).Password
```

Після інтеграції P1/P3/P4 повторити фінальні метрики всього модуля.

## Порівняння до / після

| Метрика | До | Після |
| --- | ---: | ---: |
| Bugs | 0 | 0 |
| Vulnerabilities | 2 | 2 |
| Code Smells | 22 | 13 |
| Security Hotspots | 0 | 0 |
| Coverage, % | 11.4 | 19.3 |
| Duplications, % | 1.1 | 0.4 |
| reliability rating (Software Quality) | C | A |
| security rating (Software Quality) | E | E |
| maintainability rating (Software Quality) | A | A |

Quality Gate фінального нового коду: **PASSED**. Покриття нового коду: **81,8%**; нових зауважень: **0**; дублювання нового коду: **0%**.
Рейтинги таблиці відповідають Software Quality у вебінтерфейсі. Старі API-метрики bugs/code_smells і нові Software Quality — різні класифікації: S6966 у legacy API є Code Smell, а у вебінтерфейсі належить Reliability. Тому початкові 22 Code Smells відповідають 21 Maintainability issue та 1 Reliability issue, а не 22 Maintainability issues.

Усунуто 7 власних csharpsquid-знахідок і 2 початкові INFO-поради у тестах. Залишкові 13 Code Smells переважно належать іншим пакетам або є низькопріоритетними Roslyn-порадами. Дві Vulnerabilities залишаються відкритими: зелений Gate нового коду не означає, що весь проєкт безпечний.
