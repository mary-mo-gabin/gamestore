using GameStore.Api.Dtos;

namespace GameStore.Api.Endpoints;

// contains all endpoints; extension methods always live in static classes
public static class GamesEndpoints 
{
    const string GetGameEndpoint = "GetGame";

    private static readonly List<GameDto> games = [
        new (
            1, 
            "Street Fighter II", 
            "Fighting", 
            19.99m, // m suffix indicates a decimal literal 
            new DateOnly(1992, 7, 15)),
        new (
            2, 
            "Final Fantasy VII Rebirth", 
            "RPG", 
            69.99m, // m suffix indicates a decimal literal 
            new DateOnly(2024, 2, 29)),
        new (
            3, 
            "Astro Bot", 
            "Platformer", 
            59.99m, // m suffix indicates a decimal literal 
            new DateOnly(2024, 9, 06)),
    ];

    public static void MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games");

        // GET /games
        group.MapGet("/", () => games);

        // GET /games/1
        group.MapGet("/{id}", (int id) => 
        {
            var game = games.Find(game => game.Id == id);

            return game is null ? Results.NotFound() : Results.Ok(game);
        })
        .WithName(GetGameEndpoint);

        // POST /games
        group.MapPost("/", (CreateGameDto newGame) =>
        {
            GameDto game = new(
                games.Count + 1,
                newGame.Name,
                newGame.Genre,
                newGame.Price,
                newGame.ReleaseDate
            );

            games.Add(game);

            return Results.CreatedAtRoute(GetGameEndpoint, new {id = game.Id}, game); 
        });

        // PUT requests should be thread-safe to prevent data inconsistencies; not implemented here yet
        // PUT /games/1
        group.MapPut("/{id}", (int id, UpdateGameDto updatedGame) =>
        {
            var index = games.FindIndex(game => game.Id == id);

            if (index == -1) // if game not found
            {
                return Results.NotFound();
            }

            games[index] = new GameDto(
                id,
                updatedGame.Name,
                updatedGame.Genre,
                updatedGame.Price,
                updatedGame.ReleaseDate
            );

            return Results.NoContent();
        });

        // DELETE /games/1
        group.MapDelete("/{id}", (int id) =>
        {
            games.RemoveAll(game => game.Id == id);

            return Results.NoContent(); 
        });
    }
}