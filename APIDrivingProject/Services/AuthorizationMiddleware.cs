using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, APIDrivingProject.Services.AuthService authService)
    {
        var path = context.Request.Path;

        // אם הנתיב אינו מוגן, המשך לבקשה הבאה
        if (!path.StartsWithSegments("/request-assignment"))
        {
            await _next(context);
            return;
        }

        // בדיקה אם המשתמש מחובר
        if (!authService.IsAuthenticated)
        {
            context.Response.Redirect("/login");
            return;
        }

        // ניהול גישה לדף /request-assignment
        if (path.StartsWithSegments("/request-assignment"))
        {
            if (authService.IsInstructor)
            {
                // אם המשתמש הוא מורה
                context.Response.Redirect("/unauthorized");
                return;
            }

            if (authService.IsStudent)
            {
                var isAssigned = await IsStudentAssignedToInstructor(authService.UserId, context);
                if (isAssigned)
                {
                    // אם התלמיד משויך למורה
                    context.Response.Redirect("/settings");
                    return;
                }
            }
        }

        // המשך לעיבוד הבקשה אם הכל תקין
        await _next(context);
    }

    private async Task<bool> IsStudentAssignedToInstructor(int userId, HttpContext context)
    {
        try
        {
            var httpClient = context.RequestServices.GetRequiredService<HttpClient>();
            var response = await httpClient.GetAsync($"api/Instructors/{userId}/is-assigned");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }

            return false;
        }
        catch
        {
            return false;
        }
    }
}
