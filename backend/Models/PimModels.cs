namespace backend.Models;

public sealed record PimProduct(
    string Id,
    string Sku,
    string Name,
    string Category,
    string Status,
    string Description,
    decimal Price,
    int Stock,
    DateTime UpdatedAt);

public sealed record UpdatePimProductRequest(
    string Sku,
    string Name,
    string Category,
    string Status,
    string Description,
    decimal Price,
    int Stock);