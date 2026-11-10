# ChargeTags API

A .NET API for managing Clever charge tags: create a tag, verify that it can be used for charging, and block it so it can no longer be used. A charge tag is identified by an id in the format `dk-{10 digits}-clever`.

## The story

![The story of one charge tag](docs/story.svg)

Kim has just bought an electric car and becomes a Clever customer. Before Kim's new charge tag is sent, Clever registers it in the system (**Create**).

When Kim holds the tag to a charger, the charger asks the API whether the tag may be used. The tag exists and is not blocked, so charging starts (**Verify**).

One day Kim loses the tag and calls Clever, who blocks it (**Block**). When someone else later tries to charge with the lost tag, Verify answers that it is blocked, so nobody can charge on Kim's account.

## Architecture

![Architecture](docs/architecture.svg)

|---|---|---|
| Create | `POST /api/chargetags` with `{ "tagId": "dk-0123456789-clever" }` | `201` created, `400` invalid id, `409` already exists |
| Verify | `GET /api/chargetags/verify?tagId=dk-0123456789-clever` | `200` with `exists`, `isBlocked` and `canCharge`, `400` invalid id |
| Block | `POST /api/chargetags/dk-0123456789-clever/block` | `200` blocked, `400` invalid id, `404` not found |

## Try it

Swagger UI in Azure: https://app-chargetags-9496.azurewebsites.net/swagger

The first request after a quiet period can take a little longer, because the free App Service plan and the serverless database wake up on demand.

## Run locally

Requires the .NET 10 SDK and Docker.

```bash
docker run -d --name chargetags-sql -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=Clever_Dev_2026!' -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
dotnet run --project src/ChargeTags.Api   # http://localhost:5074/swagger
dotnet test
```

The password in `appsettings.Development.json` is only for this local Docker database. In Azure, the connection string is stored as an App Service setting and never in the repository.
