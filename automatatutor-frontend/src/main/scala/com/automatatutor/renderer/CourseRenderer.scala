package com.automatatutor.renderer

import scala.xml.NodeSeq
import scala.xml.Text
import com.automatatutor.model.{Course, Problem, User}
import com.automatatutor.snippet._
import net.liftweb.http._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds.jsExpToJsCmd

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
}