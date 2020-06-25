package com.automatatutor.lib

import java.net.HttpURLConnection
import java.net.URL

import scala.xml.Elem
import scala.xml.NodeSeq
import scala.xml.Text
import scala.xml.Null
import scala.xml.TopScope
import scala.xml.Node
import scala.xml.UnprefixedAttribute
import java.io.BufferedReader
import java.io.InputStreamReader
import java.io.IOException

import scala.xml.XML
import com.automatatutor.model.User
import com.automatatutor.snippet.Regex

class SOAPConnection(val url : URL) {
    def wrapSOAPEnvelope(body : NodeSeq) : NodeSeq = {
      <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
    	<soap:Body> { body } </soap:Body>
      </soap:Envelope>
    }

	def callMethod(namespace : String, methodName : String,  arguments : Map[String,Node]) : NodeSeq = {
	  def buildConnection : HttpURLConnection = {
      val connection = url.openConnection().asInstanceOf[HttpURLConnection]

      val methodNameWithNamespace = namespace + "/" + methodName

      connection.setDoOutput(true)
      connection.addRequestProperty("Content-Type", "text/xml; charset=utf-8")
      connection.addRequestProperty("SOAPAction", '"' + methodNameWithNamespace + '"')

      return connection
	  }

	  def buildRequestBody = {
      def convertArgumentsToXml = {
        def buildXmlNode(label:String, children: NodeSeq) = {
          val prefix = null
          val attributes = Null
          val scope = TopScope
          val minimizeEmpty = true

          Elem(prefix, label, attributes, scope, true, children : _*)
        }

        def buildXmlForSoapArgument(argName: String, argVal: NodeSeq) = buildXmlNode(argName, argVal)

        arguments.map({ case (argName, argVal) => buildXmlForSoapArgument(argName, argVal) } ).toSeq
      }

      def wrapInSoapBody(payload: Seq[Node]) = {
        val prefix = null
        val xmlnsAttribute = new UnprefixedAttribute("xmlns", namespace + '/', Null)
        val scope = TopScope
        val minimizeEmpty = true

        Elem(prefix, methodName, xmlnsAttribute, scope, minimizeEmpty, payload : _*)
      }
      wrapInSoapBody(convertArgumentsToXml)
	  }

	  val connection = buildConnection

	  def buildRequest = {
      val soapBody = buildRequestBody
      val requestXml = wrapSOAPEnvelope(soapBody)
      requestXml.toString
	  }
	  val requestRaw = buildRequest

	  connection.getOutputStream().write(requestRaw.getBytes())

    def responseIsOk = connection.getResponseCode() != HttpURLConnection.HTTP_OK
    def getReturnAsString = scala.io.Source.fromInputStream(connection.getInputStream()).mkString
		def getErrorAsString = scala.io.Source.fromInputStream(connection.getErrorStream()).mkString

	  try {
      if(responseIsOk) {
        return NodeSeq.Empty
      } else {
        def stripWrappingFromResponse(response : NodeSeq) = {
          // There are four levels of wrapping around the result: "soap:Envelope", "soap:Body", "Response", "Result")
          response \ "_" \ "_" \ "_" \ "_"
        }

        val returnRaw = getReturnAsString

				//TODO: remove the need for returnRaw.indexOf("<"). The backend is sending over data with bad unicode prologue
        val returnWithXmlWrapping = XML.loadString(returnRaw.substring(returnRaw.indexOf("<")))
				
				stripWrappingFromResponse(returnWithXmlWrapping)
      }
	  } catch {
	    case exception : Exception => Text(getErrorAsString)
	  }
	}
}

object GraderConnection {

  val serverUrlString = Config.grader.url.get
	val serverUrl = new URL(serverUrlString)
	val soapConnection = new SOAPConnection(serverUrl)

	val namespace = Config.grader.methodnamespace.get

	// DFA

	def getDfaFeedback(correctDfaDescription : String, attemptDfaDescription : String, maxGrade : Int) : (Int, NodeSeq) = {
	  val arguments = Map[String, Node](
	      "dfaCorrectDesc" -> XML.loadString(correctDfaDescription),
	      "dfaAttemptDesc" -> XML.loadString(attemptDfaDescription),
	      "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString)),
	      "feedbackLevel" -> Elem(null, "feedbackLevel", Null, TopScope, true, Text("Hint")),
	      "enabledFeedbacks" -> Elem(null, "enabledFeedbacks", Null, TopScope, true, Text("ignored")));

	  val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackXML", arguments)

	  return ((responseXml \ "grade").text.toInt, (responseXml \ "feedString" \ "ul" \ "li"))
	}

