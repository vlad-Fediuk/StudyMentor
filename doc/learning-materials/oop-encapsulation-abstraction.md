# OOP: Encapsulation and Abstraction

## Purpose of this material

This document is a study guide for students who learn object-oriented programming. It explains two core OOP principles: encapsulation and abstraction. The text is written as a factual knowledge source for StudyMentor, so answers generated from this material should rely on the definitions, examples, rules, and comparisons below.

## Learning outcomes

After studying this topic, a student should be able to:

- explain what encapsulation means in object-oriented programming;
- explain what abstraction means and why it reduces complexity;
- distinguish encapsulation from abstraction;
- design a simple class with private state and public behavior;
- choose which details should be hidden inside a class;
- identify bad class design where implementation details are exposed;
- explain why getters and setters are not always enough for good encapsulation.

## Basic context: what OOP tries to solve

Object-oriented programming is a programming paradigm where a program is organized around objects. An object combines data and behavior. Data describes the state of the object, and behavior describes what the object can do.

For example, in a learning system, a `Student` object may have data such as name, email, group, and current progress. It may also have behavior such as enrolling in a course, submitting an answer, or viewing progress.

OOP helps developers model real or business concepts in code. The main goal is not simply to create many classes. The goal is to make code easier to understand, change, test, and reuse. Encapsulation and abstraction are two principles that support this goal.

## Encapsulation

Encapsulation is the principle of hiding an object's internal state and implementation details from the outside world and allowing access only through a controlled public interface.

In simple words, encapsulation means that an object protects its data and controls how that data can be changed.

The usual technical tools for encapsulation are:

- private fields;
- public methods;
- properties with validation;
- constructors that create valid objects;
- methods that express business actions instead of exposing raw data changes.

Encapsulation answers the question: "How do we protect the internal state of an object from incorrect use?"

## Why encapsulation is important

Encapsulation is important because public data can be changed from anywhere in the program. When many parts of the program directly change an object's fields, it becomes difficult to know where a bug came from.

Without encapsulation, an object can easily enter an invalid state. An invalid state is a combination of values that should not be possible according to the business rules.

Example of invalid state:

```csharp
public class BankAccount
{
    public decimal Balance;
}

var account = new BankAccount();
account.Balance = -1000;
```

The code allows a bank account to have a negative balance even if the business rule says that a standard account cannot go below zero. The problem is that the field is public and there is no controlled operation.

A better design:

```csharp
public class BankAccount
{
    private decimal balance;

    public decimal Balance => balance;

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Deposit amount must be positive.");
        }

        balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Withdraw amount must be positive.");
        }

        if (amount > balance)
        {
            throw new InvalidOperationException("Not enough funds.");
        }

        balance -= amount;
    }
}
```

In this design, other code cannot directly assign any value to `balance`. It must use `Deposit` or `Withdraw`. These methods protect the rules of the object.

## Public interface

The public interface of a class is the set of members that other code is allowed to use. It can include methods, properties, constructors, and events. Good encapsulation means the public interface should describe what the object can do, not how the object is implemented internally.

For example, `Withdraw(100)` is a meaningful business action. `SetBalance(balance - 100)` is weaker because it exposes the idea that external code is responsible for calculating and changing the balance.

Good public methods often use verbs:

- `EnrollStudent`;
- `SubmitAnswer`;
- `MarkAsCompleted`;
- `CalculateFinalGrade`;
- `AddMessage`;
- `ArchiveSession`.

These names describe behavior. They are usually better than generic setters when business rules are involved.

## Getters and setters are not automatically good encapsulation

A common mistake is thinking that private fields plus public getters and setters always means encapsulation. This is not always true.

Weak encapsulation:

```csharp
public class CourseProgress
{
    public int CompletedLessons { get; set; }
    public int TotalLessons { get; set; }
}
```

This code allows invalid values:

- `CompletedLessons = -5`;
- `TotalLessons = 0`;
- `CompletedLessons = 20` while `TotalLessons = 10`.

Better design:

```csharp
public class CourseProgress
{
    public int CompletedLessons { get; private set; }
    public int TotalLessons { get; }

    public CourseProgress(int totalLessons)
    {
        if (totalLessons <= 0)
        {
            throw new ArgumentException("Total lessons must be positive.");
        }

        TotalLessons = totalLessons;
    }

    public void CompleteLesson()
    {
        if (CompletedLessons >= TotalLessons)
        {
            throw new InvalidOperationException("All lessons are already completed.");
        }

        CompletedLessons++;
    }

    public double CompletionPercent => (double)CompletedLessons / TotalLessons * 100;
}
```

This class protects its rules. External code can complete a lesson but cannot directly create impossible progress.

## Abstraction

Abstraction is the principle of representing only the essential features of an object or concept while hiding unnecessary details.

In simple words, abstraction means focusing on what something does instead of every detail of how it works.

Abstraction answers the question: "What does the user of this code need to know, and what can be ignored?"

