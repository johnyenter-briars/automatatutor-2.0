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

object WordsInGrammarSnippet extends SpecificProblemSnippet {

  override def renderCreate(createUnspecificProb: (String, String) => Problem,
                            returnFunc: (Problem) => Unit): NodeSeq = {

    def create(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val grammar = Grammar.preprocessGrammar((formValuesXml \ "grammarfield").head.text)
      val inNeeded = (formValuesXml \ "inneededfield").head.text.toInt
      val outNeeded = (formValuesXml \ "outneededfield").head.text.toInt
      val name = (formValuesXml \ "namefield").head.text

      val parsingErrors = GraderConnection.getGrammarParsingErrors(grammar)

      if (parsingErrors.isEmpty) {
        val unspecificProblem = createUnspecificProb(name, name)

        val specificProblem: WordsInGrammarProblem = WordsInGrammarProblem.create
        specificProblem.problemId(unspecificProblem).grammar(grammar).inNeeded(inNeeded).outNeeded(outNeeded)
        specificProblem.save
        return SHtml.ajaxCall("", (ignored: String) => returnFunc(unspecificProblem))
      } else {
        val error = Grammar.preprocessFeedback(parsingErrors.mkString("<br/>"))
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", Text(error))
      }

    }

    val grammarField = SHtml.textarea("S -> a S b | x | S S", value => {}, "cols" -> "80", "rows" -> "5", "id" -> "grammarfield")
    val inNeededField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Empty, value => {}, "id" -> "inneededfield")
    val outNeededField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Empty, value => {}, "id" -> "outneededfield")
    val nameField = SHtml.text("", value => {}, "id" -> "namefield")

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val grammarFieldValXmlJs: String = "<grammarfield>' + document.getElementById('grammarfield').value + '</grammarfield>"
    val inNeededFieldValXmlJs: String = "<inneededfield>' + document.getElementById('inneededfield').value + '</inneededfield>"
    val outNeededFieldValXmlJs: String = "<outneededfield>' + document.getElementById('outneededfield').value + '</outneededfield>"
    val nameFieldValXmlJs: String = "<namefield>' + document.getElementById('namefield').value + '</namefield>"
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + grammarFieldValXmlJs + inNeededFieldValXmlJs + outNeededFieldValXmlJs + nameFieldValXmlJs + "</createattempt>'"), create(_))

    //val checkGrammarAndSubmit : JsCmd = JsIf(Call("multipleAlphabetChecks",Call("parseAlphabetByFieldName", "terminalsfield"),Call("parseAlphabetByFieldName", "nonterminalsfield")), hideSubmitButton & ajaxCall)
    val submit: JsCmd = hideSubmitButton & ajaxCall

    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={submit}>Submit</button>

    val template: NodeSeq = Templates(List("templates-hidden", "words-in-grammar-problem", "create")) openOr Text("Could not find template /templates-hidden/words-in-grammar-problem/create")
    Helpers.bind("createform", template,
      "grammarfield" -> grammarField,
      "inneededfield" -> inNeededField,
      "outneededfield" -> outNeededField,
      "namefield" -> nameField,
      "submit" -> submitButton)
  }

  override def renderEdit: Box[(Problem, Problem => Unit) => NodeSeq] = Full(renderEditFunc)

  private def renderEditFunc(problem: Problem, returnFunc: (Problem => Unit)): NodeSeq = {
    val wordsInGrammarProblem = WordsInGrammarProblem.findByGeneralProblem(problem)

    var problemName: String = problem.getName
    var problemDescription: String = problem.getDescription
    var grammar: String = Grammar.preprocessLoadedGrammar(wordsInGrammarProblem.getGrammar)
    var inNeeded: Int = wordsInGrammarProblem.getInNeeded
    var outNeeded: Int = wordsInGrammarProblem.getOutNeeded

    def edit(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val grammar = Grammar.preprocessGrammar((formValuesXml \ "grammarfield").head.text)
      val inNeeded = (formValuesXml \ "inneededfield").head.text.toInt
      val outNeeded = (formValuesXml \ "outneededfield").head.text.toInt
      val name = (formValuesXml \ "namefield").head.text
      val description = (formValuesXml \ "descriptionfield").head.text

      val parsingErrors = GraderConnection.getGrammarParsingErrors(grammar)

      if (parsingErrors.isEmpty) {
        problem.setName(name).setDescription(description).save()
        wordsInGrammarProblem.grammar(grammar).inNeeded(inNeeded).outNeeded(outNeeded).save()
        return SHtml.ajaxCall("", (ignored: String) => returnFunc(problem))
      } else {
        val error = Grammar.preprocessFeedback(parsingErrors.mkString("<br/>"))
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", Text(error))
      }
    }

    val grammarField = SHtml.textarea(grammar, grammar = _, "cols" -> "80", "rows" -> "5", "id" -> "grammarfield")
    val inNeededField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Full("" + inNeeded), value => {}, "id" -> "inneededfield")
    val outNeededField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Full("" + outNeeded), value => {}, "id" -> "outneededfield")
    val nameField = SHtml.text(problemName, problemName = _, "id" -> "namefield")
    val descriptionField = SHtml.text(problemDescription, problemDescription = _, "id" -> "descriptionfield")

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val grammarFieldValXmlJs: String = "<grammarfield>' + document.getElementById('grammarfield').value + '</grammarfield>"
    val inNeededFieldValXmlJs: String = "<inneededfield>' + document.getElementById('inneededfield').value + '</inneededfield>"
    val outNeededFieldValXmlJs: String = "<outneededfield>' + document.getElementById('outneededfield').value + '</outneededfield>"
    val nameFieldValXmlJs: String = "<namefield>' + document.getElementById('namefield').value + '</namefield>"
    val descriptionValXmlJs: String = "<descriptionfield>' + document.getElementById('descriptionfield').value + '</descriptionfield>"
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + grammarFieldValXmlJs + inNeededFieldValXmlJs + outNeededFieldValXmlJs + nameFieldValXmlJs + descriptionValXmlJs + "</createattempt>'"), edit(_))

    val submit: JsCmd = hideSubmitButton & ajaxCall

    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={submit}>Save</button>

    val template: NodeSeq = Templates(List("templates-hidden", "words-in-grammar-problem", "edit")) openOr Text("Could not find template /templates-hidden/words-in-grammar-problem/edit")
    Helpers.bind("editform", template,
      "grammarfield" -> grammarField,
      "inneededfield" -> inNeededField,
      "outneededfield" -> outNeededField,
      "namefield" -> nameField,
      "descriptionfield" -> descriptionField,
      "submit" -> submitButton)
  }

  override def renderSolve(generalProblem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                           recordSolutionAttempt: (Int, Date) => SolutionAttempt, returnFunc: (Problem => Unit),
                           remainingAttempts: () => Int, bestGrade: () => Int): NodeSeq = {
    val specificProblem = WordsInGrammarProblem.findByGeneralProblem(generalProblem)

    def grade(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)

      val wordsIn = new Array[String](specificProblem.getInNeeded);
      var inI = 0
      (formValuesXml \ "ins" \ "in").foreach(
        (inWord) => {
          wordsIn(inI) = inWord.head.text.replaceAll("\\s", "")
          inI = inI + 1
        })
      val wordsOut = new Array[String](specificProblem.getOutNeeded);
      var outI = 0
      (formValuesXml \ "outs" \ "out").foreach(
        (outWord) => {
          wordsOut(outI) = outWord.head.text.replaceAll("\\s", "")
          outI = outI + 1
        })

      if (remainingAttempts() <= 0) {
        return JsShowId("feedbackdisplay") & SetHtml("feedbackdisplay", Text("You do not have any attempts left for this problem. Your final grade is " + bestGrade().toString + "/" + maxGrade.toString + "."))
      }
      val attemptTime = Calendar.getInstance.getTime()

      val gradeAndFeedback = GraderConnection.getWordsInGrammarFeedback(specificProblem.grammar.is, wordsIn, wordsOut, maxGrade.toInt)

      var numericalGrade = gradeAndFeedback._1
      val validAttempt = numericalGrade >= 0
      if (!validAttempt) {
        numericalGrade = 0
      } else {
        val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)
        if (generalAttempt != null) {
          val ins = Grammar.preprocessGrammar((formValuesXml \ "ins").toString())
          val outs = Grammar.preprocessGrammar((formValuesXml \ "outs").toString())
          WordsInGrammarSolutionAttempt.create.solutionAttemptId(generalAttempt).attemptWordsIn(ins).attemptWordsOut(outs).save
        }
      }

      val setNumericalGrade: JsCmd = SetHtml("grade", Text(gradeAndFeedback._1.toString + "/" + maxGrade.toString))
      val setFeedback: JsCmd = SetHtml("feedback", Grammar.preprocessFeedback(gradeAndFeedback._2))
      val showFeedback: JsCmd = JsShowId("feedbackdisplay")

      return setNumericalGrade & setFeedback & showFeedback & JsCmds.JsShowId("submitbutton")
    }

    //reconstruct last attempt
    val lastAttemptIn = lastAttempt.map({ generalAttempt =>
      (XML.loadString(WordsInGrammarSolutionAttempt.getByGeneralAttempt(generalAttempt).attemptWordsIn.is) \ "in")
    }
    ) openOr List()
    val lastAttemptOut = lastAttempt.map({ generalAttempt =>
      (XML.loadString(WordsInGrammarSolutionAttempt.getByGeneralAttempt(generalAttempt).attemptWordsOut.is) \ "out")
    }) openOr List()

    //build html
    val problemDescription = generalProblem.getDescription
    val grammarText = {
      specificProblem.getGrammar
        .replaceAll("->", " -> ")
        .replaceAll("=>", " -> ")
        .replaceAll("\\|", " \\| ")
        .replaceAll("\\s{2,}", " ")
        .replaceAll("_", "\u03B5")
        .split("\\s(?=\\S+\\s*->)")
        .map {
          Text(_) ++ <br/>
        } reduceLeft (_ ++ _)
    }
    var inNeededText = Text(specificProblem.inNeeded + " words")
    if (specificProblem.inNeeded == 1) inNeededText = Text(specificProblem.inNeeded + " word")
    var outNeededText = Text(specificProblem.outNeeded + " words")
    if (specificProblem.outNeeded == 1) outNeededText = Text(specificProblem.outNeeded + " word")
    val wordsInFields = new Array[NodeSeq](specificProblem.getInNeeded)
    for (i <- 0 to specificProblem.getInNeeded - 1) {
      val lastAttemt = (lastAttemptIn.lift(i)).map({ word => word.text }) getOrElse ""
      wordsInFields(i) = SHtml.text(
        lastAttemt,
        value => {},
        "id" -> ("wordinfield" + i.toString),
        "maxlength" -> "75")
    }
    val wordsInFieldNodeSeq = <ul>
      {wordsInFields.map(i => <li>
        {i}
      </li>)}
    </ul>
    val wordsOutFields = new Array[NodeSeq](specificProblem.getOutNeeded)
    for (i <- 0 to specificProblem.getOutNeeded - 1) {
      val lastAttemt = (lastAttemptOut.lift(i)).map({ word => word.text }) getOrElse ""
      wordsOutFields(i) = SHtml.text(
        lastAttemt,
        value => {},
        "id" -> ("wordoutfield" + i.toString),
        "maxlength" -> "75")
    }
    val wordsOutFieldNodeSeq = <ul>
      {wordsOutFields.map(i => <li>
        {i}
      </li>)}
    </ul>

    val insValXmlJs: StringBuilder = new StringBuilder("<ins>")
    for (i <- 0 to specificProblem.getInNeeded - 1) {
      insValXmlJs.append("<in>' + sanitizeInputForXML('wordinfield" + i.toString + "') + '</in>")
    }
    insValXmlJs.append("</ins>")
    val outsValXmlJs: StringBuilder = new StringBuilder("<outs>")
    for (i <- 0 to specificProblem.getOutNeeded - 1) {
      outsValXmlJs.append("<out>' + sanitizeInputForXML('wordoutfield" + i.toString + "') + '</out>")
    }
    outsValXmlJs.append("</outs>")

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<solveattempt>" + insValXmlJs + outsValXmlJs + "</solveattempt>'"), grade(_))
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={hideSubmitButton & ajaxCall}>Submit</button>

    val template: NodeSeq = Templates(List("templates-hidden", "words-in-grammar-problem", "solve")) openOr Text("Could not find template /templates-hidden/words-in-grammar-problem/solve")
    Helpers.bind("solveform", template,
      "problemdescription" -> problemDescription,
      "grammartext" -> grammarText,
      "wordsin" -> wordsInFieldNodeSeq,
      "wordsout" -> wordsOutFieldNodeSeq,
      "inneededtext" -> inNeededText,
      "outneededtext" -> outNeededText,
      "submitbutton" -> submitButton)
  }

  override def onDelete(generalProblem: Problem): Unit = {

  }
}