  //PDA

  def getPdaConstructionFeedback(correctPdaDescription: String, giveStackAlphabet: Boolean, attemptPdaDescription: String, maxGrade: Int): (Int, NodeSeq) = {
    val arguments = Map[String, Node](
      "xmlPdaCorrect" -> XML.loadString(correctPdaDescription),
      "xmlPdaAttempt" -> XML.loadString(attemptPdaDescription),
      "xmlGiveStackAlphabet" -> Elem(null, "giveStackAlphabet", Null, TopScope, true, Text(giveStackAlphabet.toString)),
      "xmlMaxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
    )

    val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackPDAConstruction", arguments)

    return ((responseXml \ "grade").text.toInt, (responseXml \ "feedback"))
  }

  def getPdaWordProblemFeedback(correctPdaDescription: String, wordsInLanguage: Seq[String], wordsNotInLanguage: Seq[String], maxGrade: Int): (Int, NodeSeq) = {
    val wordsInLanguageAsXml = <words>{wordsInLanguage.map(w => <word>{w}</word>)}</words>
    val wordsNotInLanguageAsXml = <words>{wordsNotInLanguage.map(w => <word>{w}</word>)}</words>

    val arguments = Map[String, Node](
      "xmlPda" -> XML.loadString(correctPdaDescription),
      "xmlWordsInLanguage" ->  wordsInLanguageAsXml,
      "xmlWordsNotInLanguage" -> wordsNotInLanguageAsXml,
      "xmlMaxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
    )

    val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackPDAWordProblem", arguments)

    return ((responseXml \ "grade").text.toInt, (responseXml \ "feedback"))
  }

	def runSimulation(pdaDescriptionXml: String, wordXml: String) : (NodeSeq) = {
		val arguments = Map[String, Node](
			"xmlPda" -> XML.loadString(pdaDescriptionXml),
			"xmlWord" -> XML.loadString(wordXml)
		)

		val responseXml = soapConnection.callMethod(namespace, "SimulateWordInPDA", arguments)
		responseXml
	}

  //Product Construction

  def getProductConstructionFeedback(correctDfaDescriptionList : List[String], attemptDfaDescription : String, booleanOperation : String, maxGrade : Int) : (Int, NodeSeq) = {
    def stringListToNodeList(xs: List[String]): List[Node] = xs match{
      case Nil => List()
      case y :: ys => Elem(null, "dfaDesc", Null, TopScope, true, XML.loadString(y)) :: stringListToNodeList(ys)
    }

    val arguments = Map[String, Node](
      "dfaDescList" -> Elem(null, "dfaDescList", Null, TopScope, true, stringListToNodeList(correctDfaDescriptionList):_*),
      "dfaAttemptDesc" -> XML.loadString(attemptDfaDescription),
      "booleanOperation" -> Elem(null, "booleanOperation", Null, TopScope, true, Text(booleanOperation)),
      "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString)),
      "feedbackLevel" -> Elem(null, "feedbackLevel", Null, TopScope, true, Text("Hint")),
      "enabledFeedbacks" -> Elem(null, "enabledFeedbacks", Null, TopScope, true, Text("ignored")))

    val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackProductConstruction", arguments)

    ((responseXml \ "grade").text.toInt, (responseXml \ "feedString" \ "ul" \ "li"))
  }

