using BuildingBlocks.Api.Middleware;
using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks.Api;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseBuildingBlocksExceptionHandling(this IApplicationBuilder app) =>
        app.UseMiddleware<GlobalExceptionMiddleware>();
}
