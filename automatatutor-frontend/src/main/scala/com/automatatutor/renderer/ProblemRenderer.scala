package com.automatatutor.renderer

import com.automatatutor.lib.TableHelper

import scala.xml.NodeSeq
import scala.xml.Text
import com.automatatutor.model.{Course, Problem, Exercise, SolutionAttempt, User}
import com.automatatutor.snippet._
import net.liftweb.common.{Box, Empty, Full}
import net.liftweb.http.S
import net.liftweb.http.SHtml
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds._
import net.liftweb.mapper.By

class ProblemRenderer(problem : Problem) {
  def renderDeleteLink(target: String) : NodeSeq = {
    def function() = problem.delete_!
    val label = if(problem.canBeDeleted) { Text("Delete") } else { Text("Cannot delete Problem") }
    val onclick : JsCmd = if(problem.canBeDeleted) { 
        JsRaw("return confirm('Are you sure you want to delete this problem?')") 
      } else  { 
        JsCmds.Alert("Cannot delete this problem:\n" + problem.getDeletePreventers.mkString("\n")) & JsRaw("return false")
      }

    SHtml.link(target, function, label, "onclick" -> onclick.toJsCmd, "style" -> "color: red")
  }

  private def renderProblemInstanceLink(exercise: Exercise): NodeSeq = {
    val course: Course = exercise.getFolder.getCourse.get

    val locationString = course.getName + "/" + exercise.getFolder.getLongDescription

    SHtml.link("/main/course/folders/index", () => {
      CurrentFolderInCourse(exercise.getFolder)
      CurrentCourse(course)
    }, Text(locationString))
  }

  def renderProblemInstances: NodeSeq = {
    val matchingExercises = problem.getProblemInstances

    val problemLocations: List[String] = matchingExercises.map(exercise => {
      val course: Course = exercise.getFolder.getCourse.get

      course.getName + "/" + exercise.getFolder.getLongDescription
    })

    if(matchingExercises.isEmpty) return Text(problemLocations.mkString("\n"))

    <div class="dropdown">
      <button>Appearing In</button>
      <div class="dropdown-content">
        {
          matchingExercises.flatMap(exercise => renderProblemInstanceLink(exercise)).theSeq
        }
      </div>
    </div>
  }

  def renderProblemInstancesTable: NodeSeq = {
    val exercises = problem.getProblemInstances

    TableHelper.renderTableWithHeader(exercises,
      ("Course/Folder", (problem: Exercise) => this.renderProblemInstanceLink(problem))
    )
  }

  def renderProblemStats: NodeSeq = {
    //get all students who tried the problem
    val students = problem.getStudentsWhoAttempted

    //map each student to the number of attempts they have on this problem
    val attemptsPerStudent: List[Int] = students.map(student => {
      SolutionAttempt
        .findAll(By(SolutionAttempt.userId, student))
        .count(_.getExercise.getProblem == problem)
    })

    //compute average number of attempts
    val averageAttempts: Float = attemptsPerStudent.sum.toFloat / attemptsPerStudent.length

    val highestGradesPerUser = students.map(student => {
      //Get all attempts for each user, and filter based on the current problem
      val attempts = SolutionAttempt.findAll(
        By(SolutionAttempt.userId, student))
        .filter(_.getExercise.getProblem == problem)

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