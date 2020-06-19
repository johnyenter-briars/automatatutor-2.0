package com.automatatutor.model

import net.liftweb.mapper._
import net.liftweb.common.Box

class UserToCourse extends LongKeyedMapper[UserToCourse] with IdPK {
	def getSingleton = UserToCourse

	protected object userId extends MappedLongForeignKey(this, User)
	protected object courseId extends MappedLongForeignKey(this, Course)
	
	def getUser : User = this.userId.obj openOrThrowException "Every UserToCourse must have a User"
	def setUser ( user : User ) = this.userId(user)
	
	def getCourse : Course = this.courseId.obj openOrThrowException "Every UserToCourse must have a Course"
	def setCourse ( course : Course ) = this.courseId(course)
}

object UserToCourse extends UserToCourse with LongKeyedMetaMapper[UserToCourse] {
	def deleteByCourse( course : Course ) = this.bulkDelete_!!(By(UserToCourse.courseId, course))
	
	def findAllByUser(user : User) : List[UserToCourse] = 
	  this.findAll(By(UserToCourse.userId, user))
	def countAllByUser(user : User) : Long = 
	  this.count(By(UserToCourse.userId, user))
	def deleteAllByUser(user : User) : Unit = 
	  this.bulkDelete_!!(By(UserToCourse.userId, user))
	  
	def findAllByCourse(course : Course) : List[UserToCourse] = 
	  this.findAll(By(UserToCourse.courseId, course))
	def countAllByCourse(course : Course) : Long = 
	  this.count(By(UserToCourse.courseId, course))
	def deleteAllByCourse(course : Course) : Unit = 
	  this.bulkDelete_!!(By(UserToCourse.courseId, course))
	
	def findByUserAndCourse(user : User, course : Course) : Box[UserToCourse] = 
	  this.find(By(UserToCourse.userId, user), By(UserToCourse.courseId, course))
	def deleteByUserAndCourse(user : User, course : Course) : Unit = 
	  this.bulkDelete_!!(By(UserToCourse.userId, user), By(UserToCourse.courseId, course))
}
