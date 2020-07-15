package com.automatatutor.model

import net.liftweb.mapper._
import net.liftweb.common.{Box, Full}
import net.liftweb.common.Empty
import net.liftweb.util.FieldError

import scala.xml.{NodeSeq, Text, Elem}
import bootstrap.liftweb.StartupHook
import com.automatatutor.lib.Config

import scala.util.Try

class User extends MegaProtoUser[User] with ProtoUser[User] {

  object externalID extends MappedText(this) {
    override def defaultValue: String = "none"
  }
  object externalSource extends MappedText(this) {
    override def defaultValue: String = "none"
  }
  object matrnr extends MappedText(this) {
    override def defaultValue: String = "none"
  }
  
  object level extends MappedLong(this){
    override def defaultValue: Long = 1 //levels: 1 = student (default), 2 = instructor, 3 = admin
  }
  
	override def getSingleton = User
	
	def isExternal : Boolean = this.externalSource.is != "none"
	
	def isStudent = (this.level.is == 1)
	def isInstructor = (this.level.is == 2)
	def isAdmin = (this.level.is == 3)
	def makeStudent = this.level(1)
	def makeInstructor = this.level(2)
	def makeAdmin = this.level(3)
	def hasAtLeastLevel(l : Long) = (this.level.is >= l)
	def getLevel : Long = this.level.is
	def setLevel (l : Long) = this.level(l)
	def getLevelString : String = getLevelString(this.level.is)
	def getLevelString (l : Long) : String = {
	  if (l == 1) return "student"
	  if (l == 2) return "instructor"
	  if (l == 3) return "admin"
	  return "unknown"
	}
	
	def getCourses : List[Course] = {
	  if (this.isAdmin) return Course.findAll()
	  else return UserToCourse.findAllByUser(this).map(_.getCourse)
	}
	def hasCourses : Boolean = {
	  if (this.isAdmin) return Course.count() > 0
	  else return UserToCourse.countAllByUser(this) > 0
	}
	def getSupervisedCourses : List[Course] = this.getCourses.filter(_.canBeSupervisedBy(this))
	def hasSupervisedCourses : Boolean = !this.getSupervisedCourses.isEmpty
	def getParticipantCourses : List[Course] = this.getCourses.filter(! _.canBeSupervisedBy(this))
	def hasParticipantCourses : Boolean = !this.getParticipantCourses.isEmpty
	

	def canBeDeleted : Boolean = !(hasSupervisedCourses || isAdmin)
	def getDeletePreventers : Seq[String] = {
	  //val supervisorPreventers : Seq[String] = if (this.hasSupervisedCourses) List("User still supervises a course") else List()
	  val adminPreventers : Seq[String] = if (this.isAdmin) List("User is Admin") else List()
	  //val preventers = supervisorPreventers ++ adminPreventers
	  return adminPreventers
	}

	override def delete_! : Boolean = {
	  if (!canBeDeleted) {
	    return false
	  } else {
	    this.getCourses.map(_.dismiss(this))
			//TODO: 7/15/2020 fix this
			//		Problem.deleteByCreator(this)
	    return super.delete_!
	  }
	}

	def signupExternalAccount(externalSource : String, externalID : String, firstname: String, lastname: String, email: String, matrnr: String): Unit =
	{
	  val user = Try(User.find(By(User.externalSource, externalSource), By(User.externalID, externalID)).get).getOrElse(createExternal(externalSource, externalID, firstname, lastname, email, matrnr))
	  User.logUserIdIn(user.id.toString)
	}

	def createExternal(externalSource : String, externalID : String, firstname: String, lastname: String, email: String, matrnr: String): User =
	{
	  val user = User.create
	  user.email(email)
	  user.firstName(firstname)
	  user.lastName(lastname)
	  user.externalID(externalID)
	  user.matrnr(matrnr)
	  user.validated(true)
	  user.superUser(false)
	  user.externalSource(externalSource)
	  user.save
	  return user
	}
	
	override def validate = (this.validateFirstName) ++ (this.validateLastName) ++ (super.validate)
	
	private def validateFirstName : List[FieldError] = {
	  if (this.firstName == null || this.firstName.is.isEmpty()) {
	    return List[FieldError](new FieldError(this.firstName, Text("First name must be set")))
	  } else {
	    return List[FieldError]()
	  }
	}
	
	private def validateLastName : List[FieldError] = {
	  if (this.lastName == null || this.lastName.is.isEmpty()) {
	    return List[FieldError](new FieldError(this.lastName, Text("Last name must be set")))
	  } else {
	    return List[FieldError]()
	  }
	}
}

