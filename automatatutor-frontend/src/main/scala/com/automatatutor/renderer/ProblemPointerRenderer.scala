package com.automatatutor.renderer

import scala.xml.NodeSeq
import scala.xml.Text
import com.automatatutor.model.{Course, Folder, Problem, ProblemPointer, User}
import com.automatatutor.snippet._
import net.liftweb.http._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds.jsExpToJsCmd

class ProblemPointerRenderer(problemPointer: ProblemPointer) {

  def renderSolve(target: String, asLink: Boolean): NodeSeq = {
    if(asLink){
      SHtml.link(
        target,
        () => {
          CurrentProblemPointerInCourse(problemPointer)
        },
        Text("Solve"))
    }

    SHtml.link(
      target,
      () => {
        CurrentProblemPointerInCourse(problemPointer)
      },
      <button type='button'>Solve</button>)
  }

  def renderSolveLink: NodeSeq = renderSolve("/main/course/problems/solve", true)
  def renderSolveButton: NodeSeq = renderSolve("/main/course/problems/solve", false)


  def renderAccess(target : String, asLink : Boolean) : NodeSeq = {
    if (asLink) return SHtml.link(target, () => {
      CurrentProblemPointerInCourse(problemPointer)}, Text("Edit"))

    val button: NodeSeq = <button type='button'>Edit</button>
    SHtml.link(target, () => {
      CurrentProblemPointerInCourse(problemPointer)}, button)
  }

  def renderAccessLink: NodeSeq = renderAccess("/main/course/problems/editproblemaccess", true)
  def renderAccessButton: NodeSeq = renderAccess("/main/course/problems/editproblemaccess", false)



  def renderDeleteLink: NodeSeq = {
    val onClick: JsCmd = JsRaw(
      "return confirm('Are you sure you want to delete this problem from the folder? " +
        "If you do, all student grades on this problem will be lost!')")

    SHtml.link(
      "/main/course/folders/index",
      () => {
        problemPointer.delete_!
      },
      Text("Delete"),
      "onclick" -> onClick.toJsCmd,
    "style" -> "color: red")
  }
}
