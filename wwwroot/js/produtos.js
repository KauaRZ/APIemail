function mostrarToast(mensagem)
{
    const toast =
        document.getElementById("toast");

    toast.innerText = mensagem;

    toast.classList.add("show");

    setTimeout(() =>
    {
        toast.classList.remove("show");
    }, 3000);
}

//Limpar Formulario 

function limparFormulario()
{
    document.getElementById("codigo").value = "";
    document.getElementById("descricao").value = "";
    document.getElementById("categoria").value = "";
    document.getElementById("unidade").value = "";
    document.getElementById("fornecedor").value = "";
    document.getElementById("estoqueMinimo").value = "";
    document.getElementById("observacoes").value = "";
}

//função salvar produto
async function carregarProdutos()
{
    const response =
        await fetch("/produtos");

    const produtos =
        await response.json();

    const tbody =
        document.querySelector(
            "#tabelaProdutos tbody");

    tbody.innerHTML = "";

    produtos.forEach(produto =>
    {
        tbody.innerHTML += `
        <tr>

            <td>${produto.codigo}</td>

            <td>${produto.descricao}</td>

            <td>${produto.categoria}</td>

            <td>${produto.unidade}</td>

            <td>${produto.fornecedor}</td>

            <td>${produto.estoqueMinimo}</td>

            <td>

                <button
                    class="btn-edit"
                    onclick="editarProduto(${produto.id})">

                    Editar

                </button>

                <button
                    class="btn-delete"
                    onclick="excluirProduto(${produto.id})">

                    Excluir

                </button>

            </td>

        </tr>`;
    });
}

async function salvarProduto()
{
    const produto =
    {
        codigo:
            document.getElementById("codigo").value,

        descricao:
            document.getElementById("descricao").value,

        categoria:
            document.getElementById("categoria").value,

        unidade:
            document.getElementById("unidade").value,

        fornecedor:
            document.getElementById("fornecedor").value,

        estoqueMinimo:
            parseInt(
                document.getElementById(
                    "estoqueMinimo").value),

        observacao:
            document.getElementById(
                "observacao").value,

        quantidadeAtual: 0
    };

    const response =
        await fetch("/produtos",
        {
            method: "POST",

            headers:
            {
                "Content-Type":
                "application/json"
            },

            body: JSON.stringify(produto)
        });

    if(response.ok)
    {
        alert(
            "Produto cadastrado com sucesso!");

        carregarProdutos();
    }
    else
{
    const erro =
        await response.text();

    alert(
        "Erro: " + erro);
}
}


async function excluirProduto(id)
{
    alert("ID recebido: " + id);

    if (!confirm("Deseja realmente excluir este produto?"))
        return;

    const response = await fetch(`/produtos/${id}`,
    {
        method: "DELETE"
    });

    if (response.ok)
    {
        mostrarToast("Produto excluído com sucesso.");
        carregarProdutos();
    }
    else
    {
        mostrarToast("Erro ao excluir produto.");
    }
}

    window.onload = () =>
{
    carregarProdutos();
};