For example, when a student sends a message to an AI tutor, the application may use HTTP requests, JSON serialization, API keys, prompts, token limits, retries, and error handling. The UI component should not know all of that. It should use a simpler abstraction, such as:

```typescript
sendMessage(text: string): Observable<ChatResponse>
```

The UI knows what it needs: send a message and receive a response. The internal network details are hidden.

## Why abstraction is important

Abstraction reduces cognitive load. Cognitive load is the amount of information a person must keep in mind to understand or solve a problem. If every part of the program exposes every technical detail, the developer must understand too much at once.

Abstraction also helps replace implementation details. For example, an application may first store data in memory, later in PostgreSQL, and later in a cloud database. If the rest of the code depends on an abstraction, the storage implementation can change with less impact.

Example:

```csharp
public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken);
}
```

The service that uses `IUserRepository` does not need to know whether users are stored in a database, file, or external API. It only depends on the operations it needs.

## Encapsulation vs abstraction

Encapsulation and abstraction are related, but they are not the same.

Encapsulation is about protecting internal state and controlling access. It is mostly about data safety and object integrity.

Abstraction is about simplifying a concept by exposing only important behavior. It is mostly about reducing complexity for the user of the code.

Comparison:

| Principle | Main question | Main goal | Example |
| --- | --- | --- | --- |
| Encapsulation | How do we protect internal state? | Prevent invalid use | Private balance with `Deposit` and `Withdraw` |
| Abstraction | What details can be hidden? | Reduce complexity | `IEmailSender.SendAsync()` instead of SMTP details |

A good class usually uses both. It hides internal fields through encapsulation and exposes a simple public interface through abstraction.

## Example in a learning application

Imagine a chat session in StudyMentor.

Poor design:

```csharp
public class ChatSession
{
    public Guid Id;
    public List<string> Messages = new();
    public bool IsClosed;
}
```

This design exposes implementation details. Any part of the program can clear messages, add empty messages, or reopen a closed session by changing `IsClosed`.

Better design:

```csharp
public class ChatSession
{
    private readonly List<string> messages = new();

    public Guid Id { get; }
    public bool IsClosed { get; private set; }
    public IReadOnlyList<string> Messages => messages;

    public ChatSession(Guid id)
    {
        Id = id;
    }

    public void AddMessage(string message)
    {
        if (IsClosed)
        {
            throw new InvalidOperationException("Cannot add messages to a closed session.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException("Message cannot be empty.");
        }

        messages.Add(message.Trim());
    }

    public void Close()
    {
        IsClosed = true;
    }
}
```

This class protects its internal list and exposes meaningful actions. It does not allow invalid messages or adding messages after closing.

## Common mistakes

Mistake 1: making all fields public. This makes the class easy to misuse.

Mistake 2: using setters for every property without validation. This may still allow invalid states.

Mistake 3: creating an interface for every class without a reason. Abstraction should solve a real complexity or dependency problem.

Mistake 4: hiding too much. If a class does not expose enough useful behavior, other code may become awkward and unclear.

Mistake 5: confusing abstraction with vagueness. A good abstraction is clear and useful. It should hide unnecessary details but still express the real operation.

## Practical rules for students

Use private fields when direct access could break business rules.

Prefer methods that describe actions over setters that expose raw state changes.

Validate data at the boundary of the object. A class should protect its own invariants.

Expose read-only collections when external code should view but not modify internal data.

Use interfaces when code needs to depend on behavior rather than a concrete implementation.

Do not create abstractions only because "OOP requires interfaces". Create them when they make code easier to replace, test, or understand.

## Key terms

Class: a blueprint that describes data and behavior of objects.

Object: an instance of a class.

State: the current data stored inside an object.

Behavior: actions that an object can perform.

Public interface: the members of a class that other code can use.

Private implementation: internal details that should not be directly accessed by other code.

Invariant: a rule that must always remain true for an object to be valid.

Encapsulation: hiding internal state and controlling access through a public interface.

Abstraction: exposing essential behavior while hiding unnecessary details.

## Self-check questions

1. What is encapsulation?
2. Why can public fields be dangerous?
3. Why are getters and setters not always enough for good encapsulation?
4. What is abstraction?
5. How does abstraction reduce complexity?
6. What is the difference between encapsulation and abstraction?
7. Why should a class protect its own invariants?
8. When is an interface useful?

## Short answers for AI tutoring

Encapsulation protects object state by hiding internal data and allowing changes only through controlled methods or properties.

Abstraction hides unnecessary details and exposes only the essential behavior that other code needs.

A class with public fields usually has weak encapsulation because any code can change its state without validation.

A good public interface uses meaningful operations, such as `Deposit`, `Withdraw`, or `CompleteLesson`, instead of forcing external code to manipulate raw fields.

Encapsulation and abstraction often work together: encapsulation protects how an object works internally, and abstraction makes the object easier to use from the outside.
