using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using API.Models;
using API.DTOs;


using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var jwtKey =
    "MinhaChaveSuperSecretaERP2026JWTSegurancaOncoclinicas";

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserRepository>();

builder.Services.AddEndpointsApiExplorer();

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)

    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey))
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Informe o token JWT"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

builder.Services
    .AddScoped<ProdutoRepository>();

var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseStaticFiles();

    app.MapGet("/", () =>
    Results.Redirect("/index.html"));

    app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

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
    AuthRequest request) =>
{
    var usuario =
        await repository.BuscarPorEmail(
            request.Email);

    if (usuario == null)
        return Results.Unauthorized();

    var hasher =
        new PasswordHasher<User>();

    var resultado =
        hasher.VerifyHashedPassword(
            usuario,
            usuario.PasswordHash,
            request.Senha);

    if (resultado ==
        PasswordVerificationResult.Failed)
        return Results.Unauthorized();

    var claims =
        new List<Claim>
    {
        new Claim(
            ClaimTypes.Name,
            usuario.Email),

        new Claim(
            ClaimTypes.Email,
            usuario.Email)
    };

    var key =
        new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

    var credenciais =
        new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

    var token =
        new JwtSecurityToken(
            claims: claims,
            expires:
                DateTime.UtcNow.AddHours(2),
            signingCredentials:
                credenciais);

    var tokenString =
        new JwtSecurityTokenHandler()
            .WriteToken(token);

    return Results.Ok(new
    {
        Token = tokenString
    });
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
})
.RequireAuthorization();

app.MapDelete("/produtos/{id}", async (int id, AppDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);

    if (produto == null)
        return Results.NotFound();

    db.Produtos.Remove(produto);

    await db.SaveChangesAsync();

    return Results.Ok();
});



// SOLICITAÇÕES


// Criar solicitação
app.MapPost("/solicitacoes",
async (SolicitacaoDTO dto, AppDbContext db) =>
{
    if (dto.Itens == null || dto.Itens.Count == 0)
    {
        return Results.BadRequest(
            "Adicione pelo menos um item na solicitação.");
    }

    var ultimaSolicitacao = await db.Solicitacoes
        .OrderByDescending(x => x.Numero)
        .FirstOrDefaultAsync();

    var ultimoNumero = ultimaSolicitacao == null
        ? 0
        : ultimaSolicitacao.Numero;

    var solicitacao = new Solicitacao
    {
        Numero = ultimoNumero + 1,

        Setor = dto.Setor,

        Solicitante = dto.Solicitante,

        CentroCusto = dto.CentroCusto,

        Prioridade = dto.Prioridade,

        Observacao = dto.Observacao,

        DataSolicitacao = DateTime.Now,

        Status = "Pendente"
    };

    foreach (var item in dto.Itens)
    {
        var produto = await db.Produtos
            .FirstOrDefaultAsync(x => x.Id == item.ProdutoId);

        if (produto == null)
        {
            return Results.BadRequest(
                $"Produto ID {item.ProdutoId} não encontrado.");
        }

        if (produto.QuantidadeAtual < item.Quantidade)
        {
            return Results.BadRequest(
                $"Estoque insuficiente para {produto.Descricao}.");
        }

        produto.QuantidadeAtual -= item.Quantidade;

        solicitacao.Itens.Add(new SolicitacaoItem
        {
            ProdutoId = produto.Id,

            Quantidade = item.Quantidade
        });
    }

    db.Solicitacoes.Add(solicitacao);

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        solicitacao.Id,
        solicitacao.Numero,
        solicitacao.Status,
        solicitacao.DataSolicitacao
    });
});


// Buscar solicitação por ID
app.MapGet("/solicitacoes/{id}", async (int id, AppDbContext db) =>
{
    var solicitacao = await db.Solicitacoes

        .Where(x => x.Id == id)

        .Select(x => new
        {
            x.Id,
            x.Numero,
            x.Setor,
            x.Solicitante,
            x.CentroCusto,
            x.Prioridade,
            x.Observacao,
            x.Status,
            x.DataSolicitacao,

            Itens = x.Itens.Select(i => new
            {
                i.Id,
                i.ProdutoId,
                Codigo = i.Produto.Codigo,
                Produto = i.Produto.Descricao,
                Categoria = i.Produto.Categoria,
                Unidade = i.Produto.Unidade,
                i.Quantidade
            })
        })

        .FirstOrDefaultAsync();

    if (solicitacao == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(solicitacao);
});


// Listar histórico de solicitações
app.MapGet("/solicitacoes", async (AppDbContext db) =>
{
    var lista = await db.Solicitacoes

        .OrderByDescending(x => x.DataSolicitacao)

        .Select(x => new
        {
            x.Id,
            x.Numero,
            x.Setor,
            x.Solicitante,
            x.Status,
            x.DataSolicitacao,
            QuantidadeItens = x.Itens.Count
        })

        .ToListAsync();

    return Results.Ok(lista);
});


// Concluir solicitação
app.MapPut("/solicitacoes/{id}/concluir", async (int id, AppDbContext db) =>
{
    var solicitacao = await db.Solicitacoes
        .FirstOrDefaultAsync(x => x.Id == id);

    if (solicitacao == null)
    {
        return Results.NotFound("Solicitação não encontrada.");
    }

    if (solicitacao.Status == "Concluído")
    {
        return Results.BadRequest("Solicitação já está concluída.");
    }

    solicitacao.Status = "Concluído";

    await db.SaveChangesAsync();

    return Results.Ok(new
    {
        solicitacao.Id,
        solicitacao.Numero,
        solicitacao.Status
    });
});


app.MapGet("/dashboard/resumo", async (AppDbContext db) =>
{
    var usuarios = await db.Users.CountAsync();

    var produtos = await db.Produtos.CountAsync();

    var estoque = await db.Produtos
        .SumAsync(x => x.QuantidadeAtual);

    var solicitacoes = await db.Solicitacoes.CountAsync();

    return Results.Ok(new
    {
        usuarios,
        produtos,
        estoque,
        solicitacoes
    });
});

app.Run();