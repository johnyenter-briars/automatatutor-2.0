$('input').on('keydown', function(event) {
   var x = event.which;
   if (x === 13) {
       event.preventDefault();
   }
});

function splitSliderStartUpdate() {
    var splitslidermid = document.getElementById("splitslidermid");
    var splitsliderstart = document.getElementById("splitsliderstart");

    splitslidermid.min = (parseInt(splitsliderstart.value)+1)+"";

    updateSplits();
}

function splitSliderMidUpdate() {
    updateSplits();
}

function updateSplits()
{
    var splitdisplaystart = document.getElementById("splitdisplaystart");
    var splitdisplaymid = document.getElementById("splitdisplaymid");
    var splitdisplayend = document.getElementById("splitdisplayend");
    var word = document.getElementById("word").value;

    splitdisplaystart.innerHTML = word.substring(0, parseInt(splitsliderstart.value));
    splitdisplaymid.innerHTML = word.substring(parseInt(splitsliderstart.value), parseInt(splitslidermid.value));
    splitdisplayend.innerHTML = word.substring(parseInt(splitslidermid.value), word.length);
    var spans = document.getElementsByClassName("lettersstart");
    for (var i = 0; i < spans.length; i++)
    {
        spans[i].innerHTML = ""+splitdisplaystart.innerHTML.length;
    }
    spans = document.getElementsByClassName("lettersmid");
    for (var i = 0; i < spans.length; i++)
    {
        spans[i].innerHTML = ""+splitdisplaymid.innerHTML.length;
    }
    spans = document.getElementsByClassName("lettersend");
    for (var i = 0; i < spans.length; i++)
    {
        spans[i].innerHTML = ""+splitdisplayend.innerHTML.length;
    }

    if (splitdisplaystart.innerHTML == "") splitdisplaystart.innerHTML = "&epsilon;"
    if (splitdisplaymid.innerHTML == "") splitdisplaymid.innerHTML = "&epsilon;"
    if (splitdisplayend.innerHTML == "") splitdisplayend.innerHTML = "&epsilon;"
}

function collectData(strArr)
{
    var output = "";
    for (var i = 0; i<strArr.length; i++)
    {
        output = output + document.getElementById(strArr[i]).value + "#";
    }
    if (output != "")
    {
        output = output.substring(0, output.length-1);
    }
    return output;
}

function drawDFA(xmlString)
{
    Editor.canvasDfa.setAutomaton(xmlString);
}

function drawNFA(xmlString)
{
    Editor.canvasNfa.setAutomaton(xmlString);
}

function drawDFAUnlock(xmlString)
{
    Editor.canvasDfa.setAutomaton(xmlString);
    Editor.canvasDfa.unlockCanvas();
}

function onSwitch()
{
    var checkboxReg = document.getElementById("regularSwitch").getElementsByTagName("input")[0];
    var checkboxInput = document.getElementById("inputSwitch").getElementsByTagName("input")[0];

    var regText = document.getElementById("regularText");
    if (checkboxReg.checked)
        regText.innerHTML = "regular";
    else
        regText.innerHTML = "not pumpable";

    var nfaText = document.getElementById("nfaInputText");
    if (checkboxInput.checked)
        nfaText.innerHTML = "Input: Arithmetic Language";
    else
        nfaText.innerHTML = "Input: NFA";

    if (!checkboxReg.checked)
    {
        var trs = document.getElementsByClassName("bothField");
        for (var i = 0; i < trs.length; i++)
        {
            trs[i].removeAttribute("style");
        }
        trs = document.getElementsByClassName("regularField");
        for (var i = 0; i < trs.length; i++)
        {
            trs[i].style.display = "none";
        }
        trs = document.getElementsByClassName("nonRegularField");
        for (var i = 0; i < trs.length; i++)
        {
            trs[i].removeAttribute("style");
        }
    }
    else
    {
            var trs = document.getElementsByClassName("bothField");
            for (var i = 0; i < trs.length; i++)
            {
                trs[i].removeAttribute("style");
            }
            trs = document.getElementsByClassName("nonRegularField");
            for (var i = 0; i < trs.length; i++)
            {
                trs[i].style.display = "none";
            }
            trs = document.getElementsByClassName("regularField");
            for (var i = 0; i < trs.length; i++)
            {
                trs[i].removeAttribute("style");
            }
            trs = document.getElementsByClassName("NfaField");
            for (var i = 0; i < trs.length; i++)
            {
                if (checkboxInput.checked)
                    trs[i].style.display = "none";
                else
                    trs[i].removeAttribute("style");
            }
            trs = document.getElementsByClassName("nonNfaField");
            for (var i = 0; i < trs.length; i++)
            {
                if (checkboxInput.checked)
                    trs[i].removeAttribute("style");
                else
                    trs[i].style.display = "none";
            }
    }
}

function chooseRegular(regular)
{
    var btns = document.getElementsByClassName("regularchoice");
    for(var i = 0; i<btns.length; i++)
    {
        btns[i].disabled = true;
    }

    var regText = "";
    if(regular) regText = "regular";
    else regText = "not regular";
    document.getElementById("regularchoicetext").innerHTML = "You chose: "+regText;
}

function setPN(n)
{
    var spans = document.getElementsByClassName("pln");
    for (var i = 0; i<spans.length; i++)
    {
        spans[i].innerHTML = n+"";
    }

    //split sliders
    document.getElementById("splitsliderstart").max = (parseInt(n)-1)+"";
    document.getElementById("splitslidermid").max = n+"";
    document.getElementById("splitsliderstart").min = "0";
    document.getElementById("splitslidermid").min = "1";
    document.getElementById("splitsliderstart").value = "0";
    document.getElementById("splitslidermid").value = "1";
}

function setSplit(start, mid, end)
{
    var spans = document.getElementsByClassName("split_start_disp");
    for (var i = 0; i<spans.length; i++)
    {
        if (start != "\u03B5") spans[i].innerHTML = start+"";
    }
    spans = document.getElementsByClassName("split_mid_disp");
    for (var i = 0; i<spans.length; i++)
    {
        if (mid != "\u03B5") spans[i].innerHTML = mid+"";
    }
    spans = document.getElementsByClassName("split_end_disp");
    for (var i = 0; i<spans.length; i++)
    {
        if (end != "\u03B5") spans[i].innerHTML = end+"";
    }

    spans = document.getElementsByClassName("split_start");
    for (var i = 0; i<spans.length; i++)
    {
        spans[i].innerHTML = start+"";
    }
    spans = document.getElementsByClassName("split_mid");
    for (var i = 0; i<spans.length; i++)
    {
        spans[i].innerHTML = mid+"";
    }
    spans = document.getElementsByClassName("split_end");
    for (var i = 0; i<spans.length; i++)
    {
        spans[i].innerHTML = end+"";
    }
}

function setAlphabet()
{
    var alphabet = parseAlphabet();
    if(alphabetChecks(alphabet))
    {
        Editor.canvasNfa.setAlphabet(alphabet);
    }
}