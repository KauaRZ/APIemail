
// VARIÁVEIS GLOBAIS


let produtos = [];

let produtoSelecionado = null;

let itensSolicitacao = [];



// INICIALIZAÇÃO


document.addEventListener("DOMContentLoaded", () => {

    configurarEventos();

    carregarProdutos();

});



// EVENTOS


function configurarEventos() {

    document
        .getElementById("btnNovaSolicitacao")
        .addEventListener("click", abrirModal);

    document
        .getElementById("btnFecharModal")
        .addEventListener("click", fecharModal);

    document
        .getElementById("btnCancelar")
        .addEventListener("click", fecharModal);

    document
        .getElementById("btnAdicionarItem")
        .addEventListener("click", adicionarItem);

    document
        .getElementById("pesquisaProduto")
        .addEventListener("input", pesquisarProduto);

}



// MODAL


function abrirModal() {

    document
        .getElementById("modalSolicitacao")
        .style.display = "flex";

}

function fecharModal() {

    document
        .getElementById("modalSolicitacao")
        .style.display = "none";

}



// CARREGAR PRODUTOS


async function carregarProdutos() {

    try {

        const response = await fetch("/produtos");

        produtos = await response.json();

    }

    catch {

        mostrarToast("Erro ao carregar produtos.");

    }

}



// PESQUISA PRODUTO


function pesquisarProduto() {

    const texto =
        document
            .getElementById("pesquisaProduto")
            .value
            .toLowerCase();

    const lista =
        document
            .getElementById("listaProdutos");

    lista.innerHTML = "";

    if (texto.length < 2) {

        lista.style.display = "none";

        return;

    }

    const encontrados =

        produtos.filter(p =>

            p.codigo.toLowerCase().includes(texto)

            ||

            p.descricao.toLowerCase().includes(texto)

        );

    encontrados.forEach(produto => {

        const item = document.createElement("div");

        item.className = "item-produto";

        item.innerHTML =

            `<strong>${produto.codigo}</strong> - ${produto.descricao}`;

        item.addEventListener("click", () => {

            selecionarProduto(produto.id);

        });

        lista.appendChild(item);

    });

    lista.style.display = encontrados.length > 0 ? "block" : "none";

}



// SELECIONAR PRODUTO


function selecionarProduto(id) {

    produtoSelecionado =

        produtos.find(p => p.id == id);

    document
        .getElementById("pesquisaProduto")
        .value =

        produtoSelecionado.codigo

        + " - "

        + produtoSelecionado.descricao;

    document
        .getElementById("estoqueAtual")
        .value =

        produtoSelecionado.quantidadeAtual;

    document
        .getElementById("listaProdutos")
        .style.display = "none";

}



// ADICIONAR ITEM


function adicionarItem() {

    if (produtoSelecionado == null) {

        mostrarToast("Selecione um produto.");

        return;

    }

    const quantidade =

        parseInt(

            document
                .getElementById("quantidade")
                .value

        );

    if (!quantidade || quantidade <= 0) {

        mostrarToast("Informe uma quantidade válida.");

        return;

    }

    if (quantidade > produtoSelecionado.quantidadeAtual) {

        mostrarToast("Quantidade maior que o estoque.");

        return;

    }

    const existente =

        itensSolicitacao.find(

            x => x.id == produtoSelecionado.id

        );

    if (existente) {

        existente.quantidade += quantidade;

    }

    else {

        itensSolicitacao.push({

            id: produtoSelecionado.id,

            codigo: produtoSelecionado.codigo,

            descricao: produtoSelecionado.descricao,

            categoria: produtoSelecionado.categoria,

            estoque: produtoSelecionado.quantidadeAtual,

            quantidade: quantidade

        });

    }

    atualizarTabelaItens();

    limparProduto();

}



// TABELA


