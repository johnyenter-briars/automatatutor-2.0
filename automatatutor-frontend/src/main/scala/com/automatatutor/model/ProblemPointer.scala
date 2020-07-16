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



class ProblemPointer extends LongKeyedMapper[ProblemPointer] with IdPK {

  def getSingleton = ProblemPointer

  protected object courseId extends MappedLongForeignKey(this, Course)
  protected object allowedAttempts extends MappedLong(this)
  protected object folderId extends MappedLongForeignKey(this, Folder)
  protected object referencedProblemId extends MappedLongForeignKey(this, Problem)
  protected object maxGrade extends MappedLong(this)
  def getProblemPointerID: Long = this.id.is

  def getProblem = this.referencedProblemId.obj openOrThrowException "Every ProblemToFolder must have a Problem"
  def setProblem ( problem : Problem ) = this.referencedProblemId(problem)

  def getCourse : Box[Course] = this.courseId.obj
  def setCourse ( course : Course ) = this.courseId(course)
  def setCourse ( course: Box[Course] ) = this.courseId(course)

  def getAllowedAttempts: Long = this.allowedAttempts.is
  def getAllowedAttemptsString: String = if (this.allowedAttempts.is == 0) "∞" else this.allowedAttempts.is.toString
  def setAllowedAttempts(attempts: Long) = this.allowedAttempts(attempts)

  def getFolder: Folder = this.folderId.obj openOrThrowException "Every ProblemToFolder must have a Folder"
  def setFolder(folder: Folder) = this.folderId(folder)
  def setFolder(folder: Box[Folder]) = this.folderId(folder)

  def getMaxGrade: Long = this.maxGrade.is
  def setMaxGrade(maxGrade: Long) = this.maxGrade(maxGrade)

  def canBeDeleted : Boolean = true


  def getAttempts(user: User): Seq[SolutionAttempt] = {
    SolutionAttempt.findAll(
      By(SolutionAttempt.userId, user),
      By(SolutionAttempt.problempointerId, this))
  }

  def getNumberAttempts(user: User): Int = {
    this.getAttempts(user).size
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
    val grades = this.getAttempts(user).map(_.grade.is)
    if (grades.isEmpty) {
      0
    } else {
      grades.max
    }
  }

  def getLongDescription: String = {
    val matchingProblems = Problem.findAll().filter(p => p == this.getProblem)
    if(matchingProblems.length > 1) throw new IllegalStateException("Each problem link must only have ONE linked problem")

    matchingProblems.head.getLongDescription
  }

  def getShortDescription: String = {
    val matchingProblems = Problem.findAll().filter(p => p == this.getProblem)
    if(matchingProblems.length > 1) throw new IllegalStateException("Each problem link must only have ONE linked problem")

    matchingProblems.head.getShortDescription
  }

  def getTypeName: String = {
    val matchingProblems = Problem.findAll().filter(p => p == this.getProblem)
    if(matchingProblems.length > 1) throw new IllegalStateException("Each problem link must only have ONE linked problem")

    matchingProblems.head.getTypeName()
  }
}

object ProblemPointer extends ProblemPointer with LongKeyedMetaMapper[ProblemPointer] {

  def findAllByCourse(course: Course): List[ProblemPointer] = findAll(By(ProblemPointer.courseId, course))

  def findAllByFolder(folder: Folder): List[ProblemPointer] = findAll(By(ProblemPointer.folderId, folder))

  def findAllByReferencedProblem(problem: Problem): List[ProblemPointer] = findAll(By(ProblemPointer.referencedProblemId, problem))

  def deleteProblemsUnderFolder(folder: Folder): Unit = this.findAllByFolder(folder).foreach(_.delete_!)

  def deleteByReferencedProblem(problem: Problem) : Unit = this.findAllByReferencedProblem(problem).foreach(_.delete_!)

}

