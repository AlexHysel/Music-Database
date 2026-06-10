using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MusicDb>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<MusicManager>();
builder.Services.AddScoped<Orchestrator>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddCors((options) =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Description = "Введите: Bearer {token}"
    });
    options.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateIssuer = true,
            ValidIssuer = "MusicDatabase",
            ValidateAudience = true,
            ValidAudience = "MusicDatabase",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("Token validated successfully");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine("Authentication challenge triggered");
                return Task.CompletedTask;
            },
            OnMessageReceived = context =>
            {
                var header = context.Request.Headers["Authorization"].FirstOrDefault();
                Console.WriteLine($"Raw Authorization header: {header ?? "Not present"}");
                Console.WriteLine($"Token received: {context.Token ?? "null"}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

var key = builder.Configuration["Jwt:Key"]!;
Console.WriteLine($"Validation key: {key}");

using (var scope = app.Services.CreateScope())
{
    MusicDb db = scope.ServiceProvider.GetRequiredService<MusicDb>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();

// Middleware to log Authorization header
app.Use(async (context, next) =>
{
    var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
    Console.WriteLine($"Authorization header: {authHeader ?? "Not present"}");
    await next();
});

// ===== GET =====

app.MapGet("/search", async (Orchestrator o, string searchLine) =>
{
    return await o.Search(searchLine);
});

app.MapGet("/tracks", async (Orchestrator o, int page = 1, int size = 20) =>
{
    return await o.GetTracksAsync(size, page);
});

app.MapGet("/track", async (Orchestrator o, Guid id) =>
{
    Result<TrackDTO> result = await o.GetTrackAsync(id);
    if (result.Success)
        return Results.Ok(result.Data);
    else
        return Results.NotFound("Track not found");
});

app.MapGet("/album", async (Orchestrator o, Guid id) =>
{
    Result<AlbumDTO?> result = await o.GetAlbumAsync(a => a.Id == id);
    if (result.Success)
        return Results.Ok(result.Data);
    else
        return Results.NotFound("Album not found");
});

app.MapGet("/albums", async (Orchestrator o, int page = 1, int size = 20) =>
{
    return await o.GetAlbumsAsync(size, page);
});

app.MapGet("/artists", async (Orchestrator o, int page = 1, int size = 20) =>
{
    return await o.GetArtistsAsync(size, page);
});

app.MapGet("/users", async (Orchestrator o, int page = 1, int size = 20) =>
{
    return await o.GetUsersAsync(size, page);
});

// ===== POST =====
app.MapPost("/tracks", async (Orchestrator o, AddTrackRequest info) =>
{
    Enum.TryParse(info.Genre, true, out Genre result);
    await o.AddTrackAsync(info.Title, info.Artist, info.Others, info.AlbumTitle, result);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapPost("/login", async (Orchestrator o, LogInRequest request) =>
{
    AuthDTO? auth = await o.LogInAsync(request.Name, request.Password);
    if (auth != null)
        return Results.Ok(auth);
    return Results.Unauthorized();
});

app.MapPost("/signup", async (Orchestrator o, SignUpRequest request) =>
{
    Result result = await o.AddUserAsync(request.Username, request.Role, request.Password);
    if (result.Success)
        return Results.Created();
    else
        return Results.Conflict(result.Message);
});

// ===== DELETE =====
app.MapDelete("/tracks", async (Orchestrator o, string title, string album) =>
{
    await o.RemoveTrackAsync(title, album);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/albums", async (Orchestrator o, string title, string artist) =>
{
    await o.RemoveAlbumAsync(title, artist);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.MapDelete("/artists", async (Orchestrator o, string artist) =>
{
    await o.RemoveArtistAsync(artist);
}).RequireAuthorization(policy => policy.RequireRole("Admin"));

app.Run();