//Tahiti
var Editor = {
  curConfig: {
    dimensions: [740,480]
  }
};
  
function initCanvas() {
  if(Editor.canvas)
    return;
    Editor.canvas = new $.SvgCanvas("#svgcanvastm", Editor.curConfig, 'tmaut'); 
    //Editor.canvas.setNumberOfTapes(3);

}
  
$(document).ready(function() {
  initCanvas();
});