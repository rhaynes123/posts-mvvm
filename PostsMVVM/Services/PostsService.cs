using System;
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

    public async Task<Result<Post[]>> GetAllAsync(CancellationToken ct = default)
    {
        try
        {
            return new Result<Post[]>(
                new Success<Post[]>(await _http.GetFromJsonAsync<Post[]>("posts", JsonOpts, ct) ?? []));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            return new Result<Post[]>(new Error(ex));
        }
    }
}
