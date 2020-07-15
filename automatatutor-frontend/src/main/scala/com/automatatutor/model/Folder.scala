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

  // "posed" information
  protected object courseId extends MappedLongForeignKey(this, Course)
  protected object longDescription extends MappedText(this)
  protected object createdBy extends MappedLongForeignKey(this, User)
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

  def getProblemsUnderFolder: List[ProblemLink] = {
    ProblemToFolder.findAllByFolder(this).map(_.getProblem)
  }

  //TODO 7/15/2020 fix this
  def getStartDate: Date = new Date(2001, 4, 1)
  def setStartDate(startDate: Date) = this.startDate(startDate)

  def getEndDate: Date = new Date(2001, 4, 1)
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
      //before deleting the folder, we must send all the problems within the folder back to their original state
      ProblemToFolder.deleteProblemsUnderFolder(this)
      super.delete_!
    }
  }


  //TODO 7/15/2020 update this isOpen function and reference it in CourseSnippit
  /**
    * A problem is defined as closed if either the user has used all attempts
    * or if they have reached the maximal possible grade
    */
//  def isOpen(user: User): Boolean = {
//    val allowedAttempts = this.allowedAttempts.is
//    val takenAttempts = this.getNumberAttempts(user)
//    val maxGrade = this.maxGrade.is
//    val userGrade = this.getGrade(user)
//
//    return takenAttempts < allowedAttempts && userGrade < maxGrade
//  }
}

object Folder extends Folder with LongKeyedMetaMapper[Folder] {
  def findAllByCourse(course: Course): List[Folder] = findAll(By(Folder.courseId, course))

  def findByID(ID: String): Folder = this.findAll().filter(_.getFolderID == ID.toLong).head

  def deleteByCourse(course: Course) : Unit = {
    this.findAllByCourse(course).filter( folder => folder.getCourse == course).foreach(folder => folder.delete_!)
  }
}

