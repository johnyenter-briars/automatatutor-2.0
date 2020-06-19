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

class DescriptionToGrammarProblem extends LongKeyedMapper[DescriptionToGrammarProblem] with IdPK with SpecificProblem[DescriptionToGrammarProblem] {
  def getSingleton = DescriptionToGrammarProblem

  object problemId extends MappedLongForeignKey(this, Problem)
  object grammar extends MappedText(this)

  def getGrammar = this.grammar.is
  def setGrammar(g: String) = this.grammar(g)

  override def copy(): DescriptionToGrammarProblem = {
    val retVal = new DescriptionToGrammarProblem
    retVal.problemId(this.problemId.get)
    retVal.grammar(this.grammar.get)
    return retVal
  }

  override def toXML(): Node = {
    return <DescriptionToGrammarProblem>
             <Grammar>{ this.getGrammar }</Grammar>
           </DescriptionToGrammarProblem>
  }

  override def setGeneralProblem(newProblem: Problem) = this.problemId(newProblem)

}

object DescriptionToGrammarProblem extends DescriptionToGrammarProblem with SpecificProblemSingleton with LongKeyedMetaMapper[DescriptionToGrammarProblem] {
  def findByGeneralProblem(generalProblem: Problem): DescriptionToGrammarProblem =
    find(By(DescriptionToGrammarProblem.problemId, generalProblem)) openOrThrowException ("Must only be called if we are sure that generalProblem is a DescriptionToGrammarProblem")

  def deleteByGeneralProblem(generalProblem: Problem): Boolean =
    this.bulkDelete_!!(By(DescriptionToGrammarProblem.problemId, generalProblem))

  override def fromXML(generalProblem: Problem, xml: Node): Boolean = {
    val retVal = new DescriptionToGrammarProblem
    retVal.problemId(generalProblem)
    retVal.grammar((xml \ "Grammar").text)
    retVal.save()
    return true
  }
}