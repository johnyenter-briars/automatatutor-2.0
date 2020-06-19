package com.automatatutor.model.problems

import com.automatatutor.model._
import scala.xml.NodeSeq
import scala.xml.XML
import net.liftweb.mapper.By
import net.liftweb.mapper.IdPK
import net.liftweb.mapper.LongKeyedMapper
import net.liftweb.mapper.LongKeyedMetaMapper
import net.liftweb.mapper.MappedString
import net.liftweb.mapper.MappedText
import net.liftweb.mapper.MappedInt
import net.liftweb.mapper.MappedLongForeignKey
import bootstrap.liftweb.StartupHook
import scala.xml.Node

class WordsInRegExProblem extends LongKeyedMapper[WordsInRegExProblem] with IdPK with SpecificProblem[WordsInRegExProblem] {
  def getSingleton = WordsInRegExProblem

  object problemId extends MappedLongForeignKey(this, Problem)
  object regEx extends MappedText(this)
  object alphabet extends MappedText(this)
  object inNeeded extends MappedInt(this)
  object outNeeded extends MappedInt(this)

  def getRegex = this.regEx.is
  def setRegex(regEx: String) = this.regEx(regEx)
  def getAlphabet = this.alphabet.is
  def setAlphabet(alphabet: String) = this.alphabet(alphabet)
  def getInNeeded = this.inNeeded.is
  def setInNeeded(i: Int) = this.inNeeded(i)
  def getOutNeeded = this.outNeeded.is
  def setOutNeeded(i: Int) = this.outNeeded(i)

  override def copy(): WordsInRegExProblem = {
    val retVal = new WordsInRegExProblem
    retVal.problemId(this.problemId.get)
    retVal.regEx(this.regEx.get)
    retVal.alphabet(this.alphabet.get)
    retVal.inNeeded(this.inNeeded.get)
    retVal.outNeeded(this.outNeeded.get)
    return retVal
  }

  override def toXML(): Node = {
    return <WordsInGrammarProblem>
      <RegEx>{ this.getRegex }</RegEx>
      <Alphabet>{ this.getAlphabet }</Alphabet>
      <InNeeded>{ this.getInNeeded }</InNeeded>
      <OutNeeded>{ this.getOutNeeded }</OutNeeded>
    </WordsInGrammarProblem>
  }

  override def setGeneralProblem(newProblem: Problem) = this.problemId(newProblem)

}

object WordsInRegExProblem extends WordsInRegExProblem with SpecificProblemSingleton with LongKeyedMetaMapper[WordsInRegExProblem] {
  def findByGeneralProblem(generalProblem: Problem): WordsInRegExProblem =
    find(By(WordsInRegExProblem.problemId, generalProblem)) openOrThrowException ("Must only be called if we are sure that generalProblem is a WordsInGrammarProblem")

  def deleteByGeneralProblem(generalProblem: Problem): Boolean =
    this.bulkDelete_!!(By(WordsInRegExProblem.problemId, generalProblem))

  override def fromXML(generalProblem: Problem, xml: Node): Boolean = {
    val retVal = new WordsInRegExProblem
    retVal.problemId(generalProblem)
    retVal.regEx((xml \ "RegEx").text)
    retVal.alphabet((xml \ "Alphabet").text)
    retVal.inNeeded((xml \ "InNeeded").text.toInt)
    retVal.outNeeded((xml \ "OutNeeded").text.toInt)
    retVal.save()
    return true
  }
}
