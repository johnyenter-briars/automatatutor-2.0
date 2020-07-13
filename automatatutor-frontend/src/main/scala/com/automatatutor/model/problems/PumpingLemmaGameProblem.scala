package com.automatatutor.model.problems

import com.automatatutor.model._
import net.liftweb.mapper.LongKeyedMapper
import net.liftweb.mapper.LongKeyedMetaMapper
import net.liftweb.mapper.MappedLongForeignKey
import net.liftweb.mapper.IdPK
import net.liftweb.mapper.By
import net.liftweb.mapper.MappedText

import scala.xml.{Node, NodeSeq, XML}

class PumpingLemmaGameProblem extends LongKeyedMapper[PumpingLemmaGameProblem] with IdPK with SpecificProblem[PumpingLemmaGameProblem] {
  def getSingleton = PumpingLemmaGameProblem

  object problemId extends MappedLongForeignKey(this, Problem)

  //instruction fields
  object regular extends MappedText(this)
  object alphabet extends MappedText(this)
  object symbolicString extends MappedText(this)
  object constraints extends MappedText(this)
  object automaton extends  MappedText(this)
  object unpumpableWord extends MappedText(this)

  def getRegular : Boolean = {
      if (this.regular.is.equals("true")) return true
      else if (this.regular.is.equals("false")) return false
      else throw new Exception("Wrong entry in Regular of problem "+this.problemId.is)
  }
  def getAlphabet  = this.alphabet.is
  def getSymbolicString = this.symbolicString.is
  def getConstraints = this.constraints.is
  def getAutomaton = this.automaton.is
  def getUnpumpableWord= this.unpumpableWord.is

  def setRegular(bool: Boolean) = {
    if(bool) this.regular("true")
    else this.regular("false")
  }
  def setRegular(s: String) = {
    this.regular(s)
  }
  def setAlphabet(s : String) = {
    this.alphabet(s)
  }
  def setSymbolicString(s : String) = {
    this.symbolicString(s)
  }
  def setConstraints(s : String) = {
    this.constraints(s)
  }
  def setAutomaton(au : String) = {
    this.automaton(au)
  }
  def setUnpumpableWord(w : String) = {
    this.unpumpableWord(w)
  }

  override def copy(): PumpingLemmaGameProblem = {
    val retVal = new PumpingLemmaGameProblem
    retVal.problemId(this.problemId.get)
    retVal.regular(this.regular.get.toString)
    retVal.alphabet(this.alphabet.get)
    retVal.symbolicString(this.symbolicString.get)
    retVal.constraints(this.constraints.get)
    retVal.automaton(this.automaton.get)
    return retVal
  }

  override def toXML(): Node = {
    return <PumpingLemmaGameProblem>
      <Alphabet>
        {this.getAlphabet}
      </Alphabet>
      <Regular>
        {this.getRegular}
      </Regular>
      <SymbolicString>
        {this.getSymbolicString}
      </SymbolicString>
      <Constraints>
        {this.getConstraints}
      </Constraints>
      <UnpumpableWord>
        {this.unpumpableWord}
      </UnpumpableWord>
      <Automaton>
        {this.getAutomaton}
      </Automaton>
    </PumpingLemmaGameProblem>
  }

  override def setGeneralProblem(newProblem: Problem) = this.problemId(newProblem)
}

object PumpingLemmaGameProblem extends PumpingLemmaGameProblem with SpecificProblemSingleton with LongKeyedMetaMapper[PumpingLemmaGameProblem] {
  def findByGeneralProblem(generalProblem: Problem): PumpingLemmaGameProblem =
    find(By(PumpingLemmaGameProblem.problemId, generalProblem)) openOrThrowException ("Must only be called if we are sure that generalProblem is a PumpingLemmaGameProblem")

  def deleteByGeneralProblem(generalProblem: Problem): Boolean =
    this.bulkDelete_!!(By(PumpingLemmaGameProblem.problemId, generalProblem))

  override def fromXML(generalProblem: Problem, xml: Node): Boolean = {
    val retVal = new PumpingLemmaGameProblem
    retVal.problemId(generalProblem)
    retVal.regular((xml \ "Regular").text)
    retVal.alphabet((xml \ "Alphabet").text)
    retVal.symbolicString((xml \ "SymbolicString").text)
    retVal.constraints((xml \ "Constraints").text)
    retVal.unpumpableWord((xml \ "UnpumpableWord").text)
    retVal.automaton((xml \ "Automaton").text)
    retVal.save()
    return true
  }
}