function logout()
{
    window.location.href =
        "/index.html";
}

window.onload = function()
{
    const hoje = new Date();

    const dataFormatada =
        hoje.toLocaleDateString(
            "pt-BR");

    const elementoData =
        document.getElementById(
            "dataAtual");

    if(elementoData)
    {
        elementoData.textContent =
            dataFormatada;
    }
}
async function carregarResumoDashboard()
{
    const response =
        await fetch("/dashboard/resumo");

    const dados =
        await response.json();

    document.getElementById("totalUsuarios").innerText =
        dados.usuarios;

    document.getElementById("totalSolicitacoes").innerText =
        dados.solicitacoes;

    document.getElementById("totalEstoque").innerText =
        dados.estoque;

    document.getElementById("totalEntradas").innerText =
        0;
}

window.onload = () =>
{
    carregarResumoDashboard();
};