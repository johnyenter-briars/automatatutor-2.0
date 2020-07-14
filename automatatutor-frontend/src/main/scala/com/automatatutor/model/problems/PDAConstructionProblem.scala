package com.automatatutor.model.problems

import com.automatatutor.model._
import net.liftweb.common.{Box, Full}

import scala.xml.NodeSeq
import scala.xml.Node
import scala.xml.XML
import net.liftweb.mapper._

class PDAConstructionProblem extends LongKeyedMapper[PDAConstructionProblem] with IdPK with SpecificProblem[PDAConstructionProblem] {
  def getSingleton = PDAConstructionProblem

  protected object problemId extends MappedLongForeignKey(this, Problem)
  protected object automaton extends MappedText(this)
  protected object giveStackAlphabet extends MappedBoolean(this)
  protected object allowSimulation extends MappedBoolean(this)

  def getGeneralProblem = this.problemId.obj openOrThrowException "Every PDAConstructionProblem must have a ProblemId"
  override def setGeneralProblem(newProblem: Problem) = this.problemId(newProblem)

  def getAutomaton = this.automaton.get
  def setAutomaton(automaton: String) = this.automaton(automaton)
  def setAutomaton(automaton: NodeSeq) = this.automaton(automaton.mkString)

  def setGiveStackAlphabet(giveStackAlphabet: Boolean) = this.giveStackAlphabet(giveStackAlphabet)
  def getGiveStackAlphabet = this.giveStackAlphabet.get

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

  override def copy(): PDAConstructionProblem = {
    val retVal = new PDAConstructionProblem
    retVal.problemId(this.problemId.get)
    retVal.automaton(this.automaton.get)
    return retVal
  }

  override def toXML(): Node = {
    return <PDAConstructionProblem>
      <Automaton>{ this.getAutomaton }</Automaton>
    </PDAConstructionProblem>
  }
}

object PDAConstructionProblem extends PDAConstructionProblem with SpecificProblemSingleton with LongKeyedMetaMapper[PDAConstructionProblem] {
  def findByGeneralProblem(generalProblem: Problem): PDAConstructionProblem =
    find(By(PDAConstructionProblem.problemId, generalProblem)) openOrThrowException ("Must only be called if we are sure that generalProblem is a PDAConstructionProblem")

  def deleteByGeneralProblem(generalProblem: Problem): Boolean =
    this.bulkDelete_!!(By(PDAConstructionProblem.problemId, generalProblem))

  override def fromXML(generalProblem: Problem, xml: Node): Box[SpecificProblem[_]] = {
    val retVal = new PDAConstructionProblem
    retVal.problemId(generalProblem)
    retVal.automaton((xml \ "Automaton").text)
    retVal.save()
    return Full(retVal)
  }
}
