package com.automatatutor.snippet

import com.automatatutor.lib.GraderConnection
import net.liftweb.http.SHtml
import net.liftweb.http.SHtml.ElemAttr.pairToBasic
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds._
import net.liftweb.http.js.JsCmds.jsExpToJsCmd
import net.liftweb.util.Helpers.strToSuperArrowAssoc

import scala.xml._

class PDASimulationRunner {

  def this(getCondition: () => Boolean) {
    this()
    this.getSubmitCondition = getCondition

    if (!getSubmitCondition()) {
      button = button % Attribute(None, "disabled", Text("true"), Null)
    }
  }

  var getSubmitCondition: () => Boolean = () => true

  val wordField: Elem = SHtml.text("", _ -> Unit, "placeholder" -> "enter word to simulate", "id" -> "wordToSimulate")

  val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<root>' + pda.exportToXml() + '<word>' + document.getElementById('wordToSimulate').value + '</word></root>'"), runSimulation)
  val callback: JsCmd = JsIf(JsRaw("pda.isValid()"), ajaxCall, Alert("the pda has at least one invalid link"))
  var button: Elem = <button type='button' id='startsimulation' onclick={callback}>Start simulation</button>

  def preprocessAutomatonXml(input: String): String = input.filter(!List('\n', '\r').contains(_)).replace("\"", "\'")

  def runSimulation(xmlPdaString: String): JsCmd = {
    if (getSubmitCondition()) {
      val xml = XML.loadString(xmlPdaString)
      val simulationResponse = GraderConnection.runSimulation((xml \ "automaton").toString, (xml \ "word").toString)
      val res: JsCmd = JsRaw("pda.startSimulation(\"" + preprocessAutomatonXml(simulationResponse.toString) + "\")").cmd
      res
    }
    else {
      val res: JsCmd = Alert("you have to try to solve the exercise first")
      res
    }
  }

  def updateSimulation(): JsCmd = {
    if (getSubmitCondition()) {
      JsRaw("document.getElementById('startsimulation').disabled = false")
    }
    else JsRaw("document.getElementById('startsimulation').disabled = true")
  }

  def getWordInputField: Elem = wordField

  def getStartButton: NodeSeq = button
}
