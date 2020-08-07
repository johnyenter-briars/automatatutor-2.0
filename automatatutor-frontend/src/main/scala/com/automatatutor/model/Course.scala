package com.automatatutor.model

import net.liftweb.common.Box
import net.liftweb.common.Empty
import net.liftweb.common.Failure
import net.liftweb.common.Full
import net.liftweb.mapper._
import net.liftweb.util.SecurityHelpers
import java.util.Date

import com.automatatutor.lib.Config

import scala.xml._

class Course extends LongKeyedMapper[Course] with IdPK {
	def getSingleton = Course

	object name extends MappedString(this, 300)
	object neededLevelForSupervise extends MappedLong(this){
		override def defaultValue: Long = 2 //default: instructor
	}
	protected object contact extends MappedEmail(this, 100)
	protected object password extends MappedString(this, 20)

	def getNeededLevelForSupervise : Long = this.neededLevelForSupervise.is
	def setNeededLevelForSupervise(l : Long) : Course = this.neededLevelForSupervise(l)
	def canBeSupervisedBy(user : User) : Boolean = user.hasAtLeastLevel(this.neededLevelForSupervise.is)

	def getName : String = this.name.is
	def setName(name : String) : Course = this.name(name)

	def getContact : String = this.contact.is
	def setContact(contact : String) : Course = this.contact(contact)

	def getPassword : String = {
	  if (this.password.is == null || this.password.is.equals("")) { this.password(SecurityHelpers.randomString(8)).save }
	  return this.password.is
	}
	def setPassword ( password : String ) : Course = this.password(password)

	def getUsers : Seq[User] = UserToCourse.findAllByCourse(this).map(_.getUser)
	def hasUsers : Boolean = !this.getUsers.isEmpty
	def getParticipants : Seq[User] = this.getUsers.filter(!this.canBeSupervisedBy(_))
	def hasParticipants : Boolean = !this.getParticipants.isEmpty
	def getSupervisors : Seq[User] = this.getUsers.filter(this.canBeSupervisedBy(_))
	def hasSupervisors : Boolean = !this.getSupervisors.isEmpty

	def enroll(user : User) = if(!this.isEnrolled(user)) { UserToCourse.create.setUser(user).setCourse(this).save }
	def dismiss(user : User) =  {
		//NOTE: dismissing a user does not delete their history of attempts. So if they rejoined, that data is still present
	  UserToCourse.deleteByUserAndCourse(user, this) 
	  if (!this.hasUsers) this.delete_!
	}

	def getCourseId : String = {
	  val fullId : String = this.id.is.toString + this.name.is.replaceAll(" ", "").toUpperCase()
	  if (fullId.length() > 8) {
	    return fullId.substring(0, 9)
	  } else {
	    return fullId.padTo(8, "X").mkString
	  }
	}

	def isEnrolled(user : User) = !UserToCourse.findByUserAndCourse(user, this).isEmpty

	//5/16/2020 Updated to use ProblemPointers instead of raw Problems
	def getProblems : List[ProblemPointer] = ProblemPointer.findAllByCourse(this)
	def getPosedProblems : List[ProblemPointer] = getProblems.filter(p => p.getFolder.getPosed)
	def getSolvableProblems : List[ProblemPointer] = getPosedProblems.filter(p => p.getFolder.getStartDate.compareTo(new Date()) < 0)

	//The total points is a sum of the highest attempt by the student on all the problems they attempted in the course
	def getTotalPoints(user: User): Int = {
		val folders = Folder.findAllByCourse(this).filter(_.getPosed)

		folders.map(_.getAchievedPoints(user)).sum
	}

	def getProblemsForUser(user : User) : List[ProblemPointer] = {
	  if (!user.isAdmin && !this.isEnrolled(user)) return List()
	  if (this.canBeSupervisedBy(user)) return this.getProblems
	  return this.getSolvableProblems
	}
	def getFoldersForUser(user: User) : List[Folder] = {
		if (!user.isAdmin && !this.isEnrolled(user)) return List()
		if (this.canBeSupervisedBy(user)) return Folder.findAllByCourse(this)
		//otherwise, this is a student, show them all posed and open folders
		return Folder.findAllByCourse(this).filter(f => f.getPosed && f.isOpen)
	}

	override def delete_! : Boolean = {
		UserToCourse.deleteByCourse(this)
		Folder.deleteByCourse(this)
		return super.delete_!
	}

	def renderGradesCsv: String = {
		val posedProblems = this.getPosedProblems
		val participantsWithGrades : Seq[(User, Seq[Int], Int)] = this.getParticipants.map(participant => (participant, posedProblems.map(_.getHighestAttempt(participant)), this.getTotalPoints(participant)))
		val firstLine = "FirstName;LastName;Email;" + posedProblems.map(_.getShortDescription).mkString(";") + ";Total;"
		val csvLines = participantsWithGrades.map(tuple => List(tuple._1.firstName, tuple._1.lastName, tuple._1.email, tuple._2.mkString(";"), tuple._3).mkString(";"))

		firstLine + "\n" + csvLines.mkString("\n")
	}

	def renderGradesXml: Node = {
		val userGrades = this.getParticipants.map(participant => {
			val userEmailAttribute = new UnprefixedAttribute("email", participant.email.is, Null)
			val userLastNameAttribute = new UnprefixedAttribute("lastname", participant.lastName.is, userEmailAttribute)
			val userFirstNameAttribute = new UnprefixedAttribute("firstname", participant.firstName.is, userLastNameAttribute)
			val children: NodeSeq = this.getPosedProblems.map(problem => {
				val problemDescriptionAttribute = new UnprefixedAttribute("shortDescription", problem.getShortDescription, Null)
				val maxGradeAttribute = new UnprefixedAttribute("maxGrade", problem.getMaxGrade.toString, problemDescriptionAttribute)
				val problemTypeAttribute = new UnprefixedAttribute("problemType", problem.getTypeName, maxGradeAttribute)
				val attemptsByUserAttribute = new UnprefixedAttribute("attemptsByUser", problem.getNumberAttempts(participant).toString, problemTypeAttribute)
				val allowedAttemptsAttribute = new UnprefixedAttribute("allowedAttempts", problem.getAllowedAttempts.toString, attemptsByUserAttribute)
				Elem(null, "grade", allowedAttemptsAttribute, TopScope, true, Text(problem.getHighestAttempt(participant).toString))
			})
			new Elem(null, "usergrades", userFirstNameAttribute, TopScope, true, children: _*)
		})
			
		return <coursegrades>
			{userGrades}
		</coursegrades>
	}
}

object Course extends Course with LongKeyedMetaMapper[Course] {

	def createNewCourse(name: String, user: User, pw: String = SecurityHelpers.randomString(8)) : Box[Course] = {
		if (name.isEmpty() || Course.findByName(name) != Empty) {
			return Empty
		} else {
			val course: Course = Course.create.setName(name).setContact(user.email.is).setPassword(pw)
			course.setNeededLevelForSupervise(user.getLevel)
			course.save

			course.enroll(user)
			return Full(course)
		}
	}

	def findByName ( name : String ) : Box[Course] = {
	  val courses = this.findAll()
	  val coursesWithId = courses.filter(_.getName.equals(name))
	  return coursesWithId.size match {
	    case 0 => Empty
	    case 1 => Full(coursesWithId.head)
	    case 2 => Failure("Multiple courses with same name")
	  }
	}

	def findTryitCourse() : Box[Course] = {
		if (! Config.try_it.enabled.get) return Empty
		return Course.find(By(Course.name, Config.try_it.course_name.get))
	}
}