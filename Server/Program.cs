using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Server;
using Server.Constants;
using Server.Models.Contexts;
using Server.Options;
using Server.Services;

var builder = WebApplication.CreateBuilder(args);
var buildConfiguration = builder.Configuration;
var builderServices = builder.Services;

AddTransients();

builderServices.AddControllers().AddNewtonsoftJson();

ConfigureDatabase();
ConfigureSwaggerGen();
ConfigureJwtOptions();

var app = builder.Build();

UseSwaggerGen();

app.UseHttpsRedirection();
        
UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();



void AddTransients()
{
    builderServices.AddTransient<IAccessTokenService, JwtService>();
    builderServices.AddTransient<IRefreshTokenService, RefreshTokenService>();
}

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