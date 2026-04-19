using System.Text;
using JunkanBackend.Services;
using JunkanBackend.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
        policy.WithOrigins("http://localhost:5173", "https://junkanlibrary-vercel.vercel.app/")
              .AllowAnyHeader()
              .AllowAnyMethod()
    );
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<UserStore>();
builder.Services.AddSingleton<TokenService>();

var app = builder.Build();

app.UseCors("frontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/auth/register", (RegisterRequest req, UserStore store) =>
{
    if (store.FindByEmail(req.Email) is not null)
        return Results.BadRequest(new { error = "Email already in use" });

    store.Create(req.Email, req.Password);
    return Results.Ok(new { message = "User created" });
});

app.MapPost("/auth/login", (LoginRequest req, UserStore store, TokenService tokens) =>
{
    var user = store.FindByEmail(req.Email);

    if (user is null || !store.VerifyPassword(user, req.Password))
        return Results.Unauthorized();

    var token = tokens.GenerateToken(user);
    return Results.Ok(new LoginResponse(token, user.Email, user.Role));
});

app.MapGet("/me", (HttpContext ctx) =>
{
    var email = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
    var role = ctx.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
    return Results.Ok(new { email, role });
}).RequireAuthorization();

app.Run();