package com.automatatutor.snippet

import scala.Array.canBuildFrom
import scala.xml._
import com.automatatutor.lib._
import com.automatatutor.model._
import com.automatatutor.renderer.{CourseRenderer, ProblemRenderer}
import com.sun.tracing.Probe
import net.liftweb.common.{Box, Empty, Full}
import net.liftweb.http.{S, _}
import net.liftweb.http.SHtml.ElemAttr.pairToBasic
import net.liftweb.http.SHtml.ElemAttr
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmds
import net.liftweb.mapper.By
import net.liftweb.util.AnyVar.whatVarIs
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers._
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.util.SecurityHelpers
import net.liftweb.util.Mailer
import net.liftweb.util.Mailer._

//TODO 7/17/2020 don't need to use this sessionvar, theres another one that lives in AutogenSnippet that does somethin similar
object CurrentEditableProblem extends SessionVar[Problem](null)

class Problempoolsnippet {
  def renderproblempool(ignored: NodeSeq): NodeSeq ={

//    val editProblemButton = SHtml.link("/main/problempool/edit", () => (), <button type='button'>Edit A Problem</button>)

    val user: User = User.currentUser openOrThrowException "Lift only allows logged-in-users here";
    val usersProblems = Problem.findAllByCreator(user)

    if (usersProblems.isEmpty) return Text("You currently have no autogenerated problems")

    def editProblemButton(problem: Problem): NodeSeq = {
      if (user.hasSupervisedCourses) {
        return SHtml.link(
          "/main/problempool/edit",
          () => {
            CurrentEditableProblem(problem)
          },
          <button type='button'>Edit</button>)
      } else {
        return NodeSeq.Empty
      }
    }

    def sendButton(problem: Problem): NodeSeq = {
      if (user.hasSupervisedCourses) {
        return SHtml.link(
          "/main/problempool/send",
          () => {
            CurrentEditableProblem(problem)
          },
          <button type='button'>Send To Course</button>)
      } else {
        return NodeSeq.Empty
      }
    }


    val deleteAllLink = SHtml.link(
      "/autogen/index",
      () => {
        usersProblems.map(_.delete_!)
      },
      <button type='button'>Delete All</button>,
      "onclick" -> JsRaw("return confirm('Are you sure you want to delete all your autogenerated problems?')").toJsCmd)

    TableHelper.renderTableWithHeader(
      usersProblems,
      ("Description", (problem: Problem) => Text(problem.getShortDescription)),
      ("Problem Type", (problem: Problem) => Text(problem.getTypeName)),
      ("", (problem: Problem) => editProblemButton(problem)),
      ("", (problem: Problem) => SHtml.link(
        "/main/problempool/practice",
        () => {
          CurrentEditableProblem(problem)
        },
        <button type='button'>Solve</button>) ++ sendButton(problem)),
      ("", (problem: Problem) => {
        (new ProblemRenderer(problem)).renderDeleteLink("/autogen/index")
      })) ++ deleteAllLink
  }

  def renderproblemedit(ignored: NodeSeq): NodeSeq ={
    if (CurrentEditableProblem.is == null) {
      S.warning("Please first choose a problem to edit")
      return S.redirectTo("/main/problempool/index")
    }

    val problem : Problem = CurrentEditableProblem.is
    val problemSnippet: SpecificProblemSnippet = problem.getProblemType.getProblemSnippet

    def returnFunc(ignored : Problem) = {
      CurrentProblemInCourse(problem)
      S.redirectTo("/main/problempool/index")
    }

    problemSnippet.renderEdit match {
      case Full(renderFunc) => renderFunc(problem, returnFunc)
      case Empty            =>
        S.error("Editing not implemented for this problem type"); S.redirectTo("/main/course/index")
      case _                => S.error("Error when retrieving editing function"); S.redirectTo("/main/course/index")
    }
  }

  def renderpractice(ignored: NodeSeq): NodeSeq = {
    if (CurrentEditableProblem == null) {
      S.warning("Please first choose a problem")
      return S.redirectTo("/autogen/index")
    }

    val problem: Problem = CurrentEditableProblem.is
    val problemSnippet: SpecificProblemSnippet = problem.getProblemType.getProblemSnippet

    def returnFunc(problem: Problem) = {
      S.redirectTo("/main/problempool/index")
    }

    val returnLink = SHtml.link("/main/problempool/index", () => {}, Text("Return"))

    problemSnippet.renderSolve(problem, 10, Empty,
      (grade, date) => SolutionAttempt, returnFunc, () => 1, () => 0) ++ returnLink
  }

