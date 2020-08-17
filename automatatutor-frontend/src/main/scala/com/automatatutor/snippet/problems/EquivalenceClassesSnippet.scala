package com.automatatutor.snippet.problems

import com.automatatutor.snippet._
import java.util.Calendar
import java.util.Date
import scala.Array.canBuildFrom
import scala.Array.fallbackCanBuildFrom
import scala.xml.{Elem, NodeSeq, Text, XML}
import scala.xml.NodeSeq.seqToNodeSeq
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

object EquivalenceClassesSnippet extends SpecificProblemSnippet {

  override def renderCreate(createUnspecificProb: (String, String) => Problem,
                            returnFunc:          (Problem) => Unit) : NodeSeq = {

    def create(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val regEx = (formValuesXml \ "regexfield").head.text
      val alphabet = (formValuesXml \ "alphabetfield").head.text
      val inNeeded = (formValuesXml \ "inneededfield").head.text.toInt
      val problemType = (formValuesXml \ "problemtypefield").head.text.toInt
      val firstWord = (formValuesXml \ "firstword").head.text
      val secondWord = (formValuesXml \ "secondword").head.text
      val representative = (formValuesXml \ "representative").head.text
      val shortDescription = (formValuesXml \ "shortdescfield").head.text

      val alphabetList = alphabet.split(" ").filter(_.length()>0)
      val parsingErrors = GraderConnection.getRegexParsingErrors(regEx, alphabetList)

      if (parsingErrors.isEmpty) {
        val unspecificProblem = createUnspecificProb(shortDescription, shortDescription)

        val specificProblem: EquivalenceClassesProblem = EquivalenceClassesProblem.create
        specificProblem.problemId(unspecificProblem).regEx(regEx).inNeeded(inNeeded).alphabet(alphabet).problemType(problemType).representative(representative).firstWord(firstWord).secondWord(secondWord)
        specificProblem.save

        return SHtml.ajaxCall("", (ignored : String) => returnFunc(unspecificProblem))
      } else {
        val errors = "<ul>" + parsingErrors.map("<li>" + _ + "</li>").mkString(" ") + "</ul>"
        val errorsXml = XML.loadString(errors)
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", errorsXml)
      }
    }
    def evaluate(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val regEx = (formValuesXml \ "regexfield").head.text
      val alphabet = (formValuesXml \ "alphabetfield").head.text
      val firstWord = (formValuesXml \ "firstword").head.text
      val secondWord = (formValuesXml \ "secondword").head.text

      val alphabetList = alphabet.split(" ").filter(_.length()>0)
      val parsingErrors = GraderConnection.getRegexParsingErrors(regEx, alphabetList)

      if (parsingErrors.isEmpty) {
        val evaluation = GraderConnection.getEquivalencyClassTwoWordsInstructorFeedback(regEx, alphabetList, firstWord, secondWord)
        return JsCmds.SetHtml("evaluation", evaluation)
      } else {
        val errors = "<ul>" + parsingErrors.map("<li>" + _ + "</li>").mkString(" ") + "</ul>"
        val errorsXml = XML.loadString(errors)
        return JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", errorsXml)
      }
    }

    val regexField = SHtml.text("", value => {},  "id" -> "regexfield")
    val alphabetField = SHtml.text("", value => {}, "id" -> "alphabetfield")
    // Custom radio buttons
    val radioProblemType = SHtml.radio(Array("Equivalency between two words", "Provide words from the same equivalcy class"), Empty, value => {}, "id" -> "problemtype")
    val problemTypeField = radioProblemType.items.zipWithIndex.map {case(d, i) =>
      val rid = "pt-%s".format(i)
      <div>
        {d.xhtml.asInstanceOf[Elem] % ("id", rid) % ("name", "problemtype") % ("value", i.toString)}
        <label for={rid}>
          {d.xhtml.asInstanceOf[Elem].attribute("value") match {
          case Some(value) => value
          case _ => "unspecified"
        }
          }
        </label>
      </div>
    }
    val radioWordType = SHtml.radio(Array("Lexicographically smallest", "Any word from the class"), Empty, value => {}, "id" -> "wordstype")
    val wordTypeField = radioWordType.items.zipWithIndex.map {case(d, i) =>
      val rid = "wt-%s".format(i)
      <div>
        {d.xhtml.asInstanceOf[Elem] % ("id", rid) % ("name", "wordtype") % ("value", i.toString)}
        <label for={rid}>
          {d.xhtml.asInstanceOf[Elem].attribute("value") match {
          case Some(value) => value
          case _ => "unspecified"
        }
          }
        </label>
      </div>
    }
    val inNeededField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Empty, value => {}, "id" -> "inneededfield")
    val shortDescriptionField = SHtml.text("", value => {}, "id" -> "shortdescfield")
    val firstWordField = SHtml.text("", value => {}, "id" -> "firstword")
    val secondWordField = SHtml.text("", value => {}, "id" -> "secondword")
    val representativeField = SHtml.text("", value => {}, "id" -> "representative")

