package com.automatatutor.snippet.problems

import com.automatatutor.snippet._
import java.util.Calendar
import java.util.Date
import scala.Array.canBuildFrom
import scala.Array.fallbackCanBuildFrom
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
import net.liftweb.http.SHtml.ElemAttr.pairToBasic
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
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.http.js.JE.Call
import net.liftweb.common.Empty

object DescriptionToGrammarSnippet extends SpecificProblemSnippet {

  override def renderCreate(createUnspecificProb: (String, String) => Problem,
                             returnFunc:           (Problem) => Unit) : NodeSeq = {

    def create(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val grammar = Grammar.preprocessGrammar((formValuesXml \ "grammarfield").head.text)
      val name = (formValuesXml \ "namefield").head.text
      val description = (formValuesXml \ "descriptionfield").head.text

      val parsingErrors = GraderConnection.getGrammarParsingErrors(grammar)

      if (parsingErrors.isEmpty) {
        val unspecificProblem = createUnspecificProb(name, description)

        val specificProblem: DescriptionToGrammarProblem = DescriptionToGrammarProblem.create
        specificProblem.problemId(unspecificProblem).grammar(grammar)
        specificProblem.save

        return SHtml.ajaxCall("", (ignored : String) => returnFunc(unspecificProblem))
      } else {
        val error = Grammar.preprocessFeedback(parsingErrors.mkString("<br/>"))
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", Text(error))
      }

    }
    val grammarField = SHtml.textarea("S -> a S b | x", value => {}, "cols" -> "80", "rows" -> "5", "id" -> "grammarfield")
    val nameField = SHtml.text("", value => {}, "id" -> "namefield")
    val descriptionField = SHtml.text("", value => {}, "id" -> "descriptionfield")

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val grammarFieldValXmlJs: String = "<grammarfield>' + document.getElementById('grammarfield').value + '</grammarfield>"
    val nameFieldValXmlJs: String = "<namefield>' + document.getElementById('namefield').value + '</namefield>"
    val descriptionValXmlJs: String = "<descriptionfield>' + document.getElementById('descriptionfield').value + '</descriptionfield>"
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + grammarFieldValXmlJs + nameFieldValXmlJs + descriptionValXmlJs + "</createattempt>'"), create(_))
    val submit: JsCmd = hideSubmitButton & ajaxCall
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ submit }>Save</button>

    val template: NodeSeq = Templates(List("templates-hidden", "description-to-grammar-problem", "create")) openOr Text("Could not find template /templates-hidden/description-to-grammar-problem/create")
    Helpers.bind("createform", template,
      "grammarfield" -> grammarField,
      "namefield" -> nameField,
      "descriptionfield" -> descriptionField,
      "submit" -> submitButton)
  }

  override def renderEdit: Box[(Problem, Problem => Unit) => NodeSeq] = Full(renderEditFunc)

  private def renderEditFunc(problem: Problem, returnFunc: (Problem => Unit)): NodeSeq = {

    val descriptionToGrammarProblem = DescriptionToGrammarProblem.findByGeneralProblem(problem)

    var problemName: String = problem.getName
    var problemDescription: String = problem.getDescription
    var grammar: String = Grammar.preprocessLoadedGrammar(descriptionToGrammarProblem.getGrammar)

    def edit(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val grammar = Grammar.preprocessGrammar((formValuesXml \ "grammarfield").head.text)
      val name = (formValuesXml \ "namefield").head.text
      val description = (formValuesXml \ "descriptionfield").head.text

      val parsingErrors = GraderConnection.getGrammarParsingErrors(grammar)

      if (parsingErrors.isEmpty) {
        problem.setName(name).setDescription(description).save()
        descriptionToGrammarProblem.grammar(grammar).save()
        return SHtml.ajaxCall("", (ignored : String) => returnFunc(problem))
      } else {
        val error = Grammar.preprocessFeedback(parsingErrors.mkString("<br/>"))
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", Text(error))
      }
    }

    val grammarField = SHtml.textarea(grammar, grammar = _, "cols" -> "80", "rows" -> "5", "id" -> "grammarfield")
    val nameField = SHtml.text(problemName, problemName = _, "id" -> "namefield")
    val descriptionField = SHtml.text(problemDescription, problemDescription = _, "id" -> "descriptionfield")

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val grammarFieldValXmlJs: String = "<grammarfield>' + document.getElementById('grammarfield').value + '</grammarfield>"
    val nameFieldValXmlJs: String = "<namefield>' + document.getElementById('namefield').value + '</namefield>"
    val descriptionValXmlJs: String = "<descriptionfield>' + document.getElementById('descriptionfield').value + '</descriptionfield>"
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + grammarFieldValXmlJs + nameFieldValXmlJs + descriptionValXmlJs + "</createattempt>'"), edit(_))

    val submit: JsCmd = hideSubmitButton & ajaxCall

    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ submit }>Submit</button>

    val template: NodeSeq = Templates(List("templates-hidden", "description-to-grammar-problem", "edit")) openOr Text("Could not find template /templates-hidden/description-to-grammar-problem/edit")
    Helpers.bind("editform", template,
      "grammarfield" -> grammarField,
      "namefield" -> nameField,
      "descriptionfield" -> descriptionField,
      "submit" -> submitButton)
  }

  override def renderSolve(generalProblem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                           recordSolutionAttempt: (Int, Date) => SolutionAttempt, returnFunc: (Problem => Unit), remainingAttempts: () => Int,
                           bestGrade: () => Int): NodeSeq = {
    val specificProblem = DescriptionToGrammarProblem.findByGeneralProblem(generalProblem)

    def grade(attemptGrammar: String): JsCmd = {

      if (remainingAttempts() <= 0) {
        return JsShowId("feedbackdisplay") & SetHtml("feedbackdisplay", Text("You do not have any attempts left for this problem. Your final grade is " + bestGrade().toString + "/" + maxGrade.toString + "."))
      }
      val attemptTime = Calendar.getInstance.getTime()

      val gradeAndFeedback = GraderConnection.getDescriptionToGrammarFeedback(specificProblem.grammar.is, Grammar.preprocessGrammar(attemptGrammar), maxGrade.toInt)
	  
	  var numericalGrade = gradeAndFeedback._1
      val validAttempt = numericalGrade >= 0 
	  if (!validAttempt) { 
	    numericalGrade = 0
	  } else {
	    val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)
		if (generalAttempt != null) {
          DescriptionToGrammarSolutionAttempt.create.solutionAttemptId(generalAttempt).attemptGrammar(Grammar.preprocessGrammar(attemptGrammar)).save
        }
	  }

      val setNumericalGrade: JsCmd = SetHtml("grade", Text(numericalGrade.toString + "/" + maxGrade.toString))
      val setFeedback: JsCmd = SetHtml("feedback", Grammar.preprocessFeedback(gradeAndFeedback._2))
      val showFeedback: JsCmd = JsShowId("feedbackdisplay")

      return setNumericalGrade & setFeedback & showFeedback & JsCmds.JsShowId("submitbutton")
    }
	
	//reconstruct last attempt
	val lastAttemptGrammarS = lastAttempt.map({generalAttempt => 
		DescriptionToGrammarSolutionAttempt.getByGeneralAttempt(generalAttempt).attemptGrammar.is
	}) openOr ""

	//build html
    val problemDescription = generalProblem.getDescription
    val grammarField = SHtml.textarea(lastAttemptGrammarS, value => {}, "cols" -> "80", "rows" -> "5", "id" -> "grammarfield")
    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("document.getElementById('grammarfield').value"), grade(_))
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ hideSubmitButton & ajaxCall }>Submit</button>

    val template: NodeSeq = Templates(List("templates-hidden", "description-to-grammar-problem", "solve")) openOr Text("Could not find template /templates-hidden/description-to-grammar-problem/solve")
    Helpers.bind("solveform", template,
      "problemdescription" -> problemDescription,
      "grammarfield" -> grammarField,
      "submitbutton" -> submitButton)
  }

  override def onDelete(generalProblem: Problem): Unit = {

  }
}