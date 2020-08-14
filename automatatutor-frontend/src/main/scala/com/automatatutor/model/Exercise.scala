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



class Exercise extends LongKeyedMapper[Exercise] with IdPK {

  def getSingleton = Exercise

  protected object courseId extends MappedLongForeignKey(this, Course)
  protected object allowedAttempts extends MappedLong(this)
  protected object folderId extends MappedLongForeignKey(this, Folder)
  protected object referencedProblemId extends MappedLongForeignKey(this, Problem)
  protected object maxGrade extends MappedLong(this)
  def getExerciseID: Long = this.id.is

  def getProblem = this.referencedProblemId.obj openOrThrowException "Every Exercise must have a Problem"
  def setProblem ( problem : Problem ) = this.referencedProblemId(problem)

  def getCourse : Box[Course] = this.courseId.obj
  def setCourse ( course : Course ) = this.courseId(course)
  def setCourse ( course: Box[Course] ) = this.courseId(course)

  def getAllowedAttempts: Long = this.allowedAttempts.is
  def getAllowedAttemptsString: String = if (this.allowedAttempts.is == 0) "∞" else this.allowedAttempts.is.toString
  def setAllowedAttempts(attempts: Long) = this.allowedAttempts(attempts)

  def getFolder: Folder = this.folderId.obj openOrThrowException "Every Exercise must have a Folder"
  def setFolder(folder: Folder) = this.folderId(folder)
  def setFolder(folder: Box[Folder]) = this.folderId(folder)

  def getMaxGrade: Long = this.maxGrade.is
  def setMaxGrade(maxGrade: Long) = this.maxGrade(maxGrade)

  def getAttempts(user: User): Seq[SolutionAttempt] = {
    SolutionAttempt.findAll(
      By(SolutionAttempt.userId, user),
      By(SolutionAttempt.exerciseId, this))
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

  def getHighestAttempt(user: User): Int = {
    val grades = this.getAttempts(user).map(_.grade.is)
    if (grades.isEmpty) {
      0
    } else {
      grades.max
    }
  }

  def getGrade(user: User): Float = {
    this.getHighestAttempt(user).toFloat / this.getAllowedAttempts
  }

  def getLongDescription: String = {
    val matchingProblems = Problem.findAll().filter(p => p == this.getProblem)
    if(matchingProblems.length > 1) throw new IllegalStateException("Each Exercise must only have ONE linked problem")

    matchingProblems.head.getLongDescription
  }

  def getShortDescription: String = {
    val matchingProblems = Problem.findAll().filter(p => p == this.getProblem)
    if(matchingProblems.length > 1) throw new IllegalStateException("Each Exercise must only have ONE linked problem")

    matchingProblems.head.getShortDescription
  }

  def getTypeName: String = {
    val matchingProblems = Problem.findAll().filter(p => p == this.getProblem)
    if(matchingProblems.length > 1) throw new IllegalStateException("Each Exercise must only have ONE linked problem")

    matchingProblems.head.getTypeName()
  }

  def canBeDeleted : Boolean = true

  override def delete_! : Boolean = {
    if (!canBeDeleted) {
      false
    } else {
      SolutionAttempt.deleteAllByExercise(this)
      super.delete_!
    }
  }

  /**
    * An Exercise is defined as closed if either the user has used all attempts
    * or if they have reached the maximal possible grade
    */
  def isOpen(user: User): Boolean = {
    if(user.isAdmin || user.isInstructor) return true

    val allowedAttempts = this.allowedAttempts.is
    val takenAttempts = this.getNumberAttempts(user)
    val maxGrade = this.maxGrade.is
    val userGrade = this.getHighestAttempt(user)

    takenAttempts < allowedAttempts && userGrade < maxGrade
  }

  def getStudentsWhoAttempted: List[User] = {
    SolutionAttempt
      .findAll(By(SolutionAttempt.exerciseId, this))
      .map(_.getUser)
      .filter(_.isStudent)
      .distinct
  }
}

object Exercise extends Exercise with LongKeyedMetaMapper[Exercise] {

  def findAllByCourse(course: Course): List[Exercise] = findAll(By(Exercise.courseId, course))

  def findAllByFolder(folder: Folder): List[Exercise] = findAll(By(Exercise.folderId, folder))

  def findAllByReferencedProblem(problem: Problem): List[Exercise] = findAll(By(Exercise.referencedProblemId, problem))

  def deleteProblemsUnderFolder(folder: Folder): Unit = this.findAllByFolder(folder).foreach(_.delete_!)

  //Given a Problem object, delete all the Exercises that reference said problem
  def deleteByReferencedProblem(problem: Problem) : Unit = this.findAllByReferencedProblem(problem).foreach(_.delete_!)

}

