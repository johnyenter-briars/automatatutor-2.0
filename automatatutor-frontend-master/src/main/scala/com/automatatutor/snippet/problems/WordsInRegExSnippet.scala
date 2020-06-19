package com.automatatutor.snippet.problems

import com.automatatutor.snippet._
import java.util.{Calendar, Date}
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

import scala.xml.{NodeSeq, Text, XML}

object WordsInRegExSnippet extends SpecificProblemSnippet {

  override def renderCreate(createUnspecificProb: (String, String) => Problem,
                             returnFunc:          (Problem) => Unit) : NodeSeq = {

    def create(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val regEx = (formValuesXml \ "regexfield").head.text
      val alphabet = (formValuesXml \ "alphabetfield").head.text
      val inNeeded = (formValuesXml \ "inneededfield").head.text.toInt
      val outNeeded = (formValuesXml \ "outneededfield").head.text.toInt
      val shortDescription = (formValuesXml \ "shortdescfield").head.text

      val alphabetList = alphabet.split(" ").filter(_.length()>0)
      val parsingErrors = GraderConnection.getRegexParsingErrors(regEx, alphabetList)

      if (parsingErrors.isEmpty) {
        val unspecificProblem = createUnspecificProb(shortDescription, shortDescription)

        val specificProblem: WordsInRegExProblem = WordsInRegExProblem.create
        specificProblem.problemId(unspecificProblem).regEx(regEx).inNeeded(inNeeded).outNeeded(outNeeded).alphabet(alphabet)
        specificProblem.save

        return SHtml.ajaxCall("", (ignored : String) => returnFunc(unspecificProblem))
      } else {
          val errors = "<ul>" + parsingErrors.map("<li>" + _ + "</li>").mkString(" ") + "</ul>"
          val errorsXml = XML.loadString(errors)
          return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", errorsXml)
      }

    }
    val regexField = SHtml.text("", value => {},  "id" -> "regexfield")
    val inNeededField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Empty, value => {}, "id" -> "inneededfield")
    val outNeededField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Empty, value => {}, "id" -> "outneededfield")
    val shortDescriptionField = SHtml.text("", value => {}, "id" -> "shortdescfield")
    val alphabetField = SHtml.text("", value => {}, "id" -> "alphabetfield")

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val regexFieldValXmlJs: String = "<regexfield>' + document.getElementById('regexfield').value + '</regexfield>"
    val inNeededFieldValXmlJs: String = "<inneededfield>' + document.getElementById('inneededfield').value + '</inneededfield>"
    val outNeededFieldValXmlJs: String = "<outneededfield>' + document.getElementById('outneededfield').value + '</outneededfield>"
    val shortdescFieldValXmlJs: String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"
    val alphabetFieldValXmlJs: String = "<alphabetfield>' + document.getElementById('alphabetfield').value + '</alphabetfield>"
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + regexFieldValXmlJs + inNeededFieldValXmlJs + outNeededFieldValXmlJs + shortdescFieldValXmlJs + alphabetFieldValXmlJs + "</createattempt>'"), create(_))

    val checkAlphabetAndSubmit : JsCmd = JsIf(Call("alphabetChecks",Call("parseAlphabetByFieldName", "alphabetfield")), hideSubmitButton & ajaxCall)

    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ checkAlphabetAndSubmit }>Submit</button>

    val template: NodeSeq = Templates(List("templates-hidden", "words-in-regex-problem", "create")) openOr Text("Could not find template /templates-hidden/words-in-regex-problem/create")
    Helpers.bind("createform", template,
      "regexfield" -> regexField,
      "inneededfield" -> inNeededField,
      "outneededfield" -> outNeededField,
      "shortdescription" -> shortDescriptionField,
      "alphabetfield" -> alphabetField,
      "submit" -> submitButton)
  }

  override def renderEdit: Box[(Problem, Problem => Unit) => NodeSeq] = Full(renderEditFunc)

  private def renderEditFunc(problem: Problem, returnFunc: (Problem => Unit)): NodeSeq = {
  
    val wordsInRegExProblem = WordsInRegExProblem.findByGeneralProblem(problem)

    var shortDescription: String = problem.getShortDescription
    var regEx: String = wordsInRegExProblem.getRegex
    var inNeeded: Int = wordsInRegExProblem.getInNeeded
    var outNeeded: Int = wordsInRegExProblem.getOutNeeded
    var alphabet: String = wordsInRegExProblem.getAlphabet

    def edit(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val regEx = (formValuesXml \ "regexfield").head.text
      val alphabet = (formValuesXml \ "alphabetfield").head.text
      val inNeeded = (formValuesXml \ "inneededfield").head.text.toInt
      val outNeeded = (formValuesXml \ "outneededfield").head.text.toInt
      val shortDescription = (formValuesXml \ "shortdescfield").head.text

      val alphabetList = alphabet.split(" ").filter(_.length()>0)
      val parsingErrors = GraderConnection.getRegexParsingErrors(regEx, alphabetList)

      if (parsingErrors.isEmpty) {
        problem.setShortDescription(shortDescription).setLongDescription(shortDescription).save()
        wordsInRegExProblem.regEx(regEx).inNeeded(inNeeded).outNeeded(outNeeded).alphabet(alphabet).save()
        return SHtml.ajaxCall("", (ignored : String) => returnFunc(problem))
      } else {
        val errors = "<ul>" + parsingErrors.map("<li>" + _ + "</li>").mkString(" ") + "</ul>"
        val errorsXml = XML.loadString(errors)
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", errorsXml)
      }
    }

    val regexField = SHtml.text(regEx, regEx = _,  "id" -> "regexfield")
    val inNeededField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Full("" + inNeeded), value => {}, "id" -> "inneededfield")
    val outNeededField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Full("" + outNeeded), value => {}, "id" -> "outneededfield")
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _, "id" -> "shortdescfield")
    val alphabetField = SHtml.text(alphabet, alphabet = _, "id" -> "alphabetfield")

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val regexFieldValXmlJs: String = "<regexfield>' + document.getElementById('regexfield').value + '</regexfield>"
    val alphabetFieldValXmlJs: String = "<alphabetfield>' + document.getElementById('alphabetfield').value + '</alphabetfield>"
    val inNeededFieldValXmlJs: String = "<inneededfield>' + document.getElementById('inneededfield').value + '</inneededfield>"
    val outNeededFieldValXmlJs: String = "<outneededfield>' + document.getElementById('outneededfield').value + '</outneededfield>"
    val shortdescFieldValXmlJs: String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"

    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + alphabetFieldValXmlJs +  regexFieldValXmlJs + inNeededFieldValXmlJs + outNeededFieldValXmlJs + shortdescFieldValXmlJs + "</createattempt>'"), edit(_))

    val checkAlphabetAndSubmit : JsCmd = JsIf(Call("alphabetChecks",Call("parseAlphabetByFieldName", "alphabetfield")), hideSubmitButton & ajaxCall)

    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ checkAlphabetAndSubmit }>Submit</button>

    val template: NodeSeq = Templates(List("templates-hidden", "words-in-regex-problem", "edit")) openOr Text("Could not find template /templates-hidden/words-in-regex-problem/edit")
    Helpers.bind("editform", template,
      "inneededfield" -> inNeededField,
      "outneededfield" -> outNeededField,
      "alphabetfield" -> alphabetField,
      "regexfield" -> regexField,
      "shortdescription" -> shortDescriptionField,
      "submit" -> submitButton)
  }

  override def renderSolve(generalProblem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                           recordSolutionAttempt: (Int, Date) => SolutionAttempt, returnFunc: (Problem => Unit), 
						   remainingAttempts: () => Int, bestGrade: () => Int): NodeSeq = {
    val specificProblem = WordsInRegExProblem.findByGeneralProblem(generalProblem)

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

      //Might have a problem here
      val gradeAndFeedback = GraderConnection.getWordsInRegexFeedback(specificProblem.regEx.is, wordsIn, wordsOut, maxGrade.toInt)

      val numericalGrade = gradeAndFeedback._1
      val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)

      // Only save the specific attempt if we saved the general attempt
      if (generalAttempt != null) {
        WordsInRegexSolutionAttempt.create.solutionAttemptId(generalAttempt).attemptWordsIn((formValuesXml \ "ins").toString()).attemptWordsOut((formValuesXml \ "outs").toString()).save
      }

      val setNumericalGrade: JsCmd = SetHtml("grade", Text(gradeAndFeedback._1.toString + "/" + maxGrade.toString))
      val setFeedback: JsCmd = SetHtml("feedback", gradeAndFeedback._2)
      val showFeedback: JsCmd = JsShowId("feedbackdisplay")

      return setNumericalGrade & setFeedback & showFeedback & JsCmds.JsShowId("submitbutton")
    }

    //reconstruct last attempt
    val lastAttemptIn = lastAttempt.map({ generalAttempt =>
      ( XML.loadString(WordsInRegexSolutionAttempt.getByGeneralAttempt(generalAttempt).attemptWordsIn.is) \ "in") }
    ) openOr List()
    val lastAttemptOut = lastAttempt.map({ generalAttempt =>
      ( XML.loadString(WordsInRegexSolutionAttempt.getByGeneralAttempt(generalAttempt).attemptWordsOut.is) \ "out")
    }) openOr List()

    //build html
    val problemDescription = generalProblem.getLongDescription
    val regexText = Text(Regex.encodedToVisual(specificProblem.getRegex))
    val alphabet = Text("{" + specificProblem.getAlphabet.split(" ").mkString(",") + "}")
    var inNeededText = Text(specificProblem.inNeeded + " words")
    if (specificProblem.inNeeded == 1) inNeededText = Text(specificProblem.inNeeded + " word")
    var outNeededText = Text(specificProblem.outNeeded + " words")
    if (specificProblem.outNeeded == 1) outNeededText = Text(specificProblem.outNeeded + " word")
    val wordsInFields = new Array[NodeSeq](specificProblem.getInNeeded)
    for (i <- 0 to specificProblem.getInNeeded - 1) {
      val lastAttemt = (lastAttemptIn.lift(i)).map({ word => word.text}) getOrElse ""
      wordsInFields(i) = SHtml.text(
        lastAttemt,
        value => {},
        "id" -> ("wordinfield" + i.toString),
        "maxlength" -> "75")
    }
    val wordsInFieldNodeSeq = <ul>{ wordsInFields.map(i => <li>{ i }</li>) }</ul>
    val wordsOutFields = new Array[NodeSeq](specificProblem.getOutNeeded)
    for (i <- 0 to specificProblem.getOutNeeded - 1) {
      val lastAttemt = (lastAttemptOut.lift(i)).map({ word => word.text}) getOrElse ""
      wordsOutFields(i) = SHtml.text(
        lastAttemt,
        value => {},
        "id" -> ("wordoutfield" + i.toString),
        "maxlength" -> "75")
    }
    val wordsOutFieldNodeSeq = <ul>{ wordsOutFields.map(i => <li>{ i }</li>) }</ul>

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
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ hideSubmitButton & ajaxCall }>Submit</button>

    val template: NodeSeq = Templates(List("templates-hidden", "words-in-regex-problem", "solve")) openOr Text("Could not find template /templates-hidden/words-in-regex-problem/solve")
    Helpers.bind("solveform", template,
      "problemdescription" -> problemDescription,
      "regularexpression" -> regexText,
      "alphabet" -> alphabet,
      "wordsin" -> wordsInFieldNodeSeq,
      "wordsout" -> wordsOutFieldNodeSeq,
      "inneededtext" -> inNeededText,
      "outneededtext" -> outNeededText,
      "submitbutton" -> submitButton)
  }

  override def onDelete(generalProblem: Problem): Unit = {

  }

}
