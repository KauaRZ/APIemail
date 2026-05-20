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

app.MapPost("/register", async (AppDbContext db, AuthRequest request) =>
{
    // 1. Verifica se email já existe
    var existe = await db.Users.AnyAsync(u => u.Email == request.Email);

    if (existe)
        return Results.BadRequest("Email já cadastrado");

    // 2. Cria usuário
    var user = new User { Email = request.Email };

    // 3. Criptografa senha
    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<User>();
    user.PasswordHash = hasher.HashPassword(user, request.Senha);

    // 4. Salva no banco
    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok("Usuário criado");
});

app.MapPost("/esqueci-senha", (string email) =>
{
    return Results.Ok(new
    {
        mensagem = $"Se o email {email} existir, um link foi enviado."
    });
});

app.Run();
