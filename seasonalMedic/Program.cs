using APISeasonalMedic.Data;
using APISeasonalMedic.DTOs;
using APISeasonalMedic.Extensions;
using APISeasonalMedic.Models;
using APISeasonalMedic.Services;
using APISeasonalMedic.Services.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });
builder.Services.AddEndpointsApiExplorer();

// CONFIGURACI�N DE SWAGGER CON JWT 
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Seasonal Medic API", Version = "v1" });

    // Configuraci�n de seguridad JWT para Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

//db
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

//automapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

//Services
builder.Services.AddScoped<ICreditCardService, CreditCardService>();
builder.Services.AddScoped<IAbonoService, AbonoService>();
builder.Services.AddScoped<IMovimientosAbonoService, MovimientosAbonoService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddHostedService<MonthlyDebitService>();
builder.Services.AddHttpClient<MercadoPagoService>();
builder.Services.AddScoped<MercadoPagoService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IConsultaMedicaService, ConsultaMedicaService>();
builder.Services.AddScoped<CloudinaryService>();



builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
builder.Configuration
    .AddUserSecrets<Program>()
    .AddEnvironmentVariables();
//gmail
builder.Services.Configure<GmailSettings>(builder.Configuration.GetSection("GmailSettings"));

// Identity options - CORREGIDO: Email confirmation deshabilitado para desarrollo
builder.Services.AddIdentity<User, IdentityRole<Guid>>(options =>
{
    // IMPORTANTE: Deshabilitar confirmaci�n de email para desarrollo
    options.SignIn.RequireConfirmedEmail = false; // Cambiado de true a false
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication - DEBE IR DESPU�S DE Identity
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"]);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        ValidAudience = builder.Configuration["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ClockSkew = TimeSpan.Zero // Opcional: reduce la tolerancia de tiempo
    };
});

//views
builder.Services.AddControllersWithViews();

// Add CORS services
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.ApplyMigrations();
// IMPORTANTE: Orden correcto del middleware
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseRouting();

// Authentication y Authorization DESPU�S de UseRouting
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapControllerRoute(
    name: "mercadoPago",
    pattern: "{controller=MercadoPago}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "mercadoPagoError",
    pattern: "MercadoPago/Error",
    defaults: new { controller = "MercadoPago", action = "Error" });

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await IdentityDataInitializer.SeedRolesAsync(services);
}

#region api routes

#region cards
app.MapGet("/api/cards", async(ICreditCardService creditCardService) =>
{
    return await creditCardService.GetAll();
});

app.MapGet("/api/card/{id}", async(ICreditCardService creditCardService, int id) =>
{
    var card = await creditCardService.GetCreditCardByIdAsync(id);
    if (card != null)
    {
        return Results.Ok(card);
    }
    return Results.NotFound();
});
app.MapGet("/api/card/user", async (
    ICreditCardService creditCardService,
    HttpContext httpContext) =>
{
    var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
        return Results.Unauthorized();

    if (!Guid.TryParse(userIdClaim.Value, out var userId))
        return Results.BadRequest("ID de usuario inv�lido");

    var cards = await creditCardService.GetCreditCardsByUserIdAsync(userId);
    if (cards.Any())
    {
        return Results.Ok(cards);
    }
    return Results.NotFound("No hay tarjetas guardadas para este usuario.");
})
.RequireAuthorization()
.WithName("GetUserCards")
.WithTags("Cards");

app.MapPut("/api/cards/set-main", async (
    SetMainCardRequest request,
    ICreditCardService cardService,
    HttpContext httpContext) =>
{
    var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
    if (userIdClaim == null)
        return Results.Unauthorized();

    if (!Guid.TryParse(userIdClaim.Value, out var userId))
        return Results.BadRequest("ID de usuario inv�lido");

    var success = await cardService.SetMainCardAsync(userId, request.CardId);

    if (!success)
        return Results.BadRequest("No se pudo actualizar la tarjeta principal.");

    return Results.Ok("Tarjeta principal actualizada correctamente.");
})
.RequireAuthorization()
.WithName("SetMainCard")
.WithTags("Cards");

app.MapPost("/api/card", async (ICreditCardService creditCardService, CreditCardDto creditCard) =>
{
    var card = await creditCardService.CreateCreditCardAsync(creditCard);
    return Results.Ok(card);
});
app.MapPut("/api/card/{id}", async (ICreditCardService creditCardService, CreditCardDto creditCard, int id) =>
{
    var card = await creditCardService.UpdateCreditCardAsync(creditCard, id);
    return Results.Ok(card);
});

app.MapDelete("/api/card/{id}", async (ICreditCardService creditCardService, Guid id) =>
{
    try
    {
        var result = await creditCardService.DeleteCreditCardAsync(id);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { message = ex.Message });
    }
});

#endregion

#region Movimientos

app.MapGet("/api/movimientos", async (IMovimientosAbonoService movimientosAbonoService) =>
{
    return await movimientosAbonoService.GetAll();
});

app.MapGet("/api/movimiento/{id}", async (IMovimientosAbonoService movimientosAbonoService, Guid id) =>
{
    var mov = await movimientosAbonoService.GetMovimientosAbonoByIdAsync(id);
    if (mov != null)
    {
        return Results.Ok(mov);
    }
    return Results.NotFound();
});

app.MapGet("/api/movimientos/user", async (
    HttpContext httpContext,
    IMovimientosAbonoService movimientosService
) =>
{
    var userIdClaim = httpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                      ?? httpContext.User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value
                      ?? httpContext.User.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;

    if (string.IsNullOrEmpty(userIdClaim))
        return Results.Unauthorized();

    if (!Guid.TryParse(userIdClaim, out var userId))
        return Results.BadRequest("ID de usuario inv�lido");

    var movimientos = await movimientosService.GetMovimientosByUserId(userId);

    return Results.Ok(movimientos);
});


app.MapPost("/api/movimiento", async (IMovimientosAbonoService movimientosAbonoService, MovAbonosDto movAbonoDto) =>
{
    var mov = await movimientosAbonoService.Post(movAbonoDto);
    return Results.Ok(mov);
});

app.MapGet("/api/movimiento/abono/{abonoId}", async (IMovimientosAbonoService movimientosAbonoService, Guid abonoId) =>
{
    var mov = await movimientosAbonoService.GetByAbonoId(abonoId);
    return Results.Ok(mov);
});

//app.MapPut("/api/movimiento", async (IMovimientosAbonoService movimientosAbonoService, MovAbonosDto movAbonoDto, int id) =>
//{
//    var card = await movimientosAbonoService.Update(movAbonoDto, id);
//    if (card == null)
//    {
//        return Results.NotFound();
//    }
//    return Results.Ok(card);
//});

app.MapDelete("/api/movimiento/{id}", async (IMovimientosAbonoService movimientosAbonoService, Guid id) =>
{
    var mov = await movimientosAbonoService.Delete(id);
    return Results.Ok(mov);
});

#endregion

#endregion

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<ApplicationDbContext>();

    // 1) Asegura que la base exista (crea la BD si no existe)
    await dbContext.Database.EnsureCreatedAsync();

    // 2) Aplica migraciones pendientes (tablas, esquema)
    await dbContext.Database.MigrateAsync();

    // 3) Seed roles
    await IdentityDataInitializer.SeedRolesAsync(services);
}

app.Run();

public static class IdentityDataInitializer
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        string[] roles = { "Admin", "Agente", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<Guid>(role));
        }
    }
}
