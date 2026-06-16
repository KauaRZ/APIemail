using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserRepository>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

builder.Services
    .AddScoped<ProdutoRepository>();

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
     if(string.IsNullOrWhiteSpace(request.Email))
     {
     return Results.BadRequest(
        "Email é obrigatório");
      }

     if(string.IsNullOrWhiteSpace(request.Senha))
     {
      return Results.BadRequest(
        "Senha é obrigatória");
   }

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

// Buscar cadastros para pagina de usuarios
app.MapGet("/users", async (
    AppDbContext db) =>
{
    var usuarios =
        await db.Users
            .Select(u => new
            {
                u.Email
            })
            .ToListAsync();

    return Results.Ok(
        usuarios);
});

//rotina de cadastros produtos

app.MapPost(
"/produtos",
async (
ProdutoRepository repository,
Produto produto) =>
{
    await repository.Criar(produto);

    return Results.Ok(
        "Produto cadastrado");
});

app.MapGet("/produtos", async (AppDbContext db) =>
{
    var produtos = await db.Produtos
        .OrderByDescending(p => p.Id)
        .Take(10)
        .ToListAsync();

    return Results.Ok(produtos);
});

app.MapPut(
"/produtos/{id}",
async (
ProdutoRepository repository,
int id,
Produto dados) =>
{
    var produto =
        await repository.BuscarPorId(id);

    if(produto == null)
        return Results.NotFound();

    produto.Codigo =
        dados.Codigo;

    produto.Descricao =
        dados.Descricao;

    produto.Categoria =
        dados.Categoria;

    produto.Unidade =
        dados.Unidade;

    produto.Fornecedor =
        dados.Fornecedor;

    produto.EstoqueMinimo =
        dados.EstoqueMinimo;

    produto.Observacao =
        dados.Observacao;

    await repository.Atualizar();

    return Results.Ok();
});

app.MapDelete("/produtos/{id}", async (int id, AppDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);

    if (produto == null)
        return Results.NotFound();

    db.Produtos.Remove(produto);

    await db.SaveChangesAsync();

    return Results.Ok();
});

app.Run();