namespace ClientApp.Models;

// Data model representing a product returned by the API.
// Copilot suggested = string.Empty defaults on string properties as a fix for
// CS8618 compiler warnings, keeping the properties non-nullable without
// requiring null checks at every point of use.
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
    public int Stock { get; set; }

    // Nullable because not all API responses may include a category.
    // Added alongside the back-end update to return nested Category objects,
    // as required by the assignment to demonstrate complex JSON deserialization.
    public Category? Category { get; set; }
}

// Represents the nested category object within a Product.
public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
