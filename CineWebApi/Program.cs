using Cine_TP.Data.Implementations;
using Cine_TP.Data.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Cine_TP.Data;
using ViajesRepository.Data.Implementations;
using Cine_TP.Services.Interfaces;
using Cine_TP.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Inyección de Dependencias (Servicios)

// Registrar DbContext apuntando a SQL Server con la cadena de conexión de appsettings.json
builder.Services.AddDbContext<CineDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar Repositorios (Inyección de Dependencias)
builder.Services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
builder.Services.AddScoped<IPeliculaRepository, PeliculaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
// Registrar Servicios
builder.Services.AddScoped<IComprobanteService, ComprobanteService>();

builder.Services.AddAuthorization();
// Agregar controladores y OpenAPI/Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
// 1. Permite que la raíz (/) busque automáticamente index.html -------------
app.UseDefaultFiles();

// 2. Habilita los archivos estáticos de wwwroot
app.UseStaticFiles();

app.MapControllers();

app.Run();