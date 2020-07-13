//Tahiti
var Editor = {
  curConfig: {
    dimensions: [740,480]
  },
  curConfigTapes: {
    dimensions: [740,240]
  }
};
  
function initCanvas() {
  if(!Editor.canvas){
    Editor.canvas = new $.SvgCanvas("#svgcanvastm", Editor.curConfig, 'tmaut');
  }
  if(!Editor.canvasTapes) {
    Editor.canvasTapes = new $.SvgCanvas("#svgcanvastapes", Editor.curConfigTapes, 'tmaut');
    //Editor.canvas.setNumberOfTapes(1);
    //Editor.canvasTapes.createTMTapes();
  }
}
  
$(document).ready(function() {
  initCanvas();
});