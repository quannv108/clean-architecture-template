Described with the [C4 model](https://c4model.com): each level zooms in one step. Start at the level that
matches your question and stop there. Level 4 is the source itself - each component page links the files it owns.

# Subdirectories

* [containers](containers/index.md) - C4 level 2 - the one deployed app, the data stores and the development-time processes.
* [components](components/index.md) - C4 level 3 - the layer projects and the inward dependency rule.
* [cross-cutting](cross-cutting/index.md) - Mechanisms that run through the components rather than sitting in one: CQRS, the decorator pipeline, domain event dispatch, caching, persistence, observability.
* [delivery](delivery/index.md) - How the system is built, tested and run.

# Concepts

* [System Context](context.md) - C4 level 1 - what this system is, who uses it, and which external systems it depends on.
* [Solution Layout](solution-layout.md) - The projects, directories and root files that make up the repository.
