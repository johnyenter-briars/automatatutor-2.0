package com.automatatutor.model.problems

import com.automatatutor.model._
import net.liftweb.common.{Box, Full}
import net.liftweb.mapper._

import scala.xml.{Node, NodeSeq, XML}

class PDAWordProblem extends LongKeyedMapper[PDAWordProblem] with IdPK with SpecificProblem[PDAWordProblem] {
  def getSingleton = PDAWordProblem

  protected object problemId extends MappedLongForeignKey(this, Problem)
  protected object automaton extends MappedText(this)
  protected object numberOfWordsInLanguage extends MappedInt(this)
  protected object numberOfWordsNotInLanguage extends MappedInt(this)
  protected object allowSimulation extends MappedBoolean(this)

  def getGeneralProblem = this.problemId.obj openOrThrowException "Every PDAWordProblem must have a ProblemId"
  override def setGeneralProblem(newProblem: Problem) = this.problemId(newProblem)

  def getAutomaton = this.automaton.get
  def setAutomaton(automaton: String) = this.automaton(automaton)
  def setAutomaton(automaton: NodeSeq) = this.automaton(automaton.mkString)

  def setNumberOfWordsInLanguage(numberOfWordsInLanguage: Int) = this.numberOfWordsInLanguage(numberOfWordsInLanguage)
  def getNumberOfWordsInLanguage = this.numberOfWordsInLanguage.get

  def setNumberOfWordsNotInLanguage(numberOfWordsNotInLanguage: Int) = this.numberOfWordsNotInLanguage(numberOfWordsNotInLanguage)
  def getNumberOfWordsNotInLanguage = this.numberOfWordsNotInLanguage.get

  def setAllowSimulation(allowSimulation: Boolean) = this.allowSimulation(allowSimulation)
  def getAllowSimulation = this.allowSimulation.get

  def getXmlDescription: NodeSeq = XML.loadString(this.automaton.is)

  //TODO: remove
  // Since we have ε in the alphabet, we have to remove it before handing out the alphabet
  /*def getAlphabet: Seq[String] = {
    return (getXmlDescription \ "alphabet" \ "symbol").map(_.text)
  }

  def getStackAlphabet: Seq[String] = (getXmlDescription \ "stackAlphabet" \ "symbol").map(_.text)

  def getAcceptanceConditionId: String = (getXmlDescription \ "@acceptanceConditionId").toString()

  def getDeterministic: String = (getXmlDescription \ "@deterministic").toString()

  def getWidth: String = (getXmlDescription \ "@width").toString()

  def getHeight: String = (getXmlDescription \ "@height").toString()

  def getNodeRadius: String = (getXmlDescription \ "@nodeRadius").toString()*/

  override def copy(): PDAWordProblem = {
    val retVal = new PDAWordProblem
    retVal.problemId(this.problemId.get)
    retVal.automaton(this.automaton.get)
    return retVal
  }

  override def toXML(): Node = {
    return <PDAWordProblem>
      <Automaton>{ this.getAutomaton }</Automaton>
    </PDAWordProblem>
  }
}

object PDAWordProblem extends PDAWordProblem with SpecificProblemSingleton with LongKeyedMetaMapper[PDAWordProblem] {
  def findByGeneralProblem(generalProblem: Problem): PDAWordProblem =
    find(By(PDAWordProblem.problemId, generalProblem)) openOrThrowException ("Must only be called if we are sure that generalProblem is a PDAWordProblem")

  def deleteByGeneralProblem(generalProblem: Problem): Boolean =
    this.bulkDelete_!!(By(PDAWordProblem.problemId, generalProblem))

  override def fromXML(generalProblem: Problem, xml: Node): Box[SpecificProblem[_]] = {
    val retVal = new PDAWordProblem
    retVal.problemId(generalProblem)
    retVal.automaton((xml \ "Automaton").text)
    retVal.save()
    return Full(retVal)
  }
}