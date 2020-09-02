package com.automatatutor.snippet

import scala.xml.NodeSeq
import scala.xml.Text
import net.liftweb.http.SHtml
import net.liftweb.http.S
import com.automatatutor.model._
import com.automatatutor.lib.GraderConnection
import net.liftweb.common.Box
import net.liftweb.common.Empty
import net.liftweb.common.Full
import net.liftweb.http._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds._
import com.automatatutor.lib._
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers._
import net.liftweb.util.Helpers.bind
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.http.Templates
import com.automatatutor.renderer.ProblemRenderer

object CurrentPracticeProblem extends SessionVar[Problem](null)

class Autogensnippet {
  def autogenerationform(xhtml: NodeSeq): NodeSeq = {
    val user: User = User.currentUser openOrThrowException "Lift only allows logged-in-users here";
    val usersProblems = Problem.findAllByCreator(user)

    val maxAutoGenProblems = 5

    val typeOptions = Array(
      (ProblemType.WordsInGrammarTypeName, ProblemType.WordsInGrammarTypeName),
      (ProblemType.GrammarToCNFTypeName, ProblemType.GrammarToCNFTypeName),
      (ProblemType.CYKTypeName, ProblemType.CYKTypeName),
      (ProblemType.FindDerivationTypeName, ProblemType.FindDerivationTypeName),
      (ProblemType.WhileToTMTypeName, ProblemType.WhileToTMTypeName))

    var difficulty: Int = 50
    var typeString: String = ""

    def generate(): Unit = {
      //only limited nr of generated problem per student user
      if (user.isStudent && usersProblems.length >= maxAutoGenProblems) {
        S.warning("You may only have " + maxAutoGenProblems.toString + " problems.")
        return
      }

      val difficultyMin = difficulty - 10
      val difficultyMax = difficulty + 10

      var response: NodeSeq = GraderConnection.generateProblemBestIn(typeString, difficultyMin, difficultyMax)

      //import
      val createdProblem = Problem.fromXML(response.head)

      if (createdProblem != Empty) {
        S.notice("created new problem with quality=" + (response \\ "@quality").text + " difficulty=" + (response \\ "@difficulty"))
      }
      else {
        S.notice("ERROR: " + response.text)
      }
    }

    val template: NodeSeq = Templates(List("autogen", "autogenFormTemplate")) openOr Text("Could not find template /autogen/autogenFormTemplate")
    Helpers.bind("autogenform", template,
      "typeoptions" -> SHtml.select(typeOptions, Empty, typeString = _, "id" -> "typeSelect"),
      "generatebutton" -> SHtml.submit("Generate", generate),
      "difficultyslider" -> SHtml.range(difficulty, difficulty = _, 10, 90))
  }

  def returntohomelink(xhtml: NodeSeq): NodeSeq = {
    return SHtml.link(
      "/main/index",
      () => {},
      <button type='button'>Return Home</button>)
  }

}