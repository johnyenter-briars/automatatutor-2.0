package com.automatatutor.model.problems

import com.automatatutor.model._
import net.liftweb.mapper.MappedString
import net.liftweb.mapper.LongKeyedMapper
import net.liftweb.mapper.LongKeyedMetaMapper
import net.liftweb.mapper.MappedLongForeignKey
import net.liftweb.mapper.IdPK
import net.liftweb.mapper.By
import net.liftweb.mapper.MappedText

import scala.xml.XML
import scala.xml.NodeSeq
import scala.xml.Node
import bootstrap.liftweb.StartupHook
import net.liftweb.common.{Box, Full}

class WhileToTMProblem extends LongKeyedMapper[WhileToTMProblem] with IdPK with SpecificProblem[WhileToTMProblem] {
  def getSingleton = WhileToTMProblem

  object problemId extends MappedLongForeignKey(this, Problem)

  //instruction fields
  object program extends MappedText(this)
  def getProgram = this.program.is
  def setProgram(s: String) = this.program(s)

  object numTapes extends MappedText(this)
  def getNumTapes = this.numTapes.is
  def setNumTapes(s: String) = this.numTapes(s)

  object uselessVars extends MappedText(this)
  def getUselessVars = this.uselessVars.is
  def setUselessVars(s: String) = this.uselessVars(s)

  // object alphabet extends MappedText(this)
  // def getAlphabet = this.alphabet
  // def setAlphabet(s: String) = this.alphabet(s)

  object programText extends MappedText(this)
  def getProgramText = this.programText.is
  def setProgramText(s: String) = this.programText(s)

  override def copy(): WhileToTMProblem = {
    val retVal = new WhileToTMProblem
    retVal.problemId(this.problemId.get)
    retVal.program(this.program.get)
    retVal.numTapes(this.numTapes.get)
    retVal.programText(this.programText.get)
    return retVal
  }

  override def toXML(): Node = {
    return <WhileToTMProblem>
             <Program>{ this.getProgram }</Program>
             <NumTapes>{ this.getNumTapes }</NumTapes>
             <ProgramText>{ this.getProgramText }</ProgramText>
           </WhileToTMProblem>
  }

  override def setGeneralProblem(newProblem: Problem) = this.problemId(newProblem)

}

object WhileToTMProblem extends WhileToTMProblem with SpecificProblemSingleton with LongKeyedMetaMapper[WhileToTMProblem] {
  def findByGeneralProblem(generalProblem: Problem): WhileToTMProblem =
    find(By(WhileToTMProblem.problemId, generalProblem)) openOrThrowException ("Must only be called if we are sure that generalProblem is a WhileToTMProblem")

  def deleteByGeneralProblem(generalProblem: Problem): Boolean =
    this.bulkDelete_!!(By(WhileToTMProblem.problemId, generalProblem))

  override def fromXML(generalProblem: Problem, xml: Node): Box[SpecificProblem[_]] = {
    val retVal = new WhileToTMProblem
    retVal.problemId(generalProblem)
    retVal.program((xml \ "Program").toString)
    retVal.numTapes((xml \ "Program" \ "NumVariables").text)
    retVal.uselessVars((xml \ "Program" \ "UselessVariablesText").text)
    retVal.programText((xml \ "ProgramText").text)
    retVal.save()

    return Full(retVal)
  }
}