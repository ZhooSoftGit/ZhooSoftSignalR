using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using ZCars.Shared;
using ZhooSoft.Tracker;
using ZhooSoft.Tracker.CustomEventBus;
using ZhooSoft.Tracker.Hubs;
using ZhooSoft.Tracker.Services;
using ZhooSoft.Tracker.Store;

var builder = WebApplication.CreateBuilder(args);

//Add logger
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddControllers();

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

// Authentication & JWT
builder.Services.AddAuthentication(options =>
{
    // Identity made Cookie authentication the default.
    // However, we want JWT Bearer Auth to be the default.
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
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

        // 🔥 Required so SignalR can authenticate when WebSockets can't send headers
        //options.Events = new JwtBearerEvents
        //{
        //    OnMessageReceived = context =>
        //    {
        //        // If the request is for our hub...
        //        var path = context.HttpContext.Request.Path;
        //        var accessToken = context.Request.Query["access_token"];

        //        if (!string.IsNullOrEmpty(accessToken) &&
        //            (path.StartsWithSegments("/hubs/location")))
        //        {
        //            // Read the token out of the query string
        //            context.Token = accessToken;
        //        }
        //        return Task.CompletedTask;
        //    }
        //};
    });


// Swagger

builder.Services.AddEndpointsApiExplorer();
// 🔹 Add Swagger with JWT support
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

// SignalR
builder.Services.AddSingleton<BookingStateService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<DriverLocationStore>();
builder.Services.AddScoped<BookingMonitorService>();

// Custom event Bus
// Today
builder.Services.AddSingleton<IEventBus, InMemoryEventBus>();

// Redis Client & Repositories
builder.Services.AddSingleton<RedisConnectionFactory>(provider =>
{
    var redisConnString = builder.Configuration["Redis:ConnectionString"];
    return new RedisConnectionFactory(redisConnString);
});
builder.Services.AddSingleton<DriverRedisRepository>();

builder.Services.AddScoped<DriverLocationUpdatedHandler>();

builder.Services.AddHttpClient<IMainApiService, MainApiService>();



// Azure Service Bus for SignalR events
builder.Services.AddTrackerApiServiceBus(builder.Configuration);

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
