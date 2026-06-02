using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserRepository>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseStaticFiles();

    app.MapGet("/", () =>
    Results.Redirect("/index.html"));

    app.UseHttpsRedirection();

// Criar usuário
app.MapPost("/register", async (
    UserRepository repository,
    AuthRequest request) =>
{

    var existe = await repository.EmailExiste(
        request.Email);

    if(existe)
        return Results.BadRequest(
            "Email já cadastrado");

    var user = new User
    {
        Email=request.Email
    };

    var hasher =
        new PasswordHasher<User>();

    user.PasswordHash=
        hasher.HashPassword(
            user,
            request.Senha);

    await repository.Criar(user);

     var dadosSegurosDoUsuario = new APIemail.DTOs.UserResponseDTO
    {
        Id = user.Id,
        Mensagem = "Usuário cadastrado com sucesso"
    };

    return Results.Created(
        $"/users/{user.Id}",
        dadosSegurosDoUsuario);

});


// Login
app.MapPost("/login", async (
    UserRepository repository,
    AuthRequest request)=>
{
    var usuario=
    await repository.BuscarPorEmail(
        request.Email);

    if(usuario==null)
        return Results.Unauthorized();

    var hasher=
        new PasswordHasher<User>();

    var resultado=
    hasher.VerifyHashedPassword(
        usuario,
        usuario.PasswordHash,
        request.Senha);

    if(resultado==
    PasswordVerificationResult.Failed)
        return Results.Unauthorized();

    return Results.Ok(
        "Login realizado");
});


// Buscar email
app.MapGet("/users/email/{email}",
async (
UserRepository repository,
string email)=>
{
    var existe=
    await repository.EmailExiste(
        email);

    return Results.Ok(new
    {
        Existe=existe
    });
});


// Alterar senha
app.MapPut(
"/trocar-senha",
async(
UserRepository repository,
AppDbContext db,
ChangePasswordRequest request)=>
{
    var usuario=
    await repository.BuscarPorEmail(
        request.Email);

    if(usuario==null)
        return Results.NotFound();

    var hasher=
    new PasswordHasher<User>();

    usuario.PasswordHash=
    hasher.HashPassword(
        usuario,
        request.NovaSenha);

    await db.SaveChangesAsync();

    return Results.Ok(
        "Senha alterada");
});


// Deletar usuário
app.MapDelete(
"/users/{email}",
async(
UserRepository repository,
string email)=>
{
    var usuario=
    await repository.BuscarPorEmail(
        email);

    if(usuario==null)
        return Results.NotFound();

    await repository.Deletar(
        usuario);

    return Results.Ok(
        "Usuário removido");
});

app.Run();