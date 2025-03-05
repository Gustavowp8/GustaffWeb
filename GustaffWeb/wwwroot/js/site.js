const btnMobile = document.getElementById('btn-mobile')

function toggleMenu() {
    const nav = document.getElementById('nav');
    nav.classList.toggle('active');
}

btnMobile.addEventListener('click', toggleMenu);

function chamaWhast() {
    location = 'https://wa.me/556135752752?text=Ol%C3%A1%2C+preciso+esclarecer+uma+duvida'
}

function aviso() {
    alert('Indisponível no momento!');
}

function chamaNoWhast() {
    var resultado = confirm('Vamos direcionar você para o nosso WhatsApp, lá você tera mais detalhes sobre este serviço.');
    if (resultado) {
        location = 'https://wa.me/556135752752?text=Ol%C3%A1%2C+preciso+esclarecer+uma+duvida';
    } else {
        alert('OK vamos continuar aqui');
    }
}