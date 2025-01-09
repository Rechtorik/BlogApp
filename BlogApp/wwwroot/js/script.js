// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// nejde to z nejakeho dovodu
document.querySelector('form').addEventListener('submit', function (event) {
    console.log("zacala");
    let isValid = true;
    let isValid = true;
    let inputs = document.querySelectorAll('input');

    inputs.forEach(function (input) {
        if (input.value.trim() === '') {
            isValid = false;
            console.log("tu je chyba");
            alert('Všetky polia musia byť vyplnené!');
        }
    });

    //ak nieje vyplnene vsetko
    if (!isValid) {
        event.preventDefault();
    }
});

function Back() {
    history.back();
}