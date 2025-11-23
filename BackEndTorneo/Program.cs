using BackEndTorneo.Data;
using BackEndTorneo.Helpers;
using BackEndTorneo.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Registrar servicios de Data
builder.Services.AddScoped<AuthData>();
builder.Services.AddScoped<EquiposData>();
builder.Services.AddScoped<JugadoresData>();
builder.Services.AddScoped<TorneosData>();
builder.Services.AddScoped<PartidosData>();
builder.Services.AddScoped<SedesData>();
builder.Services.AddScoped<QRData>();
builder.Services.AddScoped<EstadisticasData>();
builder.Services.AddScoped<NotificacionesData>();
builder.Services.AddScoped<UsuariosData>();
builder.Services.AddHttpClient<ClaudeService>();
builder.Services.AddScoped<ClaudeService>();



// Registrar JwtHelper
builder.Services.AddScoped<JwtHelper>();

// Configurar CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configurar JWT Authentication
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
    };
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();