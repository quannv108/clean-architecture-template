using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Performance",
    "CA1861:Avoid constant arrays as arguments",
    Justification = "Reason for suppressing the rule in this namespace",
    Scope = "namespaceanddescendants",
    Target = "~N:Infrastructure.Database.Migrations"
)]
[assembly: SuppressMessage(
    "Style",
    "IDE0161",
    Justification = "This file must use block-scoped namespace for compatibility.",
    Scope = "namespaceanddescendants",
    Target = "~N:Infrastructure.Database.Migrations"
)]
[assembly: SuppressMessage(
    "SonarAnalyzer",
    "S4581",
    Justification = "EF Core migrations may contain Guid.Empty values in seed data or other scenarios.",
    Scope = "namespaceanddescendants",
    Target = "~N:Infrastructure.Database.Migrations"
)]
