package com.automatatutor.snippet.problems

import com.automatatutor.snippet._
import java.util.{Calendar, Date}
import com.automatatutor.lib.GraderConnection
import com.automatatutor.model._
import com.automatatutor.model.problems._
import net.liftweb.common.{Box, Empty, Full}
import net.liftweb.http.SHtml.ElemAttr.pairToBasic
import net.liftweb.http.{SHtml, Templates}
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.{JsCmd, JsCmds}
import net.liftweb.http.js.JsCmds.{JsHideId, JsShowId, SetHtml, jsExpToJsCmd, _}
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers.strToSuperArrowAssoc

import scala.xml.{NodeSeq, Text, XML}

object PDAWordProblemSnippet extends SpecificProblemSnippet {
  def preprocessAutomatonXml(input: String): String = input.filter(!List('\n', '\r').contains(_)).replace("\"", "\'") //'

  /**
    * Should produce a NodeSeq that allows the user to create a new problem of
    * the type. This NodeSeq also has to handle creation of the unspecific
    */
  override def renderCreate(createUnspecificProb: (String, String) => Problem,
                             returnFunc:          (Problem) => Unit) : NodeSeq = {
    var shortDescription: String = ""
    var numberOfWordsInLanguage: Int = 2
    var numberOfWordsNotInLanguage: Int = 1
    var automaton: String = ""
    var allowSimulation: Boolean = true

    def create() = {
      val unspecificProblem = createUnspecificProb(shortDescription, shortDescription)
      val specificProblem: PDAWordProblem = PDAWordProblem.create
      specificProblem.setGeneralProblem(unspecificProblem)
        .setAutomaton(automaton)
        .setNumberOfWordsInLanguage(numberOfWordsInLanguage)
        .setNumberOfWordsNotInLanguage(numberOfWordsNotInLanguage)
        .setAllowSimulation(allowSimulation)
      specificProblem.save
      returnFunc(unspecificProblem)
    }

    val automatonField = SHtml.hidden(automatonXml => automaton = preprocessAutomatonXml(automatonXml), "", "id" -> "automatonField")
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _)
    val numberOfWordsInLanguageField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Empty, value => numberOfWordsInLanguage = value.toInt)
    val numberOfWordsNotInLanguageField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Empty, value => numberOfWordsNotInLanguage = value.toInt)
    val allowSimulationField = SHtml.checkbox(true, res => allowSimulation = res)
    val submitButton = SHtml.submit("Create", create, "onClick" -> JsIf(JsRaw("pda.isValid()"), JsRaw("document.getElementById('automatonField').value = pda.exportToXml()"), Alert("the pda has invalid links") & JsReturn(false)).toJsCmd)

    val runner = new PDASimulationRunner()

    val template: NodeSeq = Templates(List("templates-hidden", "pda-word-problem", "create")) openOr Text("Could not find template /templates-hidden/pda-word-problem/create")
    Helpers.bind("createform", template,
      "word"-> runner.getWordInputField,
      "start"-> runner.getStartButton,
      "automaton" -> automatonField,
      "shortdescription" -> shortDescriptionField,
      "numberofwordsinlanguage" -> numberOfWordsInLanguageField,
      "numberofwordsnotinlanguage" -> numberOfWordsNotInLanguageField,
      "allowsimulation" -> allowSimulationField,
      "submit" -> submitButton)
  }

  /**
    * Should produce a NodeSeq that allows the user to edit the problem
    * associated with the given unspecific problem.
    */
  override def renderEdit: Box[(Problem, Problem => Unit) => NodeSeq] = Full(renderEditFunc)

  private def renderEditFunc(problem: Problem, returnFunc: (Problem => Unit)): NodeSeq = {
    val pdaWordProblem = PDAWordProblem.findByGeneralProblem(problem)
    var shortDescription = problem.getShortDescription
    var numberOfWordsInLanguage: Int = pdaWordProblem.getNumberOfWordsInLanguage
    var numberOfWordsNotInLanguage: Int = pdaWordProblem.getNumberOfWordsNotInLanguage
    var automaton: String = ""
    var allowSimulation : Boolean = pdaWordProblem.getAllowSimulation

    def create() = {
      //TODO: better error handling
      problem.setShortDescription(shortDescription).setLongDescription(shortDescription).save()
      pdaWordProblem
        .setNumberOfWordsInLanguage(numberOfWordsInLanguage)
        .setNumberOfWordsNotInLanguage(numberOfWordsNotInLanguage)
        .setAllowSimulation(allowSimulation)
      if (!automaton.isEmpty) {
        pdaWordProblem.setAutomaton(automaton)
      }
      pdaWordProblem.save()
      returnFunc(problem)
    }

    val automatonField = SHtml.hidden(automatonXml => automaton = preprocessAutomatonXml(automatonXml), "", "id" -> "automatonField")
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _)
    val numberOfWordsInLanguageField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Full[String](numberOfWordsInLanguage.toString), value => numberOfWordsInLanguage = value.toInt)
    val numberOfWordsNotInLanguageField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Full[String](numberOfWordsNotInLanguage.toString), value => numberOfWordsNotInLanguage = value.toInt)
    val allowSimulationField = SHtml.checkbox(allowSimulation, res => allowSimulation = res)
    val submitButton = SHtml.submit("Save", create, "onClick" -> JsIf(JsRaw("pda.isValid()"), JsRaw("document.getElementById('automatonField').value = pda.exportToXml()"), Alert("the pda has at least one invalid link") & JsReturn(false)).toJsCmd)

    val setupScript =
      <script type="text/javascript">
        var pda = pdaCreator.PDA.createMutablePDAFromXMLWithMutableProperties("{preprocessAutomatonXml(pdaWordProblem.getAutomaton)}", document.getElementById('svgcanvaspda'))
      </script>

    val runner = new PDASimulationRunner()

    val template: NodeSeq = Templates(List("templates-hidden", "pda-word-problem", "edit")) openOr Text("Could not find template /templates-hidden/pda-word-problem/edit")
    Helpers.bind("editform", template,
      "word"-> runner.getWordInputField,
      "start"-> runner.getStartButton,
      "automaton" -> automatonField,
      "shortdescription" -> shortDescriptionField,
      "setupscript" -> setupScript,
      "numberofwordsinlanguage" -> numberOfWordsInLanguageField,
      "numberofwordsnotinlanguage" -> numberOfWordsNotInLanguageField,
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

    val pdaWordProblem = PDAWordProblem.findByGeneralProblem(generalProblem)

    var correct = bestGrade() == maxGrade

    val runner = new PDASimulationRunner(() => pdaWordProblem.getAllowSimulation || correct || remainingAttempts() <= 0)

    def grade(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val wordsIn = (0 to pdaWordProblem.getNumberOfWordsInLanguage - 1).map(i => (formValuesXml \ "wordsInLanguage" \ ("in" + i)).head.text.replaceAll("\\s", ""))
      val wordsNotIn = (0 to pdaWordProblem.getNumberOfWordsNotInLanguage - 1).map(i => (formValuesXml \ "wordsNotInLanguage" \ ("notin" + i)).head.text.replaceAll("\\s", ""))

      if (remainingAttempts() <= 0) {
        return JsShowId("feedbackdisplay") &
          SetHtml("feedbackdisplay", Text("You do not have any attempts left for this problem"))
      }

      val correctPdaDescription = pdaWordProblem.getXmlDescription.toString
      val attemptTime = Calendar.getInstance.getTime()

      val graderResponse = GraderConnection.getPdaWordProblemFeedback(correctPdaDescription, wordsIn, wordsNotIn, maxGrade.toInt)

      val numericalGrade = graderResponse._1
      val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)

      correct = numericalGrade == maxGrade

      // Only save the specific attempt if we saved the general attempt
      if (generalAttempt != null) {
        PDAWordProblemSolutionAttempt.create.
          solutionAttemptId(generalAttempt).attemptWordsInLanguage((formValuesXml \ "wordsInLanguage").toString()).attemptWordsNotInLanguage((formValuesXml \ "wordsNotInLanguage").toString()).save
      }

      val setNumericalGrade: JsCmd = SetHtml("grade", Text(graderResponse._1.toString + "/" + maxGrade.toString))
      val setFeedback: JsCmd = SetHtml("feedback", graderResponse._2)
      val showFeedback: JsCmd = JsShowId("feedbackdisplay")

      return setNumericalGrade & setFeedback & showFeedback & JsCmds.JsShowId("submitbutton") & runner.updateSimulation()
    }

    val wordsInLanguageFields = (0 to pdaWordProblem.getNumberOfWordsInLanguage - 1)
      .map(i => SHtml.text("", _ => {}, "id" -> ("wordin" + i), "maxlength" -> "75"))
    val wordsInFieldNodeSeq = <ul>
      {wordsInLanguageFields.map(field => <li>
        {field}
      </li>)}
    </ul>

    val wordsNotInLanguageFields = (0 to pdaWordProblem.getNumberOfWordsNotInLanguage - 1)
      .map(i => SHtml.text("", _ => {}, "id" -> ("wordnotin" + i), "maxlength" -> "75"))
    val wordsNotInFieldNodeSeq = <ul>
      {wordsNotInLanguageFields.map(field => <li>
        {field}
      </li>)}
    </ul>

    val wordsInLanguageXmlJs: StringBuilder = new StringBuilder("<wordsInLanguage>")
    (0 to pdaWordProblem.getNumberOfWordsInLanguage - 1).foreach(i => wordsInLanguageXmlJs.append("<in" + i.toString + ">' + cleanInputOf('wordin" + i.toString + "') + '</in" + i.toString + ">"))
    wordsInLanguageXmlJs.append("</wordsInLanguage>")

    val wordsNotInLanguageXmlJs: StringBuilder = new StringBuilder("<wordsNotInLanguage>")
    (0 to pdaWordProblem.getNumberOfWordsNotInLanguage - 1).foreach(i => wordsNotInLanguageXmlJs.append("<notin" + i.toString + ">' + cleanInputOf('wordnotin" + i.toString + "') + '</notin" + i.toString + ">"))
    wordsNotInLanguageXmlJs.append("</wordsNotInLanguage>")

    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<solveattempt>" + wordsInLanguageXmlJs + wordsNotInLanguageXmlJs + "</solveattempt>'"), grade(_))

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ajaxCall & hideSubmitButton}>Submit</button>

    val setupScript = <script type="text/javascript">
      var pda = pdaCreator.PDA.createImmutablePDAFromXMLWithImmutableProperties(
      "{pdaWordProblem.getAutomaton}",
      document.getElementById('svgcanvaspda'))
    </script>

    val template: NodeSeq = Templates(List("templates-hidden", "pda-word-problem", "solve")) openOr Text("Template /templates-hidden/pda-word-problem/solve not found")

    return SHtml.ajaxForm(Helpers.bind("solveform", template,
      "setupscript" -> setupScript,
      "shortdescription" -> generalProblem.getShortDescription,
      "wordsinlanguage" -> wordsInFieldNodeSeq,
      "wordsnotinlanguage" -> wordsNotInFieldNodeSeq,
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
    PDAWordProblem.deleteByGeneralProblem(problem)
  }
}
