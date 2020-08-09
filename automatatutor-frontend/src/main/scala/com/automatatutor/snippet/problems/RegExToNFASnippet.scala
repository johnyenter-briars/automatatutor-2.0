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
import net.liftweb.util.{Helpers, Html5}
import net.liftweb.util.Helpers._
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.http.js.JE.Call
import net.liftweb.common.Empty

object RegExToNFASnippet extends SpecificProblemSnippet {
  def preprocessAutomatonXml ( input : String ) : String = {
    val withoutNewlines = input.filter(!List('\n', '\r').contains(_)).replace("\u0027", "\'")
    val asXml = XML.loadString(withoutNewlines)
    val newAutomaton = <automaton> { asXml.child } </automaton>
    return newAutomaton.toString.replace("\"","\'") //Very important line, otherwise app doesn't render
  }

  override def renderCreate(createUnspecificProb: (String, String) => Problem,
                             returnFunc:          (Problem) => Unit) : NodeSeq = {



      def create(formValues: String): JsCmd = {
        val formValuesXml = XML.loadString(formValues)
        val regEx = (formValuesXml \ "regexfield").head.text
        val alphabet = (formValuesXml \ "alphabetfield").head.text
        val shortDescription = (formValuesXml \ "shortdescfield").head.text

        //Keep only the chars
        val alphabetList = alphabet.split(" ").filter(_.length() > 0)
        val parsingErrors = GraderConnection.getRegexParsingErrors(regEx, alphabetList)

        if (parsingErrors.isEmpty) {

          val unspecificProblem = createUnspecificProb(shortDescription, "")

          val alphabetToSave = alphabetList.mkString(" ")
          val specificProblem: RegExToNFAProblem = RegExToNFAProblem.create
          specificProblem.problemId(unspecificProblem).regEx(regEx).alphabet(alphabetToSave)
          specificProblem.save

          return SHtml.ajaxCall("", (ignored : String) => returnFunc(unspecificProblem))

        } else {
          val errors = "<ul>" + parsingErrors.map("<li>" + _ + "</li>").mkString(" ") + "</ul>"
          val errorsXml = XML.loadString(errors)
          return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", errorsXml)
        }
      }

      val alphabetField = SHtml.text("", value => {}, "id" -> "alphabetfield")
      val regExField = SHtml.text("", value => {}, "id" -> "regexfield")
      val shortDescriptionField = SHtml.textarea("", value => {}, "cols" -> "80", "rows" -> "5", "id" -> "shortdescfield")

      val hideSubmitButton: JsCmd = JsHideId("submitbutton")
      val alphabetFieldValXmlJs: String = "<alphabetfield>' + document.getElementById('alphabetfield').value + '</alphabetfield>"
      val regexFieldValXmlJs: String = "<regexfield>' + document.getElementById('regexfield').value + '</regexfield>"
      val shortdescFieldValXmlJs: String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"
      val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + alphabetFieldValXmlJs + regexFieldValXmlJs + shortdescFieldValXmlJs + "</createattempt>'"), create(_))

      val checkAlphabetAndSubmit: JsCmd = JsIf(Call("alphabetChecks", Call("parseAlphabetByFieldName", "alphabetfield")), hideSubmitButton & ajaxCall)

      val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={checkAlphabetAndSubmit}>Submit</button>

      val template: NodeSeq = Templates(List("templates-hidden", "regex-to-nfa-problem", "create")) openOr Text("Could not find template /templates-hidden/regex-to-nfa-problem/create")
      Helpers.bind("createform", template,
        "alphabetfield" -> alphabetField,
        "regexfield" -> regExField,
        "shortdescription" -> shortDescriptionField,
        "submit" -> submitButton)
  }

  override def renderEdit: Box[(Problem, Problem => Unit) => NodeSeq] = Full(renderEditFunc)

  private def renderEditFunc(problem: Problem, returnFunc: (Problem => Unit)): NodeSeq = {

    val regExToNFAProblem = RegExToNFAProblem.findByGeneralProblem(problem)


    var alphabet : String = regExToNFAProblem.getAlphabet
    var shortDescription : String = problem.getShortDescription
    var longDescription : String = problem.getLongDescription
    var regex : String = regExToNFAProblem.getRegex

    def edit(formValues : String) : JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val regEx = (formValuesXml \ "regexfield").head.text
      val alphabet = (formValuesXml \ "alphabetfield").head.text
      val shortDescription = (formValuesXml \ "shortdescfield").head.text

      //Keep only the chars
      val alphabetList = alphabet.split(" ").filter(_.length()>0);
      val parsingErrors = GraderConnection.getRegexParsingErrors(regEx, alphabetList)

      if(parsingErrors.isEmpty) {
          val alphabetToSave = alphabetList.mkString(" ")

          problem.setShortDescription(shortDescription).setLongDescription(longDescription).save()
          regExToNFAProblem.setAlphabet(alphabetToSave).setRegex(regEx).save()
          return SHtml.ajaxCall("", (ignored : String) => returnFunc(problem))
        }
      else {
        val errors = "<ul>" + parsingErrors.map("<li>" + _ + "</li>").mkString(" ") + "</ul>"
        val errorsXml = XML.loadString(errors)
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", errorsXml)
      }
    }

    // Remember to remove all newlines from the generated XML by using filter    
    val alphabetFieldValXmlJs : String = "<alphabetfield>' + document.getElementById('alphabetfield').value + '</alphabetfield>"
    val regexFieldValXmlJs : String = "<regexfield>' + document.getElementById('regexfield').value + '</regexfield>"
    val shortdescFieldValXmlJs : String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"

    val alphabetField = SHtml.text(alphabet, alphabet=_, "id" -> "alphabetfield")
    val regExField = SHtml.text(regex, regex=_, "id" -> "regexfield")
    val shortDescriptionField = SHtml.textarea(shortDescription, shortDescription = _, "cols" -> "80", "rows" -> "5", "id" -> "shortdescfield")

    val ajaxCall : JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + alphabetFieldValXmlJs + regexFieldValXmlJs + shortdescFieldValXmlJs + "</createattempt>'"), edit(_))
    val hideSubmitButton : JsCmd = JsHideId("submitbutton")
    val checkAlphabetAndSubmit : JsCmd = JsIf(Call("alphabetChecks",Call("parseAlphabetByFieldName", "alphabetfield")), hideSubmitButton & ajaxCall)

    val submitButton : NodeSeq = <button type='button' id='submitbutton' onclick={checkAlphabetAndSubmit}>Save</button>

    val template : NodeSeq = Templates(List("templates-hidden", "regex-to-nfa-problem", "edit")) openOr Text("Could not find template /templates-hidden/regex-to-nfa-problem/edit")
    Helpers.bind("editform", template,
      "alphabetfield" -> alphabetField,
      "regexfield" -> regExField,
      "shortdescription" -> shortDescriptionField,
      "submit" -> submitButton)
  }

  override def renderSolve(generalProblem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                           recordSolutionAttempt: (Int, Date) => SolutionAttempt, returnFunc: (Problem => Unit), 
						   remainingAttempts: () => Int, bestGrade: () => Int): NodeSeq = {
    val regexToNFAProblem = RegExToNFAProblem.findByGeneralProblem(generalProblem)

    def grade( attemptNfaDescription : String ) : JsCmd = {
      if(remainingAttempts() <= 0) {
        return JsShowId("feedbackdisplay") &
          SetHtml("feedbackdisplay", Text("You do not have any attempts left for this problem"))
      }

      val attemptNfaXml = XML.loadString(attemptNfaDescription)
      val correctRegex = regexToNFAProblem.getRegex
      val alphabetList = regexToNFAProblem.alphabet.is.split(" ").filter(_.length()>0)
      val attemptTime = Calendar.getInstance.getTime()
      val graderResponse = GraderConnection.getRegexToNfaFeedback(correctRegex, alphabetList,attemptNfaDescription, maxGrade.toInt)

      val numericalGrade = graderResponse._1
      val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)

      // Only save the specific attempt if we saved the general attempt
      if(generalAttempt != null) {
        RegExToNFASolutionAttempt.create.
          solutionAttemptId(generalAttempt).attemptAutomaton( preprocessAutomatonXml(attemptNfaDescription) ).save
      }

      val setNumericalGrade : JsCmd = SetHtml("grade", Text(graderResponse._1.toString + "/" + maxGrade.toString))
      val setFeedback : JsCmd = SetHtml("feedback", graderResponse._2)
      val showFeedback : JsCmd = JsShowId("feedbackdisplay")

      return setNumericalGrade & setFeedback & showFeedback & JsCmds.JsShowId("submitbutton")
    }

