# Інструкція до демонстрації та захисту завдань Пакета 3 (Модуль 3)

**Дисципліна:** Практикум з розробки програмного забезпечення  
**Модуль 3:** Якість, продуктивність, надійність та безпека програмного забезпечення  
**Роль:** «Продуктивність та БД» (Пакет 3)  
**Виконавець:** Федюк Владислав  
**Основна гілка:** `p/vlad-fediuk-3` (або `p/m3-fediuk-3`)

---

## 1. Стан виконання завдань відповідно до методичних рекомендацій

| Завдання методички | Вимоги методички | Статус | Що зроблено / Де подивитися |
| :--- | :--- | :---: | :--- |
| **1. Benchmark Testing (Apache Benchmark)** | Заміри $c = 5, 20, 50, 100$, 1000 запитів, Throughput, Time per request, Failed requests, $p_{50}, p_{95}, p_{99}$, таблиця результатів | **Виконано** | Звіт [`benchmark-stress-testing.md`](benchmark-stress-testing.md)<br>Скріншоти: [`screenshots/benchmark/`](screenshots/benchmark/) |
| **2. Stress Testing (Grafana k6)** | Сценарій k6, стадії 10→50→100→200 VU, перевірка статусів, визначення межі стабільності системи | **Виконано** | Сценарій [`benchmark/stress-test.js`](benchmark/stress-test.js)<br>Скріншот: `screenshots/benchmark/k6-stress-stages.png` |
| **3. Spike Testing (бонус методички)** | Різкий стрибок навантаження 10→300→10 VU, реакція сервера, використання ресурсів | **Виконано** | Сценарій [`benchmark/spike-test.js`](benchmark/spike-test.js)<br>Скріншот: `screenshots/benchmark/k6-spike-300vu.png` |
| **4. Profiling (`dotnet-trace` + `dotnet-counters`)** | Профілювання під навантаженням (30 VU), аналіз CPU/RAM/GC/Allocations, виявлення ≥1 проблеми, оптимізація та порівняння до/після | **Виконано** | Звіт [`profiling-report.md`](profiling-report.md)<br>Скріншоти: [`screenshots/profiling/`](screenshots/profiling/)<br>Код: `LectureService.cs`, `LectureEndpoints.cs` |
| **5. Database Optimization** | Аналіз повільних запитів через `EXPLAIN (ANALYZE, BUFFERS)`, оптимізація індексів та SQL, порівняння планів, буферів і часу | **Виконано** | Звіт [`db-optimization-report.md`](db-optimization-report.md)<br>Скріншоти: [`screenshots/db/`](screenshots/db/)<br>Міграція: `20261030111409_AddUserEmailIndex.cs` |
| **6. Усунення SonarQube Code Smells / Vulnerabilities** | Усунення знахідок статичного аналізу у своїх файлах (Regex timeout, LINQ simplification) | **Виконано** | `LectureChunkService.cs` (`S6444`, `SYSLIB1045`)<br>`TestService.cs` (`S3267`) |
| **7. Git-ієрархія та коміти** | Окремі підгілки для кожної таски, змістовні коміти за конвенцією, мердж у персональну гілку | **Виконано** | Гілки: `p3/performance-testing`, `p3/profiling`, `p3/db-optimization` смерджені в `p/vlad-fediuk-3` |

---

## 2. Швидкий запуск середовища для живої демонстрації

### Крок 1. Перевірка бази даних у Docker
Переконайтеся, що контейнер `study-mentor-db` запущено:
```powershell
docker start study-mentor-db
```
*(База вже наповнена тестовим скриптом `doc/module-3/benchmark/seed.sql`: 5 000 лекцій, 50 000 користувачів, 2 000 тестів, 24 000 варіантів відповідей).*

### Крок 2. Запуск API у режимі Release
```powershell
$env:RateLimiting__Enabled="false"; $env:Caching__Enabled="false"; $env:ASPNETCORE_URLS="http://localhost:5132"
dotnet run -c Release --no-build --project study-mentor-api --launch-profile http
```

### Крок 3. Запуск автоматизованих тестів
```powershell
dotnet test study-mentor-api.Tests\study-mentor-api.Tests.csproj
dotnet test study-mentor-api.IntegrationTests\study-mentor-api.IntegrationTests.csproj
```
*(Очікуваний результат: **18 passed, 0 failed**).*

---

## 3. Сценарій демонстрації викладачу по кроках

### Етап 1: Демонстрація тестування продуктивності (Benchmark + Stress + Spike)
1. Відкрити [`benchmark-stress-testing.md`](benchmark-stress-testing.md).
2. **Показати тестовий датасет:** скрипт [`benchmark/seed.sql`](benchmark/seed.sql) (5 000 лекцій, 50 000 користувачів).
3. **Показати результати Apache Benchmark (`ab`):**
   - Показати скріншоти `ab-lectures-c5.png`, `ab-lectures-c20.png`, `ab-lectures-c50.png`, `ab-lectures-c100.png`.
   - Пояснити, як throughput виходить на плато (~58 req/s), а $p_{95}$ зростає зі 192 мс до 1855 мс.
