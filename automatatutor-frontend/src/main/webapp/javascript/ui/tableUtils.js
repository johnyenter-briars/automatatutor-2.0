function filterTableRows(tableId, columnName, inputId){
    var table = document.getElementById(tableId);
    var input = document.getElementById(inputId);
    
    var filterString = input.value.toLowerCase();

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

function expandOrCollapseHiddenText(tableCell){
    tableCell.style.overflow = tableCell.style.overflow === "hidden" || 
                                tableCell.style.overflow === "" ? "visible" : "hidden";

    tableCell.style.whiteSpace = tableCell.style.whiteSpace === "nowrap" || 
                                    tableCell.style.whiteSpace === "" ? "pre-wrap" : "nowrap";

    tableCell.style.display = tableCell.style.display === "table-cell" ||
                                tableCell.style.display === "" ? "grid" : "table-cell"
}

Array.from(document.getElementsByTagName("td")).forEach(td => {
    td.onclick = (e) => {
        expandOrCollapseHiddenText(e.target);
    }
})

var tables = Array.from(document.getElementsByTagName('table'));

for(var table of tables){
    var _ = new Tablesort(table);
}

var cells = Array.from(document.getElementsByTagName("TD"));

for(var cell of cells){
    for(var child of cell.children){
        if(child.nodeName == "A")
            cell.style.cursor = "pointer";
    }
}