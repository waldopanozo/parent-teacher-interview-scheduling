using System.Text;
using InterviewScheduling.Api.Data;
using InterviewScheduling.Api.Options;
using InterviewScheduling.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<AuthOptions>(builder.Configuration.GetSection(AuthOptions.SectionName));
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<SchedulingOptions>(builder.Configuration.GetSection(SchedulingOptions.SectionName));

if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("IntegrationTests"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
}

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<SchoolSettingsService>();
builder.Services.AddScoped<SlotGenerator>();
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<WeeklyAvailabilityService>();
builder.Services.AddScoped<ParentMeetingProfileService>();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
          ?? throw new InvalidOperationException("Jwt configuration section is missing.");
if (string.IsNullOrWhiteSpace(jwt.SigningKey) || jwt.SigningKey.Length < 32)
    throw new InvalidOperationException("Jwt:SigningKey must be at least 32 characters.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:3456"];
builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var log = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    if (app.Environment.IsEnvironment("Testing"))
        await db.Database.EnsureCreatedAsync();
    else
        await db.Database.MigrateAsync();

    var testing = app.Environment.IsEnvironment("Testing");
    var production = app.Environment.IsProduction();
    var seedDemoFlag = app.Configuration.GetValue("Seed:DemoUsers", false);
    var seedDemoPasswordUsers = !testing && !production &&
                                (app.Environment.IsDevelopment() || seedDemoFlag);

    log.LogInformation(
        "Environment={Env}, Production={Prod}, Seed:DemoUsers from config={SeedFlag} → run demo user seed={RunSeed}",
        app.Environment.EnvironmentName,
        production,
        seedDemoFlag,
        seedDemoPasswordUsers);

    var schedOpts = builder.Configuration.GetSection(SchedulingOptions.SectionName).Get<SchedulingOptions>()
                    ?? new SchedulingOptions();
    await DbSeeder.SeedAsync(db, seedDemoPasswordUsers, log, schedOpts.SchoolTimeZoneId, schedOpts.UiLanguage,
        schedOpts.ThemePreset);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
await app.RunAsync();

public partial class Program;
