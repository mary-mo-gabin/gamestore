using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

// contains all endpoints; extension methods always live in static classes
public static class GamesEndpoints 
{
    const string GetGameEndpoint = "GetGame";

    public static void MapGamesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games");

        // GET /games
        group.MapGet("/", async (GameStoreContext dbContext) 
            => await dbContext.Games
                              .Include(game => game.Genre)
                              .Select(game => new GameSummaryDto(
                                game.Id,
                                game.Name,
                                game.Genre!.Name, // ! indicates that Genre is not null
                                game.Price,
                                game.ReleaseDate
                              ))
                              .AsNoTracking() // improves performance by not tracking the entities in the DbContext
                              .ToListAsync());

        // GET /games/1
        group.MapGet("/{id}", async (int id, GameStoreContext dbContext) => 
        {
            var game = await dbContext.Games.FindAsync(id);

            return game is null ? Results.NotFound() : Results.Ok(
                new GameDetailsDto(
                    game.Id,
                    game.Name,
                    game.GenreId,
                    game.Price,
                    game.ReleaseDate
                )
            );
        })
        .WithName(GetGameEndpoint);

        // POST /games
        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            Game game = new()
            {
                Name = newGame.Name,
                GenreId = newGame.GenreId,
                Price = newGame.Price,
                ReleaseDate = newGame.ReleaseDate
            };

            dbContext.Games.Add(game); // tells EF to keep track of the new game
            await dbContext.SaveChangesAsync(); // save to the db; communicating with the db, so should be async

            // new Dto with GenreId instead of Genre to show to the client without exposing the internal details of the system
            GameDetailsDto gameDto = new(
                game.Id,
                game.Name,
                game.GenreId,
                game.Price,
                game.ReleaseDate
            );

            return Results.CreatedAtRoute(GetGameEndpoint, new {id = gameDto.Id}, gameDto); 
        });

        // PUT requests should be thread-safe to prevent data inconsistencies; not implemented here yet
        // PUT /games/1
        group.MapPut("/{id}", async (
            int id, 
            UpdateGameDto updatedGame,
            GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games.FindAsync(id);

            if (existingGame is null)
            {
                return Results.NotFound();
            }

            existingGame.Name = updatedGame.Name;
            existingGame.GenreId = updatedGame.GenreId;
            existingGame.Price = updatedGame.Price;
            existingGame.ReleaseDate = updatedGame.ReleaseDate;

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE /games/1
        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games
                            .Where(game => game.Id == id)
                            .ExecuteDeleteAsync(); // deletes the game with the specified id; bulk deletion; save changes call is not required; only one trip to the db

            return Results.NoContent(); 
        });
    }
}