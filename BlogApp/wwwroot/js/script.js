// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// nejde to z nejakeho dovodu
document.querySelector('form').addEventListener('submit', function (event) {
    let isValid = true; 
    let inputs = document.querySelectorAll('input');

    inputs.forEach(function (input) {
        if (input.value.trim() === '') {
            isValid = false;
            alert('Prosím vyplňte všetky polia');
        }
    });

    //ak nieje vyplnene vsetko
    if (!isValid) {
        event.preventDefault(); 
    }
});