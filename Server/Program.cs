using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Server;
using Swashbuckle.AspNetCore.Swagger;
using Server.Models.Config;
using Server.Models.Constants;
using Server.Models.Authentication;
using Server.Models.Contexts;
using Server.Options;

var builder = WebApplication.CreateBuilder(args);
var buildConfiguration = builder.Configuration;
var builderServices = builder.Services;

builderServices.AddControllers().AddNewtonsoftJson();

// Add DbContexts to static aggregator
var dbConnectionString = buildConfiguration.GetConnectionString("DbConnectionString")!;
DbContexts.DbConnectionString = dbConnectionString;

ConfigureSwaggerGen();
ConfigureJwtOptions();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
        
#if AUTH 
        app.UseAuthentication();
#endif
app.UseAuthorization();

app.MapControllers();

app.Run();



void ConfigureSwaggerGen()
{
    builderServices!.AddEndpointsApiExplorer();
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
    var jwtOptions = buildConfiguration!
        .GetSection(nameof(JwtOptions))
        .Get<JwtOptions>();

    if (jwtOptions is null) 
        throw new ApplicationException("JWT is not configured for application");

    builderServices!
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateAudience = false, // todo валидировать аудиенцию токена
                    ValidateLifetime = true,
                    ValidAlgorithms = jwtOptions.ValidAlgorithms,
                    IssuerSigningKey = jwtOptions.GetSymmetricSecurityKey(),
                    ClockSkew = TimeSpan.Zero,
                };
            }
        );
}

