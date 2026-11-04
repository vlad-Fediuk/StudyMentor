# Рефакторинг: конкретные изменения и использованные подходы

Сравнение: `git diff sonar-baseline..sonar-after`. Этот документ описывает выполненные
изменения P2. Пагинация и новые контракты v2 относятся к API Versioning.

## 1. Extract Class: разделение генерации и сохранения упражнений

Файлы:
- `study-mentor-api/LearningContent/LearningContentGenerationService.cs`
- `study-mentor-api/LearningContent/GeneratedContentPersistence.cs`
- `study-mentor-api/Extensions/ServiceExtensions.cs`

До изменений LearningContentGenerationService одновременно организовывал генерацию через AI,
собирал контекст лекции, работал с сообщениями чата, преобразовывал результаты генерации
в EF-сущности карточек/тестов, сохранял упражнения и собирал DTO ответа.

Создан отдельный GeneratedContentPersistence. Из большого сервиса в него перенесены:

| Ранее | Теперь | Ответственность |
| --- | --- | --- |
| PersistGeneratedContentAsync | PersistAsync | Выбор сохранения карточек или теста |
| PersistFlashcardsAsync | PersistFlashcardsAsync | GeneratedFlashcardsDto → Flashcard/Card → FlashcardService.CreateAsync |
| PersistTestAsync | PersistTestAsync | GeneratedTestDto → Test/TestQuestion/TestAnswerVariant → TestService.CreateAsync |
| BuildAnswerVariants | BuildAnswerVariants | Очистка, дедупликация, правильный ответ, порядок вариантов |
| BuildExerciseName | BuildExerciseName | Название упражнения и ограничение длины |

Генератор вместо двух зависимостей FlashcardService/TestService получает один
GeneratedContentPersistence и вызывает `await contentPersistence.PersistAsync(...)`.
Он по-прежнему организует AI-запрос и сценарий чата; полной переработки всей подсистемы не было.

Это приём **Extract Class**, применённый ради **Single Responsibility Principle**.
SRP — принцип проектирования, а не название отдельного GoF-паттерна.

## 2. Dependency Injection: подключение нового компонента

Новый компонент получает FlashcardService и TestService через конструктор.
Генератор получает GeneratedContentPersistence через свой конструктор.
Регистрация: `services.AddScoped<GeneratedContentPersistence>()` в ServiceExtensions.

DI уже использовался в проекте; здесь он применён к новой зависимости.
Scoped означает экземпляр в рамках scope запроса, а не один объект на всё приложение.
Это конструкторное внедрение зависимостей. Собственного контейнера или фабрики не создавалось.
Зависимости остаются конкретными классами; введение DI не означает, что вся подсистема
переписана на интерфейсы или полностью реализует Dependency Inversion Principle.

## 3. Intercepting Filter / Pipeline: общая обработка CRUD-ошибок

Файлы: `Extensions/CrudExceptionFilter.cs` и `Flashcards/FlashcardEndpoints.cs`.

Раньше GetById/Create/Update/Delete содержали похожие try/catch. Создан IEndpointFilter,
который вызывает `await next(context)` и перехватывает ожидаемые исключения:

- NotFoundException → 404 и `{ error }`;
- ValidationException → 400 и `{ error }`, только при handleValidation=true;
- остальные ошибки проходят дальше к существующему глобальному обработчику.

Фильтр подключён к GET по id и DELETE; на POST/PUT включена обработка валидации.
Он подключён и к GET по id версии v2. GET списка не получил новую политику перехвата.

Здесь использован механизм **Intercepting Filter** в pipeline ASP.NET Core.
Фильтр оборачивает выполнение endpoint общим поведением. По устройству это близко
к декорированию вызова, но отдельную иерархию GoF Decorator я не создавал.

Почему не удалены catch с передачей всех ошибок глобальному handler:
глобальный handler выдаёт `{ error, statusCode }`, а прежний CRUD выдавал `{ error }`.
Добавление поля изменило бы внешний JSON-контракт. Фильтр сохраняет прежний формат.
Сам глобальный exception handler уже существовал до этой работы.

## 4. Общий ручной mapper: Entity → DTO

