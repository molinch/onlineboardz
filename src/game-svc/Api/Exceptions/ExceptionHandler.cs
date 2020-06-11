using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Exceptions
{
    public class ExceptionHandler
    {
        public async Task Invoke(HttpContext context)
        {
            var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
            if (contextFeature != null && contextFeature.Error != null)
            {
                // could add logging here

                context.Response.StatusCode = (int)GetErrorCode(contextFeature.Error);
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
                {
                    Title = contextFeature.Error.Message
                }));
            }
        }

        private static HttpStatusCode GetErrorCode(Exception e)
            => e switch {
                ValidationException _ => HttpStatusCode.UnprocessableEntity,
                ItemNotFoundException _ => HttpStatusCode.NotFound,
                _ => HttpStatusCode.InternalServerError
            };
    }
}
