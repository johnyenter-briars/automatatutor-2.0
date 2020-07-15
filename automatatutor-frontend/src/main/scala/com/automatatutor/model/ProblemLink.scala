package com.automatatutor.model

import com.automatatutor.model.problems._
import com.automatatutor.snippet._
import com.automatatutor.snippet.problems._
import net.liftweb.common._
import net.liftweb.mapper._
import bootstrap.liftweb.StartupHook
import com.automatatutor.lib.Config
import scala.xml.XML
import scala.xml.Node
import java.util.{Calendar, Date}



class ProblemLink extends LongKeyedMapper[ProblemLink] with IdPK {

  def getSingleton = ProblemLink

  protected object courseId extends MappedLongForeignKey(this, Course)
  protected object allowedAttempts extends MappedLong(this)
  protected object referencedProblemID extends MappedLongForeignKey(this, Problem)
  protected object maxGrade extends MappedLong(this)
  def getProblemID: Long = this.id.is

  def getCourse : Box[Course] = this.courseId.obj
  def setCourse ( course : Course ) = this.courseId(course)
  def setCourse ( course: Box[Course] ) = this.courseId(course)

  def getAllowedAttempts: Long = this.allowedAttempts.is
  def getAllowedAttemptsString: String = if (this.allowedAttempts.is == 0) "∞" else this.allowedAttempts.is.toString
  def setAllowedAttempts(attempts: Long) = this.allowedAttempts(attempts)

  def getMaxGrade: Long = this.maxGrade.is
  def setMaxGrade(maxGrade: Long) = this.maxGrade(maxGrade)

  def canBeDeleted : Boolean = true

  //TODO 7/15/20 fix this
//  def getAttempts(user: User): Seq[SolutionAttempt] = {
//    SolutionAttempt.findAll(
//      By(SolutionAttempt.userId, user),
//      By(SolutionAttempt.problemId, this))
//  }

  def getNumberAttempts(user: User): Int = {
    0
//    this.getAttempts(user).size
  }

  def getNumberAttemptsRemaining(user: User): Long = {
    if (this.allowedAttempts == 0) return 99
    else return this.allowedAttempts.is - this.getNumberAttempts(user)
  }
  def getNumberAttemptsRemainingString(user: User): String = {
    if (this.allowedAttempts == 0) return "∞"
    else return this.getNumberAttemptsRemaining(user).toString
  }

  def getGrade(user: User): Int = {
    0
//    val grades = this.getAttempts(user).map(_.grade.is)
//    if (grades.isEmpty) {
//      0
//    } else {
//      grades.max
//    }
  }

  def getLongDescription: String = {
    val matchingProblems = Problem.findAll().filter(p => p.getProblemID == this.getProblemID)
    if(matchingProblems.length > 1) throw new IllegalStateException("Each problem link must only have ONE linked problem")

    matchingProblems.head.getLongDescription
  }

  def getShortDescription: String = {
    val matchingProblems = Problem.findAll().filter(p => p.getProblemID == this.getProblemID)
    if(matchingProblems.length > 1) throw new IllegalStateException("Each problem link must only have ONE linked problem")

    matchingProblems.head.getShortDescription
  }

  def getTypeName: String = {
    val matchingProblems = Problem.findAll().filter(p => p.getProblemID == this.getProblemID)
    if(matchingProblems.length > 1) throw new IllegalStateException("Each problem link must only have ONE linked problem")

    matchingProblems.head.getTypeName()
  }
}

object ProblemLink extends ProblemLink with LongKeyedMetaMapper[ProblemLink] {

  //TODO 7/15/2020 fix these commented out methods
  //  def deleteByCreator(creator: User) : Unit = this.bulkDelete_!!(By(Problem.createdBy, creator))
  def findAllByCourse(course: Course): List[ProblemLink] = findAll(By(ProblemLink.courseId, course))
//  def deleteByCourse(course: Course) : Unit = this.bulkDelete_!!(By(Problem.courseId, course))

//  def findAllOfType(problemType: ProblemType) : List[Problem] = findAll(By(Problem.problemType, problemType))


  //  def fromXML(xml: Node): Boolean = {
  //    //find matching specific type
  //    val matchingTypes = ProblemType.findByName((xml \ "typeName").text)
  //    if (matchingTypes.isEmpty) return false
  //    val specificType = matchingTypes.head
  //    //generate general problem
  //    val generalProblem = new Problem
  //    generalProblem.problemType(specificType)
  //    generalProblem.createdBy(User.currentUser)
  //    generalProblem.shortDescription((xml \ "shortDescription").text)
  //    generalProblem.longDescription((xml \ "longDescription").text)
  //    generalProblem.save()
  //    //build specific problem
  //    val worked = specificType.getSpecificProblemSingleton().fromXML(generalProblem, (xml \ "specificProblem" \ "_").head)
  //    if (!worked) { generalProblem.delete_! }
  //    return worked
  //  }

}

