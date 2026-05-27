async function login(){

    const email =
        document.getElementById("email").value;

    const senha =
        document.getElementById("senha").value;

    const response =
        await fetch("/login",{

        method:"POST",

        headers:{
            "Content-Type":"application/json"
        },

        body:JSON.stringify({
            email,
            senha
        })
    });

    const data = await response.text();

    alert(data);
}