//    reconstruct last attempt
    val lastAttemptBlockScript = lastAttempt.map({ generalAttempt =>
      <script type="text/javascript">
        initCanvas();
        Editor.canvas.setAutomaton("{
        preprocessAutomatonXml(RegExToNFASolutionAttempt.getByGeneralAttempt(generalAttempt).attemptAutomaton.is)
        }")
      </script>
    }) openOr <div></div>

    //build html
    val problemAlphabet = regexToNFAProblem.getAlphabet
    val regexText = Text(Regex.encodedToVisual(regexToNFAProblem.getRegex))
    val alphabet = Text("{" + regexToNFAProblem.getAlphabet.split(" ").mkString(",") + "}")

//    val alphabetJavaScriptArray = "[\"" + problemAlphabet.mkString("\",\"") + "\"]"
//    Editor.canvas.setEpsilon( {problemEpsilonFlag} );
    val setAlphabetScript : NodeSeq =
      <script type="text/javascript">
        initCanvas();
        Editor.canvas.setAlphabet("{problemAlphabet}");
        Editor.canvas.setRegex("{regexToNFAProblem.getRegex}");
      </script>

    val problemAlphabetNodeSeq = Text("{" + problemAlphabet.split(" ").mkString(",") + "}")
    val problemDescriptionNodeSeq = Text(generalProblem.getLongDescription)


    val hideSubmitButton : JsCmd = JsHideId("submitbutton")
    val ajaxCall : JsCmd = SHtml.ajaxCall(JsRaw("Editor.canvas.exportAutomaton()"), grade(_))
    val submitButton : NodeSeq = <button type='button' id='submitbutton' onclick={hideSubmitButton & ajaxCall}>Submit</button>

    val template : NodeSeq = Templates(List("templates-hidden", "regex-to-nfa-problem", "solve")) openOr Text("Template /templates-hidden/description-to-nfa-problem/solve not found")
    return SHtml.ajaxForm(Helpers.bind("solveform", template,
      "setalphabetscript" -> setAlphabetScript,
      "alphabet" -> alphabet,
      "regularexpression" -> regexText,
      "blockscript" -> lastAttemptBlockScript,
      "alphabettext" -> problemAlphabetNodeSeq,
      "problemdescription" -> problemDescriptionNodeSeq,
      "submitbutton" -> submitButton))
  }

