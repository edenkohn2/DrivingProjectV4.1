using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using APIDrivingProject.Services;
using BlazorDriveApp.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Blazored.LocalStorage;


var builder = WebApplication.CreateBuilder(args);
// Add Google authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
})
.AddGoogle(options =>
{
    options.ClientId = "618520140270-il7cboe59o8jhcbf0jlf24hk0a28ctif.apps.googleusercontent.com";
    options.ClientSecret = "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";
    options.CallbackPath = "/signin-google"; // Path for redirect after Google sign-in
});
builder.Services.AddRazorComponents().AddInteractiveServerComponents();



// Register HttpClient for the API base URL
builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5082"); // API URL
});

builder.Services.AddScoped<UserService>(); // Register UserService
builder.Services.AddScoped<DatabaseService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddRazorComponents().AddInteractiveServerComponents();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5082/") });




// Add CORS services


// Register Blazor components and services


var app = builder.Build();
app.UseRouting();
app.UseAuthentication(); // זה צריך להיות לפני Authorization
app.UseAuthorization();

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
