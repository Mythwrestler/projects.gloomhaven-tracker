using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GloomhavenTracker.Database;
using GloomhavenTracker.Service.BackgroundServices;
using GloomhavenTracker.Service.Hubs;
using GloomhavenTracker.Service.Repos;
using GloomhavenTracker.Service.SeedData;
using GloomhavenTracker.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using GloomhavenTracker.Service.Models.Content;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using GloomhavenTracker.Service.Models;
using GloomhavenTracker.Service.Helpers;
using GloomhavenTracker.Service.Models.Campaign;
using GloomhavenTracker.Service.Models.Combat;
using GloomhavenTracker.Service.Models.Combat.Hub;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

#region Environment Variables / Configurations

// Authentication
bool authEnabled = bool.Parse(Environment.GetEnvironmentVariable("AUTH_ENABLED") ?? "true");
string authAuthority = Environment.GetEnvironmentVariable("AUTH_AUTHORITY") ?? String.Empty;
string authAudience = Environment.GetEnvironmentVariable("AUTH_AUDIENCE") ?? String.Empty;

// CORS
string[] allowedCORSOrigins = (Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS") ?? "").Split(',');

// Logging
bool httpLoggingEnabled = bool.Parse(Environment.GetEnvironmentVariable("HTTP_LOGGING_ENABLED") ?? "false");

// Database Connection String
string dbServer = Environment.GetEnvironmentVariable("DB_SERVER") ?? String.Empty;
string dbPort = Environment.GetEnvironmentVariable("DB_PORT") ?? String.Empty;
string dbDatabase = Environment.GetEnvironmentVariable("DB_DATABASE") ?? String.Empty;
string dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? String.Empty;
string dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? String.Empty;
string dbConnectionString = String.Format(
    builder.Configuration["ConnectionStrings:PostgreSQL"],
    dbServer,
    dbPort,
    dbDatabase,
    dbUser,
    dbPassword,
    "true"
);

#endregion

#region Authentication

var authBuilder = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});

if (authEnabled)
{
    authBuilder.AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                // If the request is for our hub...
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    (path.StartsWithSegments("/hub")))
                {
                    // Read the token out of the query string
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
        options.Authority = authAuthority;
        options.Audience = authAudience;
    });
}

builder.Services.AddAuthorization(options => {
    options.AddPolicy("authenticated", new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
});

// Add Anonoymous Authentication Handler is auth is disabled
if(!authEnabled) builder.Services.AddSingleton<IAuthorizationHandler, AllowAnonymous>();
builder.Services.AddHttpContextAccessor();

# endregion

#region Service Registry

builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "My API",
                Description = "My First ASP.NET Core Web API",
                TermsOfService = new System.Uri("https://www.talkingdotnet.com"),
                Contact = new OpenApiContact() { Name = "Talking Dotnet", Email = "contact@talkingdotnet.com" }
            });

            c.SwaggerDoc("v2", new OpenApiInfo
            {
                Version = "v2",
                Title = "New API V2",
                Description = "Sample Web API",
                TermsOfService = new System.Uri("https://www.talkingdotnet.com"),
                Contact = new OpenApiContact() { Name = "Talking Dotnet", Email = "contact@talkingdotnet.com" }
            });
        });



builder.Services.AddMemoryCache();

builder.Services.AddSignalR();
builder.Services.AddControllers(options => {
    options.InputFormatters.Insert(0, JsonPatchInputFormatter.GetJsonPatchInputFormatter());
});

builder.Services.AddCors();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
});


builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile(new ContentMapperProfile());
    cfg.AddProfile(new CampaignMapperProfile());
    cfg.AddProfile(new CombatMapperProfile());
    cfg.AddProfile(new UserMapperProfile());
});

builder.Services.AddDbContext<GloomhavenContext>(optionsAction => {
    optionsAction.UseNpgsql(
        dbConnectionString,
        assembly => assembly.MigrationsAssembly(typeof(GloomhavenContext).Assembly.FullName)
    );
});

// Register User DI
builder.Services.AddScoped<UserService, UserServiceImplementation>();
builder.Services.AddScoped<UserRepo, UserRepoImplementation>(factory => 
{
    return new UserRepoImplementation(
        factory.GetRequiredService<GloomhavenContext>(),
        factory.GetRequiredService<IMapper>(),
        authAuthority,
        factory.GetRequiredService<IHttpContextAccessor>()
    );
});

// Register Content DI
builder.Services.AddScoped<ContentService, ContentServiceImplementation>();
builder.Services.AddScoped<ContentRepo, ContentRepoImplementation>();


//  Register Campaign DI
builder.Services.AddScoped<CampaignService, CampaignServiceImplementation>();
builder.Services.AddScoped<CampaignRepo, CampaignRepoImplementation>();
builder.Services.AddScoped<CampaignRepo, CampaignRepoImplementation>();

//  Register Combat DI
builder.Services.AddScoped<CombatService, CombatServiceImplantation>();
builder.Services.AddScoped<CombatRepo, CombatRepoImplementation>();

// Register Combat HUB DI
builder.Services.AddSingleton<CombatHubClientTracker, CombatHubClientTracker>();
builder.Services.AddHostedService<CombatHubMonitor>();

if (httpLoggingEnabled)
{
    builder.Services.AddHttpLogging(options =>
    {
        options.LoggingFields = Microsoft.AspNetCore.HttpLogging.HttpLoggingFields.All;
        options.RequestBodyLogLimit = 4096;
        options.RequestBodyLogLimit = 4096;
    });
}

builder.Services.AddHealthChecks();

#endregion

#region App Startup

var app = builder.Build();

//  Build and Seed Database
bool seedDefaultData = bool.Parse(Environment.GetEnvironmentVariable("DB_SEED_DATA") ?? "false");
if(seedDefaultData) SeedData.LoadDefaultContent(dbConnectionString);

InitializeProcesses.LoadCombatHubTracker(app.Services);

if (httpLoggingEnabled)
{
    app.UseHttpLogging();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.SwaggerEndpoint("/swagger/v2/swagger.json", "My API V2");
    });

}

app.UseCors((config) =>
{
    config
        .WithMethods("POST", "PUT", "GET", "PATCH")
        .AllowAnyHeader()
        .AllowCredentials()
        .WithOrigins(allowedCORSOrigins);
});


app.UseAuthentication();

app.UseRouting();

app.UseAuthorization();

app.MapHub<CombatHub>("hub/combats").RequireAuthorization("authenticated");

app.UseEndpoints(endpoints => endpoints.MapControllers());

app.MapHealthChecks("/healthz");

app.Run();

#endregion

#region Helper Classes / Methods

public class AllowAnonymous : IAuthorizationHandler 
{
    public Task HandleAsync(AuthorizationHandlerContext context)
    {
        foreach (IAuthorizationRequirement requirement in context.PendingRequirements)
            context.Succeed(requirement); //Simply pass all requirements
        
        return Task.CompletedTask;
    }
}

# endregion