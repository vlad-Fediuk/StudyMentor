INSERT INTO majors ("Id", "Name")
SELECT
    md5('bench-major-' || gs::text)::uuid,
    'Спеціальність ' || gs::text
FROM generate_series(1, 19) AS gs
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO subjects ("Id", "Name", "MajorId")
SELECT
    md5('bench-subject-' || gs::text)::uuid,
    'Дисципліна ' || gs::text,
    md5('bench-major-' || (((gs - 1) % 19) + 1)::text)::uuid
FROM generate_series(1, 198) AS gs
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO lectures ("Id", "Name", "SubjectId")
SELECT
    md5('bench-lecture-' || gs::text)::uuid,
    'Лекція №' || gs::text || ': Архітектура та продуктивність програмних систем',
    md5('bench-subject-' || (((gs - 1) % 198) + 1)::text)::uuid
FROM generate_series(1, 4996) AS gs
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO groups ("Id", "Name")
SELECT
    md5('bench-group-' || gs::text)::uuid,
    'КН-' || (100 + gs)::text
FROM generate_series(1, 50) AS gs
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO users ("Id", "Name", "Password", "GroupId", "Email", "Roles")
SELECT
    md5('bench-user-' || gs::text)::uuid,
    'Студент ' || gs::text,
    '$2a$11$benchhashedpasswordvalue000000000000000000000000000000',
    md5('bench-group-' || (((gs - 1) % 50) + 1)::text)::uuid,
    'user' || gs::text || '@studymentor.edu.ua',
    ARRAY['User'::text]
FROM generate_series(1, 50000) AS gs
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO chat_sessions ("Id", "UserId", "LectureId")
SELECT
    md5('bench-session-' || gs::text)::uuid,
    md5('bench-user-' || gs::text)::uuid,
    md5('bench-lecture-' || gs::text)::uuid
FROM generate_series(1, 100) AS gs
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO chat_messages ("Id", "ChatSessionId", "Content", "Timestamp", "Role", "SequenceNumber", "Status")
SELECT
    md5('bench-message-' || gs::text)::uuid,
    md5('bench-session-' || (((gs - 1) % 100) + 1)::text)::uuid,
    'Навчальне повідомлення для генерації матеріалів #' || gs::text,
    TIMESTAMPTZ '2026-10-01 10:00:00+00' + (gs || ' minutes')::interval,
    1,
    1,
    'completed'
FROM generate_series(1, 200) AS gs
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO exercises ("Id", "Name", "ChatMessageId", "ExerciseType", "SourceFlashcardId")
SELECT
    md5('bench-flashcard-' || gs::text)::uuid,
    'Набір карток #' || gs::text,
    md5('bench-message-' || gs::text)::uuid,
    'Flashcard',
    NULL
FROM generate_series(1, 200) AS gs
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO exercises ("Id", "Name", "ChatMessageId", "ExerciseType", "SourceFlashcardId")
SELECT
    md5('bench-test-' || gs::text)::uuid,
    'Контрольний тест #' || gs::text,
    md5('bench-message-' || (((gs - 1) % 200) + 1)::text)::uuid,
    'Test',
    md5('bench-flashcard-' || (((gs - 1) % 200) + 1)::text)::uuid
FROM generate_series(1, 2000) AS gs
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO test_questions ("Id", "Prompt", "Order", "TestId")
SELECT
    md5('bench-question-' || t::text || '-' || q::text)::uuid,
    'Питання №' || q::text || ' до контрольного тесту #' || t::text || ': оберіть правильне твердження',
    q - 1,
    md5('bench-test-' || t::text)::uuid
FROM generate_series(1, 2000) AS t
CROSS JOIN generate_series(1, 3) AS q
ON CONFLICT ("Id") DO NOTHING;

INSERT INTO test_answer_variants ("Id", "Text", "IsCorrect", "Order", "TestQuestionId")
SELECT
    md5('bench-variant-' || t::text || '-' || q::text || '-' || v::text)::uuid,
    'Варіант відповіді ' || v::text || ' для питання ' || q::text || ' тесту #' || t::text,
    (v = 1),
    v - 1,
    md5('bench-question-' || t::text || '-' || q::text)::uuid
FROM generate_series(1, 2000) AS t
CROSS JOIN generate_series(1, 3) AS q
CROSS JOIN generate_series(1, 4) AS v
ON CONFLICT ("Id") DO NOTHING;

VACUUM ANALYZE;
