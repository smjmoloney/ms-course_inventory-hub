using System.Text.Json;
using ClientApp.Models;

namespace ClientApp.Services;

// ProductService is responsible for all HTTP communication with the back-end API.
// Extracted from the Razor page into a dedicated service class per the project
// structure requirements, separating concerns between data access and UI.
public class ProductService(HttpClient httpClient)
{
    private const string BaseUrl = "http://localhost:5108/api/productlist";
    private const int TimeoutSeconds = 5;

    // Copilot diagnosed the bug where all deserialized properties were empty:
    // System.Text.Json matches property names case-sensitively by default, but the API
    // returns camelCase JSON (e.g. "name") while the C# model uses PascalCase ("Name").
    // PropertyNameCaseInsensitive = true resolves the mismatch without changing either side.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Returns a tuple (Data, Error) so the caller can handle both outcomes without
    // exceptions propagating into UI code, keeping error handling out of the Razor page.
    public async Task<(Product[]? Data, string? Error)> GetProductsAsync(string? errorMode = null)
    {
        try
        {
            // CancellationToken enforces a hard timeout — without this, a slow server
            // would leave the UI stuck on the loading state indefinitely.
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(TimeoutSeconds));
            var url = errorMode != null ? $"{BaseUrl}?error={errorMode}" : BaseUrl;

            var response = await httpClient.GetAsync(url, cts.Token);

            // EnsureSuccessStatusCode throws HttpRequestException on 4xx/5xx responses,
            // which is caught below. This is the assignment-required pattern:
            // GetAsync → EnsureSuccessStatusCode → ReadAsStringAsync → Deserialize.
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var products = JsonSerializer.Deserialize<Product[]>(json, JsonOptions);

            if (products == null || products.Length == 0)
                return (null, "No products found.");

            return (products, null);
        }
        catch (TaskCanceledException)
        {
            // Thrown when the CancellationToken fires after TimeoutSeconds.
            return (
                null,
                $"Request timeout: Server did not respond within {TimeoutSeconds} seconds."
            );
        }
        catch (HttpRequestException ex)
        {
            // Covers network failures, DNS errors, and non-success HTTP status codes.
            return (null, $"Network error: {ex.Message}");
        }
        catch (JsonException ex)
        {
            // Thrown when the server returns a response that cannot be deserialized
            // into Product[], e.g. malformed JSON or an unexpected schema.
            Console.WriteLine($"Error: {ex.Message}");
            return (null, "Invalid response format from server.");
        }
        catch (Exception ex)
        {
            // Safety net for any unexpected errors.
            Console.WriteLine($"Error: {ex.Message}");
            return (null, $"Error: {ex.Message}");
        }
    }
}
