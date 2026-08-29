// to allow for migrations to be applied on startup

using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data;

public static class DataExtensions
{
    // migrate the database on startup
    public static void MigrateDB(this WebApplication app)
    {
        using var scope = app.Services.CreateScope(); // needed for dependency injection

        var dbContext = scope.ServiceProvider
                            .GetRequiredService<GameStoreContext>();
        dbContext.Database.Migrate();
    }

    // seed the database on startup
    public static void AddGameStoreDb(this WebApplicationBuilder builder)
    {
        var connString = "Data Source=GameStore.db";
builder.Services.AddSqlite<GameStoreContext>(
    connString,
    optionsAction: options => options.UseSeeding((context, _) =>
    {
        if (!context.Set<Genre>().Any())
        {
            context.Set<Genre>().AddRange(
                new Genre { Name = "Fighting" },
                new Genre { Name = "RPG" },
                new Genre { Name = "Platformer" },
                new Genre { Name = "Racing" },
                new Genre { Name = "Sports" }
            );

            context.SaveChanges();
        }
    })
);
    }
}