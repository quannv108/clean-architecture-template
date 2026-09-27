A *container* is something that runs separately and holds state or executes code — a process, a
database, a cache. Not a Docker container, though here most of them happen to be one locally.

One subdirectory per application in the monorepo, named after its source folder under `apps/` (or `api/` for
the backend) - see [Naming and Placement](../../engineering/conventions/naming.md) for the `<audience>-<platform>`
rule client apps follow. Each holds the containers that application owns; all client applications call the
[Web.Api Container](api/web-api.md).

# Subdirectories

* [api](api/index.md) - The .NET backend in `api/`: the deployed Web.Api, its data stores and the development-time processes.
* [admin-web](admin-web/index.md) - Planned - the admin back-office web app in `apps/admin-web/`.
* [customer-web](customer-web/index.md) - Planned - the customer web app in `apps/customer-web/`.
* [customer-mobile](customer-mobile/index.md) - Planned - the customer mobile app in `apps/customer-mobile/`.
