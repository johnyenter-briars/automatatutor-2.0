package com.automatatutor.renderer

import scala.xml.NodeSeq
import scala.xml.Text
import com.automatatutor.model.{Course, Folder, Problem, Exercise, SolutionAttempt, User}
import com.automatatutor.snippet._
import net.liftweb.http._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds.jsExpToJsCmd
import net.liftweb.mapper.By

class ExerciseRenderer(exercise: Exercise) {

  def renderSolve(target: String, asLink: Boolean): NodeSeq = {
    if(asLink){
      SHtml.link(
        target,
        () => {
          CurrentExerciseInCourse(exercise)
        },
        Text("Solve"))
    }

    SHtml.link(
      target,
      () => {
        CurrentExerciseInCourse(exercise)
      },
      <button type='button'>Solve</button>)
  }

  def renderSolveLink: NodeSeq = renderSolve("/main/course/problems/solve", true)
  def renderSolveButton: NodeSeq = renderSolve("/main/course/problems/solve", false)

  def renderAccess(target : String, asLink : Boolean) : NodeSeq = {
    if (asLink) return SHtml.link(target, () => {
      CurrentBatchExercisesInCourse.is += exercise
    }, Text("Edit"))

    val button: NodeSeq = <button type='button'>Edit</button>
    SHtml.link(target, () => {
      CurrentBatchExercisesInCourse.is += exercise
    }, button)
  }

  def renderAccessModal: NodeSeq = {
    <div>
      <button type="button" id="batch_send-modal-button" class="modal-button">Batch Send</button>

      <div id="batch_send-modal" class="modal">

        <div class="modal-content">
          <div class="modal-header">
            <span class="close">&times;</span>
            <h3>Send Problems to Folders</h3>
          </div>
          <div class="modal-body">
            <h2>Test</h2>
          </div>
        </div>
      </div>
    </div>
  }

  def renderDelete(target: String): NodeSeq = {
    val onClick: JsCmd = JsRaw(
      "return confirm('Are you sure you want to delete this problem from the folder? " +
        "If you do, all student grades on this problem will be lost!')")

    SHtml.link(
      target,
      () => {
        exercise.delete_!
      },
      Text("Delete"),
      "onclick" -> onClick.toJsCmd,
    "style" -> "color: red")
  }

  def renderDeleteLink: NodeSeq = renderDelete("/main/course/folders/index")

  def renderReferencedProblem(target : String, asLink : Boolean, previousPage: String) : NodeSeq = {
    if (asLink) return SHtml.link(target, () => {
      PreviousPage(previousPage)
      CurrentEditableProblem(exercise.getProblem)}, Text("Edit"))

    SHtml.link(target, () => {
      PreviousPage(previousPage)
      CurrentEditableProblem(exercise.getProblem)}, <button type='button'>Edit</button>)
  }

  def renderReferencedProblemLink(previousPage: String): NodeSeq = renderReferencedProblem("/main/problempool/edit", true, previousPage)
  def renderReferencedProblemButton(previousPage: String): NodeSeq = renderReferencedProblem("/main/problempool/edit", false, previousPage)

  def renderExerciseStats: NodeSeq = {
    //get all students who tried the problem
    val students = exercise.getStudentsWhoAttempted

    //map each student to the number of attempts they have on this problem
    val attemptsPerStudent: List[Int] = students.map(student => {
      SolutionAttempt.findAll(
        By(SolutionAttempt.userId, student),
        By(SolutionAttempt.exerciseId, exercise))
        .length
    })

    //compute average number of attempts
    val averageAttempts: Float = attemptsPerStudent.sum.toFloat / attemptsPerStudent.length

    val highestGradesPerUser = students.map(student => {
      //Get all attempts for each user, and filter based on the current problem
      val attempts = SolutionAttempt.findAll(
        By(SolutionAttempt.userId, student),
        By(SolutionAttempt.exerciseId, exercise))

      //map each attempt at the problem to its grade (points/maxgrade), and take the max
      attempts.map(sa => {
        sa.grade.is.toFloat / sa.getExercise.getMaxGrade
      }).max
    })

    var averageGrade: Float = highestGradesPerUser.sum / highestGradesPerUser.length
    averageGrade = (averageGrade * 100).round

    Text(averageGrade + "%/" + averageAttempts.round)
  }

}
