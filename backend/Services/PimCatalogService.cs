using System.Text.RegularExpressions;
using backend.Models;

namespace backend.Services;

public sealed class PimCatalogService
{
    private readonly List<PimProduct> products =
    [
        new("prod-1001", "SHO-10452", "Trail Runner Shoe", "Footwear", "Published", "Lightweight trail shoe with a reinforced toe and all-weather grip.", 129.99m, 84, DateTime.UtcNow.AddDays(-2)),
        new("prod-1002", "BAG-20881", "Commuter Backpack", "Accessories", "Draft", "Expandable everyday backpack with a padded laptop compartment.", 89.50m, 32, DateTime.UtcNow.AddDays(-5)),
        new("prod-1003", "JKT-44107", "All-Weather Shell", "Outerwear", "Published", "Waterproof shell jacket designed for changing conditions.", 219.00m, 16, DateTime.UtcNow.AddHours(-8)),
        new("prod-1004", "BOT-70114", "Summit Bottle", "Accessories", "Review", "Insulated stainless steel bottle with a leak-resistant lid.", 34.95m, 0, DateTime.UtcNow.AddDays(-1))
    ];

    public IReadOnlyList<PimProduct> GetProducts() => products.OrderBy(product => product.Name).ToArray();

    public PimProduct? GetProduct(string id) => products.FirstOrDefault(product => product.Id == id);

    public (PimProduct? Product, string? Error) UpdateProduct(string id, UpdatePimProductRequest request)
    {
        if (!Regex.IsMatch(request.Sku, "^[A-Z]{3}-[0-9]{5}$"))
        {
            return (null, "SKU must use the format ABC-12345.");
        }

        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.Category))
        {
            return (null, "Name and category are required.");
        }

        if (request.Price < 0 || request.Stock < 0)
        {
            return (null, "Price and stock cannot be negative.");
        }

        var index = products.FindIndex(product => product.Id == id);
        if (index < 0)
        {
            return (null, null);
        }

        var updated = new PimProduct(
            id,
            request.Sku,
            request.Name.Trim(),
            request.Category.Trim(),
            request.Status,
            request.Description.Trim(),
            request.Price,
            request.Stock,
            DateTime.UtcNow);

        products[index] = updated;
        return (updated, null);
    }
}