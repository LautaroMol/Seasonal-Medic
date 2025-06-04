using APISeasonalTicket.Data;
using APISeasonalTicket.DTOs;
using APISeasonalTicket.Models;
using APISeasonalTicket.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

//redireccion
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
});


//gmail
builder.Services.Configure<GmailSettings>(builder.Configuration.GetSection("GmailSettings"));
//login
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var key = Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]);
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
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

//identity options
// Identity options
builder.Services.AddIdentity<User, IdentityRole<int>>(options =>
{
    options.SignIn.RequireConfirmedEmail = true;
    options.User.RequireUniqueEmail = true;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

//MesageService
builder.Services.AddScoped<IMessageService, MessageService>();
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
app.UseCors("AllowAll");
app.UseAuthentication();

app.UseHttpsRedirection();
app.UseRouting();
// Usar CORS

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
app.MapGet("/api/card/user/{userId}", async (ICreditCardService creditCardService, int userId) =>
{
    var cards = await creditCardService.GetCreditCardsByUserIdAsync(userId);
    if (cards.Any())
    {
        return Results.Ok(cards);
    }
    return Results.NotFound("No hay tarjetas guardadas para este usuario.");
});
app.MapPut("/api/cards/set-main", async (
    SetMainCardRequest request,
    ICreditCardService cardService) =>
{
    var success = await cardService.SetMainCardAsync(request.UserId, request.CardId);

    if (!success)
        return Results.BadRequest("No se pudo actualizar la tarjeta principal.");

    return Results.Ok("Tarjeta principal actualizada correctamente.");
});

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

app.MapDelete("/api/card/{id}", async (ICreditCardService creditCardService, int id) =>
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

app.MapGet("/api/movimiento/{id}", async (IMovimientosAbonoService movimientosAbonoService, int id) =>
{
    var mov = await movimientosAbonoService.GetMovimientosAbonoByIdAsync(id);
    if (mov != null)
    {
        return Results.Ok(mov);
    }
    return Results.NotFound();
});

app.MapPost("/api/movimiento", async (IMovimientosAbonoService movimientosAbonoService, MovAbonosDto movAbonoDto) =>
{
    var mov = await movimientosAbonoService.Post(movAbonoDto);
    return Results.Ok(mov);
});

app.MapGet("/api/movimiento/abono/{abonoId}", async (IMovimientosAbonoService movimientosAbonoService, int abonoId) =>
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

app.MapDelete("/api/movimiento/{id}", async (IMovimientosAbonoService movimientosAbonoService, int id) =>
{
    var mov = await movimientosAbonoService.Delete(id);
    return Results.Ok(mov);
});

#endregion

#endregion



app.Run();

public static class IdentityDataInitializer
{
    public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

        string[] roles = { "Admin", "Supervisor", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }
}
