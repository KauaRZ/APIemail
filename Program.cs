using Microsoft.Extensions.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "Rodando API!");

app.MapGet("/usuarios", () => "Lista de usuários");

app.MapPost("/esqueci-senha", (string email) =>
{
    return Results.Ok(new
    {
        mensagem = $"Se o email {email} existir, um link foi enviado."
    });
});

app.Run();
