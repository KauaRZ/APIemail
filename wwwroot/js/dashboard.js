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