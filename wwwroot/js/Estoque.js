document
.getElementById("pesquisa")
.addEventListener("input", pesquisarProduto);

async function carregarEstoque()
{
    const response = await fetch("/produtos");

    const produtos = await response.json();

    preencherTabela(produtos);
}

function preencherTabela(produtos)
{
    const tbody =
        document.querySelector("#tabelaEstoque tbody");

    tbody.innerHTML = "";

    if(produtos.length == 0)
    {
        tbody.innerHTML = `
        <tr>
            <td colspan="6" style="text-align:center;">
                Nenhum produto encontrado
            </td>
        </tr>`;

        return;
    }

    produtos.forEach(produto =>
    {
        const status =
            produto.quantidadeAtual <= produto.estoqueMinimo
                ? "🔴 Baixo"
                : "🟢 Normal";

        tbody.innerHTML += `
        <tr>

            <td>${produto.codigo}</td>

            <td>${produto.descricao}</td>

            <td>${produto.categoria}</td>

            <td>${produto.quantidadeAtual}</td>

            <td>${produto.estoqueMinimo}</td>

            <td>${status}</td>

        </tr>`;
    });
}


async function pesquisarProduto()
{
    const texto =
        document
            .getElementById("pesquisa")
            .value
            .toLowerCase();

    const response =
        await fetch("/produtos");

    const produtos =
        await response.json();

    const filtrados =
        produtos.filter(p =>

            p.codigo.toLowerCase().includes(texto)

            ||

            p.descricao.toLowerCase().includes(texto)
        );

    preencherTabela(filtrados);
}


window.onload = () =>
{
    carregarEstoque();
};