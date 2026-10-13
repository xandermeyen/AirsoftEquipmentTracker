# Airsoft Equipment Tracker

[![CI](https://github.com/xandermeyen/AirsoftEquipmentTracker/actions/workflows/ci.yml/badge.svg)](https://github.com/xandermeyen/AirsoftEquipmentTracker/actions/workflows/ci.yml)
![.NET 10](https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white)
![Blazor Server](https://img.shields.io/badge/Blazor-Server-512BD4?logo=blazor&logoColor=white)
![License: MIT](https://img.shields.io/badge/license-MIT-green)

A web app I built to keep track of my airsoft gear: replicas, attachments, magazines and the rest. Every user gets their own account and inventory. You can add items with a photo, track whether you own, sold or still want something, and see at a glance what your gear is worth.

It started as a way to practice full-stack .NET properly, end to end, instead of following along with tutorials. So next to being a useful little tool, it's the project I point to when someone asks what I can build.

| Dashboard | Add / edit equipment |
| --- | --- |
| ![Dashboard](docs/screenshots/Dashboard.png) | ![Edit equipment](docs/screenshots/EditEquipment.png) |

## What it does

- **Accounts**: register, log in, log out and reset your password with ASP.NET Core Identity. Reset links expire after 30 minutes. Every page requires login and every user only sees their own items.
- **Dashboard**: number of owned items, total value, wishlist value, a per-category breakdown and the most recent additions
- **Equipment overview**: search by name or brand, filter by category and status, and sort the list
- **Full CRUD**: add, edit and delete items, with a confirmation step before anything gets removed
- **Status per item**: Owned, Sold or Wishlist. Only owned items count towards the totals, the wishlist gets its own number.
- **Photos**: upload a JPG, PNG or WebP (max 5 MB) per item, stored under a GUID name and cleaned up when replaced or deleted
- **Validation**: field-level messages, price between 0 and 100,000, and a custom `[NotInFuture]` attribute for the purchase date
- **Brands and categories**: add them straight from the item form, or rename and delete them on a separate manage page. Anything still in use can't be deleted.

## Tech stack

- Blazor Server (.NET 10), Interactive Server render mode
- C#
- ASP.NET Core Identity for authentication
- Entity Framework Core 10 with SQL Server (running in Docker)
- xUnit with SQLite in-memory for tests
- GitHub Actions for build and test on every push and pull request
- Bootstrap for the layout, with a custom theme on top

## How it's put together

Nothing exotic, but I tried to keep the layers clean:

- `Models/` holds the entities: `EquipmentItem`, `Brand`, `Category` and `ApplicationUser`, plus the `EquipmentStatus` enum and the `NotInFutureAttribute` validator. Items point at a brand and a category with `DeleteBehavior.Restrict`, so you can't delete a brand or category that's still in use.
- `Data/AppDbContext.cs` extends `IdentityDbContext`, configures the relationships and seeds brands and categories.
- `Services/EquipmentService.cs` is the only thing the pages talk to for data. Every item query is scoped to the logged-in user. It uses `IDbContextFactory` rather than a single injected context, which is the recommended approach for Blazor Server because components live longer than a normal request.
- `Services/ImageService.cs` validates, saves and deletes uploaded photos.
- `Components/Pages/` contains the Blazor pages: Dashboard, the equipment list, the add/edit form and the brand and category manager.
- `Pages/Account/` holds the Razor Pages for login, register, logout and the forgot/reset password flow. These run outside the Blazor circuit because the auth cookie has to be set on a normal HTTP request.

The connection string lives in user-secrets, not in `appsettings.json`, so no credentials end up in the repo.

## Tests

`AirsoftEquipmentTracker.Tests` covers `EquipmentService` against a real SQLite in-memory database, so relationships and constraints behave like they do in production. Among other things it checks:

- adding, updating and deleting items
- that a user can't read or delete someone else's items
- that brands and categories are deduplicated and trimmed
- that a brand or category in use can't be deleted or renamed to a duplicate

Run them with:

```bash
dotnet test
```

## Running it locally

You'll need the .NET 10 SDK and Docker.

1. Start SQL Server in a container:

   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" \
     -p 1433:1433 --name airsoft-sql -d mcr.microsoft.com/mssql/server:2022-latest
   ```

2. From the `AirsoftEquipmentTracker` folder, set the connection string in user-secrets:

   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
     "Server=localhost,1433;Database=AirsoftTracker;User Id=sa;Password=Your_password123;TrustServerCertificate=True"
   ```

3. Apply the migrations to create the database:

   ```bash
   dotnet ef database update
   ```

4. Run the app:

   ```bash
   dotnet run
   ```

Open the URL the console prints and register an account. Brands and categories are already seeded, so you can start adding gear right away.

## What I learned

This is where most of the value was for me:

- Setting up EF Core from scratch: the DbContext, relationships, migrations and seeding, and why foreign-key delete behaviour matters
- Why Blazor Server wants `IDbContextFactory` instead of a scoped context, and what goes wrong if you ignore that
- Adding Identity to an existing Blazor app, and why login and logout need Razor Pages instead of Blazor components
- Scoping all data per user in the service layer instead of trusting the UI
- Handling file uploads safely: checking type and size, and never trusting the original file name
- Writing custom validation attributes and showing field-level messages in `EditForm`
- Testing the data layer against SQLite in-memory instead of mocking everything away

## Ideas for later

- A proper loadouts feature, grouping items into kits
- Maintenance log per replica
- Sending the password reset link by email (for now it's logged to the console in development)
- Email confirmation on registration
- Export to CSV

## License

MIT, see [LICENSE](LICENSE).

## Author

Xander Meyen · [xandermeyen.be](https://xandermeyen.be)
