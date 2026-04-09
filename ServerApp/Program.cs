// InventoryHub - Minimal API Back-End
// Built following the assignment requirements using the Minimal API pattern (app.MapGet).

var builder = WebApplication.CreateBuilder(args);

// CORS is required because the Blazor WebAssembly client runs in the browser on a
// different port (5032) than this API (5108). Without this, the browser blocks requests.
// AllowAnyOrigin is appropriate for local development; in production this should be
// restricted to the specific client domain using WithOrigins("https://yourdomain.com").
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowBlazor",
        policy =>
        {
            policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    );
});

var app = builder.Build();

app.UseCors("AllowBlazor");

// GET /api/productlist
// Returns a list of products with nested Category objects.
// Accepts an optional ?error= query parameter to simulate error conditions for
// front-end error handling testing during peer review.
// A switch on the query value routes to each simulation mode, avoiding the need
// for separate endpoints.
app.MapGet(
    "/api/productlist",
    async (string? error) =>
    {
        switch (error?.ToLower())
        {
            case "timeout":
                // Delays 15s — exceeds the client's 5s CancellationToken timeout,
                // triggering TaskCanceledException on the front-end.
                await Task.Delay(15000);
                break;

            case "invalid":
                // Returns malformed JSON to trigger JsonException during deserialization.
                return Results.Content("{ invalid json }", "application/json");

            case "empty":
                // Returns an empty array to test the "no products found" UI state.
                // Simulates a real scenario where a DB query returns no results.
                return Results.Json(new object[] { });

            case "null":
                // Returns null to test null-safety handling in the front-end deserializer.
                return Results.Ok(null);

            default:
                break;
        }

        // Includes a nested Category object as required by the assignment to
        // demonstrate complex JSON deserialization on the front-end.
        return Results.Json(
            new[]
            {
                new
                {
                    Id = 1,
                    Name = "Laptop",
                    Price = 1200.50,
                    Stock = 25,
                    Category = new { Id = 101, Name = "Electronics" }
                },
                new
                {
                    Id = 2,
                    Name = "Headphones",
                    Price = 50.00,
                    Stock = 100,
                    Category = new { Id = 102, Name = "Accessories" }
                },
            }
        );
    }
);

app.Run();
