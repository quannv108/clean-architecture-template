The business model. One folder per domain slice, each with a `<slice>.md` concept, its entities, its value
objects and its errors. Modelling rules: [Domain Layer](../architecture/components/domain.md).

**Your domains do not exist here yet.** The three below ship with the template as working examples of the
machinery; adding your own — in code and in a folder here — is what turns the template into an
application. See [Extending the Template](../workflows/process/extending-the-template.md).

# Subdirectories

* [outbox](outbox/index.md) - Reliable asynchronous domain event delivery
* [audit-logs](audit-logs/index.md) - The 4W audit trail
* [emails](emails/index.md) - A worked example: entity → event → outbox → handler → sender
* [_templates](_templates/index.md) - Blanks to copy when adding a slice, entity or value object.
