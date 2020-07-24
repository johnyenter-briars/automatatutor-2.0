package com.automatatutor.renderer

import scala.xml.NodeSeq
import scala.xml.Text
import com.automatatutor.model.{Course, Folder, Problem, User}
import com.automatatutor.snippet._
import net.liftweb.http._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds.jsExpToJsCmd

class FolderRenderer(folder : Folder) {
  def renderSelect(target : String, asLink : Boolean) : NodeSeq = {
    if (asLink) return SHtml.link(target, () => {
      CurrentFolderInCourse(folder)}, Text(folder.getLongDescription))

    val button: NodeSeq = <button type='button'>View</button>
    SHtml.link(target, () => {
      CurrentFolderInCourse(folder)}, button)
  }
  def renderSelectLink : NodeSeq = renderSelect("/main/course/folders/index", true)
  def renderSelectButton : NodeSeq = renderSelect("/main/course/folders/index", false)
}