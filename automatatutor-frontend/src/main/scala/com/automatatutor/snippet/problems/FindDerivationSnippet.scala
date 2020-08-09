package com.automatatutor.snippet.problems

import com.automatatutor.snippet._
import java.util.Calendar
import java.util.Date
import scala.xml.NodeSeq
import scala.xml.NodeSeq.seqToNodeSeq
import scala.xml.Text
import scala.xml.XML
import com.automatatutor.lib.GraderConnection
import com.automatatutor.model._
import com.automatatutor.model.problems._
import net.liftweb.common.Box
import net.liftweb.common.Full
import net.liftweb.http.SHtml
import net.liftweb.http.Templates
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds._
import net.liftweb.http.js.JsCmds.JsHideId
import net.liftweb.http.js.JsCmds.JsShowId
import net.liftweb.http.js.JsCmds.SetHtml
import net.liftweb.http.js.JsCmds.cmdToString
import net.liftweb.http.js.JsCmds.jsExpToJsCmd
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers._
import net.liftweb.common.Empty

object FindDerivationSnippet extends SpecificProblemSnippet {

  override def renderCreate(createUnspecificProb: (String, String) => Problem,
                             returnFunc:          (Problem) => Unit) : NodeSeq = {

	def create(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val grammar = Grammar.preprocessGrammar((formValuesXml \ "grammarfield").head.text)
      val word = (formValuesXml \ "wordfield").head.text
      val derivationType = (formValuesXml \ "derivationtypefield").head.text.toInt
      val shortDescription = (formValuesXml \ "shortdescfield").head.text

      val parsingErrors = GraderConnection.getGrammarParsingErrors(grammar)

      if (parsingErrors.isEmpty) {
        val unspecificProblem = createUnspecificProb(shortDescription, shortDescription)

        val specificProblem : FindDerivationProblem = FindDerivationProblem.create
        specificProblem.setGeneralProblem(unspecificProblem).setGrammar(grammar).setWord(word).setDerivationType(derivationType)
        specificProblem.save()

        return SHtml.ajaxCall("", (ignored : String) => returnFunc(unspecificProblem))
      } else {
        val error = Grammar.preprocessFeedback(parsingErrors.mkString("<br/>"))
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", Text(error))
      }

    }
	
	//create HTML
    val grammarField = SHtml.textarea("S -> a S b | x | S S", value => {}, "cols" -> "80", "rows" -> "5", "id" -> "grammarfield")
    val shortDescriptionField = SHtml.text("", value => {}, "id" -> "shortdescfield")
	val wordField = SHtml.text("axbaaxbb", value => {}, "id" -> "wordfield")
	val derivationTypeField = SHtml.select(Array(("0", "ANY"), ("1", "LEFTMOST"), ("2", "RIGHTMOST")), Empty, value => {}, "id" -> "derivationtypefield")
    
	//JavaScript
    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val grammarFieldValXmlJs: String = "<grammarfield>' + document.getElementById('grammarfield').value + '</grammarfield>"
    val wordFieldValXmlJs: String = "<wordfield>' + document.getElementById('wordfield').value + '</wordfield>"
    val derivationTypeFieldValXmlJs: String = "<derivationtypefield>' + document.getElementById('derivationtypefield').value + '</derivationtypefield>"
    val shortdescFieldValXmlJs: String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + grammarFieldValXmlJs + wordFieldValXmlJs + derivationTypeFieldValXmlJs + shortdescFieldValXmlJs + "</createattempt>'"), create(_))
    val submit: JsCmd = hideSubmitButton & ajaxCall
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ submit }>Submit</button>

	//bind
    val template : NodeSeq = Templates(List("templates-hidden", "find-derivation-problem", "create")) openOr Text("Could not find template /templates-hidden/find-derivation-problem/create")
    Helpers.bind("createform", template,
        "grammarfield" -> grammarField,
        "wordfield" -> wordField,
        "derivationtypefield" -> derivationTypeField,
        "shortdescription" -> shortDescriptionField,
        "submit" -> submitButton)
  }

  override def renderEdit: Box[(Problem, Problem => Unit) => NodeSeq] = Full(renderEditFunc)

  private def renderEditFunc(problem: Problem, returnFunc: (Problem => Unit)): NodeSeq = {
    val specificProblem = FindDerivationProblem.findByGeneralProblem(problem)
    var shortDescription: String = problem.getShortDescription
    var grammar: String = Grammar.preprocessLoadedGrammar(specificProblem.getGrammar)
    var word: String = specificProblem.getWord
    var derivationType: Int = specificProblem.getDerivationType

    def edit(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val grammar = Grammar.preprocessGrammar((formValuesXml \ "grammarfield").head.text)
      val word = (formValuesXml \ "wordfield").head.text
      val derivationType = (formValuesXml \ "derivationtypefield").head.text.toInt
      val shortDescription = (formValuesXml \ "shortdescfield").head.text

      val parsingErrors = GraderConnection.getGrammarParsingErrors(grammar)

      if (parsingErrors.isEmpty) {
        problem.setShortDescription(shortDescription).save()
        specificProblem.setGrammar(grammar).setWord(word).setDerivationType(derivationType).save()

        return SHtml.ajaxCall("", (ignored : String) => returnFunc(problem))
      } else {
        val error = Grammar.preprocessFeedback(parsingErrors.mkString("<br/>"))
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", Text(error))
      }

    }

    //create HTML
    val grammarField = SHtml.textarea(grammar, value => {}, "cols" -> "80", "rows" -> "5", "id" -> "grammarfield")
    val shortDescriptionField = SHtml.text(shortDescription, value => {}, "id" -> "shortdescfield")
	val wordField = SHtml.text(word, value => {}, "id" -> "wordfield")
	val derivationTypeField = SHtml.select(Array(("0", "ANY"), ("1", "LEFTMOST"), ("2", "RIGHTMOST")), new Full("" + derivationType), value => {}, "id" -> "derivationtypefield")
    
    //JavaScript
    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val grammarFieldValXmlJs: String = "<grammarfield>' + document.getElementById('grammarfield').value + '</grammarfield>"
    val wordFieldValXmlJs: String = "<wordfield>' + document.getElementById('wordfield').value + '</wordfield>"
    val derivationTypeFieldValXmlJs: String = "<derivationtypefield>' + document.getElementById('derivationtypefield').value + '</derivationtypefield>"
    val shortdescFieldValXmlJs: String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<editattempt>" + grammarFieldValXmlJs + wordFieldValXmlJs + derivationTypeFieldValXmlJs + shortdescFieldValXmlJs + "</editattempt>'"), edit(_))
    val submit: JsCmd = hideSubmitButton & ajaxCall
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ submit }>Save</button>

    //put HTML into template
    val template: NodeSeq = Templates(List("templates-hidden", "find-derivation-problem", "edit")) openOr Text("Could not find template /templates-hidden/find-derivation-problem/edit")
    Helpers.bind("editform", template,
        "grammarfield" -> grammarField,
        "wordfield" -> wordField,
        "derivationtypefield" -> derivationTypeField,
        "shortdescription" -> shortDescriptionField,
        "submit" -> submitButton)
  }

  override def renderSolve(generalProblem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                           recordSolutionAttempt: (Int, Date) => SolutionAttempt, returnFunc: (Problem => Unit), remainingAttempts: () => Int,
                           bestGrade: () => Int): NodeSeq = {
    val specificProblem = FindDerivationProblem.findByGeneralProblem(generalProblem)

    def grade(answer: String): JsCmd = {

      if (remainingAttempts() <= 0) {
        return JsShowId("feedbackdisplay") & SetHtml("feedbackdisplay", Text("You do not have any attempts left for this problem. Your final grade is " + bestGrade().toString + "/" + maxGrade.toString + "."))
      }

      val attemptTime = Calendar.getInstance.getTime()
      val gradeAndFeedback = GraderConnection.getFindDerivationFeedback(specificProblem.getGrammar, specificProblem.getWord, specificProblem.getDerivationType, Grammar.preprocessGrammar(answer), maxGrade.toInt)

      var numericalGrade = gradeAndFeedback._1
      val validAttempt = numericalGrade >= 0 
	  if (!validAttempt) { 
	    numericalGrade = 0
	  } else {
	    val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)
		if (generalAttempt != null) {
          FindDerivationSolutionAttempt.create.solutionAttemptId(generalAttempt).attempt(Grammar.preprocessGrammar(answer)).save
        }
	  }

      val setNumericalGrade: JsCmd = SetHtml("grade", Text(numericalGrade.toString + "/" + maxGrade.toString))
      val setFeedback: JsCmd = SetHtml("feedback", Grammar.preprocessFeedback(gradeAndFeedback._2))
      val showFeedback: JsCmd = JsShowId("feedbackdisplay")

      return setNumericalGrade & setFeedback & showFeedback & JsCmds.JsShowId("submitbutton")
    }

    //reconstruct last attempt
    val lastAttemptDerivation = lastAttempt.map({generalAttempt => 
        FindDerivationSolutionAttempt.getByGeneralAttempt(generalAttempt).attempt.is
    }) openOr "S\n\n" + specificProblem.getWord

    //build html
	val grammarText = { specificProblem
    .getGrammar.replaceAll("->", " -> ")
    .replaceAll("=>", " -> ")
    .replaceAll("\\|", " \\| ")
    .replaceAll("\\s{2,}", " ")
    .replaceAll("_", "\u03B5")
    .split("\\s(?=\\S+\\s*->)")
    .map { Text(_) ++ <br/> } reduceLeft (_ ++ _) }
    val derivationField = SHtml.textarea(lastAttemptDerivation, value => {}, "cols" -> "80", "rows" -> "12", "id" -> "derivationfield")
	val wordText = Text(specificProblem.getWord)
	var derivationTypeString = ""
	if (specificProblem.getDerivationType == 1) derivationTypeString = " leftmost "
	if (specificProblem.getDerivationType == 2) derivationTypeString = " rightmost "
	val derivationTypeText = Text(derivationTypeString)
	val derivationTypeField = SHtml.select(Array(("0", "ANY"), ("1", "LEFTMOST"), ("2", "RIGHTMOST")), new Full("" + specificProblem.getDerivationType), value => {}, "id" -> "derivationtypefield")
    
	
    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("document.getElementById('derivationfield').value"), grade(_))
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ hideSubmitButton & ajaxCall }>Submit</button>

    //put HTML into template
    val template: NodeSeq = Templates(List("templates-hidden", "find-derivation-problem", "solve")) openOr Text("Could not find template /templates-hidden/find-derivation-problem/solve")
    Helpers.bind("solveform", template,
      "grammartext" -> grammarText,
      "wordtext" -> wordText,
      "derivationtypetext" -> derivationTypeText,
      "derivationfield" -> derivationField,
      "submitbutton" -> submitButton)
  }

  override def onDelete(generalProblem: Problem): Unit = {
    //can usually stay empty 
  }
}
