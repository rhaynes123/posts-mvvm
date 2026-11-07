using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PostsMVVM.Models;

namespace PostsMVVM.Services;

public class PostsService
{
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;

    public PostsService(HttpClient http) => _http = http;

    public Task<Post[]?> GetAllAsync(CancellationToken ct = default) =>
        _http.GetFromJsonAsync<Post[]>("posts", JsonOpts, ct);
}
