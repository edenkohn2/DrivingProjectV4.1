using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using APIDrivingProject.Services;
using BlazorDriveApp.Components;
using Blazored.LocalStorage;
using APIDrivingProject.Models;
using DrivingClassLibary.Models;


var builder = WebApplication.CreateBuilder(args);
// Add Google authentication

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddBlazorBootstrap();



// Register HttpClient for the API base URL
builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5082"); // API URL
});

builder.Services.AddScoped<UserService>(); // Register UserService
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddSingleton<AuthService>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRole.Admin.ToString()));
});
builder.Services.AddAuthentication("Cookies")
    .AddCookie(options =>
    {
        options.LoginPath = "/login"; // כתובת דף ההתחברות
        options.AccessDeniedPath = "/unauthorized"; // כתובת דף חוסר הרשאה
    });

builder.Services.AddAuthorization(); // הרשאות מבוססות תפקידים
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5082/") });




// Add CORS services


// Register Blazor components and services


var app = builder.Build();

app.UseRouting();

app.UseMiddleware<AuthorizationMiddleware>();

// Apply CORS policy



// Error handling configuration
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}




app.UseStaticFiles();




app.UseAntiforgery();

app.MapRazorComponents<App>().AddInteractiveServerRenderMode();


app.Run();
