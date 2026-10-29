# Пакет №2, модуль 3: выполненные задания и запуск

## 1. Какие задания были и что выполнено

| Задание | Статус | Что сделано / где показать |
| --- | --- | --- |
| SonarQube: первоначальный анализ | Выполнено | Сервер установлен, comprehensive-профиль, исходные метрики, 5 разобранных примеров, таблица ответственности, исключения миграций. Проект study-mentor-baseline, тег sonar-baseline |
| API Versioning | Выполнено | Старый /flashcards сохранён; /api/v1/flashcards возвращает прежний JSON; /api/v2/flashcards добавляет пагинацию и cardsCount. Проверены Swagger/OpenAPI обеих версий |
| Рефакторинг | Выполнено в файлах P2 | 7 собственных Sonar-знаходок исправлены; общий mapper; фильтр ошибок с сохранением JSON; сохранение упражнений выделено из большого сервиса; упрощены условия и выделены константы |
| Повторный Sonar и сравнение | Выполнено для этой ветки | Quality Gate Passed; Code Smells 22 → 13; покрытие 11,4% → 19,3%; нового кода 81,8%; дублирование 1,1% → 0,4%. Проект study-mentor, тег sonar-after |
| Автотесты | Выполнено | 23 успешных теста контрактов, CRUD, пагинации, сохранения упражнений и документов API |
| Обычный запуск с PostgreSQL | Дополнительно проверен | Отдельная PostgreSQL/pgvector, миграции применены; v1/v2 и четыре документа API вернули 200, v3 — 404 |
| Отчёты и доказательства | Выполнено | Три отчёта, JSON-экспорты, четыре скриншота Sonar, отдельная Postman-коллекция |
| Собственные Git-коммиты | Выполнено локально | Ветка codex/module-3-quality основана на origin/p/module-3; изменения разделены на смысловые коммиты |
| Раздать задачи другим участникам | Подготовлено, не отправлено | Таблица есть в sonarqube-report.md. Сообщения в ваш чат не отправлялись |
| GitHub PR, review и merge | Не выполнено | Изменения не отправлены на GitHub и не слиты в общую ветку |
| Финальный анализ после объединения всех четырёх пакетов | Ещё не выполнено | Требует готовых и слитых работ остальных участников. Текущий анализ подтверждает эту ветку |

Отличие от первоначального плана: неизвестная URL-версия `/api/v3/flashcards` возвращает **404**, а не 400.
Это штатное поведение Asp.Versioning при версии в сегменте URL. Подробности и источник — в api-versioning.md.
Рефакторинг чужих endpoint-файлов и AppDbContext не выполнялся: это зависело от интеграции работ других пакетов.

Остаются две Sonar Vulnerabilities: ключ в конфигурации и Regex без таймаута. Они описаны в отчёте
и отнесены к зонам P1/P3. Зелёный Gate относится к новому коду и не означает полного отсутствия рисков в проекте.
Rate Limiting, Caching, Reliability и оптимизация БД — задачи других пакетов, а не выполненная здесь часть.

## 2. Как запускать каждый раз

Открой Docker Desktop и дождись запуска движка.
Открой PowerShell в репозитории:

```powershell
cd E:\Github\StudyMentor
git branch --show-current
```

Ожидаемая ветка: `codex/module-3-quality`.
Затем:

```powershell
.\scripts\Start-Module3.ps1
```

Скрипт выполняет по порядку:

1. Проверяет Docker.
2. Запускает **существующий** контейнер `studymentor-sonarqube`.
3. Создаёт при первом запуске или запускает контейнер `studymentor-db` с PostgreSQL/pgvector.
4. Берёт локальные параметры подключения из конфигурации backend, не печатая пароль.
5. Дожидается готовности БД, восстанавливает инструменты и применяет миграции.
6. Запускает API на `http://localhost:5132`.

Запуск проверен 30 октября 2026 года: PostgreSQL на 55432, API на 5132, SonarQube на 9000.
После проверки процесс API остановлен, чтобы порт был свободен для твоего собственного запуска.

Оставь это окно PowerShell открытым: в нём работает backend.
Для остановки backend нажми **Ctrl+C**. База и Sonar останутся в Docker, данные БД сохраняются в volume.
Повторять `docker run --name sonarqube ...` не нужно: сервер уже существует и занимает порт 9000.

## 3. Что открыть в браузере

**Backend / Swagger:** http://localhost:5132

