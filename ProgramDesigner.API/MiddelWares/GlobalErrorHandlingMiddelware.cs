using Hospital.Domain.Exceptions.BadRequest;
using Hospital.Domain.Exceptions.NotFound;
using Hospital.Domain.Exceptions.Unauthorized;

namespace Web.MiddelWares
{
    // GlobalErrorHandlingMiddleware class to handle exceptions globally.
    public class GlobalErrorHandlingMiddleware
    {

        private readonly RequestDelegate _next;


        // - RequestDelegate next: The next middleware in the pipeline.
        public GlobalErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;

        }

        // InvokeAsync method to handle exceptions globally.
        // - HttpContext context: The HTTP context for the current request.
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
                if (context.Response.StatusCode == StatusCodes.Status404NotFound)
                {
                    var response = new ErrorDetails
                    {
                        StatusCode = context.Response.StatusCode,
                        Message = "The requested resource was not found."
                    };
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsJsonAsync(response);
                }
            }
            catch (Exception ex)
            {
                // Set the response status code.
                context.Response.StatusCode = ex switch
                {
                    NotFoundException => StatusCodes.Status404NotFound,
                    BadRequestException => StatusCodes.Status400BadRequest,
                    UnauthorizedException => StatusCodes.Status401Unauthorized,
                    _ => StatusCodes.Status500InternalServerError
                };
                // Set the response content type.
                context.Response.ContentType = "application/json";
                // Set the response message.
                var response = new ErrorDetails
                {
                    StatusCode = context.Response.StatusCode,
                    Message = ex.Message
                };
                await context.Response.WriteAsJsonAsync(response);
            }
        }
    }
}
