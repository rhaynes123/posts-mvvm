using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
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
            var posts = await _posts.GetAllAsync();
            if (posts is null) return;
            _all.AddRange(posts);
            ApplyFilter();
        }
        catch (HttpRequestException ex) { Error = ex.Message; }
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
