using BuildingBlocks.Application.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Application;

public static class DependencyInjection
{
    public static MediatRServiceConfiguration AddBuildingBlocksBehaviors(this MediatRServiceConfiguration cfg)
    {
        cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
        cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        cfg.AddOpenBehavior(typeof(UnhandledExceptionBehavior<,>));
        return cfg;
    }
}
