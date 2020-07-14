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
import net.liftweb.common.{Box, Full}

import scala.xml.Node

class FindDerivationProblem extends LongKeyedMapper[FindDerivationProblem] with IdPK with SpecificProblem[FindDerivationProblem] {
  def getSingleton = FindDerivationProblem

  object problemId extends MappedLongForeignKey(this, Problem)
  object grammar extends MappedText(this)
  object word extends MappedText(this)
  object derivationType extends MappedInt(this)

  def getGrammar = this.grammar.is
  def setGrammar(g: String) = this.grammar(g)
  def getWord = this.word.is
  def setWord(w: String) = this.word(w)
  def getDerivationType = this.derivationType.is
  def setDerivationType(t: Int) = this.derivationType(t)

  override def copy(): FindDerivationProblem = {
    val retVal = new FindDerivationProblem
    retVal.problemId(this.problemId.get)
    retVal.grammar(this.grammar.get)
    retVal.word(this.word.get)
    retVal.derivationType(this.derivationType.get)
    return retVal
  }

  override def toXML(): Node = {
    return <FindDerivationProblem>
             <Grammar>{ this.getGrammar }</Grammar>
             <Word>{ this.getWord }</Word>
             <DerivationType>{ this.getDerivationType }</DerivationType>
           </FindDerivationProblem>
  }

  override def setGeneralProblem(newProblem: Problem) = this.problemId(newProblem)

}

object FindDerivationProblem extends FindDerivationProblem with SpecificProblemSingleton with LongKeyedMetaMapper[FindDerivationProblem] {
  def findByGeneralProblem(generalProblem: Problem): FindDerivationProblem =
    find(By(FindDerivationProblem.problemId, generalProblem)) openOrThrowException ("Must only be called if we are sure that generalProblem is a DerivationTypeProblem")

  def deleteByGeneralProblem(generalProblem: Problem): Boolean =
    this.bulkDelete_!!(By(FindDerivationProblem.problemId, generalProblem))

  override def fromXML(generalProblem: Problem, xml: Node): Box[SpecificProblem[_]] = {
    val retVal = new FindDerivationProblem
    retVal.problemId(generalProblem)
    retVal.grammar((xml \ "Grammar").text)
    retVal.word((xml \ "Word").text)
    retVal.derivationType((xml \ "DerivationType").text.toInt)
    retVal.save()
    return Full(retVal)
  }
}