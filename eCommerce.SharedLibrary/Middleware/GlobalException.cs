using eCommerce.SharedLibrary.Logs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text.Json;

namespace eCommerce.SharedLibrary.Middleware
{
    public class GlobalException(RequestDelegate next)
    {
        public async Task InvokeAsync(HttpContext context)
        {
            //Declare variables 
            string title = "Internal Server Error";
            string message = "An unexpected fault happened. Try again later.";
            int statusCode = (int)HttpStatusCode.InternalServerError;
            

            try
            {
                await next(context);

                //Check if the exception is too many requests// 429 status code.
                if(context.Response.StatusCode == StatusCodes.Status429TooManyRequests)
                {
                    title = "Too Many Requests";
                    message = "Too many requests. Try again later.";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);
                }
                if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    title = "Alert!";
                    message = "You are not authorized to access";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);
                }
                if (context.Response.StatusCode == StatusCodes.Status403Forbidden)
                {
                    title = "Alert!";
                    message = "You are not allowed/required to access";
                    statusCode = (int)StatusCodes.Status429TooManyRequests;
                    await ModifyHeader(context, title, message, statusCode);
                }

            }
            catch (Exception ex)
            {   //Log the original exception file, console, and debugger.
                LogException.LogExceptions(ex);

                //check if the exception is a timeout exception., 408 timeout status code.
                if(ex is TaskCanceledException || ex is TimeoutException)
                {
                    title = "Request Timeout";
                    message = "The request has timed out. Try again later.";
                    statusCode = (int)StatusCodes.Status408RequestTimeout;
                }
                await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
            }
        }

        private async Task ModifyHeader(HttpContext context, string title, string message, int statusCode)
        {
            //display scare free message to client.
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new ProblemDetails()
            {
                Detail = message,
                Status = statusCode,
                Title = title
            }), CancellationToken.None);
            return;
        }
    }
}
