function filterTableRows(tableId, columnName, inputId){
    var table = document.getElementById(tableId);
    var input = document.getElementById(inputId);
    
    var filterString = input.value.toLowerCase();

    var rows = table.rows;
    
    var columnIndex = -1;

    Array.from(rows[0].cells).forEach((header, index) => {
        console.log(header.textContent.trim().length)
        console.log(header.textContent.trim())
        console.log(columnName.length)
        console.log(columnName)
    
        if(header.innerText.includes(columnName)){
            columnIndex = index
        }
    });

    var relevantRows = Array.from(rows).slice(1);
    for(var row of relevantRows){
        var cells = Array.from(row.cells);
        var targetCell = cells[columnIndex];
        var targetCellString = targetCell.innerText.toLowerCase().trim();
        
        if(targetCellString.includes(filterString)){
            row.style.display = 'table-row';
        }
        else{
            row.style.display = 'none'
        }
    }
}