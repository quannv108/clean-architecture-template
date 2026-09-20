Naming, placement and style rules. Read the relevant one before creating a file, not after.

Several are asserted by the architecture tests - [Constraints](../constraints.md) says which - so breaking
them fails the build rather than review. Key and lock-name formats live with their pattern:
[Cached Read](../patterns/cached-read.md), [Distributed Lock](../patterns/distributed-lock.md).

# Concepts

* [Naming and Placement](naming.md) - Where every kind of file lives and what it is called - one table, one row per kind.
* [Record Syntax](record-syntax.md) - Positional records in Web.Api, standard-syntax records with DataAnnotations in Application.
* [Visibility](visibility.md) - What is public, what is internal, and why the boundary is enforced by tests.
* [Error Codes](error-codes.md) - The {Entity}.{ErrorName} code format and where error factories live.
* [Route Conventions](route-conventions.md) - Paths are relative to the api/v1 group; resources are plural nouns and operations are HTTP verbs.

# Template

* [Convention Template](_template-convention.md) - Copy this when recording a naming, placement or style rule.
