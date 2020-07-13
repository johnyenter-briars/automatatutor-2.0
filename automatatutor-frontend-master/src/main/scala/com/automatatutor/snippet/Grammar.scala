package com.automatatutor.snippet

import scala.xml.{Elem, NodeSeq}

class Grammar {
  def rendergrammarsyntax(xhtml: NodeSeq): NodeSeq = {
	val returnSeq =
		<div class="notes">
			<button class="collapsible">HELP: Grammar Syntax</button>
			<div class="collapsible-content">
			  <div id="notes-content">
				<ul class="notes">
				  <li class="note">Grammar:</li>
				  <ul class="sublist">
					<li>1 production per line</li>
					<li>start nonterminal: left-hand side of first production</li>
				  </ul>
				  <li class="note">Productions:</li>
				  <ul class="sublist">
					<li>form: S -> X Y | a S b | \eps </li>
					<li>left-hand side: a single nonterminal </li>
					<li>right-hand side: groups of nonterminals and terminals separated by "|" </li>
				  </ul>
				  <li class="note">Nonterminals:</li>
				  <ul class="sublist">
					<li>form: one single upper case character A-Z </li>
				  </ul>
				  <li class="note">Terminals:</li>
				  <ul class="sublist">
					<li>allowed: abcdefghijklmnopqrstuvwxyz()[]{{}}</li>
				  </ul>
				  <li class="note">Empty word: </li>
				  <ul class="sublist">
					<li>empty word: &epsilon; (write \epsilon or \eps or \e)</li>				  
				  </ul>
				</ul>
			  </div>
			</div>
		
			<script type="text/javascript" src="/javascript/collapsible.js"> </script>
		</div>

    return returnSeq 
  }
}

object Grammar {

	//preprocess epsilons before sending to Grader
	def preprocessGrammar(grammar: String): String =
	{
		var g = grammar.replaceAll("[ \t\r\n]+$", "")			// trim trailing whitespaces
		g.replaceAll("(?i)(\\\\epsilon|\\\\eps|\\\\e)", "_")	// \epsilon -> "_" (frontend -> backend)
	}

	//preprocess before editing
	def preprocessLoadedGrammar(grammar: String) : String =
	{
		grammar.replaceAll("_", "\\\\epsilon")					// "_" -> "\epsilon" (backend -> frontend)
	}

	def preprocessFeedback(feedback: NodeSeq): NodeSeq =
	{
		try {
			// HACK! remove namespace (for some reason there are multiple xmlns in the response...)
			val hacked = feedback.toString.replaceAll("xmlns=\".*\"", "")
			return xml.XML.loadString(preprocessFeedback(hacked))
		}
		catch {
			case e: Exception => {println("Grammar Feedback couldn't be preprocessed..." + e.toString); return feedback}
		}
	}

	def preprocessFeedback(feedback: String): String =
	{	
		return feedback.replaceAll("_", "\u03B5")
	}
}