var Editor = {
  curConfig: {
    dimensions: [440,480]
  },
  curConfigSmall: {
      dimensions: [330, 240]
  }
};

function initCanvases() {

  if(Editor.canvases)
    return;

  Editor.canvases = [];
  if(document.getElementById('numberOfAutomataField'))
    setNumberOfCanvases();
}

function _setNumberOfCanvases(n) {
  var canvasContainers = "";
  for(var i = 0; i < n; i++) {
    canvasContainers += "<div id='svgcanvasdfano" + i + "' class='svgcanvas'></div>";
  }
  document.getElementById('workarea').innerHTML = canvasContainers;
  Editor.canvases = [];
  for(var i = 0; i < n; i++) {
      Editor.canvases.push(new $.SvgCanvas('#svgcanvasdfano' + i, Editor.curConfig, 'detaut'));
  }
}

function setNumberOfCanvases() {
    var n = document.getElementById('numberOfAutomataField').value;
    if(n >= 1) {
        n = Math.floor(n);
        _setNumberOfCanvases(n);
        document.getElementById('numberOfAutomataField').value = n;
        setAlphabet();
    }
}

function exportAutomataList() {
    var automataList = "";
    for(var i = 0; i < Editor.canvases.length; i++) {
        automataList += Editor.canvases[i].exportAutomaton() + "\n";
    }
    return "<AutomataList>\n" + automataList + "</AutomataList>\n";
}

function setAutomata(xml) {
    var parser = new DOMParser();
    var xmlDoc = parser.parseFromString(xml, "text/xml");
    var automata = xmlDoc.getElementsByTagName("automaton");

    var n = automata.length;
    document.getElementById('numberOfAutomataField').value = n;
    _setNumberOfCanvases(n);

    for(var i = 0; i < n; i++) {
        Editor.canvases[i].setAutomatonFromParsedXmlDoc(automata[i]);
    }
}

function setupSolve(xml) {
    var parser = new DOMParser();
    var xmlDoc = parser.parseFromString(xml, 'text/xml');
    var automata = xmlDoc.getElementsByTagName('automaton');
    var n = automata.length;

    var canvasContainers = "<div>";
    for(var i = 0; i < n; i++) {
        canvasContainers += "<span id='svgcanvasdfano" + i + "' class='svgcanvas'></span>";
    }
    canvasContainers += "</div><div id='svgcanvassolution' class='svgcanvas'>";
    document.getElementById('workarea').innerHTML = canvasContainers;

    for(var i = 0; i < n; i++) {
        Editor.canvases.push(new $.SvgCanvas('#svgcanvasdfano' + i, Editor.curConfigSmall, 'detaut'));
        Editor.canvases[i].setAlphabet([]);
    }
    Editor.canvassolution = new $.SvgCanvas('#svgcanvassolution', Editor.curConfig, 'detaut');

    for(var i = 0; i < n; i++) {
        Editor.canvases[i].setAutomatonFromParsedXmlDoc(automata[i]);
        Editor.canvases[i].lockCanvas();
    }

    var alph = [];
    for(var i = 0; i < n; i++) {
        var alph_i = Editor.canvases[i].getAlphabet();
        for(var j = 0; j < alph_i.length; j++) {
            if(alph.indexOf(alph_i[j]) == -1)
                alph.push(alph_i[j]);
        }
    }
    Editor.canvassolution.setAlphabet(alph);
}

$(document).ready(function() {
  initCanvases();
}); 

