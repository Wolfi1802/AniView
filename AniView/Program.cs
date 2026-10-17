using AniView.Components;
using AniView.Structure;
using AniView.Structure.API.Services;
using AniView.Structure.Services;
using MudBlazor;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

//Add MudBlazor services
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomLeft;

    config.SnackbarConfiguration.PreventDuplicates = false;
    config.SnackbarConfiguration.NewestOnTop = false;
    config.SnackbarConfiguration.ShowCloseIcon = true;
    config.SnackbarConfiguration.VisibleStateDuration = 10000;
    config.SnackbarConfiguration.HideTransitionDuration = 500;
    config.SnackbarConfiguration.ShowTransitionDuration = 500;
    config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
});

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<UiMessageService>();
builder.Services.AddScoped<ThemeService>();

builder.Services.AddMemoryCache();
builder.Services.AddTransient<ApiResponseInterceptor>();
builder.Services.AddHttpClient<IAniListClient, AniListClient>(client =>
{
    client.BaseAddress = new Uri("https://graphql.anilist.co/");
    client.Timeout = TimeSpan.FromSeconds(15);
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
})
.AddHttpMessageHandler<ApiResponseInterceptor>();
builder.Services.AddTransient<IPopularAnimeService, PopularAnimeService>();
builder.Services.AddTransient<ICurrentSimulcastService, CurrentSimulcastService>();
builder.Services.AddTransient<IAnimeDetailService, AnimeDetailService>();
builder.Services.AddTransient<IStreamingSiteService, StreamingSiteService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
