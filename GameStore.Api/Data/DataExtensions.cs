// to allow for migrations to be applied on startup

using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data;

public static class DataExtensions
{
    public static void MigrateDB(this WebApplication app)
    {
        using var scope = app.Services.CreateScope(); // needed for dependency injection

        var dbContext = scope.ServiceProvider
                            .GetRequiredService<GameStoreContext>();
        dbContext.Database.Migrate();
    }
}