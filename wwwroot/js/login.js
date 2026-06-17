async function login()
{
    const email =
        document.getElementById("email").value;

    const senha =
        document.getElementById("senha").value;

    // Validação dos campos
    if(!email || !senha)
    {
        alert("Preencha email e senha.");

        return;
    }

    try
    {
        const response =
            await fetch("/login",
            {
                method:"POST",

                headers:
                {
                    "Content-Type":"application/json"
                },

                body:JSON.stringify(
                {
                    email,
                    senha
                })
            });

        if(response.ok)
        {
            window.location.href =
                "/dashboard.html";
        }
        else
        {
            alert(
                "Email ou senha inválidos");
        }
    }
    catch(error)
    {
        console.error(error);

        alert(
            "Erro ao conectar com a API.");
    }
}