# Звіт з безпеки API: Завдання 2 «Захист API»

**Модуль:** Модуль 3 «Якість, продуктивність, надійність та безпека ПЗ»  
**Пакет:** Пакет 1 «Безпека та конфігурація»  
**Гілка:** `p1/api-security`  
**Дата:** 25 жовтня 2026 року  

---

## 1. Мета та завдання

Метою виконання Завдання 2 є всебічний аудит та усунення вразливостей автентифікації, авторизації, захист від горизонтального підвищення привілеїв (IDOR — Insecure Direct Object References), запобігання несанкціонованому самонаданню адміністративних прав та ліквідація витоків внутрішньої діагностичної інформації.

---

## 2. Результати аудиту безпеки: стан До та Після

| Ендпоінт | Вразливість (До) | Реалізований захист (Після) |
| :--- | :--- | :--- |
| `POST /users` | Відкритий, приймав довільні `Roles` із JSON (можливість створення акаунта `Admin`). | Публічна реєстрація зберігається, але роль `Admin` примусово відкидається; завжди призначається роль `User`. |
| `GET /users` | Публічний доступ, витік повного списку користувачів та їхніх email. | Захищено політикою `.RequireAuthorization("admin")`. Доступ має лише адміністратор. |
| `GET /users/{id}` | Публічний доступ без перевірки автентифікації. | `.RequireAuthorization("user")`. Звичайний користувач може переглядати тільки власний профіль (`currentUserId == id`). `Admin` має повний доступ. Для чужого профілю — `403 Forbidden`. |
| `PUT /users/{id}` | Відсутня перевірка власника (IDOR), можливість зміни ролі на `Admin`. | `.RequireAuthorization("user")`. Перевірка власника або `Admin`. Звичайному користувачу заборонено змінювати роль (зберігається `existing.Roles`). При відсутності нового пароля зберігається існуючий. |
| `DELETE /users/{id}` | Будь-який користувач міг видалити чужий акаунт за `id`. | `.RequireAuthorization("user")`. Перевірка `currentUserId == id \|\| user.IsAdmin()`. Інакше — `403 Forbidden`. |
| `GET /chat-sessions` | Публічний перегляд усіх сесій студентів. | `.RequireAuthorization("admin")`. Звичайні користувачі не мають доступу до списку чужих сесій. |
| `GET /chat-sessions/{id}` | Публічний IDOR-доступ до чужої навчальної сесії. | `.RequireAuthorization("user")` + перевірка `session.UserId == currentUserId \|\| user.IsAdmin()`. |
| `GET /chat-sessions/user/{userId}` | Публічний доступ до сесій будь-якого користувача. | `.RequireAuthorization("user")` + перевірка `userId == currentUserId \|\| user.IsAdmin()`. |
| `POST /chat-sessions` | Приймав `request.UserId` від клієнта (створення сесії від імені іншого). | Для звичайного користувача `UserId` безумовно береться з перевіреного JWT (`NameIdentifier`). Для `Admin` перевіряється непорожність (`400 Bad Request` при `Guid.Empty`). |
| `PUT /chat-sessions/{id}` | Можливість зміни власника сесії. | Перевірка власника або `Admin`. Для звичайного користувача зміна `UserId` заблокована. |
| `DELETE /chat-sessions/{id}` | Видалення чужої сесії за `id`. | Перевірка власника або `Admin` (`403 Forbidden` для чужих). |
| `GET /chat-messages` | Повертав усі повідомлення всіх користувачів. | `.RequireAuthorization("admin")`. Доступно лише для модерації/аудиту. |
| `GET /chat-messages/session/{sessionId}` | Читання листування чужої сесії. | Перевірка власника батьківської сесії через `ChatSessionService` (`403 Forbidden`). |
| `GET /chat-messages/{id}` | Читання чужого повідомлення за `id`. | Визначення сесії повідомлення та перевірка її власника (`403 Forbidden`). |
| `POST /chat-messages` | Підміна `request.ChatSessionId` для ін'єкції у чужу сесію. | Обов'язкова валідація власника батьківської сесії перед створенням повідомлення (`403 Forbidden`). |
| `PUT /chat-messages/{id}` | Зміна чужого повідомлення та перенесення в іншу сесію. | Перевірка власника поточної сесії; для звичайного користувача зміна сесії заблокована. |
| `DELETE /chat-messages/{id}` | Видалення чужих повідомлень за `id`. | Перевірка власника через батьківську сесію (`403 Forbidden`). |
| `/api/ai-chat/*` | Публічні виклики AI та читання історії повідомлень без авторизації. | `.RequireAuthorization("user")`. Повна перевірка належності сесії `chatId` поточному користувачу (`403 Forbidden`). |
| `/api/learning-content/*` | Неавторизована генерація контенту та прив'язка до чужих чатів. | `.RequireAuthorization("user")`. Перевірка належності `request.ChatId` поточному користувачу (`403 Forbidden`). |
| `/flashcards`, `/tests` | Анонімний доступ до карток і тестів. | `.RequireAuthorization("user")`. Доступ лише для автентифікованих студентів та викладачів. |
| Обробка помилок | `catch (Exception ex) => Results.Problem(ex.Message, statusCode: 500)` у контролерах. | Вилучено витік внутрішніх деталей винятків `ex.Message`. Усі непередбачені збої делегуються централізованому `UseGlobalExceptionHandler()`. |