function atualizarTabelaItens() {

    const tbody =

        document.querySelector("#tabelaItens tbody");

    tbody.innerHTML = "";

    if (itensSolicitacao.length == 0) {

        tbody.innerHTML =

            `<tr>

                <td colspan="6" style="text-align:center">

                    Nenhum item adicionado.

                </td>

            </tr>`;

        return;

    }

    itensSolicitacao.forEach(item => {

        tbody.innerHTML +=

            `<tr>

                <td>${item.codigo}</td>

                <td>${item.descricao}</td>

                <td>${item.categoria}</td>

                <td>${item.estoque}</td>

                <td>${item.quantidade}</td>

                <td>

                    <button
                        class="btn-delete"

                        onclick="removerItem(${item.id})">

                        Remover

                    </button>

                </td>

            </tr>`;

    });

}



// REMOVER ITEM


function removerItem(id) {

    itensSolicitacao =

        itensSolicitacao.filter(

            x => x.id != id

        );

    atualizarTabelaItens();

}



// LIMPAR PRODUTO


function limparProduto() {

    produtoSelecionado = null;

    document.getElementById("pesquisaProduto").value = "";

    document.getElementById("estoqueAtual").value = "";

    document.getElementById("quantidade").value = "";

}



// TOAST


function mostrarToast(texto) {

    const toast =

        document.getElementById("toast");

    toast.innerText = texto;

    toast.classList.add("show");

    setTimeout(() => {

        toast.classList.remove("show");

    }, 3000);

}


// SALVAR SOLICITAÇÃO


document
    .getElementById("btnSalvarSolicitacao")
    .addEventListener("click", salvarSolicitacao);

async function salvarSolicitacao() {

    if (itensSolicitacao.length == 0) {

        mostrarToast("Adicione pelo menos um item.");

        return;

    }

    if (document.getElementById("setor").value == "") {

        mostrarToast("Informe o setor.");

        return;

    }

    mostrarLoader(true);

    const solicitacao = {

        setor:

            document.getElementById("setor").value,

        solicitante:

            document.getElementById("solicitante").value,

        centroCusto:

            document.getElementById("centroCusto").value,

        prioridade:

            document.getElementById("prioridade").value,

        observacao:

            document.getElementById("observacao").value,

        itens:

            itensSolicitacao.map(item => ({

                produtoId: item.id,

                quantidade: item.quantidade

            }))

    };

    try {

        const response = await fetch("/solicitacoes", {

            method: "POST",

            headers: {

                "Content-Type": "application/json"

            },

            body: JSON.stringify(solicitacao)

        });

          if (!response.ok) {

            const erro = await response.text();

            console.error(erro);

            mostrarToast(erro);

            mostrarLoader(false);

            return;

}

        mostrarToast("Solicitação salva com sucesso.");

        limparFormulario();

        fecharModal();

        carregarSolicitacoes();

    }

    catch {

        mostrarToast("Erro ao salvar solicitação.");

    }

    mostrarLoader(false);

}


// HISTÓRICO


async function carregarSolicitacoes() {

    const tbody =

        document.querySelector("#tabelaSolicitacoes tbody");

    tbody.innerHTML =

        `<tr>

            <td colspan="7">

                Carregando...

            </td>

        </tr>`;

    try {

        const response =

            await fetch("/solicitacoes");

        const lista =

            await response.json();

        tbody.innerHTML = "";

        if (lista.length == 0) {

            tbody.innerHTML =

                `<tr>

                    <td colspan="7">

                        Nenhuma solicitação encontrada.

                    </td>

                </tr>`;

            return;

        }

        lista.forEach(s => {

            tbody.innerHTML +=

            `<tr>

                <td>${s.numero}</td>

                <td>${new Date(s.dataSolicitacao).toLocaleDateString()}</td>

                <td>${s.setor}</td>

                <td>${s.solicitante}</td>

                <td>${s.quantidadeItens}</td>

                <td>${s.status}</td>

                <td>

                   <button
                     class="btn-edit"
                     onclick="visualizarSolicitacao(${s.id})">

                     Visualizar

                    </button>

                </td>

            </tr>`;

        });

    }

    catch {

        tbody.innerHTML =

        `<tr>

            <td colspan="7">

                Erro ao carregar histórico.

            </td>

        </tr>`;

    }

}


// PESQUISA


document
    .getElementById("pesquisaSolicitacao")
    .addEventListener("keyup", pesquisarSolicitacoes);

