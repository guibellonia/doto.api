using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Doto.Api.Binders;
using Doto.Api.Converters;
using Doto.Api.Hubs;
using Doto.Api.Middleware;
using Doto.Api.Services;
using Doto.Application.Interfaces;
using Doto.Application.Services;
using Doto.Infrastructure.Extensions;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    // Add DateOnly model binder provider for query string parameters
    var provider = options.ModelBinderProviders.FirstOrDefault(x => x is SimpleTypeModelBinderProvider);
    if (provider != null)
    {
        var index = options.ModelBinderProviders.IndexOf(provider);
        options.ModelBinderProviders.Insert(index, new DateOnlyModelBinderProvider());
    }
    else
    {
        options.ModelBinderProviders.Insert(0, new DateOnlyModelBinderProvider());
    }
})
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        // Add converters in order - DateTime first, then DateTimeOffset
        options.JsonSerializerOptions.Converters.Add(new DateTimeJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new DateTimeOffsetJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Doto API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Insira o token JWT do Supabase no formato: **Bearer {token}**",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = new List<string>()
    });
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevPolicy", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var configuration = builder.Configuration;

var supabaseProjectRef = configuration["SUPABASE_PROJECT_REF"]
    ?? throw new InvalidOperationException("SUPABASE_PROJECT_REF missing");

var supabaseUrl = $"https://{supabaseProjectRef}.supabase.co/auth/v1";
var jwksUrl = $"{supabaseUrl}/.well-known/jwks.json";
var validIssuer = supabaseUrl;

var jwtSecret = configuration["SUPABASE_JWT_SECRET"]
    ?? throw new InvalidOperationException("SUPABASE_JWT_SECRET missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = validIssuer,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(2),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });

builder.Services.AddRouting();
builder.Services.AddAuthorization();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// SignalR
builder.Services.AddSignalR();

#region Referencia das Services da Application

builder.Services.AddScoped<IPersonService, PersonService>();
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<IScheduleService, ScheduleService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IMedicineAdherenceService, MedicineAdherenceService>();
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// Background Services
builder.Services.AddHostedService<MedicationReminderBackgroundService>();

#endregion

var app = builder.Build();

app.UseCors("DevPolicy");
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();