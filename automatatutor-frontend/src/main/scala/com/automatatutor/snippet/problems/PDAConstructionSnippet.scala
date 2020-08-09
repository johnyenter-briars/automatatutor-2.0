package com.automatatutor.snippet.problems

import com.automatatutor.snippet._
import java.util.Calendar
import java.util.Date
import scala.xml.NodeSeq
import scala.xml.Text
import com.automatatutor.lib.GraderConnection
import com.automatatutor.model._
import com.automatatutor.model.problems._
import net.liftweb.common.Box
import net.liftweb.common.Full
import net.liftweb.http.SHtml
import net.liftweb.http.SHtml.ElemAttr.pairToBasic
import net.liftweb.http.Templates
import net.liftweb.http.js.JE.{Call, JsRaw}
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds._
import net.liftweb.http.js.JsCmds.JsHideId
import net.liftweb.http.js.JsCmds.JsShowId
import net.liftweb.http.js.JsCmds.SetHtml
import net.liftweb.http.js.JsCmds.jsExpToJsCmd
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers.strToSuperArrowAssoc

object PDAConstructionSnippet extends SpecificProblemSnippet {
  def preprocessAutomatonXml(input: String): String = input.filter(!List('\n', '\r').contains(_)).replace("\"", "\'") //'

  /**
    * Should produce a NodeSeq that allows the user to create a new problem of
    * the type. This NodeSeq also has to handle creation of the unspecific
    */
  override def renderCreate(createUnspecificProb: (String, String) => Problem,
                             returnFunc:          (Problem) => Unit) : NodeSeq = {
    var shortDescription: String = ""
    var longDescription: String = ""
    var automaton: String = ""
    var giveStackAlphabet: Boolean = true
    var allowSimulation : Boolean = true

    def create() = {
      val unspecificProblem = createUnspecificProb(shortDescription, longDescription)
      val specificProblem: PDAConstructionProblem = PDAConstructionProblem.create
      specificProblem.setGeneralProblem(unspecificProblem)
        .setAutomaton(automaton)
        .setGiveStackAlphabet(giveStackAlphabet)
        .setAllowSimulation(allowSimulation)
		.save
      specificProblem.save
      returnFunc(unspecificProblem)
    }

    val automatonField = SHtml.hidden(automatonXml => automaton = preprocessAutomatonXml(automatonXml), "", "id" -> "automatonField")
    val shortDescriptionField = SHtml.text("", shortDescription = _)
    val longDescriptionField = SHtml.textarea("", longDescription = _, "cols" -> "80", "rows" -> "5")
    val giveStackAlphabetField = SHtml.checkbox(true, res => giveStackAlphabet = res)
    val allowSimulationField = SHtml.checkbox(true, res => allowSimulation = res)
    val submitButton = SHtml.submit("Create", create, "onClick" -> JsIf(JsRaw("pda.isValid()"), JsRaw("document.getElementById('automatonField').value = pda.exportToXml()"), Alert("the pda has at least one invalid link") & JsReturn(false)).toJsCmd)

    val runner = new PDASimulationRunner()

    val template: NodeSeq = Templates(List("templates-hidden", "description-to-pda-problem", "create")) openOr Text("Could not find template /templates-hidden/description-to-pda-problem/create")
    Helpers.bind("createform", template,
      "automaton" -> automatonField,
      "word"-> runner.getWordInputField,
      "start"-> runner.getStartButton,
      "shortdescription" -> shortDescriptionField,
      "longdescription" -> longDescriptionField,
      "givestackalphabet" -> giveStackAlphabetField,
      "allowsimulation" -> allowSimulationField,
      "submit" -> submitButton)
  }

  /**
    * Should produce a NodeSeq that allows the user to edit the problem
    * associated with the given unspecific problem.
    */
  override def renderEdit: Box[(Problem, Problem => Unit) => NodeSeq] = Full(renderEditFunc)

  private def renderEditFunc(problem: Problem, returnFunc: (Problem => Unit)): NodeSeq = {
    val pdaConstructionProblem = PDAConstructionProblem.findByGeneralProblem(problem)

    var shortDescription: String = problem.getShortDescription
    var longDescription: String = problem.getLongDescription
    var automaton: String = ""
    var giveStackAlphabet: Boolean = pdaConstructionProblem.getGiveStackAlphabet
    var allowSimulation: Boolean = pdaConstructionProblem.getAllowSimulation

    def create() = {
      //TODO: better error handling
      problem.setShortDescription(shortDescription).setLongDescription(longDescription).save()
      pdaConstructionProblem.setGiveStackAlphabet(giveStackAlphabet)
      pdaConstructionProblem.setAllowSimulation(allowSimulation)
      if (!automaton.isEmpty) {
        pdaConstructionProblem
          .setAutomaton(automaton)
      }
      pdaConstructionProblem.save()
      returnFunc(problem)
    }

    val automatonField = SHtml.hidden(automatonXml => automaton = preprocessAutomatonXml(automatonXml), "", "id" -> "automatonField")
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _)
    val longDescriptionField = SHtml.textarea(longDescription, longDescription = _, "cols" -> "80", "rows" -> "5")
    val giveStackAlphabetField = SHtml.checkbox(giveStackAlphabet, res => giveStackAlphabet = res)
    val allowSimulationField = SHtml.checkbox(allowSimulation, res => allowSimulation = res)
    val submitButton = SHtml.submit("Save", create, "onClick" -> JsIf(JsRaw("pda.isValid()"), JsRaw("document.getElementById('automatonField').value = pda.exportToXml()"), Alert("the pda has at least one invalid link") & JsReturn(false)).toJsCmd)

