package com.automatatutor.snippet.problems

import com.automatatutor.snippet._
import java.util.{Calendar, Date}
import scala.xml.{Elem, NodeSeq, Text, XML}
import com.automatatutor.lib.GraderConnection
import com.automatatutor.model._
import com.automatatutor.model.problems._
import net.liftweb.common.{Box, Empty, Full}
import net.liftweb.http.SHtml
import net.liftweb.http.SHtml.ElemAttr.pairToBasic
import net.liftweb.http.Templates
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds.jsExpToJsCmd
import net.liftweb.mapper.By
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers.strToSuperArrowAssoc

object PumpingLemmaGameSnippet extends SpecificProblemSnippet {

  def preprocessAutomatonFromCanvas(input: String): String = {
    var output = input
    output.filter(!List('\n', '\r').contains(_)).replace("\u0027", "\'")
    output = output.replace('\n', ' ')
    output = output.replace('\r', ' ')
    output = output.replace('\t', ' ')
    output = output.replace(949.toChar+"", "epsilon")

    return output
  }

  def preprocessAutomatonXml(input: String): String = {
    var output = input
    output.filter(!List('\n', '\r').contains(_)).replace("\u0027", "\'")
    output = output.replace('\n', ' ')
    output = output.replace("'", "\\'")
    output = output.replace('\r', ' ')
    output = output.replace('\t', ' ')

    val index = output.indexOf('>')
    if (output.startsWith("<automaton"))
      output = "<automaton>" + output.substring(index+1)
    else if (output.startsWith("<error"))
      output = "<error>" + output.substring(index+1)

    return output
  }

  def preprocessLoadedXml ( input : String ) : String = {
    val output = input.replace("\\", "")
    return output
  }