---

## 3. Матриця доступу за ролями (RBAC / PBAC)

| Ресурс / Маршрут | Анонімний клієнт | Авторизований `User` | Роль `Admin` |
| :--- | :---: | :---: | :---: |
| `POST /auth` (Автентифікація) | **200 OK** | **200 OK** | **200 OK** |
| `POST /users` (Реєстрація) | **201 Created** (роль `User`) | **201 Created** (роль `User`) | **201 Created** |
| `GET /users` (Список акаунтів) | **401 Unauthorized** | **403 Forbidden** | **200 OK** |
| `GET /users/{id}` (Профіль) | **401 Unauthorized** | **200 OK** (лише свій) / **403** (чужий) | **200 OK** |
| `PUT /users/{id}` (Оновлення) | **401 Unauthorized** | **200 OK** (свій, без ролей) / **403** | **200 OK** (повне) |
| `DELETE /users/{id}` (Видалення) | **401 Unauthorized** | **204 NoContent** (свій) / **403** | **204 NoContent** |
| `GET /chat-sessions` | **401 Unauthorized** | **403 Forbidden** | **200 OK** |
| `GET /chat-sessions/{id}` | **401 Unauthorized** | **200 OK** (своя) / **403** (чужа) | **200 OK** |
| `GET /chat-sessions/user/{userId}` | **401 Unauthorized** | **200 OK** (свої) / **403** (чужі) | **200 OK** |
| `POST /chat-sessions` | **401 Unauthorized** | **201 Created** (`userId` з JWT) | **201 Created** (або 400 на порожній ID) |
| `PUT /chat-sessions/{id}` | **401 Unauthorized** | **200 OK** (своя) / **403** (чужа) | **200 OK** |
| `DELETE /chat-sessions/{id}` | **401 Unauthorized** | **204 NoContent** (своя) / **403** | **204 NoContent** |
| `GET /chat-messages` | **401 Unauthorized** | **403 Forbidden** | **200 OK** |
| `/chat-messages/session/{id}` | **401 Unauthorized** | **200 OK** (своя сесія) / **403** (чужа) | **200 OK** |
| `/api/ai-chat/*` | **401 Unauthorized** | **200 OK** (своя сесія) / **403** (чужа) | **200 OK** |
| `/api/learning-content/*` | **401 Unauthorized** | **200 OK** | **200 OK** |
| `/flashcards`, `/tests` | **401 Unauthorized** | **200 OK** | **200 OK** |

---

## 4. Результати автоматизованого інтеграційного тестування

Для перевірки реалізованого захисту створено тестовий набір інтеграційних тестів на базі `WebApplicationFactory<Program>` та NUnit у проєкті `study-mentor-api.IntegrationTests`:

