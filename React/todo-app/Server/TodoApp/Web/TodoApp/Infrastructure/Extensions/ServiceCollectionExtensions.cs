﻿using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TodoApp.Common;
using TodoApp.Data;
using TodoApp.Data.Common.Repositories;
using TodoApp.Data.Repositories;
using TodoApp.Server.Features.Identity;
using TodoApp.Server.Infrastructure.Filters;

namespace TodoApp.Server.Infrastructure.Extensions;

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        return services;
    }

    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        AppSettings appSettings)
    {
        var key = Encoding.ASCII.GetBytes(appSettings.Secret);

        services
            .AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        => services
            .AddServicesImplementingBaseServiceInterfaces()
            .AddHashids(setup =>
            {
                setup.Salt = "Your Salt";
                setup.MinHashLength = 10;
            })
            .AddSwaggerGen(c => c.OperationFilter<HashidsOperationFilter>())
            .AddScoped(typeof(IDeletableEntityRepository<>), typeof(EfDeletableEntityRepository<>))
            .AddScoped(typeof(IRepository<>), typeof(EfRepository<>))
            .AddSingleton(AutoMapperConfig.MapperInstance);

    public static IServiceCollection AddSwagger(this IServiceCollection services)
        => services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "TodoApp API",
                Version = "v1"
            });
        });
    
    public static IServiceCollection AddServicesImplementingBaseServiceInterfaces(this IServiceCollection services)
    {
        var iServiceType = typeof(IService);
        var iScopedServiceType = typeof(IScopedService);
        var iSingletonServiceType = typeof(ISingletonService);
        var iConcreteServiceType = typeof(IConcreteService);

        var result = iServiceType
            .Assembly
            .GetExportedTypes()
            .Where(x =>
                x.IsClass && !x.IsAbstract &&
                (x.IsAssignableTo(iServiceType) ||
                 x.IsAssignableTo(iScopedServiceType) ||
                 x.IsAssignableTo(iSingletonServiceType) ||
                 x.IsAssignableTo(iConcreteServiceType)))
            .Select(x => new
            {
                Service = x.GetInterface($"I{x.Name}"),
                Implementation = x,
            });

        result.ForEach(x =>
        {
            if (x.Service == typeof(IScopedService))
            {
                services.AddScoped(x.Service, x.Implementation);
            }
            else if (x.Service == typeof(ISingletonService))
            {
                services.AddSingleton(x.Service, x.Implementation);
            }
            else if (x.Service == null)
            {
                services.AddScoped(x.Implementation);
            }
            else
            {
                services.AddTransient(x.Service, x.Implementation);
            }
        });

        return services;
    }
}