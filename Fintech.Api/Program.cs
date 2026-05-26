using Serilog;
using Serilog.Formatting.Compact;
using Fintech.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Fintech.Api.Services;
using Fintech.Api.Services.Transactions;
using Fintech.Api.Services.Auth;
using Fintech.Api.Middleware;
using Fintech.Api.Workers;
using Scalar.AspNetCore;
using OllamaSharp;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Conditional( _ => builder.Environment.IsDevelopment(),
        wt => wt.Console())
    .WriteTo.Conditional( _ => !builder.Environment.IsDevelopment(),
        wt => wt.Console(new CompactJsonFormatter()))
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var _configuration = builder.Configuration;

var connectionString = _configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

var jwtSettings = _configuration["JwtSettings:Secret"];
var key = Encoding.UTF8.GetBytes(jwtSettings!);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = null;
    options.AddServerHeader = false;
});

if(!builder.Environment.IsDevelopment())
{
    builder.Services.AddRateLimiter(options =>
    {
        options.AddFixedWindowLimiter("ApiLimiter", limiter =>
        {
            limiter.Window = TimeSpan.FromMinutes(20);
            limiter.PermitLimit = 20;
            limiter.QueueLimit = 0;
        });

        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    });
}

builder.Services.AddAuthentication(options =>{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _configuration["JwtSettings:Issuer"],
            ValidateAudience = true,
            ValidAudience = _configuration["JwtSettings:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var rawToken = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "").Trim();
                var blacklist = context.HttpContext.RequestServices.GetRequiredService<HashSet<RevokedToken>>();

                if (blacklist.FirstOrDefault(item => item.Token == rawToken) != null)
                {
                    context.Fail("This token has been revoked.");
                }

                return Task.CompletedTask;
            }
        };
    });

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<IOllamaApiClient>(new OllamaApiClient("http://localhost:11434"));
    builder.Services.AddScoped<IOllamaService, OllamaService>();
}
else
{
    builder.Services.AddScoped<IOllamaService, OllamaStubService>();
}

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<HashSet<RevokedToken>>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<ITransactionStrategy, TransferStrategy>();
builder.Services.AddScoped<ITransactionStrategy, WithdrawalStrategy>();
builder.Services.AddScoped<ITransactionStrategy, DepositStrategy>();
builder.Services.AddScoped<IIdempotencyService, IdempotencyService>();

builder.Services.AddControllers();

builder.Services.AddHostedService<TokenCleanupWorker>();
builder.Services.AddHostedService<DatabaseCapWorker>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseHsts();

if(!builder.Environment.IsDevelopment())
    app.UseRateLimiter();

app.UseMiddleware<TokenBlacklistMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

if(!builder.Environment.IsDevelopment())
    app.MapControllers().RequireRateLimiting("ApiLimiter");
else
    app.MapControllers();

app.Run();
