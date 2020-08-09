package com.automatatutor.snippet.problems

import com.automatatutor.snippet._
import java.util.Calendar
import java.util.Date
import com.automatatutor.lib.GraderConnection
import scala.xml.{Elem, NodeSeq, Text, XML}
import scala.xml.NodeSeq.seqToNodeSeq
import com.automatatutor.lib.GraderConnection
import com.automatatutor.model._
import com.automatatutor.model.problems._
import net.liftweb.common.Box
import net.liftweb.common.Full
import net.liftweb.http.S
import net.liftweb.http.SHtml
import net.liftweb.http.SHtml.ElemAttr.pairToBasic
import net.liftweb.http.Templates
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds.{Alert, JsHideId, JsIf, JsShowId, SetHtml, cmdToString, jsExpToJsCmd}
import net.liftweb.util.AnyVar.whatVarIs
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.http.js.JE.Call

object WhileToTMSnippet extends SpecificProblemSnippet {
  def preprocessAutomatonXml(input: String): String = input.filter(!List('\n', '\r').contains(_)).replace("\"", "\'") //'

  override def renderCreate(createUnspecificProb: (String, String) => Problem,
                             returnFunc:          (Problem) => Unit) : NodeSeq = {

     var shortDescription : String = ""
     var longDescription : String = ""
     var numberOfTapes : String = "1"
     var programText : String = "while x_0 != 0 do\n    x_1 = x_1 - x_0\n    x_0 = x_0 - 1\nendwhile"


     def create(formValues: String): JsCmd = {

       val formValuesXml = XML.loadString(formValues)
       shortDescription = (formValuesXml \ "shortdescfield").head.text
       longDescription = (formValuesXml \ "longdescfield").head.text
       programText = (formValuesXml \ "programtext").head.text
       numberOfTapes = (formValuesXml \\ "NumVariables").head.text
       val uselessVars = "0"
       val program = GraderConnection.whileProgramCheck((formValuesXml \ "Program").toString())

       val unspecificProblem = createUnspecificProb(shortDescription, longDescription)
       val specificProblem : WhileToTMProblem = WhileToTMProblem.create
       specificProblem.problemId(unspecificProblem).setProgramText(programText).setProgram(program).setNumTapes(numberOfTapes).setUselessVars(uselessVars)
       specificProblem.save

       return SHtml.ajaxCall("", (ignored : String) => returnFunc(unspecificProblem))
//       returnFunc(unspecificProblem)
     }

     // Remember to remove all newlines from the generated XML by using filter
     val programTextField = SHtml.textarea(programText, programText = _, "cols" -> "80", "rows" -> "5", "id" -> "whilefield")
     val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _, "id" -> "shortdescfield")
     val longDescriptionField = SHtml.textarea(longDescription, longDescription = _, "cols" -> "80", "rows" -> "5", "id" -> "longdescfield")

