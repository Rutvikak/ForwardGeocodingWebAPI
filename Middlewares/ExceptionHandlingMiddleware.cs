using System.Data;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using GeocodingAPI.Services;

namespace GeocodingAPI.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception.");

                await HandelExceptionAsync(context, ex);    
            }
        }

        private static async Task HandelExceptionAsync(HttpContext context, Exception ex)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (ex)
            {
                case NotFoundException:

                    context.Response.StatusCode = StatusCodes.Status404NotFound;

                    response.Title = "Address Not Found!";
                    response.Status = 404;
                    response.Message = ex.Message;
                    response.TimeStamp = DateTime.Now;

                    break;

                case ExternalAPIException:
                    context.Response.StatusCode = StatusCodes.Status502BadGateway;

                    response.Title = "External API Error!";
                    response.Status = 502;
                    response.Message = ex.Message;
                    response.TimeStamp = DateTime.Now;

                    break;

                case BadRequestException:
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;

                    response.Title = "Bad Request";
                    response.Status = StatusCodes.Status400BadRequest;
                    response.Message = ex.Message;
                    response.TimeStamp = DateTime.Now;
                    break;

                case ServerError:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    response.Title = "Internal Server Error!";
                    response.Status = StatusCodes.Status500InternalServerError;
                    response.Message = ex.Message;
                    response.TimeStamp = DateTime.Now;
                    break;

                default:
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;

                    response.Title = "Internal Server Error!";
                    response.Status = StatusCodes.Status500InternalServerError;
                    response.Message = ex.Message;
                    response.TimeStamp = DateTime.Now;
                    break;
            }

            var json= JsonSerializer.Serialize(response);
            context.Response.StatusCode = response.Status;
            await context.Response.WriteAsync(json);
        }
    }

}
