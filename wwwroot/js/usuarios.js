let emailSelecionado = "";

function mostrarToast(mensagem)
{
    const toast =
        document.getElementById(
            "toast");

    toast.innerText =
        mensagem;

    toast.classList.add(
        "show");

    setTimeout(() =>
    {
        toast.classList.remove(
            "show");

    }, 3000);
}

async function carregarUsuarios()
{
    const response =
        await fetch("/users");

    const usuarios =
        await response.json();

    const tbody =
        document.querySelector(
            "#tabelaUsuarios tbody");

    tbody.innerHTML = "";

    usuarios.forEach(usuario =>
    {
        tbody.innerHTML += `
        <tr>

            <td>${usuario.email}</td>

            <td>

                <button
                    class="btn-edit"
                    onclick="alterarSenha('${usuario.email}')">

                    Alterar Senha

                </button>

                <button
                    class="btn-delete"
                    onclick="excluirUsuario('${usuario.email}')">

                    Excluir

                </button>

            </td>

        </tr>`;
    });
}

/* NOVO USUÁRIO*/

function novoUsuario()
{
    document
        .getElementById("modalNovoUsuario")
        .style.display = "flex";
}

function fecharModalNovoUsuario()
{
    document
        .getElementById("modalNovoUsuario")
        .style.display = "none";
}

async function salvarNovoUsuario()
{
    const email =
        document
            .getElementById("novoEmail")
            .value
            .trim();

    const senha =
        document
            .getElementById("novaSenha")
            .value
            .trim();

    if(!email || !senha)
    {
        mostrarToast(
            "❌ Preencha todos os campos");

        return;
    }

    const response =
        await fetch("/register",
        {
            method:"POST",

            headers:
            {
                "Content-Type":
                "application/json"
            },

            body:JSON.stringify(
            {
                email,
                senha
            })
        });

    if(response.ok)
    {
        mostrarToast(
            "✅ Usuário cadastrado");

        fecharModalNovoUsuario();

        document
            .getElementById("novoEmail")
            .value = "";

        document
            .getElementById("novaSenha")
            .value = "";

        carregarUsuarios();
    }
    else
    {
        const erro =
            await response.text();

        mostrarToast(
            "❌ " + erro);
    }
}

/* ALTERAR SENHA*/

function alterarSenha(email)
{
    emailSelecionado = email;

    document
        .getElementById("usuarioSelecionado")
        .innerText = email;

    document
        .getElementById("modalAlterarSenha")
        .style.display = "flex";
}

function fecharModalAlterarSenha()
{
    document
        .getElementById("modalAlterarSenha")
        .style.display = "none";
}

async function salvarNovaSenha()
{
    const novaSenha =
        document
            .getElementById("senhaAlterada")
            .value
            .trim();

    if(!novaSenha)
    {
        mostrarToast(
            "❌ Informe uma senha");

        return;
    }

    const response =
        await fetch("/trocar-senha",
        {
            method:"PUT",

            headers:
            {
                "Content-Type":
                "application/json"
            },

            body:JSON.stringify(
            {
                email: emailSelecionado,
                novaSenha
            })
        });

    if(response.ok)
    {
        mostrarToast(
            "🔒 Senha alterada");

        fecharModalAlterarSenha();

        document
            .getElementById("senhaAlterada")
            .value = "";

        carregarUsuarios();
    }
    else
    {
        mostrarToast(
            "❌ Erro ao alterar senha");
    }
}

/* EXCLUIR USUÁRIO */

function excluirUsuario(email)
{
    emailSelecionado = email;

    document
        .getElementById("usuarioExcluir")
        .innerText =
        `Deseja excluir ${email}?`;

    document
        .getElementById("modalExcluir")
        .style.display = "flex";
}

function fecharModalExcluir()
{
    document
        .getElementById("modalExcluir")
        .style.display = "none";
}

async function confirmarExclusao()
{
    const response =
        await fetch(
            `/users/${emailSelecionado}`,
            {
                method:"DELETE"
            });

    if(response.ok)
    {
        mostrarToast(
            "🗑️ Usuário removido");

        fecharModalExcluir();

        carregarUsuarios();
    }
    else
    {
        mostrarToast(
            "❌ Erro ao remover usuário");
    }
}

window.onload =
    carregarUsuarios;