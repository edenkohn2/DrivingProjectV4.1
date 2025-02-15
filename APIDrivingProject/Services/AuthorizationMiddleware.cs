using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace APIDrivingProject.Middleware
{
    public class AssignmentRestrictionMiddleware
    {
        private readonly RequestDelegate _next;

        public AssignmentRestrictionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, APIDrivingProject.Services.AuthService authService)
        {
            string path = context.Request.Path.Value ?? string.Empty;

            // אם המשתמש אינו מחובר – המשך לעיבוד
            if (!authService.IsAuthenticated)
            {
                await _next(context);
                return;
            }

            // אם המשתמש הוא תלמיד ואינו משויך,
            // נאפשר גישה רק ל-/settings ול-/request-assignment
            if (authService.IsStudent && !authService.IsAssigned)
            {
                if (!path.StartsWith("/settings", StringComparison.OrdinalIgnoreCase) &&
                    !path.StartsWith("/request-assignment", StringComparison.OrdinalIgnoreCase) &&
                    !path.StartsWith("/student/schedule", StringComparison.OrdinalIgnoreCase))  // מאפשרים גישה ללוח הזמנים
                {
                    context.Response.Redirect("/settings");
                    return;
                }
            }


            await _next(context);
        }
    }
}
