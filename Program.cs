using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WebApiVotacionElectronica.Context;
using WebApiVotacionElectronica.Repository;
using WebApiVotacionElectronica.Repository.Interfaces;
using WebApiVotacionElectronica.Tools;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors();
builder.Services.AddDbContext<DBContext>(x => x.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConectionString")));

//interfaces and repositories
builder.Services.AddScoped<ICandidato_Repository,Candidato_Repository>();
builder.Services.AddScoped<IEstado_Candidato_Repository,Estado_Candidato_Repository>();
builder.Services.AddScoped<IEstado_Votacion_Repository,Estado_Votacion_Repository>();
builder.Services.AddScoped<ISede_Repository,Sede_Repository>();
builder.Services.AddScoped<IVotacion_Repository,Votacion_Repository>();
builder.Services.AddScoped<IVotante_Repository,Votante_Repository>();
builder.Services.AddScoped<IVoto_Repository,Voto_Repository>();

//services
 builder.Services.AddControllersWithViews().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

//token
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:key"])),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
    };
});


//cors
builder.Services.AddCors(options => options.AddPolicy("Cors",
            builder =>
            {
                builder.
                AllowAnyOrigin().
                AllowAnyMethod().
                AllowAnyHeader();
            }
));


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("Cors");

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

app.MapControllers();

app.Run();
