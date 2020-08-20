function openModal(e){
    var targetButtonId = e.target.id.split("_")[2];

    var modal = Array.from(document.getElementsByClassName("modal")).filter((modal, index) => {
        return modal.id.split("_")[2] === targetButtonId;
    })[0];

    modal.style.display = "block";
}

function closeModal(e){
    var targetButtonId = e.target.id.split("_")[2];

    var modal = Array.from(document.getElementsByClassName("modal")).filter((modal, index) => {
        return modal.id.split("_")[2] === targetButtonId;
    })[0];

    modal.style.display = "none";
}

function initializeModal(){
    var modals = document.getElementsByClassName("modal")
    
    if(modals.length == 0) return;

    var buttons = document.getElementsByClassName("modal-button");
    
    var spans = document.getElementsByClassName("close");


    for(var modal of modals){
        
        var modalProblemId = modal.id.split("_")[2];
        
        var connectedButton = Array.from(buttons).filter((button, index) => {
            return button.id.split("_")[2] === modalProblemId
        })[0]
    
        connectedButton.onclick = openModal;

        var connectedSpan = Array.from(spans).filter((span, index) => {
            return span.id.split("_")[2] === modalProblemId
        })[0]
        
        connectedSpan.onclick = closeModal;
    }

    window.onclick = (event) => {
        
        if (event.target.className === "modal") {
            event.target.style.display = "none";
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
