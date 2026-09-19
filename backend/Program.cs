using backend.Models;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<HiringGameService>();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();

app.MapGet("/api/game", (HiringGameService gameService) => gameService.GetSnapshot())
    .WithName("GetGameSnapshot");

app.MapPost("/api/game/evaluate", (EvaluationRequest request, HiringGameService gameService) =>
    Results.Ok(gameService.Evaluate(request.ResolvedIssueIds)))
    .WithName("EvaluateGameProgress");

app.Run();
