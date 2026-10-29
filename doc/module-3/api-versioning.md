# API Versioning — пакет 2, модуль 3

## Реалізація

Minimal API використовує `Asp.Versioning.Http` 10.2.3 та версію в URL.
Реєстрація винесена в `AddApplicationApiVersioning`, маршрути — в `FlashcardEndpoints`.
Обов'язковий ресурс для демонстрації: Flashcards. Решта ресурсів зберігають свої маршрути.

| Маршрут | Контракт |
| --- | --- |
| `/flashcards` | Старий CRUD, без зміни JSON |
| `/api/v1/flashcards` | Той самий CRUD і JSON, що в старого маршруту |
| `/api/v2/flashcards?page=1&pageSize=20` | GET: `{ items, page, pageSize, total }`; елементи містять `cardsCount` |
| `/api/v2/flashcards/{id}` | GET: поля v1 + `cardsCount` |

v2 не додає `createdAt`: у моделі немає достовірної дати створення.
Записи впорядковані за Id; Skip/Take виконуються в БД. Розмір сторінки: 1–100,
номер сторінки додатний; переповнення зміщення відхиляється з 400.
Порожня або завелика сторінка повертає порожній `items` і правильний `total`.
Count і вибірка сторінки є окремими запитами: за одночасних змін даних це не snapshot.

## Сумісність

Старі клієнти продовжують користуватися `/flashcards`; переносити їх на v2 не потрібно.
Успішні версійовані відповіді повідомляють `api-supported-versions`.
Жодна з підтримуваних версій поки не deprecated, тому заголовок про deprecated-версії
не є обов'язковим і строк вимкнення старих маршрутів не призначено.

Невідома URL-версія, наприклад `/api/v3/flashcards`, повертає **404**.
У початковому плані очікувалося 400, але Asp.Versioning навмисно використовує 404
для версіювання сегментом URL; параметр UnsupportedApiVersionStatusCode це не змінює.
Джерело: [офіційні release notes](https://github.com/dotnet/aspnet-api-versioning/releases).

## Перевірка

`dotnet test StudyMentor.sln --settings coverage.runsettings --collect:"XPlat Code Coverage"`

HTTP-тести запускають справжні маршрути в TestServer, перевіряють JSON v1/старого маршруту,
пагінацію без повторів між сторінками, метадані v2, заголовок версій, невідому версію,
помилкові параметри й старий формат 404.
Тестова БД — InMemory; лише в тестовому контексті ігнорується vector-поле LectureChunk.
Це перевірка HTTP-контракту, а не перевірка PostgreSQL/pgvector або повного production-host.
