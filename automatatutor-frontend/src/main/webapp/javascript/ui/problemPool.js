//Loving lifted from https://www.w3schools.com/howto/howto_js_sort_table.asp
function sortTable(columnNumber, tableId) {
    var rows, rowIndex, switchcount = 0;
    var shouldSwitch = false
    var table = document.getElementById(tableId);
    
    var switching = true;
    
    var direction = "asc";
    
    while (switching) {
        switching = false;
        rows = table.rows;
        
        /* Loop through all table rows (except the
        first, which contains table headers): */
        for (rowIndex = 1; rowIndex < (rows.length - 1); rowIndex++) {
            shouldSwitch = false;
            
            var eleOne = rows[rowIndex].getElementsByTagName("TD")[columnNumber];
            var eleTwo = rows[rowIndex + 1].getElementsByTagName("TD")[columnNumber];
            
            if ((direction == "asc" && (eleOne.innerHTML.toLowerCase() > eleTwo.innerHTML.toLowerCase())) 
            || (direction == "desc" && (eleOne.innerHTML.toLowerCase() < eleTwo.innerHTML.toLowerCase())))  {
                shouldSwitch = true;
                break;
            } 
        }
        if (shouldSwitch) {
            rows[rowIndex].parentNode.insertBefore(rows[rowIndex + 1], rows[rowIndex]);
            switching = true;
            
            switchcount++;
        } else if (switchcount == 0 && direction == "asc") {
            direction = "desc";
            switching = true;
        }
    }
}