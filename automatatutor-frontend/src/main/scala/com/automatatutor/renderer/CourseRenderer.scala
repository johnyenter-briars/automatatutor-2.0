package com.automatatutor.renderer

import scala.xml.NodeSeq
import scala.xml.Text
import com.automatatutor.model.{Course, Problem, Exercise, SolutionAttempt, User}
import com.automatatutor.snippet._
import net.liftweb.http._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds.jsExpToJsCmd
import net.liftweb.mapper.By

class CourseRenderer(course : Course) {
  def renderSelect(target : String, asLink : Boolean) : NodeSeq = {
    if (asLink) return SHtml.link(target, () => {CurrentCourse(course)}, Text(course.getName))
	
	val button: NodeSeq = <button type='button'>View</button>
    return SHtml.link(target, () => {CurrentCourse(course)}, button)
  }
  def renderSelectLink : NodeSeq = renderSelect("/main/course/index", true)
  def renderSelectButton : NodeSeq = renderSelect("/main/course/index", false)
  
  def renderContactLink : NodeSeq = {
    SHtml.link("mailto:" + course.getContact, () => (), Text(course.getContact))
  }
  
  def renderDeleteLink : NodeSeq = renderDeleteLink("/main/index")
  def renderDeleteLink(target: String) : NodeSeq = {
    def function() = course.delete_!
    val label = Text("Delete") 
    val onclick : JsCmd = JsRaw("return confirm('Are you sure you want to delete this course?')") 
    return SHtml.link(target, function, label, "onclick" -> onclick.toJsCmd)
  }

  def renderAverageGrade(user: User): NodeSeq = {
    //get all exercises in the course
    val exercises = Exercise.findAllByCourse(course)

    //map each exercise to the students grade on said exercise
    val gradesPerProblem: List[Float] = exercises.map(_.getGrade(user))

    //take the average across all exercises
    var averageGrade: Float = gradesPerProblem.sum / gradesPerProblem.length
    averageGrade = (averageGrade * 100).toInt

    Text(averageGrade + "%")
  }
}