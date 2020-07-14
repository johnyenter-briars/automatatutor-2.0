package com.automatatutor.model

import net.liftweb.common.{Box, Full}
import net.liftweb.mapper._

import scala.xml.Node

class EquivalenceClassesProblem extends LongKeyedMapper[EquivalenceClassesProblem] with IdPK with SpecificProblem[EquivalenceClassesProblem] {
  def getSingleton = EquivalenceClassesProblem

  object problemId extends MappedLongForeignKey(this, Problem)
  object regEx extends MappedText(this)
  object alphabet extends MappedText(this)
  object problemType extends MappedInt(this)

  object firstWord extends MappedText(this)
  object secondWord extends MappedText(this)
  object inNeeded extends MappedInt(this)
  object representative extends MappedText(this)

  def getRegex = this.regEx.is
  def setRegex(regEx: String) = this.regEx(regEx)
  def getAlphabet = this.alphabet.is
  def setAlphabet(alphabet: String) = this.alphabet(alphabet)
  def getInNeeded = this.inNeeded.is
  def setInNeeded(i: Int) = this.inNeeded(i)
  def getProblemType = this.problemType.is
  def setProblemType(problemType: Int) = this.problemType(problemType)
  def getFirstWord = this.firstWord.is
  def setFirstWord(word: String) = this.firstWord(word)
  def getSecondWord = this.secondWord.is
  def setSecondWord(word: String) = this.secondWord(word)
  def getRepresentative = this.representative.is
  def setRepresentative(word: String) = this.representative(word)

  override def copy(): EquivalenceClassesProblem = {
    val retVal = new EquivalenceClassesProblem
    retVal.problemId(this.problemId.get)
    retVal.regEx(this.regEx.get)
    retVal.alphabet(this.alphabet.get)
    retVal.inNeeded(this.inNeeded.get)
    retVal.problemType(this.problemType.get)
    retVal.firstWord(this.firstWord.get)
    retVal.secondWord(this.secondWord.get)
    retVal.representative(this.representative.get)
    return retVal
  }

  // Remember to check what happens if empty
  override def toXML(): Node = {
    return <EquivalencyProblem>
      <RegEx>{ this.getRegex }</RegEx>
      <Alphabet>{ this.getAlphabet }</Alphabet>
      <InNeeded>{ this.getInNeeded }</InNeeded>
      <ProblemType>{ this.problemType }</ProblemType>
      <FirstWord>{ this.firstWord }</FirstWord>
      <SecondWord>{ this.secondWord }</SecondWord>
      <Representative>{ this.representative }</Representative>
    </EquivalencyProblem>
  }

  override def setGeneralProblem(newProblem: Problem) = this.problemId(newProblem)

}

object EquivalenceClassesProblem extends EquivalenceClassesProblem with SpecificProblemSingleton with LongKeyedMetaMapper[EquivalenceClassesProblem] {
  def findByGeneralProblem(generalProblem: Problem): EquivalenceClassesProblem =
    find(By(EquivalenceClassesProblem.problemId, generalProblem)) openOrThrowException ("Must only be called if we are sure that generalProblem is a EqivalencyProblem")

  def deleteByGeneralProblem(generalProblem: Problem): Boolean =
    this.bulkDelete_!!(By(EquivalenceClassesProblem.problemId, generalProblem))

  override def fromXML(generalProblem: Problem, xml: Node): Box[SpecificProblem[_]] = {
    val retVal = new EquivalenceClassesProblem
    retVal.problemId(generalProblem)
    retVal.regEx((xml \ "RegEx").text)
    retVal.alphabet((xml \ "Alphabet").text)
    retVal.inNeeded((xml \ "InNeeded").text.toInt)
    retVal.problemType((xml \ "ProblemType").text.toInt)
    retVal.firstWord((xml \ "FirstWord").text)
    retVal.secondWord((xml \ "SecondWord").text)
    retVal.representative((xml \ "Representative").text)
    retVal.save()
    return Full(retVal)
  }
}
