# Документація: Завдання 3 «Rate Limiting»

**Модуль:** Модуль 3 «Якість, продуктивність, надійність та безпека ПЗ»  
**Пакет:** Пакет 1 «Безпека та конфігурація»  
**Гілка:** `p1/rate-limiting`  
**Дата:** 25 жовтня 2026 року  

---

## 1. Мета та огляд

Метою реалізації Rate Limiting є захист системи від перевантаження (DDoS), запобігання вичерпанню квот та бюджету сторонніх платних AI-сервісів (NVIDIA API), а також захист маршрутів автентифікації від атак підбору паролів (brute-force).

Для реалізації використано вбудований у .NET механізм `Microsoft.AspNetCore.RateLimiting` на основі алгоритму **Fixed Window** без накопичення черги (`QueueLimit = 0`).

---

## 2. Політики обмеження частоти запитів

| Політика | Призначення | Обмеження за замовчуванням | Вікно (сек) | Ключ партиції (Partition Key) | Поведінка при черзі |
| :--- | :--- | :---: | :---: | :--- | :--- |
| **`ai`** | Захист ШІ-чатів (`/api/ai-chat/*`) та генерації контенту (`/api/learning-content/*`) | **10 запитів** | 60 | `userId` (з JWT) або IP клієнта | `QueueLimit = 0` (миттєве відхилення) |
| **`auth`** | Захист ендпоінта автентифікації (`POST /auth`) від перебору облікових даних | **10 запитів** | 60 | IP-адреса клієнта (`RemoteIpAddress`) | `QueueLimit = 0` (миттєве відхилення) |
| **`api`** | Загальний ліміт для захищених бізнес-ресурсів (`/users`, `/chat-sessions`, `/chat-messages`, `/flashcards`, `/tests`) | **100 запитів** | 60 | `userId` (з JWT) або IP клієнта | `QueueLimit = 0` (миттєве відхилення) |

> **Публічні довідники:**  
> Такі маршрути, як `GET /lectures`, `GET /majors`, `GET /subjects`, не обмежуються загальним лімітером згідно з вимогами методички.

---

## 3. Обробка відхилення запитів (`OnRejected`)

При перевищенні ліміту запитів повертається:
- **HTTP Status Code:** `429 Too Many Requests`.
- **HTTP Header:** `Retry-After` (кількість секунд до скидання вікна).
- **Content-Type:** `application/json`.
- **Тіло відповіді:**

```json
{
  "error": "Too many requests",
  "retryAfterSeconds": 58
}
```

---

## 4. Конфігурація та змінні середовища

Усі параметри лімітів можна змінювати через `appsettings.json` або перевизначати через `.env`:

```env
RateLimiting__Enabled=true
RateLimiting__Api__PermitLimit=100
RateLimiting__Api__WindowSeconds=60
RateLimiting__Ai__PermitLimit=10
RateLimiting__Ai__WindowSeconds=60
RateLimiting__Auth__PermitLimit=10
RateLimiting__Auth__WindowSeconds=60
```

### Прапорець вимкнення (`RateLimiting:Enabled`)
Якщо вказано `RateLimiting__Enabled=false` (або `false` в `appsettings.json`), механізм Rate Limiting переходить у режим `RateLimitPartition.GetNoLimiter(...)` для кожної зареєстрованої політики. Запити проходять без обмежень і без помилок резолвінгу політик. Це критично важливо для команди (зокрема учасників P3 та P4) під час проведення навантажувальних та стрес-тестів.

---

## 5. Результати інтеграційного тестування

Тести реалізовано у класі `RateLimitingIntegrationTests` за допомогою `WebApplicationFactory<Program>` та тестового середовища `Testing`:

- **Файл:** `study-mentor-api.IntegrationTests/RateLimiting/RateLimitingIntegrationTests.cs`
- **Загальна кількість тестів у проєкті:** 18 (12 тестів безпеки + 6 тестів rate limiting).
- **Результат виконання:**
  ```text
  Passed!  - Failed: 0, Passed: 18, Skipped: 0, Total: 18, Duration: 6 s
  study-mentor-api.IntegrationTests.dll (net10.0)
  ```

### Перелік тестів Rate Limiting:
1. `AiPolicy_WhenLimitExceeded_Returns429TooManyRequestsAndRetryAfterHeader`:
   - Підтверджено повернення статусу `429`, наявність заголовка `Retry-After` та JSON-відповіді з інформацією про час очікування.
2. `WindowRecovery_AfterWindowExpires_AllowsRequestsAgain`:
   - Підтверджено відновлення доступу (статус `200 OK`) після завершення періоду вікна ліміту.
3. `PartitionIsolation_DifferentUsers_HaveIndependentLimits`:
   - Підтверджено ізоляцію: вичерпання ліміту користувачем A не блокує запити користувача B.
4. `RateLimitingDisabled_WhenDisabledViaConfig_DoesNotThrottle`:
   - Підтверджено, що встановлення `RateLimiting:Enabled = false` пропускає всі запити понад встановлений ліміт без `429`.
5. `AuthPolicy_WhenLimitExceeded_Returns429TooManyRequests`:
   - Підтверджено блокування надмірних спроб входу за IP-адресою зі статусом `429` та заголовком `Retry-After`.
6. `PublicEndpoint_Lectures_IsNotThrottledByRateLimiter`:
   - Підтверджено вільний доступ до публічних лекцій без застосування обмежень AI чи API політик.