  //TODO 7/17/2020 Have capability to delete a problempointer from a folder
  //also need to have finer control over sending a problem and taking it back
  //also what if that folder already has that problem in it?
  //Better error handling overall
  def sendtocourseform(xhtml: NodeSeq): NodeSeq = {
    if (CurrentEditableProblem.is == null) {
      S.warning("Please first choose a problem to send")
      return S.redirectTo("/main/problempool/index")
    }

    def selectionCallback(folderID: String): Unit = {
      val user: User = User.currentUser openOrThrowException "Lift only allows logged-in-users here";
      val courses = user.getSupervisedCourses
      //The select box remained at "----" meaning "no folder"
      if (folderID.equals("0")) {
        return;
      }

      val folder = Folder.findByID(folderID)
      val problem = CurrentEditableProblem.is
      val problemPointer = new ProblemPointer
      problemPointer.setProblem(problem)
        .setFolder(folder)
        //TODO 7/15/2020 add a method by which the user can set these settings on first transfer
        .setAllowedAttempts(10)
        .setMaxGrade(10)
        .setCourse(folder.getCourse)
        .save
    }

    val user: User = User.currentUser openOrThrowException "Lift only allows logged-in-users here";
    val supervisedCourses = user.getSupervisedCourses

    val cancelButton = SHtml.link("/main/problempool/index", () => {}, <button type='button'>Cancel</button>)

    <form action="/main/problempool/send">
      <p>Please select a folder within
        <strong>ONE</strong>
        course, to transfer the problem to</p>
      <p>Leave the rest as "----"</p>
      {(TableHelper.renderTableWithHeader(
        supervisedCourses,
        ("Name", (course: Course) => Text(course.getName)),
        ("", (course: Course) => {

          val folderOptions = ("0" -> "----") :: Folder.findAllByCourse(course)
            .map(f => (f.getFolderID.toString -> f.getLongDescription.toString))

          val default = folderOptions.head
          SHtml.select(folderOptions, Box(default._1), selectionCallback)
        })
      )
        ++ SHtml.button("Send", () => {}, "type" -> "submit")
        ++ cancelButton
        )}
    </form>
  }


  def renderproblemoptions(ignored: NodeSeq): NodeSeq = {
    if (CurrentFolderInCourse.is == null) {
      S.warning("Please first choose a problem to send")
      return S.redirectTo("/main/course/index")
    }

    def problemsAreIdentical(problem1: Problem, problem2: Problem): Boolean = {
      problem1.getProblemID == problem2.getProblemID
    }

    val user: User = User.currentUser openOrThrowException "Lift only allows logged-in-users here";
    val folder = CurrentFolderInCourse.is
    //Only show the problems which are not in the current folder
    val problems = Problem.findAllByCreator(user).filterNot(folder.getProblemsUnderFolder.contains(_))

    def checkBoxForProblem(potentionalProblem: Problem): NodeSeq = {

      SHtml.checkbox(false, (chosen: Boolean) => {
        if(chosen){
          //TODO 7/21/2020 refactor this to make less ugly and more responsive. Something like returning a JSCmd to alert the user
          //If a ProblemPointer with the same problem already exists with the folder don't add it
          var isDuplicate: Boolean = false
          ProblemPointer.findAllByFolder(folder).map(_.getProblem).foreach(problem =>{
            if(problemsAreIdentical(problem, potentionalProblem)) isDuplicate = true
          })

          if(!isDuplicate){
            val problemPointer = new ProblemPointer
            problemPointer.setProblem(potentionalProblem)
              .setFolder(folder)
              //TODO 7/17/2020 add a method by which the user can set these settings on first transfer
              .setAllowedAttempts(10)
              .setMaxGrade(10)
              .setCourse(folder.getCourse)
              .save
          }
        }
      })
    }

    <form>
      {
        TableHelper.renderTableWithHeader(
          problems,
          ("Description", (problem: Problem) => Text(problem.getShortDescription)),
          ("Problem Type", (problem: Problem) => Text(problem.getTypeName)),
          ("Click to add problem", (problem: Problem) => checkBoxForProblem(problem))
        )
      }
      {
        SHtml.button("Add Problems", () => {
          S.redirectTo("/main/course/index")
        })
      }
    </form>
  }
}

object Problempoolsnippet{
}