    val shortdescFieldValXmlJs: String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"
    val longdescFieldValXmlJs : String = "<longdescfield>' + document.getElementById('longdescfield').value + '</longdescfield>"
    val whileFieldValXmlJs : String = "<programtext>' + document.getElementById('whilefield').value + '</programtext>"

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + "' + getProgram() + '" + shortdescFieldValXmlJs+ longdescFieldValXmlJs+ whileFieldValXmlJs + "</createattempt>'"), create(_))
    val checkAlphabetAndSubmit: JsCmd = JsIf(Call("whileChecks", "whilefield"), hideSubmitButton & ajaxCall)
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={checkAlphabetAndSubmit}>Submit</button>

    val template : NodeSeq = Templates(List("templates-hidden", "while-to-tm-problem", "create")) openOr Text("Could not find template /templates-hidden/while-to-tm-problem/create")
    Helpers.bind("createform", template,
      "programtextfield" -> programTextField,
         "shortdescription" -> shortDescriptionField,
         "longdescription" -> longDescriptionField,
         "submit" -> submitButton)
  }

  override def renderEdit: Box[(Problem, Problem => Unit) => NodeSeq] = Full(renderEditFunc)

  private def renderEditFunc(problem: Problem, returnFunc: (Problem => Unit)): NodeSeq = {
     val specificProblem = WhileToTMProblem.findByGeneralProblem(problem)

     var shortDescription : String = problem.getShortDescription
     var longDescription : String = problem.getLongDescription
     var program : String = specificProblem.getProgram
    var programText : String = specificProblem.getProgramText
    var numTapes : String = specificProblem.getNumTapes
    var uselessVars : String = specificProblem.getUselessVars


     def edit(formValues: String): JsCmd = {

       val formValuesXml = XML.loadString(formValues)
       shortDescription = (formValuesXml \ "shortdescfield").head.text
       longDescription = (formValuesXml \ "longdescfield").head.text
       programText = (formValuesXml \ "programtext").head.text
       numTapes = (formValuesXml \\ "NumVariables").head.text
       uselessVars = "0"
       program = GraderConnection.whileProgramCheck((formValuesXml \ "Program").toString())

       problem.setShortDescription(shortDescription).setLongDescription(longDescription)
       specificProblem.setProgramText(programText).setProgram(program).setNumTapes(numTapes).setUselessVars(uselessVars)
       specificProblem.save

       return SHtml.ajaxCall("", (ignored : String) => returnFunc(problem))
     }

    val programTextField = SHtml.textarea(programText, programText = _, "cols" -> "80", "rows" -> "5", "id" -> "whilefield")
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _, "id" -> "shortdescfield")
     val longDescriptionField = SHtml.textarea(longDescription, longDescription = _, "cols" -> "80", "rows" -> "5", "id" -> "longdescfield")

    val shortdescFieldValXmlJs: String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"
    val longdescFieldValXmlJs : String = "<longdescfield>' + document.getElementById('longdescfield').value + '</longdescfield>"
    val whileFieldValXmlJs : String = "<programtext>' + document.getElementById('whilefield').value + '</programtext>"

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + "' + getProgram() + '" + shortdescFieldValXmlJs+ longdescFieldValXmlJs+ whileFieldValXmlJs + "</createattempt>'"), edit(_))
    val checkAlphabetAndSubmit: JsCmd = JsIf(Call("whileChecks", "whilefield"), hideSubmitButton & ajaxCall)
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={checkAlphabetAndSubmit}>Save</button>


    val template : NodeSeq = Templates(List("templates-hidden", "while-to-tm-problem", "edit")) openOr Text("Could not find template /templates-hidden/while-to-tm-problem/edit")
    Helpers.bind("editform", template,
      "programtextfield" -> programTextField,
      "shortdescription" -> shortDescriptionField,
      "longdescription" -> longDescriptionField,
      "submit" -> submitButton)
  }

  override def renderSolve(generalProblem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                           recordSolutionAttempt: (Int, Date) => SolutionAttempt, returnFunc: (Problem => Unit), 
						   remainingAttempts: () => Int, bestGrade: () => Int): NodeSeq = {

    val specificProblem = WhileToTMProblem.findByGeneralProblem(generalProblem)

    def grade( attemptTmDescription : String ) : JsCmd = {

      if(remainingAttempts() <= 0) {
        return JsShowId("feedbackdisplay") &
          SetHtml("feedbackdisplay",
           Text("You do not have any attempts left for this problem. Your final grade is " +
            bestGrade().toString + "/" +
            maxGrade.toString + "."))
      }

      val attemptTime = Calendar.getInstance.getTime()

      val graderResponse = GraderConnection.getWhileToTMFeedback(specificProblem.getProgram, attemptTmDescription, maxGrade.toInt)

      val numericalGrade = graderResponse._1
      val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)

      // Only save the specific attempt if we saved the general attempt
      if(generalAttempt != null) {
        WhileToTMSolutionAttempt.create.solutionAttemptId(generalAttempt).attemptTM(attemptTmDescription).save
      }

      val setNumericalGrade : JsCmd = SetHtml("grade", Text(graderResponse._1.toString + "/" + maxGrade.toString))
      val setFeedback : JsCmd = SetHtml("feedback", graderResponse._2 \ "feedString" \ "ul" \ "li")
      val showFeedback : JsCmd = JsShowId("feedbackdisplay")
      val setSampleInput : JsCmd = SetHtml("sampleinput", Text((graderResponse._2 \ "sampleInput").text))

      val resetTaps : JsCmd = JsRaw("tm.resetTapes()")

      return setNumericalGrade & setFeedback & showFeedback & setSampleInput & JsCmds.JsShowId("submitbutton") & resetTaps
    }

    // val problemAlphabet = (XML.loadString(specificProblem.alphabet.get) \ "symbol").map(_.text)
    val problemAlphabet = Array("0", "1") //, "\u25FB"
    val problemTapeNumber = specificProblem.getNumTapes

    val lastAttemptXml = lastAttempt.map({ generalAttempt =>
      preprocessAutomatonXml(WhileToTMSolutionAttempt.getByGeneralAttempt(generalAttempt).attemptTM.is)
    }) openOr ""

    val alphabetJavaScriptArray = "[\"" + problemAlphabet.mkString("\",\"") + "\"]"
    val alphabetScript : NodeSeq = <script type="text/javascript"> </script>
    //Editor.canvas.setNumberOfTapes( { problemTapeNumber } ); Editor.canvas.setAlphabet( { alphabetJavaScriptArray } ); Editor.canvas.initializeTmOperationList(); Editor.canvasTapes.createTMTapes( { problemTapeNumber } );

    val initTMScript : NodeSeq = <script type="text/javascript">
      var tm = tmCreator.TuringMachine.createMutableTMWithImmutableProperties(
      { alphabetJavaScriptArray }, { problemTapeNumber }, document.getElementById('svgcanvastm'),
      document.getElementById('sampleinput'),
      "{lastAttemptXml}" );
    </script>

    val problemAlphabetNodeSeq = Text("{" + problemAlphabet.mkString(",") + "}")
    val problemDescriptionNodeSeq = Text(generalProblem.getLongDescription)
    val programTextNodeSeq = Text(specificProblem.getProgramText)
    val uselessVars = specificProblem.getUselessVars
    var uselessVarsNodeSeq = Text("");
    if(uselessVars.length() > 0)
    {
      uselessVarsNodeSeq = Text(" Tape(s) " + specificProblem.getUselessVars + " will always start with \"0\".")
    }

    val hideSubmitButton : JsCmd = JsHideId("submitbutton")
    val ajaxCall : JsCmd = SHtml.ajaxCall(JsRaw("tm.exportToXml()"), grade(_)) //FIXME: check if TM is valid
    val activateStep : JsCmd = JsRaw("tm.allowSimulation()")
    val submitButton : NodeSeq = <button type='button' id='submitbutton' onclick={JsIf(JsRaw("tm.isValid()"), hideSubmitButton & ajaxCall & activateStep, Alert("the tm has at least one invalid link"))}>Submit</button>

    //FIXME: check if TM is valid

    val template : NodeSeq = Templates(List("templates-hidden", "while-to-tm-problem", "solve")) openOr Text("Template /templates-hidden/while-to-tm-problem/solve not found")
    return SHtml.ajaxForm(Helpers.bind("dfaeditform", template,
        "alphabetscript" -> alphabetScript,
        "inittmscript" -> initTMScript,
        "alphabettext" -> problemAlphabetNodeSeq,
        "problemdescription" -> problemDescriptionNodeSeq,
        "programtext" -> programTextNodeSeq,
        "uselessvars" -> uselessVarsNodeSeq,
        "submitbutton" -> submitButton))
  }

  override def onDelete( generalProblem : Problem ) : Unit = {
    WhileToTMProblem.deleteByGeneralProblem(generalProblem)
  }
}

