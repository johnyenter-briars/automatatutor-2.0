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

object CYKProblemSnippet extends SpecificProblemSnippet {

  override def renderCreate(createUnspecificProb: (String, String) => Problem,
                             returnFunc:          (Problem) => Unit) : NodeSeq = {

    def create(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val grammar = Grammar.preprocessGrammar((formValuesXml \ "grammarfield").head.text)
      val word = (formValuesXml \ "wordfield").head.text
      val shortDescription = (formValuesXml \ "shortdescfield").head.text

      val parsingErrors = GraderConnection.getCNFParsingErrors(grammar)

      if (parsingErrors.isEmpty) {
        val unspecificProblem = createUnspecificProb(shortDescription, shortDescription)

        val specificProblem: CYKProblem = CYKProblem.create
        specificProblem.problemId(unspecificProblem).grammar(grammar).word(word)
        specificProblem.save

        return SHtml.ajaxCall("", (ignored : String) => returnFunc(unspecificProblem))
      } else {
        val error = Grammar.preprocessFeedback(parsingErrors.mkString("<br/>"))
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", Text(error))
      }

    }
    val grammarField = SHtml.textarea("S -> AS B | x | S S \nAS -> A S \nA -> a \nB -> b", value => {}, "cols" -> "80", "rows" -> "5", "id" -> "grammarfield")
    val wordField = SHtml.text("abaxb", value => {}, "id" -> "wordfield")
    val shortDescriptionField = SHtml.text("", value => {}, "id" -> "shortdescfield")

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val grammarFieldValXmlJs: String = "<grammarfield>' + document.getElementById('grammarfield').value + '</grammarfield>"
    val wordFieldValXmlJs: String = "<wordfield>' + document.getElementById('wordfield').value + '</wordfield>"
    val shortdescFieldValXmlJs: String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + grammarFieldValXmlJs + wordFieldValXmlJs + shortdescFieldValXmlJs + "</createattempt>'"), create(_))

    val submit: JsCmd = hideSubmitButton & ajaxCall

    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ submit }>Submit</button>

    val template: NodeSeq = Templates(List("templates-hidden", "cyk-problem", "create")) openOr Text("Could not find template /templates-hidden/cyk-problem/create")
    Helpers.bind("createform", template,
      "grammarfield" -> grammarField,
      "wordfield" -> wordField,
      "shortdescription" -> shortDescriptionField,
      "submit" -> submitButton)
  }

  override def renderEdit: Box[(Problem, Problem => Unit) => NodeSeq] = Full(renderEditFunc)

  private def renderEditFunc(problem: Problem, returnFunc: (Problem => Unit)): NodeSeq = {

    val cykProblem = CYKProblem.findByGeneralProblem(problem)

    var shortDescription: String = problem.getName
    var grammar: String = Grammar.preprocessLoadedGrammar(cykProblem.getGrammar)
    var word: String = cykProblem.getWord

    def edit(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val grammar = Grammar.preprocessGrammar((formValuesXml \ "grammarfield").head.text)
      val word = (formValuesXml \ "wordfield").head.text
      val shortDescription = (formValuesXml \ "shortdescfield").head.text

      val parsingErrors = GraderConnection.getCNFParsingErrors(grammar)

      if (parsingErrors.isEmpty) {
        problem.setName(shortDescription).setDescription(shortDescription).save()
        cykProblem.grammar(grammar).word(word).save()
        return SHtml.ajaxCall("", (ignored : String) => returnFunc(problem))
      } else {
        val error = Grammar.preprocessFeedback(parsingErrors.mkString("<br/>"))
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", Text(error))
      }
    }

    val grammarField = SHtml.textarea(grammar, grammar = _, "cols" -> "80", "rows" -> "5", "id" -> "grammarfield")
    val wordField = SHtml.text(word, word = _, "id" -> "wordfield")
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _, "id" -> "shortdescfield")

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val grammarFieldValXmlJs: String = "<grammarfield>' + document.getElementById('grammarfield').value + '</grammarfield>"
    val wordFieldValXmlJs: String = "<wordfield>' + document.getElementById('wordfield').value + '</wordfield>"
    val shortdescFieldValXmlJs: String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + grammarFieldValXmlJs + wordFieldValXmlJs + shortdescFieldValXmlJs + "</createattempt>'"), edit(_))

    val submit: JsCmd = hideSubmitButton & ajaxCall

    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ submit }>Save</button>

    val template: NodeSeq = Templates(List("templates-hidden", "cyk-problem", "edit")) openOr Text("Could not find template /templates-hidden/cyk-problem/edit")
    Helpers.bind("editform", template,
      "grammarfield" -> grammarField,
      "wordfield" -> wordField,
      "shortdescription" -> shortDescriptionField,
      "submit" -> submitButton)
  }

  override def renderSolve(generalProblem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                           recordSolutionAttempt: (Int, Date) => SolutionAttempt, returnFunc: (Problem => Unit), remainingAttempts: () => Int,
                           bestGrade: () => Int): NodeSeq = {
    val specificProblem = CYKProblem.findByGeneralProblem(generalProblem)

    def grade(cyk_table: String): JsCmd = {
      if (remainingAttempts() <= 0) {
        return JsShowId("feedbackdisplay") & SetHtml("feedbackdisplay", Text("You do not have any attempts left for this problem. Your final grade is " + bestGrade().toString + "/" + maxGrade.toString + "."))
      }
      val attemptTime = Calendar.getInstance.getTime()

      val gradeAndFeedback = GraderConnection.getCYKFeedback(specificProblem.grammar.is, specificProblem.word.is, cyk_table, maxGrade.toInt)
	  
	  var numericalGrade = gradeAndFeedback._1
      val validAttempt = numericalGrade >= 0 
	  if (!validAttempt) { 
	    numericalGrade = 0
	  } else {
	    val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)
		if (generalAttempt != null) {
          CYKSolutionAttempt.create.solutionAttemptId(generalAttempt).attempt(cyk_table).save
        }
	  }

      val setNumericalGrade: JsCmd = SetHtml("grade", Text(gradeAndFeedback._1.toString + "/" + maxGrade.toString))
      val setFeedback: JsCmd = SetHtml("feedback", Grammar.preprocessFeedback(gradeAndFeedback._2))
      val showFeedback: JsCmd = JsShowId("feedbackdisplay")

      return setNumericalGrade & setFeedback & showFeedback & JsCmds.JsShowId("submitbutton")
    }
	
	//reconstruct last attempt
	var lastAttemptXMLTable = lastAttempt.map({ generalAttempt => 
		XML.loadString( CYKSolutionAttempt.getByGeneralAttempt(generalAttempt).attempt.is ) 
	}) openOr <cyk><cell start="1" end="1">NT1 NT2 ...</cell></cyk>
	
	//build html
    val problemDescription = generalProblem.getDescription
    val grammarText = { specificProblem
      .getGrammar.replaceAll("->", " -> ")
      .replaceAll("=>", " -> ")
      .replaceAll("\\|", " \\| ")
      .replaceAll("\\s{2,}", " ")
      .replaceAll("_", "\u03B5")
      .split("\\s(?=\\S+\\s*->)")
      .map { Text(_) ++ <br/> } reduceLeft (_ ++ _) }
    val wordText = Text("" + specificProblem.word)

	//build table values
    val word = specificProblem.getWord
    val n = word.length()
    val cyk = new Array[Array[(Int, Int, String)]](n);
    for (i <- 0 to n - 1) {
      cyk(i) = new Array[(Int, Int, String)](i + 1)
      for (j <- 0 to i) {
		val start = j + 1
		val end = n + j - i
		//try find cell value in lastAttempt
		var lastAttempt = ""
		(lastAttemptXMLTable \ "cell").foreach(cell => {
			if ((cell \ "@start").text == start.toString && (cell \ "@end").text == end.toString) lastAttempt = cell.text
		})
        cyk(i)(j) = (start, end, lastAttempt)
      }
    }
	//build html table
    val cykTable = <table style="border-collapse: collapse;">{ cyk.map(row => <tr>{ row.map(col => <td><span style="white-space: nowrap;">{ SHtml.text(col._3, value => {}, "class" -> "cyk", "start" -> col._1.toString(), "end" -> col._2.toString(), "size" -> "12") }{ Text("(" + col._1.toString() + "," + col._2.toString() + ")") }</span></td>) }</tr>) } <tr>{ word.map(c => <td style="text-align: center">{ "'" + c.toString + "'" }</td>) }</tr></table>


    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("buildCYKTableXML()"), grade(_))
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ hideSubmitButton & ajaxCall }>Submit</button>

    val template: NodeSeq = Templates(List("templates-hidden", "cyk-problem", "solve")) openOr Text("Could not find template /templates-hidden/cyk-problem/solve")
    Helpers.bind("solveform", template,
      "problemdescription" -> problemDescription,
      "grammartext" -> grammarText,
      "wordtext" -> wordText,
      "cyktable" -> cykTable,
      "submitbutton" -> submitButton)
  }

  override def onDelete(generalProblem: Problem): Unit = {

  }
}
