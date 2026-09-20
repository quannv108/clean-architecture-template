---
type: Convention
title: "Record Syntax"
description: "Positional records in Web.Api, standard-syntax records with DataAnnotations in Application."
tags: [records, syntax, validation, conventions]
status: stable
---

# Record Syntax

Two styles, and the reason is mechanical rather than aesthetic.

## Web.Api - positional

```csharp
internal sealed record CreateFeatureRequest(string Name, string Description);
internal sealed record FeatureResponse(Guid Id, string Name);
```

Transport shapes are short and validated by the command behind them.

## Application - standard syntax with attributes

```csharp
public sealed record CreateUserCommand : ICommand<Guid>
{
    [Required, EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required, MaxLength(100)]
    public string DisplayName { get; init; } = string.Empty;
}
```

**Positional record parameters cannot carry property attributes** in a form
[`ValidationDecorator`](../../../src/Application/Abstractions/Behaviors/ValidationDecorator.cs) can read. Since validation is declared on the
command, commands and queries must use standard syntax.

## Rules

* Commands and queries: `sealed record`, standard syntax, `init` setters, DataAnnotations.
* Requests and responses: positional records, in the same file as the endpoint.
* Domain events: positional records - they carry ids and are never validated.
* Response DTOs returned by cached repositories: positional records.

Related: [ADR 0013: DataAnnotations for shape validation, domain errors for business rules](../../adr/0013-dataannotations-validation.md).
