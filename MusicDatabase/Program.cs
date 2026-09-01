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
builder.Services.AddControllers();
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
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors();
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
        Logging.Success("Database successfully migrated");
    }
    catch (Exception e)
    {
        Logging.Error("Database migration failed: \n" + e.Message);
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/search", async (Orchestrator o, string searchLine) => await o.Search(searchLine));

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

app.Run();