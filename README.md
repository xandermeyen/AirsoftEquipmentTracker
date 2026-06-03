# Airsoft Equipment Tracker

A small web app I built to keep track of my airsoft gear: replicas, attachments, magazines and the rest. You can add items, edit and delete them, filter by category, and see a quick overview of what you own and what it's worth.

It started as a way to practice full-stack .NET properly, end to end, instead of following along with tutorials. So next to being a useful little tool, it's the project I point to when someone asks what I can build.

## What it does

- Dashboard with totals (number of items, total value, categories used), a per-category breakdown and the most recent additions
- Equipment overview with a category filter
- Full CRUD: add, edit and delete items, with a confirmation step before anything gets removed
- Brands and categories linked through foreign keys, so an item always references existing data, with the option to add a new brand or category straight from the item form
- Seed data so the app isn't empty on first run

## Tech stack

- Blazor Server (.NET 10), Interactive Server render mode
- C#
- Entity Framework Core 10 with SQL Server
- SQL Server running in Docker
- Bootstrap for the layout, with a custom theme on top

## How it's put together

Nothing exotic, but I tried to keep the layers clean:

- `Models/` holds the three entities: `EquipmentItem`, `Brand` and `Category`. Items point at a brand and a category, and those references use `DeleteBehavior.Restrict` so you can't delete a brand or category that's still in use.
- `Data/AppDbContext.cs` configures the relationships and seeds the database.
- `Services/EquipmentService.cs` is the only thing the pages talk to for data. It uses `IDbContextFactory` rather than a single injected context, which is the recommended approach for Blazor Server because components live longer than a normal request.
- `Components/Pages/` contains the Dashboard, the equipment list and the add/edit forms.

The connection string lives in user-secrets, not in `appsettings.json`, so no credentials end up in the repo.

## Running it locally

You'll need the .NET 10 SDK and Docker.

1. Start SQL Server in a container:

   ```bash
   docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_password123" \
     -p 1433:1433 --name airsoft-sql -d mcr.microsoft.com/mssql/server:2022-latest
   ```

2. From the project folder, set the connection string in user-secrets:

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

Then open the URL the console prints. The database starts pre-filled with a handful of brands, categories and items.

## What I learned

This is where most of the value was for me:

- Setting up EF Core from scratch: the DbContext, relationships, migrations and seeding, and why foreign-key delete behaviour matters
- Why Blazor Server wants `IDbContextFactory` instead of a scoped context, and what goes wrong if you ignore that
- Keeping data access in a service layer instead of scattering queries through components
- Building forms with `EditForm` and binding dropdowns to foreign keys
- Handling state on the client for things like the delete confirmation, without reaching for JavaScript

## Ideas for later

- Stronger form validation (field-level messages, no negative prices)
- Search next to the category filter
- Images per item
- A proper loadouts feature, grouping items into kits

## Author

Xander Meyen
