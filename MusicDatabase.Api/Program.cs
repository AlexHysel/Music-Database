using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

using MusicDatabase.Core.Services;
using MusicDatabase.Data;
using MusicDatabase.Contracts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt__Key"] ?? builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey))
    throw new InvalidOperationException("JWT key is missing. Set the 'Jwt__Key' secret before starting the application.");

builder.Services.AddDbContext<MusicDb>(opt => opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<MusicManager>();
builder.Services.AddScoped<AlbumService>();
builder.Services.AddScoped<TrackService>();
builder.Services.AddScoped<ArtistService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SearchService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
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
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    MusicDb db = scope.ServiceProvider.GetRequiredService<MusicDb>();
    try
    {
        db.Database.Migrate();
    }
    catch (Exception e)
    {
        Console.Error.WriteLine("Database migration failed:");
        Console.Error.WriteLine(e.Message);
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/", async context =>
{
    context.Response.Redirect("/search.html");
    await Task.CompletedTask;
});

app.MapGet("/search", async (SearchService search, string searchLine, CancellationToken ctg) =>
{
    await search.Search(searchLine);
});

app.MapPost("/login", async (UserService users, LogInRequest request, CancellationToken ctg) =>
{
    var result = await users.LogInAsync(request.Username, request.Password);
    if (result.IsSuccess)
        return Results.Ok(result.Value);
    return Results.Unauthorized();
});

app.MapPost("/signup", async Task<IResult> (UserService users, SignUpRequest request, CancellationToken ctg) =>
{
    if (request.Username.Trim().Length is < 3 or > 20)
        return Results.BadRequest("Username must be between 3 and 20 chars long.");
    if (request.Username.Trim().ContainsAny([' ', '\n', '\t']))
        return Results.BadRequest("Username must contain no whitespaces.");
    if (request.Password.Trim().Length is < 8 or > 30)
        return Results.BadRequest("Password must be between 8 and 30 chars long.");
    if (request.Password.Trim().ContainsAny([' ', '\n', '\t']))
        return Results.BadRequest("Password must contain no whitespaces.");
    
    var result = await users.CreateUserAsync(request);
    if (result.IsSuccess)
        return Results.Created();
    else
        return Results.Conflict(result.Error.Message);
});

app.Run();