// We have this as an extra class in order to get around Lift's problems with proper capitalization when looking for snippets
class Turingmachinecreationsnippet2 {
  def preprocessAutomatonXml ( input : String ) : String = input.filter(!List('\n', '\r').contains(_)).replace("\u0027", "\'")

  /*def editform( xhtml : NodeSeq ) : NodeSeq = {
    val unspecificProblem : Problem = chosenProblem
    val tmConstructionProblem : TMConstructionProblem = TMConstructionProblem.findByGeneralProblem(chosenProblem)

    var shortDescription : String = chosenProblem.getShortDescription
    var longDescription : String = chosenProblem.getLongDescription
    var wordTest : String = tmConstructionProblem.getWordTest
    var maxTestingSteps : String = tmConstructionProblem.getMaxTestingSteps
    var automaton : String = "" // Will get replaced by an XML-description of the canvas anyways

    def edit() = {
      unspecificProblem.setShortDescription(shortDescription).setLongDescription(longDescription).save
      tmConstructionProblem.setAutomaton(automaton).setWordTest(wordTest).setMaxTestingSteps(maxTestingSteps).save
      
      S.redirectTo("/problems/index")
    }
    
    val automatonField = SHtml.hidden(automatonXml => automaton = preprocessAutomatonXml(automatonXml), "", "id" -> "automatonField")
    val maxTestingStepsField = SHtml.text(maxTestingSteps, maxTestingSteps = _)
    val wordTestField = SHtml.textarea(wordTest, wordTest = _, "cols" -> "80", "rows" -> "5")
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _)
    val longDescriptionField = SHtml.textarea(longDescription, longDescription = _, "cols" -> "80", "rows" -> "5")
    val submitButton = SHtml.submit("Submit", edit, "onClick" -> "document.getElementById('automatonField').value = Editor.canvas.exportAutomaton()")
    
    Helpers.bind("createform", xhtml,
        "automaton" -> automatonField,
        "maxtestingsteps" -> maxTestingStepsField,
        "inandoutcometotest" -> wordTestField,
        "shortdescription" -> shortDescriptionField,
        "longdescription" -> longDescriptionField,
        "submit" -> submitButton)
  }*/
}