- **Файл тестів:** `study-mentor-api.IntegrationTests/Security/ApiSecurityIntegrationTests.cs`
- **Середовище запуску:** `Testing` (ізольована база даних InMemory, підписані тестові JWT-токени).
- **Результат запуску:**

```text
Passed!  - Failed: 0, Passed: 12, Skipped: 0, Total: 12, Duration: 770 ms
study-mentor-api.IntegrationTests.dll (net10.0)
```

### Перелік підтверджених тестів:

1. `AnonymousRequest_ToProtectedEndpoint_ShouldReturn401Unauthorized`:
   - Підтверджено статус `401 Unauthorized` без передачі токена для `/users`, `/chat-sessions`, `/api/ai-chat/*`, `/flashcards`, `/tests`.
2. `RegularUser_GetAllUsers_ShouldReturn403Forbidden`:
   - Підтверджено `403 Forbidden` для звичайного користувача при спробі читання списку всіх користувачів.
3. `RegularUser_GetOtherUserProfile_ShouldReturn403Forbidden`:
   - Підтверджено захист від IDOR: користувач `User A` не може переглянути профіль `User B` (`403 Forbidden`).
4. `RegularUser_UpdateOrDeleteOtherUserProfile_ShouldReturn403Forbidden`:
   - Підтверджено заборону модифікації або видалення чужого профілю користувачем `User A` (`403 Forbidden`).
5. `RegularUser_GetOrModifyOtherUserChatSession_ShouldReturn403Forbidden`:
   - Підтверджено блокування доступу до сесії `User B` за ID або за `userId` (`403 Forbidden` на GET, PUT, DELETE).
6. `RegularUser_AccessOrPostMessagesInOtherUserSession_ShouldReturn403Forbidden`:
   - Підтверджено заборону читання, ін'єкції повідомлень або видалення повідомлень у чужій сесії (`403 Forbidden`).
7. `RegularUser_AccessAiOrGenerateContentInOtherUserSession_ShouldReturn403Forbidden`:
   - Підтверджено блокування виклику AI Chat або генерації контенту в чужій сесії (`403 Forbidden`).
8. `AdminUser_CanAccessOtherUserChatSession_ShouldReturn200Ok`:
   - Підтверджено легітимний адміністративний доступ до сесій, повідомлень і списку користувачів (`200 OK`).
9. `PublicRegistration_WithAdminRoleSupplied_ShouldIgnoreRolesAndAssignUserRole`:
   - Підтверджено, що спроба реєстрації з тілом `{"roles": ["Admin"]}` ігнорує поле ролей і зберігає виключно роль `User`.
10. `Owner_CanReadOwnProfileAndSession_ShouldReturn200Ok`:
    - Підтверджено успішний доступ власника до свого профілю, сесії та повідомлень (`200 OK`).
11. `Admin_CreateChatSessionWithEmptyGuid_ShouldReturn400BadRequest`:
    - Підтверджено повернення `400 Bad Request` при передачі адміністратором некоректного `Guid.Empty` замість валідного `UserId`.
12. `RegularUser_CreateChatSession_ShouldForceOwnerUserId`:
    - Підтверджено примусове встановлення `UserId` із JWT для звичайного користувача, навіть якщо клієнт намагався підставити чужий `userId`.

---

## 5. Відомі ризики та подальші завдання

1. **Обмеження частоти запитів (Rate Limiting):**
   - Наразі маршрути захищені від неавторизованого та чужого доступу, проте захист від перевантаження AI-ендпоінтів та брутфорсу аутентифікації винесено в наступне **Завдання 3: Rate Limiting** (`p1/rate-limiting`), де буде налаштовано політики `api`, `ai` та `auth` зі статусом `429 Too Many Requests`.
2. **Паролі в БД:**
   - Поточна навчальна модель бази даних зберігає тестові паролі без хешування (відповідає навчальному завданню попередніх модулів). Захист API гарантує неможливість перегляду чужих паролів через API.
3. **Хмарні провайдери AI:**
   - Для інтеграційного тестування генерації AI використовуються мок- або ізольовані сесії без здійснення реальних платних викликів до NVIDIA API.