    val problemTypeScript =
      <script type="text/javascript">
        $('input[name="problemtype"]').on('click', function(){{
        // Every member of the current radio group except the clicked one
        $('input[name="' + $(this).attr('name') + '"]').not($(this)).trigger('deselect');
        $(`.problem-type-${{$(this).val()}}`).show();
        $('#submitbutton').show();
        }});
        $('input[name="problemtype"]').on('deselect', function(){{
        $(`.problem-type-${{$(this).val()}}`).hide();
        }})
      </script>

    val wordTypeScript =
      <script type="text/javascript">
        $('#wt-0').on('click', function(){{
        $(`.problem-type-2`).hide();
        }});
        $('#wt-1').on('click', function(){{
        $(`.problem-type-2`).show();
        }})
        $(function() {{
        $('#submitbutton').hide();
        $('#wt-0').prop('checked', true);
        }})
      </script>

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val regexFieldValXmlJs: String = "<regexfield>' + document.getElementById('regexfield').value + '</regexfield>"
    val inNeededFieldValXmlJs: String = "<inneededfield>' + document.getElementById('inneededfield').value + '</inneededfield>"
    val shortdescFieldValXmlJs: String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"
    val alphabetFieldValXmlJs: String = "<alphabetfield>' + document.getElementById('alphabetfield').value + '</alphabetfield>"
    val firstWordFieldValXmlJs: String = "<firstword>' + document.getElementById('firstword').value + '</firstword>"
    val secondWordFieldValXmlJs: String = "<secondword>' + document.getElementById('secondword').value + '</secondword>"
    val representativeFieldValXmlJs: String = "<representative>' + document.getElementById('representative').value + '</representative>"
    val problemTypeFieldValXmlJs: String = "<problemtypefield>' + ($('input[name=\"problemtype\"]:checked').val() === '0' ? '0' : (parseInt($('input[name=\"problemtype\"]:checked').val()) + parseInt($('input[name=\"wordtype\"]:checked').val())).toString()) + '</problemtypefield>"
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + regexFieldValXmlJs + inNeededFieldValXmlJs + shortdescFieldValXmlJs + alphabetFieldValXmlJs + firstWordFieldValXmlJs + secondWordFieldValXmlJs + representativeFieldValXmlJs + problemTypeFieldValXmlJs + "</createattempt>'"), create(_))

    //val checkGrammarAndSubmit : JsCmd = JsIf(Call("multipleAlphabetChecks",Call("parseAlphabetByFieldName", "terminalsfield"),Call("parseAlphabetByFieldName", "nonterminalsfield")), hideSubmitButton & ajaxCall)
    //    val submit: JsCmd = hideSubmitButton & ajaxCall
    val checkAlphabetAndSubmit: JsCmd = JsIf(Call("alphabetChecks", Call("parseAlphabetByFieldName", "alphabetfield")), hideSubmitButton & ajaxCall)

    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ checkAlphabetAndSubmit }>Save</button>
    val ajaxEvaluate: JsCmd = SHtml.ajaxCall(JsRaw("'<evaluation>" + regexFieldValXmlJs + alphabetFieldValXmlJs + firstWordFieldValXmlJs + secondWordFieldValXmlJs + "</evaluation>'"), evaluate(_))
    val evaluateButton: NodeSeq = <button type='button' id="evaluatebutton" onclick={ ajaxEvaluate }>Evaluate</button>

    val template: NodeSeq = Templates(List("templates-hidden", "equivalence-classes-problem", "create")) openOr Text("Could not find template /templates-hidden/equivalence-classes-problem/create")
    Helpers.bind("createform", template,
      "regexfield" -> regexField,
      "inneededfield" -> inNeededField,
      "shortdescription" -> shortDescriptionField,
      "alphabetfield" -> alphabetField,
      "problemtype" -> problemTypeField,
      "problemtypescript" -> problemTypeScript,
      "firstwordfield" -> firstWordField,
      "secondwordfield" -> secondWordField,
      "representative" -> representativeField,
      "wordtype" -> wordTypeField,
      "wordtypescript" -> wordTypeScript,
      "numberofwords" -> inNeededField,
      "evaluate" -> evaluateButton,
      "submit" -> submitButton)
  }

  override def renderEdit: Box[(Problem, Problem => Unit) => NodeSeq] = Full(renderEditFunc)

  private def renderEditFunc(problem: Problem, returnFunc: (Problem => Unit)): NodeSeq = {

    val specificProblem = EquivalenceClassesProblem.findByGeneralProblem(problem)

    var shortDescription: String = problem.getName
    var regEx: String = specificProblem.getRegex
    var inNeeded: Int = specificProblem.getInNeeded
    var alphabet: String = specificProblem.getAlphabet
    var problemType: Int = specificProblem.getProblemType
    var firstWord: String = specificProblem.getFirstWord
    var secondWord: String = specificProblem.getSecondWord
    var representative: String = specificProblem.getRepresentative

    def edit(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val regEx = (formValuesXml \ "regexfield").head.text
      val alphabet = (formValuesXml \ "alphabetfield").head.text
      val inNeeded = (formValuesXml \ "inneededfield").head.text.toInt
      val problemType = (formValuesXml \ "problemtypefield").head.text.toInt
      val firstWord = (formValuesXml \ "firstword").head.text
      val secondWord = (formValuesXml \ "secondword").head.text
      val representative = (formValuesXml \ "representative").head.text
      val shortDescription = (formValuesXml \ "shortdescfield").head.text

      val alphabetList = alphabet.split(" ").filter(_.length()>0)
      val parsingErrors = GraderConnection.getRegexParsingErrors(regEx, alphabetList)

      if (parsingErrors.isEmpty) {
        problem.setName(shortDescription).setDescription(shortDescription).save()
        specificProblem.regEx(regEx).inNeeded(inNeeded).alphabet(alphabet).problemType(problemType).representative(representative).firstWord(firstWord).secondWord(secondWord)
        specificProblem.save

        return SHtml.ajaxCall("", (ignored : String) => returnFunc(problem))
      } else {
        val errors = "<ul>" + parsingErrors.map("<li>" + _ + "</li>").mkString(" ") + "</ul>"
        val errorsXml = XML.loadString(errors)
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", errorsXml)
      }
    }
    def evaluate(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      val regEx = (formValuesXml \ "regexfield").head.text
      val alphabet = (formValuesXml \ "alphabetfield").head.text
      val firstWord = (formValuesXml \ "firstword").head.text
      val secondWord = (formValuesXml \ "secondword").head.text

      val alphabetList = alphabet.split(" ").filter(_.length()>0)
      val parsingErrors = GraderConnection.getRegexParsingErrors(regEx, alphabetList)

      if (parsingErrors.isEmpty) {
        val evaluation = GraderConnection.getEquivalencyClassTwoWordsInstructorFeedback(regEx, alphabetList, firstWord, secondWord)
        return JsCmds.SetHtml("evaluation", evaluation)
      } else {
        val errors = "<ul>" + parsingErrors.map("<li>" + _ + "</li>").mkString(" ") + "</ul>"
        val errorsXml = XML.loadString(errors)
        return JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", errorsXml)
      }
    }

    val regexField = SHtml.text(regEx, regEx = _,  "id" -> "regexfield")
    val inNeededField = SHtml.select(Array(("1", "1"), ("2", "2"), ("3", "3"), ("4", "4"), ("5", "5")), Full("" + inNeeded), value => {}, "id" -> "inneededfield")
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _, "id" -> "shortdescfield")
    val alphabetField = SHtml.text(alphabet, alphabet = _, "id" -> "alphabetfield")
    val radioProblemType = SHtml.radio(Array("Equivalency between two words", "Provide words from the same equivalcy class"), Full("" + Math.min(problemType, 1)), value => {}, "id" -> "problemtype")
    val problemTypeField = radioProblemType.items.zipWithIndex.map {case(d, i) =>
      val rid = "pt-%s".format(i)
      <div>
        {d.xhtml.asInstanceOf[Elem] % ("id", rid) % ("name", "problemtype") % ("value", i.toString)}
        <label for={rid}>
          {d.xhtml.asInstanceOf[Elem].attribute("value") match {
          case Some(value) => value
          case _ => "unspecified"
        }
          }
        </label>
      </div>
    }
    val radioWordType = SHtml.radio(Array("Lexicographically smallest", "Any word from the class"), Empty, value => {}, "id" -> "wordstype")
    val wordTypeField = radioWordType.items.zipWithIndex.map {case(d, i) =>
      val rid = "wt-%s".format(i)
      <div>
        {d.xhtml.asInstanceOf[Elem] % ("id", rid) % ("name", "wordtype") % ("value", i.toString)}
        <label for={rid}>
          {d.xhtml.asInstanceOf[Elem].attribute("value") match {
          case Some(value) => value
          case _ => "unspecified"
        }
          }
        </label>
      </div>
    }
    val firstWordField = SHtml.text(firstWord, firstWord = _, "id" -> "firstword")
    val secondWordField = SHtml.text(secondWord, secondWord = _, "id" -> "secondword")
    val representativeField = SHtml.text(representative, representative = _, "id" -> "representative")

    val problemTypeScript =
      <script type="text/javascript">
        $('input[name="problemtype"]').on('click', function(){{
        // Every member of the current radio group except the clicked one
        $('input[name="' + $(this).attr('name') + '"]').not($(this)).trigger('deselect');
        $(`.problem-type-${{$(this).val()}}`).show();
        }});
        $('input[name="problemtype"]').on('deselect', function(){{
        $(`.problem-type-${{$(this).val()}}`).hide();
        }})
        $(function() {{ $(`#pt-${{ {Math.min(problemType, 1)} }}`).trigger('click') }});
      </script>

    val wordTypeScript =
      <script type="text/javascript">
        $('#wt-0').on('click', function(){{
        $(`.problem-type-2`).hide();
        }});
        $('#wt-1').on('click', function(){{
        $(`.problem-type-2`).show();
        }})
        $(function() {{
        if({problemType} > 0)
        $(`#wt-${{ {problemType - 1} }}`).trigger('click');
        else
        $('#wt-0').prop('checked', true);
        }});
      </script>

    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val regexFieldValXmlJs: String = "<regexfield>' + document.getElementById('regexfield').value + '</regexfield>"
    val alphabetFieldValXmlJs: String = "<alphabetfield>' + document.getElementById('alphabetfield').value + '</alphabetfield>"
    val inNeededFieldValXmlJs: String = "<inneededfield>' + document.getElementById('inneededfield').value + '</inneededfield>"
    val shortdescFieldValXmlJs: String = "<shortdescfield>' + document.getElementById('shortdescfield').value + '</shortdescfield>"
    val firstWordFieldValXmlJs: String = "<firstword>' + document.getElementById('firstword').value + '</firstword>"
    val secondWordFieldValXmlJs: String = "<secondword>' + document.getElementById('secondword').value + '</secondword>"
    val representativeFieldValXmlJs: String = "<representative>' + document.getElementById('representative').value + '</representative>"
    val problemTypeFieldValXmlJs: String = "<problemtypefield>' + ($('input[name=\"problemtype\"]:checked').val() === '0' ? '0' : (parseInt($('input[name=\"problemtype\"]:checked').val()) + parseInt($('input[name=\"wordtype\"]:checked').val())).toString()) + '</problemtypefield>"

    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>"+ regexFieldValXmlJs + inNeededFieldValXmlJs + shortdescFieldValXmlJs + alphabetFieldValXmlJs + firstWordFieldValXmlJs + secondWordFieldValXmlJs + representativeFieldValXmlJs + problemTypeFieldValXmlJs +"</createattempt>'"), edit(_))

    val checkAlphabetAndSubmit: JsCmd = JsIf(Call("alphabetChecks", Call("parseAlphabetByFieldName", "alphabetfield")), hideSubmitButton & ajaxCall)

    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ checkAlphabetAndSubmit }>Submit</button>
    val ajaxEvaluate: JsCmd = SHtml.ajaxCall(JsRaw("'<evaluation>" + regexFieldValXmlJs + alphabetFieldValXmlJs + firstWordFieldValXmlJs + secondWordFieldValXmlJs + "</evaluation>'"), evaluate(_))
    val evaluateButton: NodeSeq = <button type='button' id="evaluatebutton" onclick={ ajaxEvaluate }>Evaluate</button>


    val template: NodeSeq = Templates(List("templates-hidden", "equivalence-classes-problem", "edit")) openOr Text("Could not find template /templates-hidden/equivalence-classes-problem/edit")
    Helpers.bind("editform", template,
      "regexfield" -> regexField,
      "inneededfield" -> inNeededField,
      "shortdescription" -> shortDescriptionField,
      "alphabetfield" -> alphabetField,
      "problemtype" -> problemTypeField,
      "problemtypescript" -> problemTypeScript,
      "firstwordfield" -> firstWordField,
      "secondwordfield" -> secondWordField,
      "representative" -> representativeField,
      "wordtype" -> wordTypeField,
      "wordtypescript" -> wordTypeScript,
      "numberofwords" -> inNeededField,
      "evaluate" -> evaluateButton,
      "submit" -> submitButton)
  }

  override def renderSolve(generalProblem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                           recordSolutionAttempt: (Int, Date) => SolutionAttempt, returnFunc: (Problem => Unit),
                           remainingAttempts: () => Int, bestGrade: () => Int): NodeSeq = {
    val specificProblem = EquivalenceClassesProblem.findByGeneralProblem(generalProblem)
    val problemType = specificProblem.getProblemType

    def grade(formValues: String): JsCmd = {
      if (remainingAttempts() <= 0) {
        return JsShowId("feedbackdisplay") & SetHtml("feedbackdisplay", Text("You do not have any attempts left for this problem. Your final grade is " + bestGrade().toString + "/" + maxGrade.toString + "."))
      }

      val formValuesXml = XML.loadString(formValues)
      val alphabetList = specificProblem.getAlphabet.split(" ").filter(_.length()>0)

      val gradeAndFeedback =
        if(problemType == 0){
          // 1 if not equiv and 0 if equiv
          val notEquivalent = (formValuesXml \ "choice").head.text.toInt
          val reason = (formValuesXml \ "reason").head.text
          GraderConnection.getSameEquivalencyClassFeedback(specificProblem.getRegex, alphabetList, specificProblem.getFirstWord, specificProblem.getSecondWord, notEquivalent, reason, maxGrade.toInt)
        }
        else if(problemType == 1){
          val shortest = (formValuesXml \ "shortest").head.text
          GraderConnection.getEquivalentShortestFeedback(specificProblem.getRegex, alphabetList, specificProblem.getRepresentative, shortest, maxGrade.toInt)
        }
        else {
          val wordsIn = new Array[String](specificProblem.getInNeeded);
          var inI = 0
          (formValuesXml \ "ins" \ "in").foreach(
            (inWord) => {
              // Replace spaces
              wordsIn(inI) = inWord.head.text.replaceAll("\\s", "")
              inI = inI + 1
            })
          GraderConnection.getEquivalentWordsFeedback(specificProblem.getRegex, alphabetList, specificProblem.getRepresentative, wordsIn, maxGrade.toInt)
        }

      val numericalGrade = gradeAndFeedback._1
      val attemptTime = Calendar.getInstance.getTime()
      // Recording solution attempt
      val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)

      // Only save the specific attempt if we saved the general attempt
      if (generalAttempt != null) {
        if(problemType == 2)
          EquivalenceClassesSolutionAttempt.create.solutionAttemptId(generalAttempt).attemptWordsIn((formValuesXml \ "ins").toString()).save
        else if(problemType == 1)
          EquivalenceClassesSolutionAttempt.create.solutionAttemptId(generalAttempt).representative((formValuesXml \ "shortest").head.text).save
        else
          EquivalenceClassesSolutionAttempt.create.solutionAttemptId(generalAttempt)
            .areEquivalent((formValuesXml \ "choice").head.text.toInt)
            .reason((formValuesXml \ "reason").head.text)
            .save
      }

      val setNumericalGrade: JsCmd = SetHtml("grade", Text(gradeAndFeedback._1.toString + "/" + maxGrade.toString))
      val setFeedback: JsCmd = SetHtml("feedback", gradeAndFeedback._2)
      val showFeedback: JsCmd = JsShowId("feedbackdisplay")

      return setNumericalGrade & setFeedback & showFeedback & JsCmds.JsShowId("submitbutton")
    }

    //build html
    val regexText = Text(Regex.encodedToVisual(specificProblem.getRegex))
    val alphabet = Text("{" + specificProblem.getAlphabet.split(" ").mkString(",") + "}")
    val representativeText = Text(Regex.wordToVisual(specificProblem.getRepresentative))
    val template: NodeSeq = Templates(List("templates-hidden", "equivalence-classes-problem", "solve")) openOr Text("Could not find template /templates-hidden/equivalence-classes-problem/solve")

    if(problemType == 2) {
      //reconstruct last attempt
      val lastAttemptIn = lastAttempt.map({ generalAttempt =>
        (XML.loadString(EquivalenceClassesSolutionAttempt.getByGeneralAttempt(generalAttempt).attemptWordsIn.is) \ "in")
      }) openOr List()

      var inNeededText = Text(specificProblem.inNeeded + " words")
      if (specificProblem.inNeeded == 1) inNeededText = Text(specificProblem.inNeeded + " word")
      val wordsInFields = new Array[NodeSeq](specificProblem.getInNeeded)
      for (i <- 0 to specificProblem.getInNeeded - 1) {
        val lastAttemt = lastAttemptIn.lift(i).map({ word => word.text }) getOrElse ""
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
      val showSection = <script type="text/javascript"> $('#problem-type-2').show() </script>

      val insValXmlJs: StringBuilder = new StringBuilder("<ins>")
      for (i <- 0 to specificProblem.getInNeeded - 1) {
        insValXmlJs.append("<in>' + sanitizeInputForXML('wordinfield" + i.toString + "') + '</in>")
      }
      insValXmlJs.append("</ins>")

      val hideSubmitButton: JsCmd = JsHideId("submitbutton")
      val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<solveattempt>" + insValXmlJs + "</solveattempt>'"), grade(_))
      val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={hideSubmitButton & ajaxCall}>Submit</button>

      Helpers.bind("solveform", template,
        "inneededtext" -> inNeededText,
        "submitbutton" -> submitButton,
        "wordsin" -> wordsInFieldNodeSeq,
        "regularexpression" -> regexText,
        "alphabet" -> alphabet,
        "representative" -> representativeText,
        "script2" -> showSection
      )
    }
    else if(problemType == 1){
      val lastAttemptRepresentative = lastAttempt.map({ generalAttempt =>
        EquivalenceClassesSolutionAttempt.getByGeneralAttempt(generalAttempt).representative.is
      }) openOr ""

      val shortestField = SHtml.text(lastAttemptRepresentative, value => {}, "id" -> "shortest")
      val shortestValXmlJs = "<shortest>' + document.getElementById('shortest').value + '</shortest>"

      val hideSubmitButton: JsCmd = JsHideId("submitbutton")
      val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<solveattempt>" + shortestValXmlJs + "</solveattempt>'"), grade(_))
      val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={hideSubmitButton & ajaxCall}>Submit</button>

      val showSection = <script type="text/javascript"> $('#problem-type-1').show(); </script>
      Helpers.bind("solveform", template,
        "submitbutton" -> submitButton,
        "regularexpression" -> regexText,
        "alphabet" -> alphabet,
        "representative" -> representativeText,
        "shortest" -> shortestField,
        "script1" -> showSection
      )
    }
    else{
      // Problemtype 0
      val lastAttemptChoice = lastAttempt.map({generalAttempt =>
        EquivalenceClassesSolutionAttempt.getByGeneralAttempt(generalAttempt).areEquivalent.is
      }) openOr -1
      val lastAttemptReason = lastAttempt.map({generalAttempt =>
        EquivalenceClassesSolutionAttempt.getByGeneralAttempt(generalAttempt).reason.is
      }) openOr ""

      val radioChoice = SHtml.radio(Array("Equivalent", "Not equivalent"), Empty, value => {}, "id" -> "problemtype")
      val choiceField = radioChoice.items.zipWithIndex.map {case(d, i) =>
        val rid = "eq-%s".format(i)
        <div>
          {d.xhtml.asInstanceOf[Elem] % ("id", rid) % ("name", "choice") % ("value", i.toString)}
          <label for={rid}>
            {d.xhtml.asInstanceOf[Elem].attribute("value") match {
            case Some(value) => value
            case _ => "unspecified"
          }
            }
          </label>
        </div>
      }
      val choiceScript =
        <script type="text/javascript">
          $('#eq-0').on('click', function(){{
          $('#contextual').html("Give the language of suffixes:");
          $('#reason-form').show();
          $('#submitbutton').show();
          }});
          $('#eq-1').on('click', function(){{
          $('#contextual').html("Give a differentiating suffix:");
          $('#reason-form').show();
          $('#submitbutton').show();
          }})
          $(function() {{
          $('#problem-type-0').show();
          $('#submitbutton').hide();
          if({lastAttemptChoice} >= 0)
          $(`#eq-${{ {lastAttemptChoice} }}`).trigger('click');
          }});
        </script>
      val reasonField = SHtml.text(lastAttemptReason, value => (), "id" -> "reason")
      val reasonValXmlJs = "<reason>' + document.getElementById('reason').value + '</reason>"
      val choiceValXmlJs = "<choice>' + $('input[name=\"choice\"]:checked').val() + '</choice>"

      val hideSubmitButton: JsCmd = JsHideId("submitbutton")
      val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<solveattempt>" + reasonValXmlJs + choiceValXmlJs + "</solveattempt>'"), grade(_))
      val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={hideSubmitButton & ajaxCall}>Submit</button>

      val firstWordText = Text(Regex.wordToVisual(specificProblem.getFirstWord))
      val secondWordText = Text(Regex.wordToVisual(specificProblem.getSecondWord))
      Helpers.bind("solveform", template,
        "submitbutton" -> submitButton,
        "regularexpression" -> regexText,
        "firstword" -> firstWordText,
        "secondword" -> secondWordText,
        "equivalentradio" -> choiceField,
        "alphabet" -> alphabet,
        "reasonfield" -> reasonField,
        "script0" -> choiceScript
      )
    }
  }

  override def onDelete(generalProblem: Problem): Unit = {

  }

}
