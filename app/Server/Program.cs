using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Domain;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
           .UseSnakeCaseNamingConvention());

// ── Repositories ──────────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository,      EfUserRepository>();
builder.Services.AddScoped<IPortfolioRepository, EfPortfolioRepository>();
builder.Services.AddScoped<ISecurityRepository,  EfSecurityRepository>(); 
builder.Services.AddScoped<IHoldingRepository, EfHoldingRepository>();
builder.Services.AddScoped<ITradeRepository,   EfTradeRepository>();
builder.Services.AddScoped<IBankAccountRepository,  EfBankAccountRepository>();
builder.Services.AddScoped<IFundTransferRepository, EfFundTransferRepository>();

// ── Services ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddScoped<IHoldingService, HoldingService>();
builder.Services.AddScoped<ITradeService,   TradeService>();
builder.Services.AddScoped<IBankAccountService,  BankAccountService>();
builder.Services.AddScoped<IFundTransferService, FundTransferService>();

// ── Http ─────────────────────────────────────────────────────────────

builder.Services.AddHttpClient<IMarketDataService, MarketDataService>(); 

// ── JWT Authentication ────────────────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

// ── CORS ──────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAngular", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi();

// ── Pipeline ──────────────────────────────────────────────────────────
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseCors("AllowAngular");          // CORS before everything
app.UseHttpsRedirection();
app.UseAuthentication();              // Authentication before Authorization
app.UseAuthorization();
app.MapControllers();

app.Run();