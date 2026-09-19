using backend.Models;
using backend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSingleton<HiringGameService>();
builder.Services.AddSingleton<PimCatalogService>();
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

app.MapGet("/api/products", (PimCatalogService catalog) => catalog.GetProducts())
    .WithName("GetProducts");

app.MapGet("/api/products/{id}", (string id, PimCatalogService catalog) =>
    catalog.GetProduct(id) is { } product ? Results.Ok(product) : Results.NotFound())
    .WithName("GetProduct");

app.MapPut("/api/products/{id}", (string id, UpdatePimProductRequest request, PimCatalogService catalog) =>
{
    var result = catalog.UpdateProduct(id, request);
    if (result.Product is null && result.Error is null)
    {
        return Results.NotFound();
    }

    return result.Product is null
        ? Results.BadRequest(new { message = result.Error })
        : Results.Ok(result.Product);
})
    .WithName("UpdateProduct");

app.Run();
