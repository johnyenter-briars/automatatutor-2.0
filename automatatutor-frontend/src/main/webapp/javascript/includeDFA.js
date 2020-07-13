var Editor = {
  curConfig: {
    dimensions: [740,480]
  }
};

function initCanvas() {
  if(Editor.canvas)
    return;
  Editor.canvas = new BlockCanvas("#svgcanvasdfa", Editor.curConfig.dimensions, true, false, false);
}

$(document).ready(function() {
  initCanvas();
}); 

