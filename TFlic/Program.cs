using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using TFlic;
using TFlic.Constants;
using TFlic.Models.Services.Contexts;
using TFlic.Options;
using TFlic.Services;

var builder = WebApplication.CreateBuilder(args);
var buildConfiguration = builder.Configuration;
var builderServices = builder.Services;

ConfigureServices();
ConfigureDatabase();
ConfigureSwaggerGen();
ConfigureJwtOptions();

var app = builder.Build();

UseSwaggerGen();

app.UseHttpsRedirection();
        
UseAuthentication();
app.UseAuthorization();

MapControllers();

app.Run();



void ConfigureServices()
{
    builderServices.AddSingleton<IAccessTokenService, JwtService>();
    builderServices.AddSingleton<IRefreshTokenService, RefreshTokenService>();

    ConfigureDbContexts();
    builderServices.AddControllers().AddNewtonsoftJson();
    
    
    
    void ConfigureDbContexts()
    {
        var dbConnectionString = GetDbConnectionString();
        builderServices.AddDbContext<TFlicDbContext>(
            options =>
            {
                options.UseNpgsql(dbConnectionString);
                options.EnableSensitiveDataLogging();
            });



        string GetDbConnectionString()
        {
            var connectionString = buildConfiguration.GetConnectionString("DbConnectionString");
            if (connectionString is null)
                throw new NullReferenceException("Database is not configured for this app");

            return connectionString;
        }
    }
}

[Obsolete]
void ConfigureDatabase()
{
    // Add DbContexts to static aggregator
    var dbConnectionString = buildConfiguration.GetConnectionString("DbConnectionString")!;
    DbContexts.DbConnectionString = dbConnectionString;
}

void ConfigureSwaggerGen()
{
    builderServices.AddEndpointsApiExplorer();
    builderServices.AddSwaggerGen(options => options.DocumentFilter<JsonPatchDocumentFilter>());
    
#if AUTH
    builderServices.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(
                "Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                                  "Enter your access token in the text input below.",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer"
                });

            options.AddSecurityRequirement(
                new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Name = "Bearer",
                            Reference = new OpenApiReference
                            {
                                Id = "Bearer",
                                Type = ReferenceType.SecurityScheme,
                            },
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
        }
    );
#endif
}

void ConfigureJwtOptions()
{
    var jwtOptions = JwtOptions.GetJwtOptionsFromAppConfiguration(buildConfiguration);

    builderServices!
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = DefaultJwtOptions.ValidateIssuer,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateIssuerSigningKey = DefaultJwtOptions.ValidateIssuerSigningKey,
                    IssuerSigningKey = jwtOptions.GetSymmetricSecurityKey(),
                    ValidateAudience = DefaultJwtOptions.ValidateAudience, 
                    ValidateLifetime = DefaultJwtOptions.ValidateLifetime,
                    ValidAlgorithms = jwtOptions.ValidAlgorithms,
                    ClockSkew = DefaultJwtOptions.ClockSkew,
                };
            }
        );
}

void UseAuthentication()
{
#if AUTH 
        app.UseAuthentication();
#endif
}

void UseSwaggerGen()
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
}

void MapControllers()
{
    var conventionBuilder = app.MapControllers();
    
#if !AUTH
    conventionBuilder.AllowAnonymous();
#endif
}