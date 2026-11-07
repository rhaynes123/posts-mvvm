# PostsMVVM

Small Avalonia desktop app that lists posts from `jsonplaceholder.typicode.com/posts` in a filterable `DataGrid` with a detail pane. Built as a learning sample for MVVM, DI, and `IHttpClientFactory` in a .NET desktop app.

## Requirements

- .NET 10 SDK

## Run

```bash
dotnet run --project PostsMVVM/PostsMVVM.csproj
```

## What it demonstrates

- **MVVM with [CommunityToolkit.Mvvm](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/)** — `[ObservableProperty]` source generators on `MainViewModel`, no hand-written `INotifyPropertyChanged`.
- **Dependency injection** — `Microsoft.Extensions.DependencyInjection` wired up in `App.axaml.cs`; `MainViewModel` is resolved from the container, not `new`-ed.
- **`HttpClient` via `IHttpClientFactory`** — `AddHttpClient<PostsService>(...)` with a `BaseAddress`, so `PostsService` gets a pooled client with handler rotation instead of a long-lived singleton.
- **Service seam** — `PostsService` owns HTTP + JSON; the viewmodel only handles presentation (loading flag, filter, error binding).
- **Compiled bindings** — `x:DataType` on the window gives the XAML compiler type-checked bindings.

## Project layout

```
PostsMVVM/
├── App.axaml(.cs)        # DI container + app startup
├── Models/Post.cs
├── Services/PostsService.cs
├── ViewModels/MainViewModel.cs
└── Views/MainWindow.axaml(.cs)
```
