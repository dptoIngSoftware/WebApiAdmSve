using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApiVotacionElectronica.Context;
using WebApiVotacionElectronica.Repository;
using WebApiVotacionElectronica.Repository.Interfaces;
using WebApiVotacionElectronica.Services;
using WebApiVotacionElectronica.Tools;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddDbContext<DBContext>(x =>
    x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConectionString")));

// Repositorios
builder.Services.AddScoped<ICandidato_Repository, Candidato_Repository>();
builder.Services.AddScoped<IEstado_Candidato_Repository, Estado_Candidato_Repository>();
builder.Services.AddScoped<IEstado_Votacion_Repository, Estado_Votacion_Repository>();
builder.Services.AddScoped<ISede_Repository, Sede_Repository>();
builder.Services.AddScoped<IVotacion_Repository, Votacion_Repository>();
builder.Services.AddScoped<IVotante_Repository, Votante_Repository>();
builder.Services.AddScoped<IVoto_Repository, Voto_Repository>();

builder.Services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
);

// Configurar JWT
//token
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:key"])),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true, // valida exp
            ClockSkew = TimeSpan.Zero, // sin tolerancia extra
        };

        // Ignoramos por completo iat
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                // No hacemos nada con iat
                return Task.CompletedTask;
            }
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Cors", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddSingleton<IBackgroundEmailQueue, BackgroundEmailQueue>();

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

var app = builder.Build();

//Middleware HTTP
app.UseHttpsRedirection();
app.UseRouting();

// CORS debe ir antes de Authentication
app.UseCors("Cors");

app.UseAuthentication();
app.UseAuthorization();

// Middleware personalizado (manejo de errores)
app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();

app.Run();