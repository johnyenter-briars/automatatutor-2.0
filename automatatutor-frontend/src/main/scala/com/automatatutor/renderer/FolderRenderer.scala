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

  def renderDelete(target : String, asLink : Boolean) : NodeSeq = {
    val onClick: JsCmd = JsRaw(
      "return confirm('Are you sure you want to delete this folder? " +
        "If you do, all student grades under this folder will be lost!')")

    if (asLink) return SHtml.link(
      target,
      () => { folder.delete_! },
      Text("Delete"),
      "onclick" -> onClick.toJsCmd,
      "style" -> "color: red")

    val button: NodeSeq = <button type='button'>Delete</button>
    SHtml.link(target, () => {folder.delete_!}, button, "onclick" -> onClick.toJsCmd,
      "style" -> "color: red")
  }

  def renderDeleteLink : NodeSeq = renderDelete("/main/course/index", true)
  def renderDeleteButton : NodeSeq = renderDelete("/main/course/index", false)

}