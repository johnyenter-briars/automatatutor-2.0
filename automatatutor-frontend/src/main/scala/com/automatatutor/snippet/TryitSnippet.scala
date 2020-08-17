package com.automatatutor.snippet

import scala.xml.NodeSeq
import scala.xml.Text
import net.liftweb.http.SHtml
import net.liftweb.http.S
import com.automatatutor.model._
import com.automatatutor.lib.GraderConnection
import net.liftweb.common.Empty
import net.liftweb.common.Full
import net.liftweb.http._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds._
import com.automatatutor.lib._
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers.bind
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.http.Templates
import com.automatatutor.renderer.ProblemRenderer

object CurrentTryitExercise extends SessionVar[Exercise](null)

class Tryitsnippet {

  def showiftryitisenabled(content: NodeSeq): NodeSeq = {
    if (Course.findTryitCourse() == Empty) NodeSeq.Empty else content
  }

  def tryitproblemlist(xhtml: NodeSeq): NodeSeq = {
    val tryitCourseBox = Course.findTryitCourse()
    if (tryitCourseBox == Empty) {
      S.warning("There is no try out section...")
      return S.redirectTo("/index")
    }

    val tryitCourse = tryitCourseBox openOrThrowException "tryitCourseBox should not be empty here..."
    val tryitProblems: List[Exercise] = tryitCourse.getVisibleExercises

    if (tryitProblems.isEmpty) return Text("There are currently no problems to try... Please ask the admin to add some!")

    return TableHelper.renderTableWithHeader(
      tryitProblems,
      ("Description", (exercise: Exercise) => Text(exercise.getProblem.getName)),
      ("Problem Type", (exercise: Exercise) => Text(exercise.getProblem.getTypeName)),
      ("", (problem: Exercise) => SHtml.link(
        "/tryit/practice",
        () => {
          CurrentTryitExercise(problem)
        },
        <button type='button'>Try it!</button>)))
  }


  def renderpractice(ignored: NodeSeq): NodeSeq = {
    if (CurrentTryitExercise == null) {
      S.warning("Please first choose a problem")
      return S.redirectTo("/tryit/index")
    }

    val problemPointer: Exercise = CurrentTryitExercise.is
    val problem = problemPointer.getProblem
    val problemSnippet: SpecificProblemSnippet = problem.getProblemType.getProblemSnippet()

    def returnFunc(problem: Problem) = {
      S.redirectTo("/tryit/index")
    }

    val returnLink = SHtml.link("/tryit/index", () => {}, Text("Let me try a different problem!"))

    return problemSnippet.renderSolve(problem, problemPointer.getMaxGrade, Empty,
      (grade, date) => SolutionAttempt, returnFunc, () => 1, () => 0) ++ returnLink
  }
}