Файл: `Flashcards/FlashcardMappings.cs`.

Прежде endpoint и генерация учебного контента отдельно собирали FlashcardResponse.
Теперь `ToResponse(this Flashcard)` создаёт прежний DTO: Id, Name, ChatMessageId и Cards.
`ToV2Response(this Flashcard)` использует результат ToResponse и добавляет CardsCount.

Mapper вызывают FlashcardEndpoints, FlashcardService при пагинации и GeneratedContentPersistence.
Это устраняет копии преобразования и зависимость сервиса пагинации от класса endpoint.
Использованы extension methods и ручной DTO mapping; AutoMapper не добавлялся.

## 5. Упрощение условий и константы

LearningContentGenerationService:
- вложенный тернарный выбор Count заменён отдельным defaultCount;
- BuildSuccessMessage переписан на последовательные if с ранним return;
- 5 и 8 получили имена DefaultTestQuestionCount и DefaultFlashcardCount;
- убран лишний null-forgiving оператор после проверки значения.

GeneratedContentPersistence: 48 заменено на MaxExerciseTitleLength.

PromptComposer: четыре копии строки отсутствующего контекста заменены MissingContext;
JSON-формат вынесен в JsonOutputFormat. Значения строк сохранены.

AiChatService: пять повторов completed заменены на CompletedStatus.

Это **Extract Constant**, упрощение условных выражений и ранние возвраты.
Builder в PromptComposer/PromptBuilder уже существовал; в этом рефакторинге он не вводился.

Program: вместо Run() в async Main — await RunAsync(); добавлен protected-конструктор
для класса, используемого как entry point. Регистрации отсутствующих MVC-контроллеров убраны.

## 6. Какие реальные Sonar-проблемы закрыты

| Правило | Файл | Количество |
| --- | --- | ---: |
| S1192 | PromptComposer | 1 |
| S1192 | AiChatService | 1 |
| S3358 | LearningContentGenerationService | 2 |
| S8969 | LearningContentGenerationService | 1 |
| S6966 | Program | 1 |
| S1118 | Program | 1 |

Итого: **7 собственных csharpsquid-знаходок**. Отдельно устранены две исходные
информационные Roslyn-подсказки CA1861 в тестах посредством static readonly массивов.
Разделению класса и общему mapper не приписываются выдуманные Sonar rule IDs.

## 7. Как проверено сохранение поведения

`StudyMentor.QualityTests/FlashcardContractTests.cs` проверяет старые поля JSON,
точный формат error-ответов, создание/чтение/обновление/удаление через HTTP,
Location созданного ресурса и очистку пробелов. В этом же файле — отдельные проверки
новой версии API и четырёх документов Swagger/OpenAPI.

`StudyMentor.QualityTests/GeneratedContentPersistenceTests.cs` проверяет:
- сохранение связи с исходным ChatMessageId;
- trim термина/определения и длину названия 48 символов;
- правильный ответ, в том числе когда его нет среди вариантов;
- дедупликацию вариантов без учёта регистра;
- порядок ответов и fallback второго варианта;
- отклонение неподдерживаемого типа результата.

Весь набор: **23 успешных теста**. Это суммарно тесты рефакторинга и API Versioning,
а не 23 отдельных теста только рефакторинга. Persistence-тесты используют подставные
CRUD-сервисы, HTTP-тесты — TestServer и InMemory. Они не являются проверкой живого AI-провайдера.

Дополнительно обычный backend проверен с PostgreSQL/pgvector: v1/v2 и четыре документа API
вернули 200, неизвестная v3 — 404. Полный AI-чат в рамках этой проверки не прогонялся.

## Формулировка для защиты

«Я разделил генерацию и сохранение учебного контента приёмом Extract Class в соответствии
с SRP. Новый компонент подключил через конструкторное DI. Повторную обработку ожидаемых
ошибок вынес в endpoint filter ASP.NET Core, сохранив старые HTTP-коды и структуру JSON.
Преобразование карточек в DTO объединил в ручном mapper. Упростил вложенные условия и
выделил константы. Исправления связал с реальными правилами Sonar и проверил тестами».
