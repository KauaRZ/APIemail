using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
options.UseSqlServer(
builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserRepository>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Digite: Bearer SEU_TOKEN"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters =
        new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer =
                builder.Configuration["Jwt:Issuer"],

            ValidAudience =
                builder.Configuration["Jwt:Audience"],

            IssuerSigningKey =
                new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(
                        builder.Configuration["Jwt:Key"]))
        };
});

builder.Services.AddAuthorization();


var app = builder.Build();


    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseStaticFiles();

   app.MapGet("/", () =>
    Results.Redirect("/index.html"));



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
    Email = user.Email
};

return Results.Created($"/users/{user.Id}", dadosSegurosDoUsuario);
     
       
});


// Login
app.MapPost("/login", async (
    UserRepository repository,
    AuthRequest request,
    IConfiguration configuration)=>
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

    var claims = new[]
    {
        new Claim(
            ClaimTypes.Email,
            usuario.Email)
    };

    var key =
        new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                configuration["Jwt:Key"]));

    var creds =
        new SigningCredentials(
            key,
            SecurityAlgorithms.HmacSha256);

    var token =
        new JwtSecurityToken(
            issuer:
                configuration["Jwt:Issuer"],

            audience:
                configuration["Jwt:Audience"],

            claims: claims,

            expires:
                DateTime.Now.AddHours(1),

            signingCredentials: creds);

    var tokenString =
        new JwtSecurityTokenHandler()
            .WriteToken(token);

    return Results.Ok(new
    {
        token = tokenString
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
})
.RequireAuthorization();


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
})
.RequireAuthorization();

app.Run();