var Editor = {
  curConfig: {
    dimensions: [740,480]
  }
};

function initCanvas() {
  if(Editor.canvas)
    return;
    Editor.canvas = new BlockCanvas("#svgcanvasnfa", Editor.curConfig.dimensions, false, true, false);
}

$(document).ready(function() {
  initCanvas();
}); 

