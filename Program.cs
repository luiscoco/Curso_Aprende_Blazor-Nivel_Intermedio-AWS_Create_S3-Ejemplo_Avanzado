using BlazorAWSSample.Components;
using BlazorAWSSample.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register your S3 service
builder.Services.AddSingleton<S3Service>();

// Enable detailed errors for development mode
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddServerSideBlazor()
           .AddCircuitOptions(options => { options.DetailedErrors = true; });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
