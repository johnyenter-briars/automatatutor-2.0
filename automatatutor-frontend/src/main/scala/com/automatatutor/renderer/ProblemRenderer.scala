package com.automatatutor.renderer

import scala.xml.NodeSeq
import scala.xml.Text

import com.automatatutor.model.Problem
import com.automatatutor.model.User
import com.automatatutor.snippet._

import net.liftweb.common.Empty
import net.liftweb.common.Full
import net.liftweb.http.S
import net.liftweb.http.SHtml
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds._

class ProblemRenderer(problem : Problem) {
  def renderDeleteLink(target: String) : NodeSeq = {
    def function() = problem.delete_!
    val label = if(problem.canBeDeleted) { Text("Delete") } else { Text("Cannot delete Problem") }
    val onclick : JsCmd = if(problem.canBeDeleted) { 
        JsRaw("return confirm('Are you sure you want to delete this problem?')") 
      } else  { 
        JsCmds.Alert("Cannot delete this problem:\n" + problem.getDeletePreventers.mkString("\n")) & JsRaw("return false")
      }

    return SHtml.link(target, function, label, "onclick" -> onclick.toJsCmd, "style" -> "color: red")
  }
}