    val setupScript =
      <script type="text/javascript">
        var pda = pdaCreator.PDA.createMutablePDAFromXMLWithMutableProperties("{preprocessAutomatonXml(pdaConstructionProblem.getAutomaton)}", document.getElementById('svgcanvaspda'))
      </script>

    val runner = new PDASimulationRunner()

    val template: NodeSeq = Templates(List("templates-hidden", "description-to-pda-problem", "edit")) openOr Text("Could not find template /templates-hidden/description-to-pda-problem/edit")
    Helpers.bind("editform", template,
      "automaton" -> automatonField,
      "word"-> runner.getWordInputField,
      "start"-> runner.getStartButton,
      "setupscript" -> setupScript,
      "shortdescription" -> shortDescriptionField,
      "longdescription" -> longDescriptionField,
      "givestackalphabet" -> giveStackAlphabetField,
      "allowsimulation" -> allowSimulationField,
      "submit" -> submitButton)
  }

  /**
    * Should produce a NodeSeq that allows the user a try to solve the problem
    * associated with the given unspecific problem. The function
    * recordSolutionAttempt must be called once for every solution attempt
    * and expects the grade of the attempt (which must be <= maxGrade) and the
    * time the attempt was made. After finishing the solution attempt, the
    * snippet should send the user back to the overview of problems in the
    * set by calling returnToSet
    */
  override def renderSolve(generalProblem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                           recordSolutionAttempt: (Int, Date) => SolutionAttempt, returnFunc: (Problem => Unit), remainingAttempts: () => Int,
                           bestGrade: () => Int): NodeSeq = {

    val pdaConstructionProblem = PDAConstructionProblem.findByGeneralProblem(generalProblem)
    val giveStackAlphabet = pdaConstructionProblem.getGiveStackAlphabet

    var correct = bestGrade() == maxGrade

    val runner = new PDASimulationRunner(() => pdaConstructionProblem.getAllowSimulation || correct || remainingAttempts() <= 0)

    def grade(attemptPdaDescription: String): JsCmd = {
      if (remainingAttempts() <= 0) {
        return JsShowId("feedbackdisplay") &
          SetHtml("feedbackdisplay", Text("You do not have any attempts left for this problem"))
      }

      val correctPdaDescription = pdaConstructionProblem.getXmlDescription.toString
      val attemptTime = Calendar.getInstance.getTime()
      val graderResponse = GraderConnection.getPdaConstructionFeedback(correctPdaDescription, giveStackAlphabet, attemptPdaDescription, maxGrade.toInt)

      val numericalGrade = graderResponse._1
      val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)

      // Only save the specific attempt if we saved the general attempt
      if (generalAttempt != null) {
        PDAConstructionSolutionAttempt.create.
          solutionAttemptId(generalAttempt).attemptAutomaton(attemptPdaDescription).save
      }

      correct = graderResponse._1 == maxGrade

      val setNumericalGrade: JsCmd = SetHtml("grade", Text(graderResponse._1.toString + "/" + maxGrade.toString))
      val setFeedback: JsCmd = SetHtml("feedback", graderResponse._2)
      val showFeedback: JsCmd = JsShowId("feedbackdisplay")

      return setNumericalGrade & setFeedback & showFeedback & JsCmds.JsShowId("submitbutton") & runner.updateSimulation()
    }

    val lastAttemptXml = lastAttempt.map({ generalAttempt =>
      preprocessAutomatonXml(PDAConstructionSolutionAttempt.getByGeneralAttempt(generalAttempt).attemptAutomaton.is)
    }) openOr ""

    val initpadscript = if (giveStackAlphabet) {
      <script type="text/javascript">
        var pda = pdaCreator.PDA.createMutablePDAFromXMLWithImmutableProperties(
        "{preprocessAutomatonXml(pdaConstructionProblem.getXmlDescription.toString)}",
        document.getElementById('svgcanvaspda'),
        "{lastAttemptXml}" )
      </script>
    }
    else {
      <script type="text/javascript">
        var pda = pdaCreator.PDA.createMutablePDAFromXMLWithOnlyStackAlphabetMutable(
        "{preprocessAutomatonXml(pdaConstructionProblem.getXmlDescription.toString)}",
        document.getElementById('svgcanvaspda'),
        "{lastAttemptXml}")
      </script>
    }

    val problemDescriptionNodeSeq = Text(generalProblem.getLongDescription)

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("pda.exportToXml()"), grade)
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={JsIf(JsRaw("pda.isValid()"), hideSubmitButton & ajaxCall, Alert("the pda has at least one invalid link"))}>Submit</button>

    val template: NodeSeq = Templates(List("templates-hidden", "description-to-pda-problem", "solve")) openOr Text("Template /templates-hidden/description-to-pda-problem/solve not found")

    return SHtml.ajaxForm(Helpers.bind("solveform", template,
      "problemdescription" -> problemDescriptionNodeSeq,
      "initpadscript" -> initpadscript,
      "submitbutton" -> submitButton,
      "word"-> runner.getWordInputField,
      "start"-> runner.getStartButton))
  }

  /**
    * Is called before the given unspecific problem is deleted from the database.
    * This method should delete everything associated with the given unspecific
    * problem from the database
    */
  override def onDelete(problem: Problem): Unit = {
    PDAConstructionProblem.deleteByGeneralProblem(problem)
  }
}
