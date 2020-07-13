package com.automatatutor.model

import net.liftweb.common.Box
import net.liftweb.common.Empty
import net.liftweb.common.Failure
import net.liftweb.common.Full
import net.liftweb.mapper._
import net.liftweb.util.SecurityHelpers
import java.util.Date

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

	def getProblems : List[Problem] = Problem.findAllByCourse(this)
	def getPosedProblems : List[Problem] = getProblems.filter(_.getPosed)
	def getSolvableProblems : List[Problem] = getPosedProblems.filter(problem => problem.getStartDate.compareTo(new Date()) < 0)
	def getProblemsForUser(user : User) : List[Problem] = {
	  if (!user.isAdmin && !this.isEnrolled(user)) return List()
	  if (this.canBeSupervisedBy(user)) return this.getProblems
	  return this.getSolvableProblems
	}
	def getFoldersForUser(user: User) : List[Folder] = {
		if (!user.isAdmin && !this.isEnrolled(user)) return List()
		if (this.canBeSupervisedBy(user)) return Folder.findAllByCourse(this)
		//otherwise, this is a student, show them all posted foldrs
		return Folder.findAllByCourse(this).filter(_.getPosed)
	}

    override def delete_! : Boolean = {
        UserToCourse.deleteByCourse(this)
	    Problem.deleteByCourse(this)
	    return super.delete_!
    }

	def renderGradesCsv: String = {
		val posedProblems = this.getPosedProblems
		val participantsWithGrades : Seq[(User, Seq[Int])] = this.getParticipants.map(participant => (participant, posedProblems.map(_.getGrade(participant))))
		val firstLine = "FirstName;LastName;Email;" + posedProblems.map(_.getShortDescription).mkString(";") 
		val csvLines = participantsWithGrades.map(tuple => List(tuple._1.firstName, tuple._1.lastName, tuple._1.email, tuple._2.mkString(";")).mkString(";"))
		return firstLine + "\n" + csvLines.mkString("\n")
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
				Elem(null, "grade", allowedAttemptsAttribute, TopScope, true, Text(problem.getGrade(participant).toString))
			})
			new Elem(null, "usergrades", userFirstNameAttribute, TopScope, true, children: _*)
		})
			
		return <coursegrades>
			{userGrades}
		</coursegrades>
	}
}

object Course extends Course with LongKeyedMetaMapper[Course] {
	def findByName ( name : String ) : Box[Course] = {
	  val courses = this.findAll()
	  val coursesWithId = courses.filter(_.getName.equals(name))
	  return coursesWithId.size match {
	    case 0 => Empty
	    case 1 => Full(coursesWithId.head)
	    case 2 => Failure("Multiple courses with same name")
	  }
	}
}