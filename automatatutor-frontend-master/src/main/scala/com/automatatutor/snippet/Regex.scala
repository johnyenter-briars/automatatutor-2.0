package com.automatatutor.snippet

import scala.xml.{Elem, NodeSeq, Text, Node}

class Regex {
  def rendersyntax(xhtml: NodeSeq): NodeSeq = {
    val returnSeq =
      <div class="notes">
        <button class="collapsible">HELP: Regular Expression Syntax</button>
        <div class="collapsible-content">
          <div id="notes-content">
            <ul class="notes">
              <li class="note">Terminals:</li>
              <ul class="sublist">
                <li>Any character from the alphabet is a terminal</li>
                <li>Epsilon (ε) is expressed as: \e, \eps or \epsilon</li>
                <li>The empty set is expressed as: \emp or \emptyset</li>
              </ul>
              <li class="note">Operations (R is a regular expression):</li>
              <ul class="sublist">
                <li>Union is expressed as R|R</li>
                <li>Star as R*</li>
                <li>Concatenation as RR</li>
                <li>Plus as R+</li>
              </ul>
            </ul>
          </div>
        </div>
        <script type="text/javascript" src="/javascript/collapsible.js"> </script>
      </div>

    return returnSeq
  }
}


object Regex {
  def encodedToVisual(regex : String) : String = {
    regex.replaceAll("\\\\emptyset|\\\\emp", "∅").replaceAll("\\\\epsilon|\\\\eps|\\\\e", "ε");
  }
  def visualToEncoded(regex : String) : String = {
    regex.replaceAll("∅", "\\\\emp").replaceAll("ε", "\\\\e");
  }
  def wordToVisual(word : String) : String = {
    word.replaceAll("\\\\epsilon|\\\\eps|\\\\e", "ε")
  }
  def preprocessFeedback(feedback: NodeSeq): NodeSeq =
  {
    def dfs(nodes : Seq[Node]): Seq[Node] =
      nodes.map(f = node => node match {
        case Text(text) => Text(text
          .replaceAll("(\\\\\\\\emptyset)|(\\\\\\\\emp)", "∅")
          .replaceAll("(\\\\\\\\epsilon)|(\\\\\\\\eps)|(\\\\\\\\e)", "ε")
          .replaceAll("(\\\\emptyset)|(\\\\emp)", "∅")
          .replaceAll("(\\\\epsilon)|(\\\\eps)|(\\\\e)", "ε"))
        // case Elem(prefix, label, attributes, scope, child @ _*) => Elem(prefix, label, attributes, scope, dfs(child) : _*)
        case other => other
      })
    return dfs(feedback)
  }
}