4. **Показати результати Grafana k6:**
   - Скріншот ступінчастого тесту `k6-stress-stages.png`: визначення межі стабільної роботи ($p_{95} < 500\text{ мс}$) — до **18 одночасних користувачів**.
   - Скріншот пікового тесту `k6-spike-300vu.png`: сплеск до 300 VU, стрибок споживання пам'яті до 1.6 ГБ, відсутність помилок HTTP (`0% Error Rate`).
5. **(За бажанням) Запустити живий тест:**
   ```powershell
   ab -n 500 -c 20 http://localhost:5132/lectures
   ```

---

### Етап 2: Демонстрація результатів профілювання (Profiling)
1. Відкрити [`profiling-report.md`](profiling-report.md).
2. **Показати виявлену проблему за допомогою `dotnet-trace` (Speedscope):**
   - Відкрити скріншот `profiling-before-trace.png`.
   - Пояснити: `StateManager.StartTrackingFromQuery` займав **84.8% часу матеріалізації EF Core**, тому що 5 220 сутностей реєструвалися у `ChangeTracker` на кожен HTTP-запит без `.AsNoTracking()`, викликаючи важкий `NavigationFixer`.
3. **Показати метрики `dotnet-counters`:**
   - Відкрити скріншот `profiling-counters-comparison.png`.
   - GC паузи займали **11.6% часу** (115 мс/с), алокація сягала **250 МБ/с**, Gen 2 збирався щосекунди.
4. **Показати виконану оптимізацію в коді:**
   - Відкрити `study-mentor-api/Lectures/LectureService.cs`: перехід на `.AsNoTracking()` та метод `GetAllResponsesAsync` із прямою проєкцією `.Select(...)` у DTO.
   - Відкрити `study-mentor-api/Lectures/LectureEndpoints.cs`: усунення методу `BuildSubjectResponsesAsync` і двох зайвих SQL-запитів до `subjects` та `majors`.
   - Відкрити `study-mentor-api/LectureChunks/LectureChunkService.cs`: усунення Sonar-вразливості `S6444` (Regex ReDoS timeout) через `[GeneratedRegex]`.
5. **Показати результати ПІСЛЯ оптимізації:**
   - Відкрити скріншот `profiling-after-trace.png` (0% витрат на StateManager) та `profiling-k6-after-30vu.png`.
   - GC паузи скоротилися на **80.8%** (у 5.2 раза менше), алокації скоротилися на **52.4%**, споживання CPU — на **72%**, затримка $p_{95}$ знизилася з 538 мс до **361 мс**.

---

### Етап 3: Демонстрація оптимізації бази даних (Database Optimization)
1. Відкрити [`db-optimization-report.md`](db-optimization-report.md).
2. **Оптимізація 1 (Індекс `Users.Email`):**
   - Показати `explain-users-before.png`: `Seq Scan` по 50 000 користувачів, 625 буферів, 6.03 мс.
   - Показати міграцію `20261030111409_AddUserEmailIndex.cs` (`CREATE UNIQUE INDEX "IX_users_Email"`).
   - Показати `explain-users-after.png`: `Index Scan`, 4 буфери, **0.15 мс (прискорення у 40 разів, дисковий I/O знижено у 156 разів)**.
   - Пояснити компроміс: розмір індексу `2.4 МБ` повністю виправданий співвідношенням операцій читання/запису.
3. **Оптимізація 2 (Перенесення фільтрації `GET /tests?sourceFlashcardId` у SQL):**
   - Показати `explain-tests-before.png`: сканування та передача всіх 24 000 рядків тестів/варіантів у пам'ять C# (18 498 буферів, 68 мс, пропускна здатність API лише `5.17 req/s`, час відповіді 4.1 секунди!).
   - Показати `TestService.cs` (`GetBySourceFlashcardIdAsync` + `AsNoTracking`).
   - Показати `explain-tests-after.png`: `Bitmap Index Scan on IX_exercises_SourceFlashcardId`, вибірка рівно 10 цільових тестів (160 буферів, 3.38 мс).
   - Показати `ab-tests-after-c20.png`: пропускна здатність API зросла з `5.17 req/s` до **`385.67 req/s` (у 74.6 раза!)**, час відповіді скоротився з 4 089 мс до **118 мс**.

---

### Етап 4: Демонстрація історії Git та гілок
```powershell
git log --oneline --graph -n 20
```
Показати чітку структуру комітів відповідно до методички та плану:
1. `test(perf): add db seed script for load tests`
2. `test(perf): add k6 stress and spike scenarios`
3. `docs(module-3): add benchmark and stress testing report (baseline)`
4. `perf(lectures): use single projection query with no tracking for lecture list`
5. `fix(sonar): use compile-time regex with timeout in lecture chunk service`
6. `docs(module-3): add profiling report with before and after traces`
7. `perf(db): add unique index on users email`
8. `perf(tests): filter tests by source flashcard in sql instead of memory`
9. `docs(module-3): add db optimization report`
10. Акуратні merge-коміти підгілок `p3/*` у персональну гілку `p/vlad-fediuk-3`.
