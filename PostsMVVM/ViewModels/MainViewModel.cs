using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PostsMVVM.Models;
using PostsMVVM.Services;

namespace PostsMVVM.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    private readonly PostsService _posts;
    private readonly List<Post> _all = new();

    [ObservableProperty] public partial ObservableCollection<Post> Posts { get; set; } = new();
    [ObservableProperty] public partial Post? SelectedPost { get; set; }
    [ObservableProperty] public partial string Filter { get; set; } = "";
    [ObservableProperty] public partial bool IsLoading { get; set; }
    [ObservableProperty] public partial string? Error { get; set; }

    // ponytail: parameterless ctor stays for Avalonia's <Design.DataContext> previewer
    public MainViewModel() : this(new PostsService(
        new HttpClient { BaseAddress = new Uri("https://jsonplaceholder.typicode.com/") })) { }

    public MainViewModel(PostsService posts)
    {
        _posts = posts;
        _ = LoadAsync();
    }   

    private async Task LoadAsync()
    {
        IsLoading = true;
        try
        {
            switch (await _posts.GetAllAsync())
            {
                case Success<Post[]>(var posts):
                    _all.AddRange(posts);
                    ApplyFilter();
                    break;
                case Error(var ex):
                    Error = ex.Describe();
                    break;
            }
        }
        finally { IsLoading = false; }
    }

    partial void OnFilterChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        var q = Filter?.Trim() ?? "";
        IEnumerable<Post> result = _all;
        if (q.Length > 0)
            result = _all.Where(p =>
                p.Title.Contains(q, System.StringComparison.OrdinalIgnoreCase) ||
                p.Body.Contains(q, System.StringComparison.OrdinalIgnoreCase));

        Posts.Clear();
        foreach (var p in result) Posts.Add(p);
    }
}

public static class ExceptionExtensions
{
    extension(Exception ex)
    {
        public string Describe() => ex switch
        {
            TaskCanceledException                                                      => "Request timed out. Check your connection and retry.",
            JsonException                                                              => "Server returned data we couldn't read.",
            HttpRequestException { StatusCode: HttpStatusCode.NotFound }               => "Posts endpoint not found (404).",
            HttpRequestException { StatusCode: HttpStatusCode.Unauthorized }           => "You need to sign in (401).",
            HttpRequestException { StatusCode: HttpStatusCode.Forbidden }              => "You don't have access (403).",
            HttpRequestException { StatusCode: HttpStatusCode.TooManyRequests }        => "Rate limited — slow down and try again (429).",
            HttpRequestException { StatusCode: >= HttpStatusCode.InternalServerError } => "Server is having a bad day. Try again shortly.",
            HttpRequestException { StatusCode: null }                                  => "Network unreachable.",
            HttpRequestException h                                                     => $"HTTP {(int)h.StatusCode!}: {h.Message}",
            _                                                                          => ex.Message,
        };
    }
}
