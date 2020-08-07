package com.automatatutor.model

import com.automatatutor.model.problems._
import com.automatatutor.snippet._
import com.automatatutor.snippet.problems._
import com.automatatutor.lib.Config
import net.liftweb.common._
import net.liftweb.mapper._
import bootstrap.liftweb.StartupHook
import com.automatatutor.lib.Config
import scala.xml.XML
import scala.xml.Node
import java.util.{Calendar, Date}

class Folder extends LongKeyedMapper[Folder] with IdPK {
  def getSingleton = Folder

  protected object courseId extends MappedLongForeignKey(this, Course)
  protected object longDescription extends MappedText(this)
  protected object createdBy extends MappedLongForeignKey(this, User)

  // "posed" information
  protected object isPosed extends MappedBoolean(this)
  protected object startDate extends MappedDateTime(this)
  protected object endDate extends MappedDateTime(this)

  def getFolderID: Long = this.id.is

  def getCreator: User = this.createdBy.obj openOrThrowException "Every Folder must have a CreatedBy"
  def setCreator(creator: User) = this.createdBy(creator)

  def getLongDescription = this.longDescription.is
  def setLongDescription(description: String) = this.longDescription(description)

  def getCourse : Box[Course] = this.courseId.obj
  def setCourse ( course : Course ) = this.courseId(course)
  def setCourse ( course: Box[Course] ) = this.courseId(course)

  def getPosed: Boolean = this.isPosed.is
  def setPosed(posed: Boolean) = this.isPosed(posed)

  def getProblemPointersUnderFolder: List[ProblemPointer] = {
    ProblemPointer.findAllByFolder(this)
  }

  def getOpenProblemPointersUnderFolder(user: User): List[ProblemPointer] = {
    if(user.isAdmin || user.isInstructor) return ProblemPointer.findAllByFolder(this)
    ProblemPointer.findAllByFolder(this).filter(_.isOpen(user))
  }

  def getProblemsUnderFolder: List[Problem] = {
    ProblemPointer.findAllByFolder(this).map(_.getProblem)
  }

  def getPossiblePoints: Long = {
    this.getProblemPointersUnderFolder.map(_.getMaxGrade).sum
  }

  def getAchievedPoints(user: User): Int = {
    this.getProblemPointersUnderFolder.map(_.getHighestAttempt(user)).sum
  }

  def getOverallGrade(user: User): Float = {
    val grade = this.getAchievedPoints(user).toFloat / this.getPossiblePoints

    (grade * 100).round
  }

  def getNumAttemptsAcrossAllProblems(user: User): Int = {
    val solutionAttempts =
      SolutionAttempt
        .findAll(By(SolutionAttempt.userId, user))
        .filter(_.getProblemPointer.getFolder == this)
    solutionAttempts.length
  }

  def getStartDate: Date = this.startDate.is
  def setStartDate(startDate: Date) = this.startDate(startDate)

  def getEndDate: Date = this.endDate.is
  def setEndDate(endDate: Date) = this.endDate(endDate)

  def getTimeToExpirationInMs : Long = {
    val nowTimestamp = Calendar.getInstance().getTime().getTime()
    val endDateTimestamp = this.endDate.is.getTime()
    return endDateTimestamp - nowTimestamp
  }
  def getTimeToExpirationString : String = {
    val ms = this.getTimeToExpirationInMs
    if (ms < 0) return "ended"

    val msPerSecond = 1000
    val msPerMinute = 60 * msPerSecond
    val msPerHour = 60 * msPerMinute
    val msPerDay = 24 * msPerHour

    val days = ms / msPerDay
    val hours = (ms % msPerDay) / msPerHour
    val minutes = (ms % msPerHour) / msPerMinute
    val seconds = (ms % msPerMinute) / msPerSecond

    return (days + " days, " + hours + ":" + minutes + ":" + seconds + " hours")
  }


  def canBeDeleted : Boolean = true

  override def delete_! : Boolean = {
    if (!canBeDeleted) {
      false
    } else {
      //before deleting the folder, we must delete all the ProblemPointers that are under the folder
      ProblemPointer.deleteProblemsUnderFolder(this)
      super.delete_!
    }
  }

  /**
    * A folder is defined as open if it's end date falls after the current date
    */
  def isOpen: Boolean = {
    this.getEndDate.compareTo(Calendar.getInstance().getTime) > 0
  }

  def renderGradesCsv: String = {
    val posedProblems = this.getProblemPointersUnderFolder
    val participants = this.getCourse.get.getParticipants
    val participantsWithGrades : Seq[(User, Seq[Int], Int)]
          = participants.map(
            participant => (participant, posedProblems.map(_.getHighestAttempt(participant)), this.getAchievedPoints(participant)))
    val firstLine = "FirstName;LastName;Email;" + posedProblems.map(_.getShortDescription).mkString(";") + ";Total;"
    val csvLines = participantsWithGrades.map(tuple => List(tuple._1.firstName, tuple._1.lastName, tuple._1.email, tuple._2.mkString(";"), tuple._3).mkString(";"))
    firstLine + "\n" + csvLines.mkString("\n")
  }
}

object Folder extends Folder with LongKeyedMetaMapper[Folder] {
  def findAllByCourse(course: Course): List[Folder] = findAll(By(Folder.courseId, course))

  def findByID(ID: String): Folder = this.findAll().filter(_.getFolderID == ID.toLong).head

  def deleteByCourse(course: Course) : Unit = {
    this.findAllByCourse(course).filter( folder => folder.getCourse == course).foreach(folder => folder.delete_!)
  }
}

