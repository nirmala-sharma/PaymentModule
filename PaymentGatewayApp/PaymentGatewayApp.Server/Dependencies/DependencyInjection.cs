﻿using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PaymentGatewayApp.Server.Configurations;
using PaymentGatewayApp.Server.Interfaces;
using PaymentGatewayApp.Server.Services;
using RabbitMQ.Client;
using System.Text;
using Microsoft.OpenApi.Models;
using PaymentGatewayApp.Server.DatabaseContext;
using Serilog;
using Microsoft.Extensions.DependencyInjection;

namespace PaymentGatewayApp.Server.Dependencies
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddService(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddSwaggerConfiguration();
            services.AddDatabaseProvider(configuration);
            services.AddPasswordHash(configuration);
            services.AddScoped<ISeedService, SeedServices>();
            services.AddAuth(configuration);
            services.AddRabbmitMQConfiguration(configuration);
            services.AddScoped<IPaymentTransactionService, PaymentTransactionService>();
            services.AddScoped<IIdempotencyKeyService, IdempotencyKeyService>();

            return services;
        }
        public static IServiceCollection AddAuth(this IServiceCollection services, ConfigurationManager configuration)
        {
            var jwtSettings = new JWTSettings();
            configuration.Bind(JWTSettings.SectionName, jwtSettings);
            services.AddSingleton(Options.Create(jwtSettings));
            services.AddTransient<IJWTTokenGenerator, JWTTokenGenerator>();
            services.AddAuthentication(defaultScheme: JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
            });
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddHttpContextAccessor();
            services.AddSerilogConfiguration(configuration);
            return services;
        }
        public static void AddSwaggerConfiguration(this IServiceCollection services)
        {
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "PaymentGatewayApis",
                    Version = "v1",
                    Description = "To test API from Swagger",
                    Contact = new OpenApiContact
                    {
                        Name = "API Support",
                        Url = new Uri("https://www.api.com/support"),
                        Email = "niru0102sharma@gmail.com"
                    },
                    TermsOfService = new Uri("https://www.api.com/termsandservices"),
                });

                config.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                //  Add a security requirement to all endpoints in Swagger UI
                config.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }                      
                        },
                        new List<string>()
                    }
                });
            });
        }
        public static IServiceCollection AddRabbmitMQConfiguration(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.Configure<RabbitMQConfiguration>(configuration.GetSection(RabbitMQConfiguration.SectionName));

            services.AddSingleton<IConnection>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<RabbitMQConfiguration>>().Value;
                if (string.IsNullOrEmpty(options.HostName))
                {
                    throw new ArgumentNullException(nameof(options.HostName), "RabbitMQ HostName is not configured.");
                }

                // Create Connection with the RabbitMQ server
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    Port = 5672,
                    UserName = "guest",
                    Password = "guest"
                };

                return factory.CreateConnection();
            });
            services.AddSingleton<IModel>(sp =>
            {
                var connection = sp.GetRequiredService<IConnection>();
                return connection.CreateModel();
            });
            services.AddScoped<IPaymentPublisher, PaymentEventPublisher>();

            services.AddHttpClient();

            services.AddHostedService<PaymentEventConsumer>();  // Starts immediately with the app

            return services;
        }
        public static IServiceCollection AddDatabaseProvider(this IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });
            return services;
        }
        /// <summary>
        /// Configures Serilog and adds it to the service collection.
        /// </summary>
        public static IServiceCollection AddSerilogConfiguration(this IServiceCollection services, ConfigurationManager configuration)
        {
            // Build Serilog logger from configuration
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration)
                .CreateLogger();
            // Register Serilog as the logging provider without disposing it on shutdown
            services.AddSerilog(Log.Logger, dispose: false);
            return services;
        }
        public static IServiceCollection AddPasswordHash(this IServiceCollection services, ConfigurationManager configuration)
        {
            var passwordHashSettings = new PasswordHashSettings();
            configuration.Bind(PasswordHashSettings.SectionName, passwordHashSettings);
            services.AddSingleton(Options.Create(passwordHashSettings));
            services.AddSingleton<IPasswordHasher, PasswordHasher>();
            return services;
        }
    }
}
