package com.automatatutor.model.problems

import com.automatatutor.model._
import net.liftweb.mapper._
import scala.xml.XML
import scala.xml.NodeSeq
import scala.xml.Node

class RegExConstructionProblem extends LongKeyedMapper[RegExConstructionProblem] with IdPK with SpecificProblem[RegExConstructionProblem] {
  def getSingleton = RegExConstructionProblem

  object problemId extends MappedLongForeignKey(this, Problem)
  object regEx extends MappedText(this)
  object equivalent extends MappedText(this)
  def getEquivalent = this.equivalent.is
  def setEquivalent(equivalent: String) = this.equivalent(equivalent)
  def setEquivalent(equivalentList: Seq[String]) = equivalentList.mkString(" ")
  object alphabet extends MappedText(this)

  def getAlphabet = this.alphabet.is
  def getRegex = this.regEx.is

  def setRegex(regEx: String) = this.regEx(regEx)
  def setAlphabet(alphabet: String) = this.alphabet(alphabet)

  override def copy(): RegExConstructionProblem = {
    val retVal = new RegExConstructionProblem
    retVal.problemId(this.problemId.get)
    retVal.regEx(this.regEx.get)
    retVal.alphabet(this.alphabet.get)
    retVal.equivalent(equivalent.get)
    return retVal
  }

  override def toXML(): Node = {
    return <RegExConstructionProblem>
             <RegEx>{ this.getRegex }</RegEx>

                <Equivalent> {getEquivalent} </Equivalent>

             <Alphabet>{ this.getAlphabet }</Alphabet>
           </RegExConstructionProblem>
  }

  override def setGeneralProblem(newProblem: Problem) = this.problemId(newProblem)

}

object RegExConstructionProblem extends RegExConstructionProblem with SpecificProblemSingleton with LongKeyedMetaMapper[RegExConstructionProblem] {
  def findByGeneralProblem(generalProblem: Problem): RegExConstructionProblem =
    find(By(RegExConstructionProblem.problemId, generalProblem)) openOrThrowException ("Must only be called if we are sure that generalProblem is a RegExConstructionProblem")
	
  def deleteByGeneralProblem(generalProblem: Problem): Boolean =
    this.bulkDelete_!!(By(RegExConstructionProblem.problemId, generalProblem))

  override def fromXML(generalProblem: Problem, xml: Node): Boolean = {
    val retVal = new RegExConstructionProblem
    retVal.problemId(generalProblem)
    retVal.regEx((xml \ "RegEx").text)
    retVal.alphabet((xml \ "Alphabet").text)
    retVal.equivalent((xml \ "Equivalent").text)

    retVal.save()
    return true
  }
}