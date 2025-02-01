const btnMobile = document.getElementById('btn-mobile')

function toggleMenu() {
    const nav = document.getElementById('nav');
    nav.classList.toggle('active');
}

btnMobile.addEventListener('click', toggleMenu);

function chamaWhast() {
    location = 'https://wa.me/556135752752?text=Ol%C3%A1%2C+preciso+esclarecer+uma+duvida'
}