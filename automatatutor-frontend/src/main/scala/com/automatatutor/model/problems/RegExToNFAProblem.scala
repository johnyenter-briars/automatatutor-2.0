package com.automatatutor.model.problems

import com.automatatutor.model._
import net.liftweb.common.{Box, Full}
import net.liftweb.mapper._

import scala.xml.XML
import scala.xml.NodeSeq
import scala.xml.Node

class RegExToNFAProblem extends LongKeyedMapper[RegExToNFAProblem] with IdPK with SpecificProblem[RegExToNFAProblem] {
  def getSingleton = RegExToNFAProblem

  object problemId extends MappedLongForeignKey(this, Problem)
  object regEx extends MappedText(this)
  object alphabet extends MappedText(this)

  def getAlphabet = this.alphabet.is
  def getRegex = this.regEx.is

  def setRegex(regEx: String) = this.regEx(regEx)
  def setAlphabet(alphabet: String) = this.alphabet(alphabet)

  override def copy(): RegExToNFAProblem = {
    val retVal = new RegExToNFAProblem
    retVal.problemId(this.problemId.get)
    retVal.regEx(this.regEx.get)
    retVal.alphabet(this.alphabet.get)
    return retVal
  }

  override def toXML(): Node = {
    return <RegExToNFAProblem>
      <RegEx>{ this.getRegex }</RegEx>
      <Alphabet>{ this.getAlphabet }</Alphabet>
    </RegExToNFAProblem>
  }

  override def setGeneralProblem(newProblem: Problem) = this.problemId(newProblem)

}

object RegExToNFAProblem extends RegExToNFAProblem with SpecificProblemSingleton with LongKeyedMetaMapper[RegExToNFAProblem] {
  def findByGeneralProblem(generalProblem: Problem): RegExToNFAProblem =
    find(By(RegExToNFAProblem.problemId, generalProblem)) openOrThrowException ("Must only be called if we are sure that generalProblem is a RegExToNFAProblem")
	
  def deleteByGeneralProblem(generalProblem: Problem): Boolean =
    this.bulkDelete_!!(By(RegExToNFAProblem.problemId, generalProblem))

  override def fromXML(generalProblem: Problem, xml: Node): Box[SpecificProblem[_]] = {
    val retVal = new RegExToNFAProblem
    retVal.problemId(generalProblem)
    retVal.regEx((xml \ "RegEx").text)
    retVal.alphabet((xml \ "Alphabet").text)

    retVal.save()
    return Full(retVal)
  }
}