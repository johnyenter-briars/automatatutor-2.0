var Editor = {
    curConfig: {
        dimensions: [750,480]
    }
};

function initCanvas() {
    if(Editor.canvas)
        return;
    Editor.canvas = new BlockCanvas("#svgcanvasblock", Editor.curConfig.dimensions);
}

$(document).ready(function() {
    initCanvas();
});