	def getProdConFeedback(DfaDescriptionList : List[String], attemptDfaDescription : String, setOperationDescription : String, maxGrade : Int) : (Int, NodeSeq) = {
		def stringToNode(s: String) : Node = Elem(null, "dfaDesc", Null, TopScope, true, XML.loadString(s))

		val arguments = Map[String, Node](
			"dfaDescList" 		-> Elem(null, "dfaDescList", Null, TopScope, true, (DfaDescriptionList map stringToNode):_*),
			"dfaAttemptDesc" 				-> XML.loadString(attemptDfaDescription),
			"setOperation" 					-> Elem(null, "setOperation", Null, TopScope, true, Text(setOperationDescription)),
			"maxGrade" 							-> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString)),
			"feedbackLevel" 				-> Elem(null, "feedbackLevel", Null, TopScope, true, Text("Hint")),
			"enabledFeedbacks" 			-> Elem(null, "enabledFeedbacks", Null, TopScope, true, Text("ignored"))
		)

		val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackProdCon", arguments)

		((responseXml \ "grade").text.toInt, (responseXml \ "feedString" \ "ul" \ "li"))
	}

  // Minimization

  //TODO: Pass Minimization Table to variable 'minimizationTableAttempt'
  def getMinimizationFeedback(dfaDescription : String, minimizationTableDescription : String, attemptDfaDescription : String, maxGrade : Int) : (Int, NodeSeq) = {

    val arguments = Map[String, Node](
      "dfaDesc" -> XML.loadString(dfaDescription),
			"minimizationTableAttempt" -> XML.loadString(minimizationTableDescription),
      "dfaAttemptDesc" -> XML.loadString(attemptDfaDescription),
      "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString)),
      "feedbackLevel" -> Elem(null, "feedbackLevel", Null, TopScope, true, Text("Hint")),
      "enabledFeedbacks" -> Elem(null, "enabledFeedbacks", Null, TopScope, true, Text("ignored")));

    //TODO: Implement 'ComputeFeedbackMinimization' in Backend
    val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackMinimization", arguments)

    ((responseXml \ "grade").text.toInt, (responseXml \ "feedString" \ "ul" \ "li"))
  }

	/**************************
		*
		* @param correctRegex
		* @param attemptNfaDescription
		* @param maxGrade
		* @return
		*/
	def getRegexToNfaFeedback(correctRegex : String, alphabet : Seq[String], attemptNfaDescription : String, maxGrade : Int) : (Int, NodeSeq) = {
		val arguments = Map[String, Node](
			"regex" -> <div> {correctRegex} </div>,
			"alphabet" -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>,
			"attemptNfa" -> XML.loadString(attemptNfaDescription),
			"maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
		)

		val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackRegexToNfa", arguments)

		return ((responseXml \ "grade").text.toInt, Regex.preprocessFeedback(responseXml \ "feedback"))
	}
	
	// NFA

	def getNfaFeedback(correctNfaDescription : String, attemptNfaDescription : String, maxGrade : Int) : (Int, NodeSeq) = {
	  val arguments = Map[String, Node](
	      "nfaCorrectDesc" -> XML.loadString(correctNfaDescription),
	      "nfaAttemptDesc" -> XML.loadString(attemptNfaDescription),
	      "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString)),
	      "feedbackLevel" -> Elem(null, "feedbackLevel", Null, TopScope, true, Text("Hint")),
	      "enabledFeedbacks" -> Elem(null, "enabledFeedbacks", Null, TopScope, true, Text("ignored")),
		  "userId" -> Elem(null, "userId", Null, TopScope, true, Text(User.currentUserIdInt.toString))
		  );

	  val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackNFAXML", arguments)

	  return ((responseXml \ "grade").text.toInt, (responseXml \ "feedString" \ "ul" \ "li"))
	}

	// NFA to DFA

	def getNfaToDfaFeedback(correctNfaDescription : String, attemptDfaDescription : String, maxGrade : Int) : (Int, NodeSeq) = {
	  val arguments = Map[String, Node](
	      "nfaCorrectDesc" -> XML.loadString(correctNfaDescription),
	      "dfaAttemptDesc" -> XML.loadString(attemptDfaDescription),
	      "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString)));

	  val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackNfaToDfa", arguments)

	  return ((responseXml \ "grade").text.toInt, (responseXml \ "feedString" \ "ul" \ "li"))
	}

	// Regular expressions

	// This method
	def getEquivRegexFeedback(correctRegex : String, equivalentRegex1: String, equivalentRegex2: String, attemptRegex : String, alphabet : Seq[String], maxGrade: Int) : (Int, NodeSeq) = {

		val arguments = Map[String, Node](
			"regexCorrectDesc" -> <div> { correctRegex } </div>,
			"equivRegex1" -> <div> {equivalentRegex1} </div>,
			"equivRegex2" -> <div> {equivalentRegex2} </div>,
			"regexAttemptDesc" -> <div> { attemptRegex } </div>,
			"alphabet" -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>,
			"maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
		)

		val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackRegexpEditDistance", arguments)

		((responseXml \ "grade").head.text.toInt, (responseXml \ "feedback"))
	}

	def getDynamicEquivRegexFeedback(correctRegex : String, equivalent: String, attemptRegex : String, alphabet : Seq[String], maxGrade: Int) : (Int, NodeSeq) = {

		val arguments = Map[String, Node](
			"regexCorrectDesc" -> <div> { correctRegex } </div>,
			"equivRegex" -> <div> {equivalent} </div>,
			"regexAttemptDesc" -> <div> { attemptRegex } </div>,
			"alphabet" -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>,
			"maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
		)

		val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackDynamicRegexpEditDistance", arguments)

		((responseXml \ "grade").head.text.toInt, (Regex.preprocessFeedback(responseXml\ "feedback")))
	}
	

	def getRegexFeedback(correctRegex : String, attemptRegex : String, alphabet : Seq[String], maxGrade : Int) : (Int, NodeSeq) = {
	  val arguments = Map[String, Node](
	      "regexCorrectDesc" -> <div> { correctRegex } </div>,
	      "regexAttemptDesc" -> <div> { attemptRegex } </div>,
	      "alphabet" -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>,
	      "feedbackLevel" -> Elem(null, "feedbackLevel", Null, TopScope, true, Text("Hint")),
	      "enabledFeedbacks" -> Elem(null, "enabledFeedbacks", Null, TopScope, true, Text("ignored")),
	      "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString)))

	  val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackRegexp", arguments)

	  return ((responseXml \ "grade").head.text.toInt, (responseXml \ "feedback"))
	}

	def getWordsInRegexFeedback(regex: String, wordsIn : Seq[String], wordsOut : Seq[String], maxGrade : Int) : (Int, NodeSeq) = {
		val arguments = Map[String, Node](
			"regEx" -> Elem(null, "regex", Null, TopScope, true, Text(regex)),
			"wordsIn" -> <div> { wordsIn.map((symbol : String) => Elem(null, "word", Null, TopScope, true, Text(symbol))) } </div>,
			"wordsOut" -> <div> { wordsOut.map((symbol : String) => Elem(null, "word", Null, TopScope, true, Text(symbol))) } </div>,
			"maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
		)

		val responseXml = soapConnection.callMethod(namespace, "ComputeWordsInRegexpFeedback", arguments)

		return ((responseXml \ "grade").head.text.toInt, Regex.preprocessFeedback(responseXml \ "feedback"))
	}

	/********************************
		* Equivalence classes feedback
		*/
	def getEquivalentWordsFeedback(regex: String, alphabet: Seq[String], representative: String, wordsIn: Array[String], maxGrade: Int) : (Int, NodeSeq) = {
		val arguments = Map[String, Node](
			"regex" -> Elem(null, "regex", Null, TopScope, true, Text(regex)),
			"alphabet" -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>,
			"representative" -> Elem(null, "representative", Null, TopScope, true, Text(representative)),
			"wordsIn" -> <div> { wordsIn.map((symbol : String) => Elem(null, "word", Null, TopScope, true, Text(symbol))) } </div>,
			"maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
		)

		val responseXml = soapConnection.callMethod(namespace, "ComputeEquivalentWordsFeedback", arguments)
		return ((responseXml \ "grade").head.text.toInt, Regex.preprocessFeedback(responseXml \ "feedback"))
	}
	def getEquivalentShortestFeedback(regex: String, alphabet: Seq[String], representative: String, shortest: String, maxGrade: Int) : (Int, NodeSeq) = {
		val arguments = Map[String, Node](
			"regex" -> Elem(null, "regex", Null, TopScope, true, Text(regex)),
			"alphabet" -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>,
			"representative" -> Elem(null, "representative", Null, TopScope, true, Text(representative)),
			"shortest" -> Elem(null, "shortest", Null, TopScope, true, Text(shortest)),
			"maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
		)

		val responseXml = soapConnection.callMethod(namespace, "ComputeEquivalentShortestFeedback", arguments)
		return ((responseXml \ "grade").head.text.toInt, Regex.preprocessFeedback(responseXml \ "feedback"))
	}
	def getSameEquivalencyClassFeedback(regex: String, alphabet: Seq[String], firstWord: String, secondWord: String, notEquivalent: Int, reason: String, maxGrade: Int) : (Int, NodeSeq) = {
		val arguments = Map[String, Node](
			"regex" -> Elem(null, "regex", Null, TopScope, true, Text(regex)),
			"alphabet" -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>,
			"firstWord" -> Elem(null, "firstword", Null, TopScope, true, Text(firstWord)),
			"secondWord" -> Elem(null, "firstword", Null, TopScope, true, Text(secondWord)),
			"notEquivalent" -> Elem(null, "notEquivalent", Null, TopScope, true, Text(notEquivalent.toString)),
			"reason" -> Elem(null, "reason", Null, TopScope, true, Text(reason)),
			"maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
		)

		val responseXml = soapConnection.callMethod(namespace, "ComputeSameEquivalencyClassFeedback", arguments)
		return ((responseXml \ "grade").head.text.toInt, Regex.preprocessFeedback(responseXml \ "feedback"))
	}
	def getEquivalencyClassTwoWordsInstructorFeedback(regex: String, alphabet: Seq[String], firstWord: String, secondWord: String) : (NodeSeq) = {
		val arguments = Map[String, Node](
			"regex" -> Elem(null, "regex", Null, TopScope, true, Text(regex)),
			"alphabet" -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>,
			"firstWord" -> Elem(null, "firstword", Null, TopScope, true, Text(firstWord)),
			"secondWord" -> Elem(null, "firstword", Null, TopScope, true, Text(secondWord))
		)

		val responseXml = soapConnection.callMethod(namespace, "ComputeEquivalencyClassTwoWordsFeedback", arguments)
		return Regex.preprocessFeedback(responseXml \ "feedback")
	}

	def getRegexParsingErrors(potentialRegex : String, alphabet : Seq[String]) : Seq[String] = {
	  val arguments = Map[String, Node](
	      "regexDesc" -> <div> { potentialRegex } </div>,
	      "alphabet" -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>)

	  val responseXml = soapConnection.callMethod(namespace, "CheckRegexp", arguments)

	  if(responseXml.text.equals("CorrectRegex")) return List() else return List(Regex.preprocessFeedback(responseXml).text)
	}

	// Checking Equivalency
	def isRegexEquivalent(initial : String, equiv: String, alphabet : Seq[String]) : Seq[String] = {
		val arguments = Map[String, Node](
			"correctRegex" -> <div> { initial } </div>,
			"equivRegex" -> <div> { equiv } </div>,
			"alphabet" -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>)

		val responseXml = soapConnection.callMethod(namespace, "CheckEquivalentRegexp", arguments)

		if(responseXml.text.equals("Equivalent")) return List() else return List(Regex.preprocessFeedback(responseXml).text)
	}

	//Grammar
	def getGrammarParsingErrors(potentialGrammar : String) : Seq[String] = {
	  val arguments = Map[String, Node](
	      "grammar" -> Elem(null, "grammar", Null, TopScope, true, Text(potentialGrammar))
	  )

	  val responseXml = soapConnection.callMethod(namespace, "CheckGrammar", arguments)

	  if(responseXml.text.equals("CorrectGrammar")) return List() else return List(responseXml.text)
	}

	def getCNFParsingErrors(potentialGrammar : String) : Seq[String] = {
	  val arguments = Map[String, Node](
	      "grammar" -> Elem(null, "grammar", Null, TopScope, true, Text(potentialGrammar))
	  )

	  val responseXml = soapConnection.callMethod(namespace, "isCNF", arguments)

	  if((responseXml \ "res").head.text.equals("y")) return List() else return List((responseXml \ "feedback").head.text)
	}

	def getWordsInGrammarFeedback(grammar: String, wordsIn : Seq[String], wordsOut : Seq[String], maxGrade : Int) : (Int, NodeSeq) = {
	  val arguments = Map[String, Node](
	      "grammar" -> Elem(null, "grammar", Null, TopScope, true, Text(grammar)),
		  "wordsIn" -> <div> { wordsIn.map((symbol : String) => Elem(null, "word", Null, TopScope, true, Text(symbol))) } </div>,
		  "wordsOut" -> <div> { wordsOut.map((symbol : String) => Elem(null, "word", Null, TopScope, true, Text(symbol))) } </div>,
	      "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
	  );

	  val responseXml = soapConnection.callMethod(namespace, "ComputeWordsInGrammarFeedback", arguments)

	  return ((responseXml \ "grade").head.text.toInt, (responseXml \ "feedback"))
	}

	def getDescriptionToGrammarFeedback(solution: String, attempt: String, maxGrade : Int) : (Int, NodeSeq) = {
	  val arguments = Map[String, Node](
	      "solution" -> Elem(null, "solution", Null, TopScope, true, Text(solution)),
		  "attempt" -> Elem(null, "attempt", Null, TopScope, true, Text(attempt)),
	      "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString)),
	      "checkEmptyWord" -> Elem(null, "checkEmptyWord", Null, TopScope, true, Text(true.toString))
	  );

	  val responseXml = soapConnection.callMethod(namespace, "ComputeGrammarEqualityFeedback", arguments)

	  return ((responseXml \ "grade").head.text.toInt, (responseXml \ "feedback"))
	}

	def getGrammarToCNFFeedback(solution: String, attempt: String, maxGrade : Int) : (Int, NodeSeq) = {
	  //check that attempt is in CNF
	  val arguments1 = Map[String, Node](
	      "grammar" -> Elem(null, "grammar", Null, TopScope, true, Text(attempt))
	  );
	  val responseXml1 = soapConnection.callMethod(namespace, "isCNF", arguments1)
	  if ((responseXml1 \ "res").head.text == "n") return (-1, (responseXml1 \ "feedback"))

	  val arguments2 = Map[String, Node](
	      "solution" -> Elem(null, "solution", Null, TopScope, true, Text(solution)),
		  "attempt" -> Elem(null, "attempt", Null, TopScope, true, Text(attempt)),
	      "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString)),
	      "checkEmptyWord" -> Elem(null, "checkEmptyWord", Null, TopScope, true, Text(false.toString))
	  );
	  val responseXml2 = soapConnection.callMethod(namespace, "ComputeGrammarEqualityFeedback", arguments2)
	  return ((responseXml2 \ "grade").head.text.toInt, (responseXml2 \ "feedback"))
	}

	def getCYKFeedback(grammar: String, word: String, cyk_attempt: String, maxGrade : Int) : (Int, NodeSeq) = {
	  val arguments = Map[String, Node](
	      "grammar" -> Elem(null, "grammar", Null, TopScope, true, Text(grammar)),
		  "word" -> Elem(null, "word", Null, TopScope, true, Text(word)),
		  "attempt" -> XML.loadString(cyk_attempt),
	      "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
	  );

	  val responseXml = soapConnection.callMethod(namespace, "ComputeCYKFeedback", arguments)

	  return ((responseXml \ "grade").head.text.toInt, (responseXml \ "feedback"))
	}

	def getFindDerivationFeedback(grammar: String, word: String, derivationType: Int, derivation: String, maxGrade : Int) : (Int, NodeSeq) = {
	  val arguments = Map[String, Node](
	      "grammar" -> Elem(null, "grammar", Null, TopScope, true, Text(grammar)),
		  "word" -> Elem(null, "word", Null, TopScope, true, Text(word)),
	      "derivationType" -> Elem(null, "derivationType", Null, TopScope, true, Text(derivationType.toString)),
		  "derivation" -> Elem(null, "derivation", Null, TopScope, true, Text(derivation)),
	      "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString))
	  );

	  val responseXml = soapConnection.callMethod(namespace, "ComputeFindDerivationFeedback", arguments)

	  return ((responseXml \ "grade").head.text.toInt, (responseXml \ "feedback"))
	}

	//Turing Machines

	def getWhileToTMFeedback(correctProgram : String, attemptTmDescription : String, maxGrade : Int) : (Int, NodeSeq) = {
	  val arguments = Map[String, Node](
    	// "correctProgram" -> XML.loadString(correctProgram),
    	"correctProgram" -> Elem(null, "correctProgram", Null, TopScope, true, Text(correctProgram)),
	    "attemptTM" -> XML.loadString(attemptTmDescription),
	    "maxGrade" -> Elem(null, "maxGrade", Null, TopScope, true, Text(maxGrade.toString)),
	    "feedbackLevel" -> Elem(null, "feedbackLevel", Null, TopScope, true, Text("Hint")),
	    "enabledFeedbacks" -> Elem(null, "enabledFeedbacks", Null, TopScope, true, Text("ignored")));

	  val responseXml = soapConnection.callMethod(namespace, "ComputeFeedbackWhileToTM", arguments)

	  return ((responseXml \ "grade").text.toInt, (responseXml \ "feedback" ))
	}

	def whileProgramCheck(whileProgram : String) : String = {
		val arguments = Map[String, Node](
			"program" -> Elem(null, "correctProgram", Null, TopScope, true, Text(whileProgram)))

		val responseXml = soapConnection.callMethod(namespace, "CheckWhileFormat", arguments)
		return responseXml.head.text
	}

	//Problem generation
	def generateProblem(problemType: String, minQual: Double) : (NodeSeq) = {
	  val arguments = Map[String, Node](
	      "type" -> Elem(null, "type", Null, TopScope, true, Text(problemType)),
		  "minQual" -> Elem(null, "minQual", Null, TopScope, true, Text(minQual.toString))
	  );

	  val responseXml = soapConnection.callMethod(namespace, "GenerateProblem", arguments)

	  return responseXml
	}

	def generateProblemHardest(problemType: String, minQual: Double) : (NodeSeq) = {
	  val arguments = Map[String, Node](
	      "type" -> Elem(null, "type", Null, TopScope, true, Text(problemType)),
		  "minQual" -> Elem(null, "minQual", Null, TopScope, true, Text(minQual.toString))
	  );

	  val responseXml = soapConnection.callMethod(namespace, "GenerateProblemHardest", arguments)

	  return responseXml
	}

	def generateProblemBestIn(problemType: String, minDiff: Int, maxDiff: Int) : (NodeSeq) = {
	  val arguments = Map[String, Node](
	      "type" -> Elem(null, "type", Null, TopScope, true, Text(problemType)),
		  "minDiff" -> Elem(null, "minDiff", Null, TopScope, true, Text(minDiff.toString)),
		  "maxDiff" -> Elem(null, "maxDiff", Null, TopScope, true, Text(maxDiff.toString))
	  );
	  val responseXml = soapConnection.callMethod(namespace, "GenerateProblemBestIn", arguments)

	  return responseXml
	}

	// Pumping lemma

  def getPLParsingErrors(languageDesc : String, constraintDesc : String,
                      alphabet : Seq[String], pumpingString : String) : Seq[String] = {
    val arguments = Map[String, Node](
        "languageDesc"   -> <div> { languageDesc } </div>,
        "constraintDesc" -> <div> { constraintDesc } </div>,
        "alphabet"       -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>,
        "pumpingString"  -> <div> { pumpingString } </div>
    )

    val responseXml = soapConnection.callMethod(namespace, "CheckArithLanguageDescription", arguments)

    if (responseXml.text.equals("CorrectLanguageDescription")) return List() else return List(responseXml.text)
  }

  def getPLSplits(languageDesc : String, constraintDesc : String,
                      alphabet : Seq[String], pumpingString : String) : NodeSeq = {
    val arguments = Map[String, Node](
        "languageDesc"   -> <div> { languageDesc } </div>,
        "constraintDesc" -> <div> { constraintDesc } </div>,
        "alphabet"       -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>,
        "pumpingString"  -> <div> { pumpingString } </div>
    )

    return soapConnection.callMethod(namespace, "GenerateStringSplits", arguments)

    //if (responseXml.text.equals("CorrectLanguageDescription")) return List() else return List(responseXml.text)
  }

  def getPLFeedback(languageDesc : String, constraintDesc : String,
                      alphabet : Seq[String], pumpingString : String,
                      pumpingNumbers : Node) : NodeSeq = {
    val arguments = Map[String, Node](
        "languageDesc"   -> <div> { languageDesc } </div>,
        "constraintDesc" -> <div> { constraintDesc } </div>,
        "alphabet"       -> <div> { alphabet.map((symbol : String) => Elem(null, "symbol", Null, TopScope, true, Text(symbol))) } </div>,
        "pumpingString"  -> <div> { pumpingString } </div>,
        "pumpingNumbers" -> {pumpingNumbers}
    )

    return soapConnection.callMethod(namespace, "GetPumpingLemmaFeedback", arguments)

    //if (responseXml.text.equals("CorrectLanguageDescription")) return List() else return List(responseXml.text)
  }


	// -------------------
	// PUMPING LEMMA GAME
	// -------------------

	def PLGRegularGetDFAFromSymbolicString(alphabet : String, symbolicString : String, constraints : String): NodeSeq =
	{
		val arguments = Map[String, Node] (
			"alphabet" -> <div>{ alphabet }</div>,
			"symbolicString" -> <div>{ symbolicString }</div>,
			"constraints" -> <div>{ constraints }</div>
		)

		return soapConnection.callMethod(namespace, "PLGRegularGetDFAFromSymbolicString", arguments)
	}

	def PLGNonRegularCheckValidity(alphabet : String, symbolicString : String, constraints : String, unpumpableWord : String): NodeSeq =
	{
		val arguments = Map[String, Node] (
			"alphabet" -> <div>{ alphabet }</div>,
			"symbolicString" -> <div>{ symbolicString }</div>,
			"constraints" -> <div>{ constraints }</div>,
			"unpumpableWord" -> <div>{ unpumpableWord }</div>
		)

		return soapConnection.callMethod(namespace, "PLGNonRegularCheckValidity", arguments)
	}

	def PLGRegularGetN(automaton : String) : String = {
		val arguments = Map[String, Node](
			"automaton" -> XML.loadString(automaton)
		)

		val responseXML = soapConnection.callMethod(namespace, "PLGRegularGetN", arguments)
		return responseXML.text
	}

	def PLGNonRegularGetN(max : String) : String =
	{
		val arguments = Map[String, Node](
			"max" -> <div>{ max }</div>
		)

		return soapConnection.callMethod(namespace, "PLGNonRegularGetN", arguments).text
	}

	def PLGRegularGetWord(automaton : String, n : String): NodeSeq =
	{
		val arguments=Map[String, Node] (
			"automaton" -> XML.loadString(automaton),
			"n" -> <div>{ n }</div>
		)

		return soapConnection.callMethod(namespace, "PLGRegularGetWord", arguments)
	}

	def PLGNonRegularGetWord(alphabet: String, symbolicString : String, constraints : String, n : String, unpumpableWord : String): NodeSeq =
	{
		val arguments = Map[String, Node] (
			"alphabet" -> <div>{ alphabet }</div>,
			"symbolicString" -> <div>{ symbolicString }</div>,
			"constraints" -> <div>{ constraints }</div>,
			"n" -> <div>{ n }</div>,
			"unpumpableWord" -> <div>{ unpumpableWord }</div>
		)

		return soapConnection.callMethod(namespace, "PLGNonRegularGetWord", arguments)
	}

	def PLGRegularCheckI(automaton: String, start : String, mid : String, end : String, i : String): NodeSeq =
	{
		val arguments = Map[String, Node] (
			"automaton" -> XML.loadString(automaton),
			"start" -> <div>{ start.replace("\u03B5", "") }</div>,
			"mid" -> <div>{ mid.replace("\u03B5", "") }</div>,
			"end" -> <div>{ end.replace("\u03B5", "") }</div>,
			"i" -> <div>{ i }</div>
		)

		return soapConnection.callMethod(namespace, "PLGRegularCheckI", arguments)
	}

	def PLGNonRegularCheckI(alphabet : String, symbolicString : String, constraints : String, start : String, mid : String, end : String, i : String): NodeSeq =
	{
		val arguments = Map[String, Node] (
			"alphabet" -> <div>{ alphabet }</div>,
			"symbolicString" -> <div>{ symbolicString }</div>,
			"constraints" -> <div>{ constraints }</div>,
			"start" -> <div>{ start.replace("\u03B5", "") }</div>,
			"mid" -> <div>{ mid.replace("\u03B5", "") }</div>,
			"end" -> <div>{ end.replace("\u03B5", "") }</div>,
			"i" -> <div>{ i }</div>
		)

		return soapConnection.callMethod(namespace, "PLGNonRegularCheckI", arguments)
	}

	def PLGRegularCheckWordGetSplit(automaton : String, n : String, word : String): NodeSeq =
	{
		val arguments = Map[String, Node] (
			"automaton" -> XML.loadString(automaton),
			"n" -> <div>{ n }</div>,
			"word" -> <div>{ word }</div>
		)

		return soapConnection.callMethod(namespace, "PLGRegularCheckWordGetSplit", arguments)
	}

	def PLGNonRegularCheckWordGetSplit(alphabet : String, symbolicString : String, constraints : String, n : String, word : String): NodeSeq =
	{
		val arguments = Map[String, Node] (
			"alphabet" -> <div>{ alphabet }</div>,
			"symbolicString" -> <div>{ symbolicString }</div>,
			"constraints" -> <div>{ constraints }</div>,
			"n" -> <div>{ n }</div>,
			"word" -> <div>{ word }</div>
		)

		return soapConnection.callMethod(namespace, "PLGNonRegularCheckWordGetSplit", arguments)
	}

	def PLGRegularGetI(automaton : String, start : String, mid: String, end: String): NodeSeq =
	{
		val arguments=Map[String, Node] (
			"automaton" -> XML.loadString(automaton),
			"start" -> <div>{ start.replace("\u03B5", "") }</div>,
			"mid" -> <div>{ mid.replace("\u03B5", "") }</div>,
			"end" -> <div>{ end.replace("\u03B5", "") }</div>
		)
		return soapConnection.callMethod(namespace, "PLGRegularGetI", arguments)
	}

	def PLGNonRegularGetI(alphabet: String, symbolicString : String, constraints : String, start : String, mid: String, end: String): NodeSeq =
	{
		val arguments=Map[String, Node] (
			"alphabet" -> <div>{ alphabet }</div>,
			"symbolicString" -> <div>{ symbolicString }</div>,
			"constraints" -> <div>{ constraints }</div>,
			"start" -> <div>{ start.replace("\u03B5", "") }</div>,
			"mid" -> <div>{ mid.replace("\u03B5", "") }</div>,
			"end" -> <div>{ end.replace("\u03B5", "") }</div>
		)

		return soapConnection.callMethod(namespace, "PLGNonRegularGetI", arguments)
	}

	def PLGNfaToDfa(automaton: String): NodeSeq =
	{
		val arguments=Map[String, Node] (
			"automaton" -> XML.loadString(automaton)
		)
		return soapConnection.callMethod(namespace, "PLGNfaToDfa", arguments)
	}




}
