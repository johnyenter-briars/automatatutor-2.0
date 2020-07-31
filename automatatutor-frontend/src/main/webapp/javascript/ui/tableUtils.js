// //Lovingly lifted from https://www.w3schools.com/howto/howto_js_sort_table.asp
// function sortTable(columnNumber, tableId) {
//     var rows, rowIndex, switchCount = 0;
//     var shouldSwitch = false
//     var table = document.getElementById(tableId);
    
//     var switching = true;
    
//     var direction = "asc";
    
//     while (switching) {
//         switching = false;
//         rows = table.rows;
        
//         /* Loop through all table rows (except the
//         first, which contains table headers): */
//         for (rowIndex = 1; rowIndex < (rows.length - 1); rowIndex++) {
//             shouldSwitch = false;
            
//             var eleOne = rows[rowIndex].getElementsByTagName("TD")[columnNumber];
//             var eleTwo = rows[rowIndex + 1].getElementsByTagName("TD")[columnNumber];
            
//             if ((direction == "asc" && (eleOne.innerHTML.toLowerCase() > eleTwo.innerHTML.toLowerCase())) 
//             || (direction == "desc" && (eleOne.innerHTML.toLowerCase() < eleTwo.innerHTML.toLowerCase())))  {
//                 shouldSwitch = true;
//                 break;
//             } 
//         }
//         if (shouldSwitch) {
//             rows[rowIndex].parentNode.insertBefore(rows[rowIndex + 1], rows[rowIndex]);
//             switching = true;
            
//             switchCount++;
//         } else if (switchCount == 0 && direction == "asc") {
//             direction = "desc";
//             switching = true;
//         }
//     }
// }

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

function sortTable(columnNumber, tableId) {

}

// $('problemPoolTable').tablesort();

var x = new Tablesort(document.getElementById('problemPoolTable'));
console.log(x);