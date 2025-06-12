using Api.BackGroundServices;
using Api.BackGroundServices.ProccessContract;
using Api.BackGroundServices.ProcessImplimentations;
using Api.NotificationHub;
using Infrastructure;
using Infrastructure.Contracts;
using Infrastructure.Helper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SurveillanceDevice.Integration.HIKVision;
using SurveillanceDevice.Integration.HttpClientBuilder;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "JwtBearer";
        options.DefaultChallengeScheme = "JwtBearer";
        options.DefaultScheme = "JwtBearer"; // optional but safe
    })
    .AddJwtBearer("JwtBearer", options =>
    {
        options.SaveToken = true;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey =
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(configuration["JwtSettings:SecurityKey"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
            },
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<DataContext>(x => x.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<CustomConnectionStringService>();
builder.Services.AddScoped<DynamicDbContextFactory>();
builder.Services.AddSingleton<IHikVisionMachineService, HikVisionMachineService>();
builder.Services.AddSingleton<ICustomHttpClientBuilder, CustomHttpClientBuilder>();
builder.Services.AddSingleton<IProgressManager, ProgressManager>();
builder.Services.AddHostedService<Syncronizer>();
builder.Services.AddHostedService<AttendanceSyncronizer>();
builder.Services.AddHostedService<PersonAndVisitorSyncronizer>();
builder.Services.AddHostedService<DataExchangeSyncronizer>();
builder.Services.AddSignalR();
builder.Services.AddCors(options => options.AddPolicy("AllowAll", builder =>
{
    builder.WithOrigins("*")
        .AllowAnyMethod()
        .AllowAnyHeader();
}));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<NotificationsHub>("/notificationshub");
app.MapControllers();

app.Run();