  // Helper method to execute the JavaScript command built by 'func'
  // after the regular SHtml.ajaxCall
  def ajaxCallString(cmd:String, func: String => JsCmd) : String = {
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw(cmd), func)
    return ajaxCall.toString().substring(6, ajaxCall.toString().length - 2)
  }

  def calcDFA(data:String) : JsCmd = {
    val dataSplit = data.split("#", -1)

    val automaton = GraderConnection.PLGRegularGetDFAFromSymbolicString(dataSplit(0), dataSplit(1), dataSplit(2))
    val str = preprocessAutomatonXml(automaton.toString())

    if (str.startsWith("<automaton>"))
    {
      val feedback: JsCmd = JsCmds.Run("document.getElementById('feedbackdisplay').style.display = 'none'")
      val jsDFA: JsCmd = JsCmds.Run("drawDFA('" + str + "')")
      val jsGraph: JsCmd = JsCmds.Run("Editor.canvasDfa.layoutNodes()")
      return jsDFA & jsGraph & feedback
    }
    else if (str.startsWith("<error>"))
    {
      val txt: String = scala.xml.XML.loadString(str).text
      val feedback: JsCmd = JsCmds.Run("document.getElementById('feedbackdisplay').style.display = 'block'")
      val errordisplay: JsCmd = JsCmds.Run("document.getElementById('errordisplay').innerHTML = '"+txt+"'")
      return feedback & errordisplay
    }
    else
    {
      val feedback: JsCmd = JsCmds.Run("document.getElementById('feedbackdisplay').style.display = 'block'")
      val errordisplay: JsCmd = JsCmds.Run("document.getElementById('errordisplay').innerHTML = '"+str+"'")
      return feedback & errordisplay
    }

  }

  def checkValidity(data : String): JsCmd =
  {
    val dataSplit = data.split("#", -1)
    val feedback = GraderConnection.PLGNonRegularCheckValidity(dataSplit(0), dataSplit(1), dataSplit(2), dataSplit(3))

    if ((feedback \\ "error").nonEmpty)
    {
      val cmd1: JsCmd = JsCmds.Run("document.getElementById('feedbackdisplay').style.display = 'block'")
      val cmd2: JsCmd = JsCmds.Run("document.getElementById('errordisplay').innerHTML = '"+(feedback \\ "error")+"'")
      return cmd1 & cmd2
    }
    else if ((feedback \\ "true").nonEmpty)
    {
      val cmd1: JsCmd = JsCmds.Run("document.getElementById('feedbackdisplay').style.display = 'block'")
      val cmd2: JsCmd = JsCmds.Run("document.getElementById('errordisplay').innerHTML = 'Language is valid.'")
      return cmd1 & cmd2
    }
    return JsCmds.Run("")
  }

  def nfaToDfa(data:String): JsCmd =
  {
    val automaton = GraderConnection.PLGNfaToDfa(preprocessAutomatonFromCanvas(data))
    val str = preprocessAutomatonXml(automaton.toString())

    if (str.startsWith("<automaton>"))
    {
      val feedback: JsCmd = JsCmds.Run("document.getElementById('feedbackdisplay').style.display = 'none'")
      val jsDFA: JsCmd = JsCmds.Run("drawDFA('" + str + "')")
      val jsGraph: JsCmd = JsCmds.Run("Editor.canvasDfa.layoutNodes()")
      return jsDFA & jsGraph & feedback
    }
    else if (str.startsWith("<error>"))
    {
      val txt: String = scala.xml.XML.loadString(str).text
      val feedback: JsCmd = JsCmds.Run("document.getElementById('feedbackdisplay').style.display = 'block'")
      val errordisplay: JsCmd = JsCmds.Run("document.getElementById('errordisplay').innerHTML = '"+txt+"'")
      return feedback & errordisplay
    }
    else
    {
      val feedback: JsCmd = JsCmds.Run("document.getElementById('feedbackdisplay').style.display = 'block'")
      val errordisplay: JsCmd = JsCmds.Run("document.getElementById('errordisplay').innerHTML = '"+str+"'")
      return feedback & errordisplay
    }
  }

  override def renderCreate(createUnspecificProb: (String, String) => Problem,
                             returnFunc:          (Problem) => Unit) : NodeSeq = {

    var alphabet: String = "a b"
    var symbolicString : String = "a^j b^k"
    var constraints : String = "j <= k"
    var unpumpableWord : String = "a^n b^n"
    var shortDescription: String = "Example"
    var longDescription: String = "Put description of language here (e.g. {a^j b^k | j <= k} or 'a s followed by at least as many b s')"
    var automatonString: String = ""
    var regularString: String = "not pumpable"

    def create() = {
      val unspecificProblem = createUnspecificProb(shortDescription, longDescription)

      val specificProblem : PumpingLemmaGameProblem = PumpingLemmaGameProblem.create
      specificProblem.setGeneralProblem(unspecificProblem).setAlphabet(alphabet)
      specificProblem.setGeneralProblem(unspecificProblem).setSymbolicString(symbolicString)
      specificProblem.setGeneralProblem(unspecificProblem).setConstraints(constraints)

      if (regularString.contains("regular")) {
        specificProblem.setGeneralProblem(unspecificProblem).setRegular(true)
        specificProblem.setGeneralProblem(unspecificProblem).setAutomaton(automatonString)
      }
      else if (regularString.contains("not pumpable")){
        specificProblem.setGeneralProblem(unspecificProblem).setRegular(false)
        specificProblem.setGeneralProblem(unspecificProblem).setUnpumpableWord(unpumpableWord)
      }
      else throw new IllegalArgumentException("unknown regular String: "+regularString)

      specificProblem.save

      returnFunc(unspecificProblem)
    }


    //create HTML
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _, "id" -> "shortDescriptionField")
    val longDescriptionField = SHtml.textarea(longDescription, longDescription = _, "cols" -> "80", "rows" -> "5", "id" -> "longDescriptionField")
    val alphabetField = SHtml.text(alphabet, alphabet = _, "id" -> "alphabetField")
    val symbolicStringField = SHtml.text(symbolicString, symbolicString = _, "id" -> "symbolicStringField")
    val constraintsField = SHtml.text(constraints, constraints = _, "id" -> "constraintsField")
    val unpumpableField = SHtml.text(unpumpableWord, unpumpableWord = _, "id" -> "unpumpableField")

    val exportData : String = "document.getElementById('automatonField').value = Editor.canvasDfa.exportAutomaton();" +
      " document.getElementById('regularField').value = document.getElementById('regularText').innerHTML; "
    val submit = SHtml.submit("Create", create, "onclick" -> exportData, "id" -> "submitButton")

    val ajaxCheckDFAString : String = ajaxCallString("collectData(['alphabetField', 'symbolicStringField', 'constraintsField'])", calcDFA(_))
    val ajaxCheckValidityString : String = ajaxCallString("collectData(['alphabetField', 'symbolicStringField', 'constraintsField', 'unpumpableField'])", checkValidity(_))
    val ajaxNfaToDfaString : String = ajaxCallString("Editor.canvasNfa.exportAutomaton()", nfaToDfa(_))
    val checkDFAbutton : NodeSeq = <button type='button' id='checkdfabutton' onclick={ ajaxCheckDFAString }>request DFA</button>
    val checkValidityButton : NodeSeq = <button type='button' id='checkvaliditybutton' onclick={ ajaxCheckValidityString }>check validity</button>
    val nfaToDfaButton : NodeSeq = <button type='button' id='nfatodfabutton' onclick={ ajaxNfaToDfaString }>NFA to DFA</button>

    val automatonField = SHtml.hidden(automatonXml => automatonString = preprocessAutomatonXml(automatonXml), "", "id" -> "automatonField")
    val regularField = SHtml.hidden(str => regularString = str, "", "id" -> "regularField")
    //put HTML into template
    val template: NodeSeq = Templates(List("templates-hidden", "pumping-lemma-game-problem", "create")) openOr Text("Could not find template /templates-hidden/pumping-lemma-game-problem/create")
    Helpers.bind("pumpinggame", template,
      "shortdescription" -> shortDescriptionField,
      "longdescription" -> longDescriptionField,
      "alphabetfield" -> alphabetField,
      "symbolicstringfield" -> symbolicStringField,
      "constraintsfield" -> constraintsField,
      "unpumpablefield" -> unpumpableField,
      "regularfield" -> regularField,
      "automaton" ->  automatonField,
      "submit" -> submit,
      "checkdfabutton" -> checkDFAbutton,
      "checkvaliditybutton" -> checkValidityButton,
      "nfatodfabutton" -> nfaToDfaButton)
  }



  override def renderEdit: Box[(Problem, Problem => Unit) => NodeSeq] = Full(renderEditFunc)

  private def renderEditFunc(problem: Problem, returnFunc: (Problem => Unit)): NodeSeq = {

    val specificProblem = PumpingLemmaGameProblem.findByGeneralProblem(problem)
    var shortDescription: String = problem.getShortDescription
    var longDescription: String = problem.getLongDescription

    var alphabet: String = specificProblem.getAlphabet
    var symbolicString : String = specificProblem.getSymbolicString
    var regular: Boolean = specificProblem.getRegular
    var regularString: String = if (regular) "regular" else "not pumpable"
    var constraints : String = if(regular) "" else specificProblem.getConstraints
    var unpumpableWord : String = if (regular) "" else specificProblem.getUnpumpableWord
    var automatonString: String = if (regular) specificProblem.getAutomaton else ""


    def edit() = {

      problem.setShortDescription(shortDescription)
      problem.setLongDescription(longDescription)
      specificProblem.setAlphabet(alphabet)

      if (regular)
        {
          specificProblem.setAutomaton(automatonString)
        }
      else
      {
        specificProblem.setSymbolicString(symbolicString)
        specificProblem.setConstraints(constraints)
        specificProblem.setUnpumpableWord(unpumpableWord)
      }

      problem.save
      specificProblem.save
      returnFunc(problem)
    }

    //create HTML
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _, "id" -> "shortDescriptionField")
    val longDescriptionField = SHtml.textarea(longDescription, longDescription = _, "cols" -> "80", "rows" -> "5", "id" -> "longDescriptionField")
    val alphabetField = SHtml.text(alphabet, alphabet = _, "id" -> "alphabetField")
    val symbolicStringField = SHtml.text(symbolicString, symbolicString = _, "id" -> "symbolicStringField")
    val constraintsField = SHtml.text(constraints, constraints = _, "id" -> "constraintsField")
    val unpumpableField = SHtml.text(unpumpableWord, unpumpableWord = _, "id" -> "unpumpableField")

    val exportData: String = "document.getElementById('automatonField').value = Editor.canvasDfa.exportAutomaton(); "
    //val submitString: String = ajaxCallString(exportData, edit(_))
    //val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ submitString }>Submit</button>
    val submitButton: NodeSeq = SHtml.submit("Edit", edit, "onclick" -> exportData, "id" -> "submitButton")
    //val submitStringNoExport: String = ajaxCallString("", edit(_))
    val submitButtonNoExport: NodeSeq = SHtml.submit("Edit", edit, "onclick" -> "", "id" -> "submitButton")

    val ajaxCheckDFAString : String = ajaxCallString("collectData(['alphabetField', 'symbolicStringField', 'constraintsField'])", calcDFA(_))
    val ajaxCheckValidityString : String = ajaxCallString("collectData(['alphabetField', 'symbolicStringField', 'constraintsField', 'unpumpableField'])", checkValidity(_))
    val ajaxNfaToDfaString : String = ajaxCallString("Editor.canvasNfa.exportAutomaton()", nfaToDfa(_))
    val checkDFAbutton : NodeSeq = <button type='button' id='checkdfabutton' onclick={ ajaxCheckDFAString }>request DFA</button>
    val checkValidityButton : NodeSeq = <button type='button' id='checkvaliditybutton' onclick={ ajaxCheckValidityString }>check validity</button>
    val nfaToDfaButton : NodeSeq = <button type='button' id='nfatodfabutton' onclick={ ajaxNfaToDfaString }>NFA to DFA</button>

    val automatonField = SHtml.hidden(automatonXml => automatonString = preprocessAutomatonXml(automatonXml), "", "id" -> "automatonField")
    val regularField = SHtml.hidden(str => regularString = str, if(regular) "regular" else "not regular", "id" -> "regularField")

    val drawDFAScript =
      <script type="text/javascript">
        drawNFA("{specificProblem.getAutomaton}");
        drawDFA("{specificProblem.getAutomaton}");
        document.getElementById("automata_applet_nfa").removeAttribute("style");
      </script>

    //put HTML into template
    if (regular)
      {
        val template: NodeSeq = Templates(List("templates-hidden", "pumping-lemma-game-problem", "editRegular")) openOr Text("Could not find template /templates-hidden/pumping-lemma-game-problem/editRegular")
        Helpers.bind("pumpinggame", template,
          "shortdescription" -> shortDescriptionField,
          "longdescription" -> longDescriptionField,
          "alphabetfield" -> alphabetField,
          "regularfield" -> regularField,
          "automaton" ->  automatonField,
          "checkdfabutton" -> checkDFAbutton,
          "nfatodfabutton" -> nfaToDfaButton,
          "submit" -> submitButton,
          "drawdfa" -> drawDFAScript)
      }

    else
      {
        val template: NodeSeq = Templates(List("templates-hidden", "pumping-lemma-game-problem", "editNonPumpable")) openOr Text("Could not find template /templates-hidden/pumping-lemma-game-problem/editNonPumpable")
        Helpers.bind("pumpinggame", template,
          "shortdescription" -> shortDescriptionField,
          "longdescription" -> longDescriptionField,
          "alphabetfield" -> alphabetField,
          "symbolicstringfield" -> symbolicStringField,
          "constraintsfield" -> constraintsField,
          "unpumpablefield" -> unpumpableField,
          "regularfield" -> regularField,
          "submit" -> submitButtonNoExport)
      }

  }

  override def renderSolve(generalProblem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                           recordSolutionAttempt: (Int, Date) => SolutionAttempt, returnFunc: (Problem => Unit), 
						   remainingAttempts: () => Int, bestGrade: () => Int): NodeSeq = {

    val specificProblem = PumpingLemmaGameProblem.findByGeneralProblem(generalProblem)

    def splitInputValidity(start: String, mid: String, end: String): Boolean =
    {
      val solutionAttempt = getLastAttempt().get
      solutionAttempt.choiceWord.toString().equals(start+mid+end) && solutionAttempt.choiceN.toString().toInt >= (start+mid).length
    }

    def grade(): Unit =
    {
      val solutionAttempt = getLastAttempt().get
      if (solutionAttempt.win.is == "true")
      {
        val generalAttempt = recordSolutionAttempt(10, Calendar.getInstance.getTime())
        //            generalAttempt.save
        solutionAttempt.solutionAttemptId(generalAttempt.id.get)
        solutionAttempt.save
      }
      else
      {
        val generalAttempt = recordSolutionAttempt(0, Calendar.getInstance.getTime())
        //            generalAttempt.save
        solutionAttempt.solutionAttemptId(generalAttempt.id.get)
        solutionAttempt.save
      }
    }

    def getLastAttempt(): Box[PumpingLemmaGameSolutionAttempt] =
    {
      val allAttempts = PumpingLemmaGameSolutionAttempt.findAll(By(PumpingLemmaGameSolutionAttempt.userId, User.currentUser_!),
        By(PumpingLemmaGameSolutionAttempt.problemId, specificProblem.problemId.get))
      if (!allAttempts.isEmpty)
        return Full(allAttempts.maxBy(attempt => attempt.dateTime.is.getTime()))
      else
        return Empty
    }

    def newSolutionAttempt(regular: Boolean) =
    {
      val solutionAttempt = PumpingLemmaGameSolutionAttempt.create
      solutionAttempt.userId(User.currentUser_!.id.get)
      solutionAttempt.dateTime(Calendar.getInstance.getTime())
      solutionAttempt.problemId(specificProblem.problemId.get)
      if (regular)
        solutionAttempt.choiceRegular("regular")
      else
        solutionAttempt.choiceRegular("not regular")
      solutionAttempt.save
    }

    def regularChoice(empty: String) : JsCmd = {
      newSolutionAttempt(true)
      return JsCmds.Run("chooseRegular(true);location.reload();")
    }

    def nonRegularChoice(empty: String) : JsCmd = {
      newSolutionAttempt(false)
      return JsCmds.Run("chooseRegular(false);location.reload();")
    }

    def chooseN(empty : String): JsCmd = {
      val solutionAttempt = getLastAttempt().getOrElse(return JsCmds.Run("location.reload();"))
      var n : String = null
      if (specificProblem.getRegular)
      {
        n = GraderConnection.PLGRegularGetN(preprocessLoadedXml(specificProblem.getAutomaton))
      }
      else
        {
          n = GraderConnection.PLGNonRegularGetN("10")
        }

      solutionAttempt.choiceN(n)
      solutionAttempt.save

      return chooseNCmd(n)
    }
    def chooseNCmd(n: String): JsCmd = {
      val cmd1 = JsCmds.Run("document.getElementById('aichoicen').style.display = 'block'")
      val cmd2 = JsCmds.Run("setPN('"+n+"')")
      val cmd3 = JsCmds.Run("document.getElementById('plchoicew').style.display = 'block'")
      return cmd1 & cmd2 & cmd3
    }


    def checkWordChooseSplit(data: String) : JsCmd = {
      val solutionAttempt = getLastAttempt().getOrElse(return JsCmds.Run("location.reload();"))
      var feedback : NodeSeq = null
      if (specificProblem.getRegular)
        {
          feedback = GraderConnection.PLGRegularCheckWordGetSplit(preprocessLoadedXml(specificProblem.getAutomaton), solutionAttempt.choiceN.get, data)
        }
      else
        {
          feedback = GraderConnection.PLGNonRegularCheckWordGetSplit(specificProblem.getAlphabet, specificProblem.getSymbolicString, specificProblem.getConstraints, solutionAttempt.choiceN.get, data)
        }

        if ((feedback \\ "false").nonEmpty)
          {
            val txt = feedback.head
            val cmd = JsCmds.Run("document.getElementById('wordchoicefeedback').innerHTML = '"+txt.toString()+"'")
            return cmd
          }
        else if ((feedback \\ "split").nonEmpty)
          {
            val start = if ((feedback \\ "start").text == "") "\u03B5" else (feedback \\ "start").text
            val mid = if ((feedback \\ "mid").text == "") "\u03B5" else (feedback \\ "mid").text
            val end = if ((feedback \\ "end").text == "") "\u03B5" else (feedback \\ "end").text

            solutionAttempt.choiceWord(data)
            solutionAttempt.choiceSplit(start+"#"+mid+"#"+end)
            solutionAttempt.save

            return chooseWordCmd(start, mid, end)
          }
      return JsCmds.Run("")
    }
    def chooseWordCmd(start: String, mid: String, end: String) : JsCmd = {
      val cmd1 = JsCmds.Run("document.getElementById('wordchoicefeedback').innerHTML = 'You chose a valid word.'")
      val cmd2 = JsCmds.Run("document.getElementById('wordchoicefield').disabled = true")
      val cmd3 = JsCmds.Run("document.getElementById('wordsubmitbutton').disabled = true")
      val cmd4 = JsCmds.Run("setSplit('"+ start+"', '"+ mid + "', '"+ end +"')")
      val cmd5 = JsCmds.Run("document.getElementById('aichoicesplit').style.display = 'block'")
      val cmd6 = JsCmds.Run("document.getElementById('plchoicei').style.display = 'block'")
      return cmd1 & cmd2 & cmd3 & cmd4 & cmd5 & cmd6
    }

    def checkI(data: String) : JsCmd = {
      val solutionAttempt = getLastAttempt().getOrElse(return JsCmds.Run("location.reload();"))
      var feedback : NodeSeq = null
      val split = solutionAttempt.choiceSplit.get.split("#", -1)

      if (specificProblem.getRegular)
        {
          feedback = GraderConnection.PLGRegularCheckI(preprocessLoadedXml(specificProblem.getAutomaton), split(0), split(1), split(2), data)
        }
      else
        {
          feedback = GraderConnection.PLGNonRegularCheckI(specificProblem.getAlphabet, specificProblem.getSymbolicString, specificProblem.getConstraints, split(0), split(1), split(2), data)
        }

      if ((feedback \\"error").nonEmpty)
        {
          val txt = feedback.head
          val cmd = JsCmds.Run("document.getElementById('ichoicefeedback').innerHTML = '"+txt.toString()+"'")
          return cmd
        }
      else if ((feedback \\ "true").nonEmpty)
        {
          solutionAttempt.choiceI(data)
          solutionAttempt.win("false")
          solutionAttempt.save
          grade()
          return checkICmd(true)
        }
      else if ((feedback \\ "false").nonEmpty)
      {
        solutionAttempt.choiceI(data)
        solutionAttempt.win("true")
        solutionAttempt.save
        grade()
        return checkICmd(false)
      }
    }

    def checkICmd(win: Boolean) : JsCmd = {
      if (win)
        {
          val cmd1 = JsCmds.Run("document.getElementById('result').innerHTML = 'The word is in the given language.<br>You lose!'")
          val cmd2 = JsCmds.Run("document.getElementById('ichoicefield').disabled = true")
          val cmd3 = JsCmds.Run("document.getElementById('isubmitbutton').disabled = true")
          val cmd4 = JsCmds.Run("document.getElementById('retrybutton').style.display = 'block'")
          return cmd1 & cmd2 & cmd3 & cmd4
        }
      else
        {
          val cmd1 = JsCmds.Run("document.getElementById('result').innerHTML = 'The word is not in the given language.<br>You win!'")
          val cmd2 = JsCmds.Run("document.getElementById('ichoicefield').disabled = true")
          val cmd3 = JsCmds.Run("document.getElementById('isubmitbutton').disabled = true")
          val cmd4 = JsCmds.Run("document.getElementById('retrybutton').style.display = 'block'")
          return cmd1 & cmd2 & cmd3 & cmd4
        }
    }

    def checkNAndChooseWord(data: String) : JsCmd = {
      val solutionAttempt = getLastAttempt().getOrElse(return JsCmds.Run("location.reload();"))

    try
      {
        val n_int = data.toInt

        //validity checks for PLN
        if (n_int < 1)
          return JsCmds.Run("document.getElementById('nchoicefeedback').innerHTML = 'Number must be grater than zero.'")
        if (n_int > 20)
          return JsCmds.Run("document.getElementById('nchoicefeedback').innerHTML = " +
            "'Your chosen number is legal, but we do not have infinite computing resources. Please choose a number less than 21.'")

        val cmd = JsCmds.Run("setPN('"+data+"')")

        var feedback : NodeSeq = null
        if (specificProblem.getRegular)
          {
            feedback = GraderConnection.PLGRegularGetWord(preprocessLoadedXml(specificProblem.getAutomaton), data)
          }
        else
          {
            feedback = GraderConnection.PLGNonRegularGetWord(specificProblem.getAlphabet, specificProblem.getSymbolicString, specificProblem.getConstraints, data, specificProblem.getUnpumpableWord)
          }

        if ((feedback \\"error").nonEmpty)
          {
            solutionAttempt.win("true")
            solutionAttempt.choiceWord("error")
            solutionAttempt.save
            grade()

            return cmd & JsCmds.Run("document.getElementById('nchoicefeedback').innerHTML = 'AI could not find a word that is at least as long as n. You win!'")
          }
        else if ((feedback \\"word").nonEmpty)
          {
            val word = (feedback \\ "word").text
            solutionAttempt.choiceWord(word)
            solutionAttempt.choiceN(data)
            solutionAttempt.save

            return checkNAndChooseWordCmd(data, word)
          }

      }
      catch
      {
        case e: NumberFormatException =>
          return JsCmds.Run("document.getElementById('nchoicefeedback').innerHTML = 'You must specify a number.'")
      }
      return JsCmds.Run("")
    }
    def checkNAndChooseWordCmd(n: String, word: String) : JsCmd = {
      val cmd = JsCmds.Run("setPN('"+n+"')")
      val cmd1 = JsCmds.Run("document.getElementById('aichoiceword').innerHTML = 'AI chose: "+ word +"';" +
        "document.getElementById('aichoiceword').style.display = 'block';")
      val cmd2 = JsCmds.Run("document.getElementById('word').value = '"+word+"'")
      val cmd3 = JsCmds.Run("document.getElementById('plchoicesplit').style.display = 'block'")
      val cmd4 = JsCmds.Run("document.getElementById('nsubmitbutton').disabled = true")
      val cmd5 = JsCmds.Run("updateSplits()")

      return cmd & cmd1 & cmd2 & cmd3 & cmd4 & cmd5
    }

    def chooseI(data: String) : JsCmd = {
      val solutionAttempt = getLastAttempt().getOrElse(return JsCmds.Run("location.reload();"))

      val dataSplit = data.split("#", -1)
      var feedback : NodeSeq = null
      for (i <- 0 to 2)
      {
        if (dataSplit(i)(0) == 949) //epsilon
          {
            dataSplit(i) = ""
          }
      }

      if (!splitInputValidity(dataSplit(0), dataSplit(1), dataSplit(2)))
        {
          return JsCmds.Run("document.getElementById('splitchoicefeedback').innerHTML = 'Please do not try to cheat me. I am smarter than you anyways.'")
        }

      if (specificProblem.getRegular)
        {
          feedback = GraderConnection.PLGRegularGetI(preprocessLoadedXml(specificProblem.getAutomaton), dataSplit(0), dataSplit(1), dataSplit(2))
        }
      else
        {
          feedback = GraderConnection.PLGNonRegularGetI(specificProblem.getAlphabet, specificProblem.getSymbolicString, specificProblem.getConstraints, dataSplit(0), dataSplit(1), dataSplit(2))
        }
      if ((feedback \\"error").nonEmpty)
      {
        solutionAttempt.win("true")
        solutionAttempt.choiceI("error")
        solutionAttempt.choiceSplit(data)
        solutionAttempt.save
        grade()

        return JsCmds.Run("document.getElementById('splitchoicefeedback').innerHTML = 'Could not find valid i. You win!'")
      }
      else if ((feedback \\"loss").nonEmpty)
      {
        val word = (feedback \\ "loss").text
        solutionAttempt.win("false")
        solutionAttempt.choiceI(word)
        solutionAttempt.choiceSplit(data)
        solutionAttempt.save
        grade()

        return chooseICmd(false, word)
      }
      else if ((feedback \\"win").nonEmpty)
      {
        val word = (feedback \\ "win").text
        solutionAttempt.win("true")
        solutionAttempt.choiceI(word)
        solutionAttempt.choiceSplit(data)
        solutionAttempt.save
        grade()

        return chooseICmd(true, word)
      }
      return JsCmds.Run("")
    }
    def chooseICmd(win: Boolean, word: String) : JsCmd = {
      if (win)
        {
          val cmd1 = JsCmds.Run("document.getElementById('aichoicei').innerHTML = 'AI chose: "+ word +"';" +
            "document.getElementById('aichoicei').style.display = 'block';")
          val cmd2 = JsCmds.Run("document.getElementById('splitsubmit').disabled = true")
          val cmd3 = JsCmds.Run("document.getElementById('retrybutton').style.display = 'block'")
          val cmd4 = JsCmds.Run("document.getElementById('result').innerHTML = 'You win!'")
          val cmd5 = JsCmds.Run("document.getElementById('splitsliderstart').disabled = true")
          val cmd6 = JsCmds.Run("document.getElementById('splitslidermid').disabled = true")
          return cmd1 & cmd2 & cmd3 & cmd4 & cmd5 & cmd6
        }
      else
        {
          val cmd1 = JsCmds.Run("document.getElementById('aichoicei').innerHTML = 'AI chose: "+ word +"';" +
            "document.getElementById('aichoicei').style.display = 'block';")
          val cmd2 = JsCmds.Run("document.getElementById('splitsubmit').disabled = true")
          val cmd3 = JsCmds.Run("document.getElementById('retrybutton').style.display = 'block'")
          val cmd4 = JsCmds.Run("document.getElementById('result').innerHTML = 'You lose!'")
          val cmd5 = JsCmds.Run("document.getElementById('splitsliderstart').disabled = true")
          val cmd6 = JsCmds.Run("document.getElementById('splitslidermid').disabled = true")
          return cmd1 & cmd2 & cmd3 & cmd4 & cmd5 & cmd6
        }
    }

    def loadLastAttempt(): JsCmd =
    {
      val solutionAttempt = getLastAttempt().getOrElse(return JsCmds.Run(""))
      if (solutionAttempt.win.is != null || !solutionAttempt.choiceRegular.is.contains("regular"))
      {
        return JsCmds.Run("")
      }

      val cReg = JsCmds.Run("chooseRegular(true); document.getElementById('plchoicen').style.display = 'block';")
      val cNReg = JsCmds.Run("chooseRegular(false);")

      if (solutionAttempt.choiceRegular.is == "regular" && solutionAttempt.choiceN.is == null)
      {
        return cReg
      }
      else if (solutionAttempt.choiceRegular.is == "not regular" && solutionAttempt.choiceN.is == null)
      {
        return cNReg & chooseN("")
      }
      else if (solutionAttempt.choiceRegular.is == "regular" && solutionAttempt.choiceWord.is == null)
      {
        val n = solutionAttempt.choiceN.is
        return cReg & JsCmds.Run("document.getElementById('nchoicefield').value = '"+n+"';") & checkNAndChooseWord(n)
      }
      else if (solutionAttempt.choiceRegular.is == "not regular" && solutionAttempt.choiceWord.is == null)
      {
        return cNReg & chooseNCmd(solutionAttempt.choiceN.is)
      }
      else if (solutionAttempt.choiceRegular.is == "regular" && solutionAttempt.choiceSplit.is == null)
      {
        val n = solutionAttempt.choiceN.is
        val word = solutionAttempt.choiceWord.is
        return cReg & JsCmds.Run("document.getElementById('nchoicefield').value = '"+n+"';") & checkNAndChooseWordCmd(n, word)
      }
      else if (solutionAttempt.choiceRegular.is == "not regular" && solutionAttempt.choiceSplit.is == null)
      {
        val n = solutionAttempt.choiceN.is
        val word = solutionAttempt.choiceWord.is
        return cNReg & chooseNCmd(n) & checkWordChooseSplit(word) & JsCmds.Run("document.getElementById('wordchoicefield').value = '"+word+"';")
      }
      else if (solutionAttempt.choiceRegular.is == "regular" && solutionAttempt.choiceI.is == null)
      {
        val n = solutionAttempt.choiceN.is
        val word = solutionAttempt.choiceWord.is
        val split = solutionAttempt.choiceSplit.is.replace("\u03B5", "")
        return cReg & JsCmds.Run("document.getElementById('nchoicefield').value = '"+n+"';") & checkNAndChooseWordCmd(n, word) & chooseI(split)
      }
      else if (solutionAttempt.choiceRegular.is == "not regular" && solutionAttempt.choiceI.is == null)
      {
        val n = solutionAttempt.choiceN.is
        val word = solutionAttempt.choiceWord.is
        val split = solutionAttempt.choiceSplit.is.replace("\u03B5", "")
        val splitsplit = split.split("#", -1)
        return cNReg & chooseNCmd(n) & chooseWordCmd(splitsplit(0), splitsplit(1), splitsplit(2)) & JsCmds.Run("document.getElementById('wordchoicefield').value = '"+word+"';")
      }
      else
        {
          return JsCmds.Run("alert('Error while reading the database. Please report this bug to the instructor.')")
        }
    }

    val ajaxCallRegularString : String = ajaxCallString("", regularChoice(_))
    val ajaxCallNonRegularString : String = ajaxCallString("", nonRegularChoice(_))
    val ajaxCallNString : String = ajaxCallString("", chooseN(_))
    val ajaxCallWordString : String = ajaxCallString("document.getElementById('wordchoicefield').value", checkWordChooseSplit(_))
    val ajaxCallIString : String = ajaxCallString("document.getElementById('ichoicefield').value", checkI(_))
    val ajaxCallGetNChoiceString : String = ajaxCallString("document.getElementById('nchoicefield').value", checkNAndChooseWord(_))
    val ajaxCallFindIString : String = ajaxCallString("document.getElementById('splitdisplaystart').innerHTML+" +
      "'#'+document.getElementById('splitdisplaymid').innerHTML+'#'+document.getElementById('splitdisplayend').innerHTML", chooseI(_))

    val languageDescription = Text(generalProblem.getLongDescription)
    val alphabet = Text("{"+ specificProblem.getAlphabet +"}")
    val regularButton : NodeSeq = <button id="regularbutton" class="regularchoice" onclick={ajaxCallRegularString}>regular</button>
    val nonRegularButton : NodeSeq = <button id="nonregularbutton" class="regularchoice" onclick={ajaxCallNonRegularString + "; "+ajaxCallNString}>not regular</button>
    val wordSubmitButton : NodeSeq = <button id="wordsubmitbutton" onclick={ajaxCallWordString}>submit</button>
    val iSubmitButton : NodeSeq = <button id="isubmitbutton" onclick={ajaxCallIString}>submit</button>
    val nSubmitButton : NodeSeq = <button id="nsubmitbutton" onclick={ajaxCallGetNChoiceString}>submit</button>
    val splitSubmitButton: NodeSeq = <button id="splitsubmit" onclick={ajaxCallFindIString}>submit</button>
    val cmd = loadLastAttempt()
    val solAttScript = <script>{cmd.toString().substring(6, cmd.toString().length - 1)}</script>

    //put HTML into template
    val template: NodeSeq = Templates(List("templates-hidden", "pumping-lemma-game-problem", "solve")) openOr Text("Could not find template /templates-hidden/pumping-lemma-game-problem/solve")
    Helpers.bind("pumpinggame", template,
      "languagedescription" -> languageDescription,
      "alphabet" -> alphabet,
      "regularbutton" -> regularButton,
      "nonregularbutton" -> nonRegularButton,
      "wordsubmitbutton" -> wordSubmitButton,
      "isubmitbutton" -> iSubmitButton,
      "nsubmitbutton" -> nSubmitButton,
      "splitsubmit" -> splitSubmitButton,
      "loadsolutionattempt" -> solAttScript
    )
  }

  override def onDelete(generalProblem: Problem): Unit = {
    //can usually stay empty
  }
}
