# QA Engineering: Test Design Techniques and Test Cases

## Purpose of this material

This document is a study guide for students who learn QA Engineering. It explains test design techniques and test cases. The text is intended as a factual source for StudyMentor, so AI answers based on this document should use the definitions, examples, rules, and terminology below.

## Learning outcomes

After studying this topic, a student should be able to:

- explain what QA Engineering is;
- distinguish quality assurance from software testing;
- explain what a test case is and what fields it usually contains;
- write clear test cases with preconditions, steps, expected result, and test data;
- use equivalence partitioning;
- use boundary value analysis;
- use decision table testing;
- use negative testing;
- explain why good test design is more important than simply writing many tests.

## What QA Engineering means

QA Engineering is the engineering activity focused on improving and verifying the quality of a software product. QA is not only clicking buttons and finding bugs. QA includes understanding requirements, planning testing, designing test cases, checking risks, reporting defects, and helping the team prevent problems before release.

Quality assurance and testing are related but not identical.

Quality assurance is a broader process. It focuses on preventing defects by improving processes, requirements, development practices, and review activities.

Software testing is a part of quality assurance. It focuses on executing checks or tests to find defects and confirm that the software behaves as expected.

In simple words: QA asks "How do we build quality into the process?", while testing asks "Does the product work correctly in these conditions?"

## What a requirement is

A requirement describes what the software must do or what qualities it must have. Requirements can be functional or non-functional.

Functional requirement example:

The system must allow a user to log in using a Microsoft account.

Non-functional requirement example:

The login request should complete in less than 3 seconds under normal network conditions.

QA engineers analyze requirements because unclear requirements lead to unclear tests. If a requirement is ambiguous, a QA engineer should ask questions before writing final test cases.

Ambiguous requirement:

"The system should load quickly."

Better requirement:

"The chat page should load in less than 2 seconds for 95% of requests under normal network conditions."

## What a test case is

A test case is a documented set of conditions, steps, input data, and expected results used to verify a specific behavior of the system.

A good test case should be clear enough that another tester can execute it and understand whether the result is passed or failed.

Typical fields of a test case:

- ID;
- title;
- priority;
- preconditions;
- test data;
- steps;
- expected result;
- actual result;
- status;
- notes or attachments.

Example test case:

| Field | Value |
| --- | --- |
| ID | AUTH-001 |
| Title | Successful login with Microsoft account |
| Priority | High |
| Preconditions | User has a valid Microsoft account. Application is opened on the login page. |
| Test data | Valid Microsoft account credentials |
| Steps | 1. Click "Sign in with Microsoft". 2. Complete Microsoft authentication. 3. Wait for redirect back to the application. |
| Expected result | User is authenticated and redirected to the main chat page. JWT token is stored by the client. |
| Status | Not executed |

## Properties of a good test case

A good test case is specific. It checks one main idea and has a clear expected result.

A good test case is reproducible. Another person should be able to repeat the same steps and get the same result if the system has not changed.

A good test case is independent when possible. It should not depend on a long chain of previous tests unless that dependency is clearly stated.

A good test case uses realistic data. Test data should represent real user behavior or important edge cases.

A good test case has an objective expected result. "Works correctly" is not a good expected result because it is vague. "User is redirected to `/chat` and sees the chat input field" is better.

## Test design techniques

Test design techniques are systematic methods for choosing what to test. They help QA engineers find important cases without testing every possible input manually.

The goal of test design is not to create the maximum number of tests. The goal is to create useful tests that cover risks, requirements, and important behavior.

## Equivalence partitioning

Equivalence partitioning is a test design technique where input data is divided into groups that should be processed in the same way by the system. Each group is called an equivalence class or equivalence partition.

The idea is that if many values should behave the same, it is usually enough to test one or a few representative values from that group.

Example requirement:

"Password must be from 8 to 20 characters."

Equivalence partitions:

| Partition | Description | Example |
| --- | --- | --- |
| Invalid | Less than 8 characters | `Abc123` |
| Valid | 8 to 20 characters | `Abc12345` |
| Invalid | More than 20 characters | `Abc12345678901234567890` |

Instead of testing every password length, QA chooses representative values from each partition.

## Boundary value analysis

Boundary value analysis is a test design technique focused on values at the edges of valid and invalid ranges. Many defects happen near boundaries because developers may use incorrect comparison operators such as `<` instead of `<=`.

For the password length requirement "8 to 20 characters", important boundary values are:

- 7 characters: invalid;
- 8 characters: valid;
- 9 characters: valid;
- 19 characters: valid;
- 20 characters: valid;
- 21 characters: invalid.

Boundary value analysis is often used together with equivalence partitioning.

Example test cases:

| Test ID | Input | Expected result |
| --- | --- | --- |
| PASS-001 | 7-character password | Validation error |
| PASS-002 | 8-character password | Password accepted |
| PASS-003 | 20-character password | Password accepted |
| PASS-004 | 21-character password | Validation error |

## Negative testing

Negative testing checks how the system behaves with invalid input, incorrect actions, missing data, expired sessions, or unexpected conditions.

Negative testing is important because real users make mistakes and external systems fail. A stable application should not crash or expose sensitive data when something goes wrong.

Examples of negative tests:

- login with an expired Microsoft token;
- submit an empty chat message;
- open a protected endpoint without a JWT token;
- send a request with an invalid `Authorization` header;
- upload a file with an unsupported format;
- enter letters into a field that accepts only numbers.

Expected behavior in negative tests should be clear. For example, when a protected endpoint is called without a token, the expected result is `401 Unauthorized`, not a server crash.

