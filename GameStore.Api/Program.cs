using GameStore.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation(); // register the validators

var app = builder.Build();

app.MapGamesEndpoints();

app.Run();