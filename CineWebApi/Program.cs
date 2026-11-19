using Cine_TP.Data.Implementations;
using Cine_TP.Data.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Cine_TP.Data;
using Cine_TP.Services.Interfaces;
using Cine_TP.Services.Implementations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "CineWebApi",
        ValidAudience = "CineWebApi",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "token"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 1. Configuración de Inyección de Dependencias (Servicios)

// Registrar DbContext apuntando a SQL Server con la cadena de conexión de appsettings.json
builder.Services.AddDbContext<CineDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registrar Repositorios (Inyección de Dependencias)
builder.Services.AddScoped<IComprobanteRepository, ComprobanteRepository>();
builder.Services.AddScoped<IPeliculaRepository, PeliculaRepository>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<ICatalogoRepository, CatalogoRepository>();
builder.Services.AddScoped<IFuncionRepository, FuncionRepository>();
// Registrar Servicios
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IComprobanteService, ComprobanteService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPeliculaService, PeliculaService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IFuncionService, FuncionService>();

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