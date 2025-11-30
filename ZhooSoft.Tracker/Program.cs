using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using ZhooSoft.Tracker.CustomEventBus;
using ZhooSoft.Tracker.Hubs;
using ZhooSoft.Tracker.Services;
using ZhooSoft.Tracker.Store;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Authentication & JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwt = builder.Configuration.GetSection("JwtSettings");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]))
        };

        // 👇 Required so SignalR can authenticate via access_token query
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                    context.Token = accessToken;

                return Task.CompletedTask;
            }
        };
    });

// Swagger

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Description = "Bearer Authentication with JWT Token",
        Type = SecuritySchemeType.Http
    });
});

// SignalR
builder.Services.AddSingleton<BookingStateService>();

builder.Services.AddSignalR();
builder.Services.AddSingleton<DriverLocationStore>();
builder.Services.AddSingleton<BookingMonitorService>();

// Custom event Bus
// Today
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

// Tomorrow
//builder.Services.AddSingleton<IEventBus>(sp =>
//    new AzureServiceBusEventBus("<AzureSB-ConnectionString>"));

builder.Services.AddScoped<DriverLocationUpdatedHandler>();

builder.Services.AddHttpClient<IMainApiService, MainApiService>();

// 👇 Add CORS for SignalR (important for mobile/web clients)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(_ => true);
    });
});

var app = builder.Build();

var eventBus = app.Services.GetRequiredService<IEventBus>();
eventBus.Subscribe<DriverLocationUpdatedEvent, DriverLocationUpdatedHandler>();

app.UseHttpsRedirection();

// 👇 Order matters
app.UseRouting();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<DriverLocationHub>("/hubs/location");



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