## Decision table testing

Decision table testing is a technique used when the result depends on a combination of conditions. It is useful for business rules with multiple inputs.

Example: access to the chat page.

Conditions:

- user has a valid JWT token;
- token is not expired;
- route is protected.

Decision table:

| Rule | Has token | Token valid | Protected route | Expected result |
| --- | --- | --- | --- | --- |
| 1 | Yes | Yes | Yes | Access allowed |
| 2 | Yes | No | Yes | Redirect to login |
| 3 | No | No | Yes | Redirect to login |
| 4 | No | No | No | Access allowed if page is public |

This technique helps QA engineers avoid missing combinations.

## Test case examples for authentication

### AUTH-001: Successful login redirects user to main page

Preconditions:

User has a valid Microsoft account. Backend authentication endpoint is available.

Steps:

1. Open the login page.
2. Click the Microsoft login button.
3. Complete Microsoft authentication.
4. Wait until the application returns from Microsoft.

Expected result:

The application exchanges the Microsoft ID token for the backend JWT token. The user is redirected to `/chat`. The login page is not shown after successful authentication.

### AUTH-002: Protected page is not available without token

Preconditions:

No JWT token is stored in browser local storage.

Steps:

1. Open `/chat` directly in the browser.

Expected result:

The application redirects the user to `/login`.

### AUTH-003: Authenticated user should not stay on login page

Preconditions:

Valid JWT token exists in browser local storage.

Steps:

1. Open `/login`.

Expected result:

The application redirects the user to `/chat`.

### AUTH-004: Protected API endpoint rejects request without token

Preconditions:

Backend API is running.

Steps:

1. Send `GET /chat-sessions` without `Authorization` header.

Expected result:

The server returns `401 Unauthorized`.

### AUTH-005: Protected API endpoint accepts request with valid token

Preconditions:

User is authenticated and has a valid JWT token.

Steps:

1. Send `GET /chat-sessions` with header `Authorization: Bearer <valid token>`.

Expected result:

The server processes the request according to the endpoint logic. The response is not rejected because of missing authentication.

## Bug report basics

A bug report is a document that describes a defect found in the system. It should help developers reproduce and fix the problem.

Typical bug report fields:

- title;
- environment;
- preconditions;
- steps to reproduce;
- actual result;
- expected result;
- severity;
- priority;
- attachments;
- additional notes.

Example:

Title: User is redirected to login page after successful Microsoft authentication.

Steps to reproduce:

1. Open login page.
2. Click Microsoft login.
3. Complete authentication.
4. Wait for redirect.

Actual result:

User returns to `/login`.

Expected result:

User is redirected to `/chat`.

Severity:

High, because successful login does not lead the user to the main application page.

## Severity and priority

Severity describes how strongly a defect affects the system.

Priority describes how quickly the defect should be fixed.

Examples:

High severity and high priority: users cannot log in.

High severity and low priority: rare data corruption in an old admin tool used once per year.

Low severity and high priority: typo on the main login button before a public demo.

Low severity and low priority: small alignment issue on a rarely used settings page.

Severity is usually about technical or user impact. Priority is usually about business urgency.

## Common mistakes in QA work

Mistake 1: writing test cases without understanding requirements.

Mistake 2: checking only the happy path. The happy path is the scenario where everything goes correctly. Real systems also need negative and edge-case testing.

Mistake 3: using vague expected results such as "system works".

Mistake 4: creating too many duplicate test cases that do not increase coverage.

Mistake 5: forgetting about authorization and security checks.

Mistake 6: reporting bugs without clear steps to reproduce.

## Practical rules for students

Start testing from requirements. If the requirement is unclear, ask questions.

Write test cases that another person can execute.

Use equivalence partitioning to reduce repeated tests.

Use boundary value analysis for numeric ranges, lengths, dates, and limits.

Always include negative tests for important features.

For authentication features, test both frontend route protection and backend endpoint protection.

Do not only check what happens when everything is correct. Also check missing data, invalid data, expired tokens, and unauthorized access.

## Key terms

QA Engineering: activities that help prevent defects and verify product quality.

Software testing: checking software behavior against requirements and expectations.

Requirement: a description of what the system must do or what quality it must have.

Test case: documented conditions, steps, data, and expected result for verifying behavior.

Precondition: a state that must exist before a test starts.

Expected result: the behavior that should happen if the system works correctly.

Actual result: the behavior that actually happened during test execution.

Equivalence partitioning: dividing inputs into groups that should behave similarly.

Boundary value analysis: testing values at the edges of valid and invalid ranges.

Negative testing: checking how the system behaves with invalid input or incorrect conditions.

Decision table: a table that maps combinations of conditions to expected outcomes.

Severity: impact of a defect.

Priority: urgency of fixing a defect.

## Self-check questions

1. What is the difference between QA and testing?
2. What fields should a test case contain?
3. Why should expected results be specific?
4. What is equivalence partitioning?
5. What is boundary value analysis?
6. Why are negative tests important?
7. When should decision table testing be used?
8. What is the difference between severity and priority?

## Short answers for AI tutoring

QA Engineering focuses on improving and verifying software quality. Testing is one part of QA.

A test case documents what to check, under what conditions, with what data, and what result is expected.

Equivalence partitioning reduces the number of tests by grouping inputs that should behave the same.

Boundary value analysis checks values near limits because defects often occur at the edges.

Negative testing verifies that the system handles invalid input, unauthorized access, and error conditions correctly.

For authentication, QA should test successful login, failed login, expired tokens, protected frontend routes, and protected backend endpoints.