function pesquisarSolicitacoes() {

    const texto =

        document
            .getElementById("pesquisaSolicitacao")
            .value
            .toLowerCase();

    const linhas =

        document.querySelectorAll(

            "#tabelaSolicitacoes tbody tr"

        );

    linhas.forEach(linha => {

        linha.style.display =

            linha.innerText
                .toLowerCase()
                .includes(texto)

            ?

            ""

            :

            "none";

    });

}


// LOADER


function mostrarLoader(mostrar) {

    document
        .getElementById("loader")
        .style.display =

        mostrar

        ?

        "flex"

        :

        "none";

}


// LIMPAR

function limparFormulario() {

    document.getElementById("setor").value = "";

    document.getElementById("solicitante").value = "";

    document.getElementById("centroCusto").value = "";

    document.getElementById("prioridade").value = "Normal";

    document.getElementById("observacao").value = "";

    limparProduto();

    itensSolicitacao = [];

    atualizarTabelaItens();

}

document.addEventListener("DOMContentLoaded", () => {

    configurarEventos();

    carregarProdutos();

    carregarSolicitacoes();

});


async function visualizarSolicitacao(id)
{
    mostrarLoader(true);

    try
    {
        const response =
            await fetch(`/solicitacoes/${id}`);

        if (!response.ok)
        {
            mostrarToast("Erro ao buscar solicitação.");
            mostrarLoader(false);
            return;
        }

        const solicitacao =
            await response.json();

        let itensHtml = "";

        solicitacao.itens.forEach(item =>
        {
            itensHtml += `
                <tr>
                    <td>${item.codigo}</td>
                    <td>${item.produto}</td>
                    <td>${item.categoria}</td>
                    <td>${item.unidade}</td>
                    <td>${item.quantidade}</td>
                </tr>
            `;
        });

        const botaoConcluir =
            solicitacao.status === "Pendente"
            ? `
                <button
                    class="btn-edit"
                    onclick="concluirSolicitacao(${solicitacao.id})">

                    Concluir Solicitação

                </button>
              `
            : "";

        const conteudo = `
            <h2>Solicitação Nº ${solicitacao.numero}</h2>

            <p><strong>Data:</strong> ${new Date(solicitacao.dataSolicitacao).toLocaleDateString()}</p>
            <p><strong>Setor:</strong> ${solicitacao.setor}</p>
            <p><strong>Solicitante:</strong> ${solicitacao.solicitante}</p>
            <p><strong>Centro de Custo:</strong> ${solicitacao.centroCusto}</p>
            <p><strong>Prioridade:</strong> ${solicitacao.prioridade}</p>
            <p><strong>Status:</strong> ${solicitacao.status}</p>
            <p><strong>Observação:</strong> ${solicitacao.observacao || "Sem observação"}</p>

            <br>

            <h3>Itens da Solicitação</h3>

            <table class="users-table">
                <thead>
                    <tr>
                        <th>Código</th>
                        <th>Produto</th>
                        <th>Categoria</th>
                        <th>Unidade</th>
                        <th>Quantidade</th>
                    </tr>
                </thead>

                <tbody>
                    ${itensHtml}
                </tbody>
            </table>

            <br>

            <div style="text-align:right;">

                ${botaoConcluir}

                <button
                    class="btn-delete"
                    onclick="fecharVisualizacao()">

                    Fechar

                </button>

            </div>
        `;

        document.getElementById("modalVisualizarBody").innerHTML =
            conteudo;

        document.getElementById("modalVisualizar").style.display =
            "flex";
    }
    catch
    {
        mostrarToast("Erro ao visualizar solicitação.");
    }

    mostrarLoader(false);
}


function fecharVisualizacao()
{
    document.getElementById("modalVisualizar").style.display =
        "none";
}

async function concluirSolicitacao(id)
{
    if (!confirm("Deseja concluir esta solicitação?"))
        return;

    const response = await fetch(`/solicitacoes/${id}/concluir`,
    {
        method: "PUT"
    });

    if (response.ok)
    {
        mostrarToast("Solicitação concluída com sucesso.");

        fecharVisualizacao();

        carregarSolicitacoes();
    }
    else
    {
        const erro = await response.text();

        console.error(erro);

        mostrarToast(erro || "Erro ao concluir solicitação.");
    }
}