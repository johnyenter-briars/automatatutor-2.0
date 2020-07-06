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

  def getProblemsUnderFolder(): List[Problem] = {
    val problems = ProblemToFolder.findAllByFolder(this).map(_.getProblem)
    return problems
  }

  def canBeDeleted : Boolean = true
}

object Folder extends Folder with LongKeyedMetaMapper[Folder] {
  def findAllByCourse(course: Course): List[Folder] = findAll(By(Folder.courseId, course))
}