В верхнем переключателе Swagger выбери v1 или v2. Открой GET Flashcards → Try it out → Execute.
Для наглядного сравнения можно открыть прямо:

- http://localhost:5132/api/v1/flashcards
- http://localhost:5132/api/v2/flashcards?page=1&pageSize=2

На новой пустой БД v1 возвращает `[]`, а v2 — объект с items/page/pageSize/total.
Поле cardsCount видно на элементах, когда в БД есть наборы карточек; автотесты проверяют его на созданных данных.

Для этих демонстраций AI-ключ не нужен: они читают БД и не вызывают NVIDIA/LM Studio.
Полная работа AI-чата требует отдельной конфигурации провайдера и учебных материалов.
Frontend для защиты этих трёх backend-заданий запускать необязательно.

**SonarQube после изменений:** http://127.0.0.1:9000/dashboard?id=study-mentor

**Исходный код до изменений:** http://127.0.0.1:9000/dashboard?id=study-mentor-baseline

Дождись готовности сервера, если Docker только что запускался. Для входа логин `admin`.
Чтобы увидеть пароль в своей PowerShell-сессии, открой второе окно и выполни:

```powershell
$localSonar = Import-Clixml "$env:USERPROFILE/.codex/studymentor-local/sonar-secrets.xml"
[Net.NetworkCredential]::new('', $localSonar.Password).Password
```

Скопируй полученное значение в поле пароля SonarQube. Это локальный пароль; в Git он не сохранён.
В Sonar выбери **Summary → Overall Code**, чтобы увидеть общие метрики.
Вкладка **New Code** показывает метрики только изменённого кода. Затем открой **Issues**.

## 4. Как запустить тесты

Во втором окне PowerShell:

```powershell
cd E:\Github\StudyMentor
dotnet test StudyMentor.sln --settings coverage.runsettings --collect:"XPlat Code Coverage"
```

Ожидаемый результат: **23 passed, 0 failed**. Тестам не нужна живая PostgreSQL или Sonar.

## 5. Как повторить Sonar-анализ

Для просмотра готовых результатов повторное сканирование не нужно.
Если изменил код и хочешь обновить результаты, выполни из корня репозитория:

```powershell
$localSonar = Import-Clixml "$env:USERPROFILE/.codex/studymentor-local/sonar-secrets.xml"
$env:SONAR_TOKEN = [Net.NetworkCredential]::new('', $localSonar.Token).Password
.\scripts\Invoke-SonarAnalysis.ps1 -ServerUrl http://127.0.0.1:9000 -Label after
.\scripts\Export-SonarResults.ps1 -Label after
```

Скрипт выполнит сборку, тесты с покрытием, анализ и ожидание обработки сервером.
Он не просто отправляет исходники на Sonar. Окно надо оставить открытым до завершения.

## 6. Что показывать преподавателю по порядку

1. **Sonar до:** исходные проблемы и метрики; открыть baseline или его скриншоты.
2. **Разбор проблем:** sonarqube-report.md, пункты S6418, S6444, S3358, S1192, S927.
3. **Версии API:** Swagger v1/v2, массив старой версии и объект с пагинацией новой; старый маршрут работает.
4. **Рефакторинг:** refactoring-report.md и Git diff; объяснить, зачем общий mapper, фильтр ошибок и отдельный persistence-компонент.
5. **Тесты:** показать результат 23 успешных тестов.
6. **Sonar после:** Passed, отсутствие новых issues, улучшение метрик; честно показать две оставшиеся Vulnerabilities.

Git-команды для просмотра своей работы:

```powershell
git log --oneline origin/p/module-3..HEAD
git diff sonar-baseline..sonar-after -- study-mentor-api/Flashcards
git diff sonar-baseline..sonar-after -- study-mentor-api/LearningContent
```

## 7. Где все материалы

- [Основной отчёт Sonar](sonarqube-report.md)
- [Отчёт о версиях API](api-versioning.md)
- [Отчёт о рефакторинге](refactoring-report.md)
- [Postman-коллекция](flashcards-versioning.postman_collection.json)
- `screenshots/sonar` — изображения до/после
- `sonar/baseline` и `sonar/after` — исходные JSON-доказательства

Работай в **E:\Github\StudyMentor**. Две дополнительные папки worktree были техническими
копиями для разработки и исходного сканирования; для запуска они не нужны.

Образ PostgreSQL с vector-расширением: [официальный pgvector](https://github.com/pgvector/pgvector#docker).