object User extends User with MetaMegaProtoUser[User] with StartupHook {
	// Don't send out emails to users after registration. Remember to set this to false before we go into production
	override def skipEmailValidation = !Config.security.verifyEmails.get
	
	// this overridse the noreply@... address that is set by default right now
	// for this to work the correct properties must
	//Mailer.authenticator.map(_.user) openOr 
	override def emailFrom = Config.mail.from.get
	
	// Display the standard template around the User-defined pages
	override def screenWrap = Full(<lift:surround with="default" at="content"><lift:bind /></lift:surround>)

	// Only query given name, family name, email address and password on signup
	override def signupFields = List(firstName, lastName, email, password)

	// Only display given name, family name and email address for editing
	override def editFields = List(firstName, lastName)
	
	// HACK: change the link in signup & PWReset mail
	// (Needed because of load balancing. Standart links dont work properly...)
	override def signupMailBody(user: User, validationLink: String): Elem = {
	  val importent_part = validationLink.substring(validationLink.lastIndexOf("/"))
	  val link = Config.base_url.url.get + "/user_mgt/validate_user" + importent_part
	  return super.signupMailBody(user, link)
	}
	
	override def passwordResetMailBody(user: TheUserType, resetLink: String): Elem = {
	  val importent_part = resetLink.substring(resetLink.lastIndexOf("/"))
	  val link = Config.base_url.url.get + "/user_mgt/reset_password" + importent_part
	  return super.passwordResetMailBody(user, link)
	}
	
	override def afterCreate = List(
		(user : User) => { }
	)
	
	override def changePassword : NodeSeq = {
		if (this.isExternal) return Text("You cannot change your password because you login with an external login service.")
		else return super.changePassword
	}
	
	def onStartup = {
	  val adminEmail = Config.admin.email.get
	  val adminPassword = Config.admin.password.get
	  val adminFirstName = Config.admin.firstname.get
	  val adminLastName = Config.admin.lastname.get
	  
	  /* Delete all existing admin accounts, in case there are any leftover from
	   * previous runs */
	  val adminAccounts = User.findAll(By(User.email, adminEmail))
	 
	  //User.bulkDelete_!!(By(User.email, adminEmail))
	  
	  // Create new admin only if the user in the config does not exists	  
	  if (adminAccounts.isEmpty){
		  val adminUser = User.create
		  adminUser.firstName(adminFirstName).lastName(adminLastName).email(adminEmail).password(adminPassword).validated(true).makeAdmin.save
	  } else {
		  // otherwise just change the password for his account to the one in the config
		  var user = adminAccounts.head
		  var passwordList = List(adminPassword,adminPassword)
		  passwordList
		  user.setPasswordFromListString(passwordList)
		  user.makeAdmin
		  user.save
	  }
	  
	  // create student dummy test account
	  val studentTestAccounts = User.findAll(By(User.email, "student"))
	  if (studentTestAccounts.isEmpty){
		  User.create.firstName("student").lastName("student").email("student").password("student").validated(true).makeStudent.save
	  } else {
		  // otherwise just change the password for his account to the one in the config
		  var user = studentTestAccounts.head
		  var passwordList = List("student","student")
		  user.setPasswordFromListString(passwordList)
		  user.makeStudent
		  user.save
	  }
	  
	  // create teacher dummy test account
	  val instructorTestAccounts = User.findAll(By(User.email, "teacher"))
	  if (instructorTestAccounts.isEmpty){
		  User.create.firstName("teacher").lastName("teacher").email("teacher").password("teacher").validated(true).makeInstructor.save
	  } else {
		  // otherwise just change the password for his account to the one in the config
		  var user = instructorTestAccounts.head
		  var passwordList = List("teacher","teacher")
		  user.setPasswordFromListString(passwordList)
		  user.makeInstructor
		  user.save
	  }
	}

	def findByEmail(email : String) : Box[User] = {
	  val users = User.findAll(By(User.email, email))
	  if(users.size == 1) {
	    return Full(users.head)
	  } else {
	    return Empty
	  }
	}
	
	def currentUser_! : User = {
	  this.currentUser openOrThrowException "This method may only be called if we are certain that a user is logged in"
	}	
	
	def currentUserIdInt : Int = {	  
	  this.currentUserId match { 	  
		case Full(myId) => myId.toInt;
		case _ => 0
		}
	}
}