//  override def renderSolve(generalProblem : Problem, maxGrade : Long, lastAttempt : Box[SolutionAttempt],
//                           recordSolutionAttempt : (Int, Date) => SolutionAttempt, returnFunc : () => Unit, remainingAttempts: () => Int,
//                           bestGrade: () => Int) : NodeSeq = {
//    val specificProblem = RegExToNFAProblem.findByGeneralProblem(generalProblem)
//
//    def grade(regexAttempt : String) : JsCmd = {
//      val alphabetList = specificProblem.alphabet.is.split(" ").filter(_.length()>0)
//      val parsingErrors = GraderConnection.getRegexParsingErrors(regexAttempt, alphabetList)
//
//      if(parsingErrors.isEmpty) {
//        if(remainingAttempts() <= 0) {
//          return JsShowId("feedbackdisplay") & SetHtml("feedbackdisplay", Text("You do not have any attempts left for this problem. Your final grade is " + bestGrade().toString + "/" + maxGrade.toString + "."))
//        }
//        val attemptTime = Calendar.getInstance.getTime()
//
//        val gradeAndFeedback = GraderConnection.getDynamicEquivRegexFeedback(specificProblem.getRegex, specificProblem.getEquivalent, regexAttempt, alphabetList, maxGrade.toInt)
//
//        val numericalGrade = gradeAndFeedback._1
//        val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)
//
//        // Only save the specific attempt if we saved the general attempt
//        if(generalAttempt != null) {
//          RegexConstructionSolutionAttempt.create.solutionAttemptId(generalAttempt).attemptRegex(regexAttempt).save
//        }
//
//        val setNumericalGrade : JsCmd = SetHtml("grade", Text(gradeAndFeedback._1.toString + "/" + maxGrade.toString))
//        val setFeedback : JsCmd = SetHtml("feedback", gradeAndFeedback._2)
//        val showFeedback : JsCmd = JsShowId("feedbackdisplay")
//
//        return setNumericalGrade & setFeedback & showFeedback & JsCmds.JsShowId("submitbutton")
//      } else {
//        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("grade", Text("")) & JsCmds.SetHtml("feedback", Text(parsingErrors.mkString("<br/>")))
//      }
//
//    }
//    // Remember to remove all newlines from the generated XML by using filter
//    val alphabetText = Text("{" + specificProblem.alphabet + "}")
//    val problemDescription = generalProblem.getLongDescription
//    val regExField = SHtml.text("", value => {}, "id" -> "regexfield")
//
//    val hideSubmitButton : JsCmd = JsHideId("submitbutton")
//    val ajaxCall : JsCmd = SHtml.ajaxCall(JsRaw("document.getElementById('regexfield').value"), grade(_))
//    val submitButton : NodeSeq = <button type='button' id='submitbutton' onclick={hideSubmitButton & ajaxCall}>Submit</button>
//
//    val template : NodeSeq = Templates(List("description-to-regex-problem", "solve")) openOr Text("Could not find template /description-to-regex-problem/solve")
//    Helpers.bind("regexsolveform", template,
//      "alphabettext" -> alphabetText,
//      "problemdescription" -> problemDescription,
//      "regexattemptfield" -> regExField,
//      "submitbutton" -> submitButton)
//  }

  override def onDelete( generalProblem : Problem ) : Unit = {

  }
}
