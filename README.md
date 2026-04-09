# InventoryHub

A full-stack inventory management application built with Blazor WebAssembly and ASP.NET Core Minimal API.

---

## Project Structure

- **ServerApp** — ASP.NET Core Minimal API serving product data on `http://localhost:5108`
- **ClientApp** — Blazor WebAssembly front-end on `http://localhost:5032`
  - `Models/` — Data models (`Product`, `Category`)
  - `Services/` — `ProductService` handles all HTTP communication
  - `Components/` — `ProductList` reusable presentation component
  - `Pages/` — `FetchProducts` page component

---

## Copilot Reflection

### How Copilot Assisted

**Generating integration code**

Copilot implemented all integration code from my prompts and the assignment steps. This included the full `ProductService` class — the `HttpClient` call chain (`GetAsync`, `EnsureSuccessStatusCode`, `ReadAsStringAsync`, `JsonSerializer.Deserialize`), the `CancellationTokenSource` timeout, the four catch blocks for different failure modes, and the `(Product[]? Data, string? Error)` tuple return so errors never propagate into UI code. It also implemented the `RunTest()` centralisation helper in `FetchProducts.razor`, which meant the `isLoading`/`StateHasChanged()` logic only needed to be written once rather than repeated in every button handler.

**Debugging issues**

Two bugs during development were diagnosed by Copilot rather than spotted by me:

The first was a `CS0542` compile error — the fetch method inside `FetchProducts.razor` was originally named `FetchProducts`, which conflicts with the Razor compiler's generated class name for the page. Copilot identified the cause and renamed the method to resolve it.

The second was more subtle: products were loading successfully (HTTP 200, valid JSON) but every property displayed as empty or zero. Copilot identified that `System.Text.Json` is case-sensitive by default — the API returns camelCase keys (`"name"`, `"price"`) while the C# model uses PascalCase (`Name`, `Price`). Adding `PropertyNameCaseInsensitive = true` to `JsonSerializerOptions` fixed the mismatch without changing the API or the model.

**Structuring JSON responses**

Copilot implemented the server-side test simulation pattern — a single `?error=` query parameter handled by a switch in the `app.MapGet` handler — which let the front-end exercise all four error paths (timeout, malformed JSON, empty array, null) without needing separate endpoints. It also structured the nested `Category` object in the API response and the corresponding model, demonstrating that the deserializer handles complex JSON correctly.

**Fixing compiler warnings**

Copilot applied `= string.Empty` defaults to string properties in the `Product` and `Category` models, resolving `CS8618` non-nullable field warnings without requiring null checks at every point of use.

**Null safety**

In `ProductList.razor`, Copilot used `product.Category?.Name ?? "Uncategorised"` so that products returned without a category display a readable fallback rather than throwing a `NullReferenceException`.

**Optimizing performance**

Two performance-relevant decisions were applied in `ProductService`:

The first is the `CancellationTokenSource` timeout. Without it, a slow or unresponsive server would hold an open HTTP connection indefinitely, blocking the UI and consuming resources until the browser eventually closed the tab or the OS recycled the socket. The 5-second cancellation token ensures the request is abandoned promptly and the connection released.

The second is declaring `JsonSerializerOptions` as `static readonly`. In .NET, constructing a `JsonSerializerOptions` instance is relatively expensive — it builds internal reflection caches the first time it processes a type. Declaring it as a static field means it is constructed once when the class is first loaded and reused on every subsequent call to `GetProductsAsync`, rather than being rebuilt on each request.

---

### Challenges and How Copilot Helped

**Loading state never appeared**

The `isLoading` loading banner was set to `true` before the `await`, but the UI never updated until after the request completed — making the loading state invisible. Copilot diagnosed that Blazor batches re-renders and does not flush them mid-method. The fix was to call `StateHasChanged()` explicitly before the `await`, forcing an immediate render cycle. This was not obvious from the Blazor documentation and would have taken significant time to track down manually.

**Empty data despite a successful response**

As described above, the camelCase/PascalCase mismatch is a silent failure — no exception is thrown, the data just comes back empty. Without Copilot flagging `PropertyNameCaseInsensitive`, this would have required trial and error across the model, the serialiser, and the API to locate.

**Naming conflicts**

The `CS0542` error produced a compiler message that does not clearly explain the Razor class generation behaviour. Copilot resolved it immediately without me needing to understand the underlying Razor compilation model.

---

### What I Learned About Using Copilot Effectively

**Directing is faster than describing.** Copilot works best when given a specific, concrete task: "implement this method", "fix this error", "add this catch block". Open-ended requests like "make this better" produce generic output that needs significant revision.

**Copilot accelerates implementation, not decisions.** Structural decisions — separating data access into `ProductService`, splitting rendering into `ProductList`, choosing a tuple return over exceptions — were personal choices. Copilot's role was to implement those decisions correctly and quickly, not to make them.

**Bug diagnosis is where Copilot adds the most value.** Writing straightforward code is fast either way. Tracking down a silent data mismatch or a batching behaviour that only manifests at runtime costs much more time without assistance. Copilot identified both the camelCase bug and the `StateHasChanged()` issue faster than reading documentation or searching Stack Overflow would have.

**Generated code still needs review.** Early in the project, some generated comments overstated Copilot's role — attributing structural decisions to Copilot that were actually directed by me or required by the assignment. Reviewing and correcting that was a useful reminder that Copilot output should be treated as a capable first draft, not an authoritative record.