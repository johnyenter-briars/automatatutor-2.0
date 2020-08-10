function initializeModal(){
    var modals = document.getElementsByClassName("modal")
    if(modals.length == 0) return;
    var modal= modals[0]

    var btn = document.getElementsByClassName("modal-button")[0];
    
    var span = document.getElementsByClassName("close")[0];


    btn.onclick = () => {
        modal.style.display = "block";
    }

    span.onclick = () => {
        modal.style.display = "none";
    }

    window.onclick = (event) => {
        if (event.target == modal) {
            modal.style.display = "none";
        }
    }
}

function initializeTree(){
    var togglers = document.getElementsByClassName("caret");

    for (var i = 0; i < togglers.length; i++) {
        togglers[i].addEventListener("click", function(){
            this.parentElement.querySelector(".nested").classList.toggle("active");
            this.classList.toggle("caret-down");
        });
    }
}

initializeModal();

initializeTree();
