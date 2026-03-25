using Microsoft.Extensions.Localization;

var builder = WebApplication.CreateBuilder(args);

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
app.MapPost("/usuarios", (string nome) =>
{
    return $"Usuario {nome} criado";
});



app.Run();
