function filterTableRows(tableId, columnName, inputId){
    var table = document.getElementById(tableId);
    var input = document.getElementById(inputId);
    
    var filterString = input.value.toLowerCase();
    console.log(filterString)
    var rows = table.rows;
    
    var columnIndex = -1;

    Array.from(rows[0].cells).forEach((header, index) => {
        if(header.innerHTML.trim() == columnName){
            columnIndex = index
        }
    });

    var relevantRows = Array.from(rows).slice(1);
    
    for(var row of relevantRows){
        var cells = Array.from(row.cells);
        var targetCell = cells[columnIndex];
        var targetCellString = targetCell.innerHTML.toLowerCase().trim();
        if(targetCellString.includes(filterString)){
            row.style.display = 'table-row';
        }
        else{
            row.style.display = 'none'
        }
    }
}

function collapseCell(e){
    var targetCell = e.target;

    var children = Array.from(targetCell.children);
    children[1].style.display = "block";
    children.slice(2).forEach((link, index) => {
        link.style.display = "none";
    })

    targetCell.style.display = "flex";
}

function expandCell(e){
    var ellipsis = e.target;

    ellipsis.style.display = "none";

    var parentCell = e.target.parentElement;

    parentCell.style.display = "grid";
    
    var children = Array.from(parentCell.children)

    children.slice(2).forEach((link, index) => {
        link.style.display = "inline";
    })

    //give the click enough time to not be registered anymore
    //otherwise the collapseCell function will fire on ever click
    setTimeout(() => {  
        parentCell.onclick = collapseCell;
    }, 10);
    
}

Array.from(document.getElementsByTagName("td")).forEach(td => {
    var firstATag = Array.from(td.children).find((child, index) => {
        return child.nodeName == "A";
    });

    var cellChildren = Array.from(td.children);

    if(!firstATag || !(cellChildren.length > 1))
        return;

    td.style.display = "flex";

    var para = document.createElement("p");
    var node = document.createTextNode(". . .");
    para.style.fontSize = "15px";
    
    para.appendChild(node);
    
    para.onclick = expandCell;


    td.insertBefore(para, cellChildren[1]);
    
    Array.from(td.children).slice(2).forEach((element, index) => {
        element.style.display = "none";
    })
});

var tables = Array.from(document.getElementsByTagName('table'));

for(var table of tables){
    try{
        var _ = new Tablesort(table);
    }
    catch(e){
        console.log(e);
    }
    
}

//All links that have a special class name also need their parent (the table cell) to be clickable as well
Array.from(document.getElementsByClassName("folder-link")).forEach(td => {
    td.parentElement.style.cursor = "pointer";
    td.parentElement.onclick = () => window.location.href = td.href;
});

//All links that have a special class name also need their parent (the table cell) to be clickable as well
Array.from(document.getElementsByClassName("course-link")).forEach(td => {
    td.parentElement.style.cursor = "pointer";
    td.parentElement.onclick = () => window.location.href = td.href;
});