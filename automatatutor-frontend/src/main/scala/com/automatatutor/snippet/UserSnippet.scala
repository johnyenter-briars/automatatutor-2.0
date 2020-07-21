package com.automatatutor.snippet

import scala.xml.NodeSeq
import scala.xml.Text
import com.automatatutor.lib.TableHelper
import com.automatatutor.model._
import com.automatatutor.model.problems._
import com.automatatutor.renderer.UserRenderer
import net.liftweb.http.RequestVar
import net.liftweb.http.S
import net.liftweb.http.SHtml
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.util.HttpHelpers
import net.liftweb.http.SortedPaginatorSnippet
import net.liftweb.mapper._
import com.automatatutor.lib.Config
import net.liftweb.http.js._
import net.liftweb.http.js.JsCmds._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.common._
import net.liftweb.util.Mailer
import net.liftweb.util.Mailer._

object userToEdit extends RequestVar[User](null)

class Usersnippet extends SortedPaginatorSnippet[User, BaseOwnedMappedField[User]] {
  override def itemsPerPage = Config.layout.usersPerPage.get
  
  // this is the best way I found... not perfect (a lot of cases) but works.
  // ... wasn't able to find abstract type that allows direct creation of OrderBy()
  // i.e. MappedField[Any, User] does not work
  //      and BaseOwnedMappedField[User] does not work
  def headers = List[(String, BaseOwnedMappedField[User])](
    ("id", User.id), 
    ("first", User.firstName), 
    ("last", User.lastName), 
    ("email", User.email), 
    ("level", User.level))
  
  var searchParams = List[QueryParam[User]]()
  if ((S.param("first") openOr "").length > 0) {
    searchParams = Cmp(User.firstName, OprEnum.Like, Full("%" + (S.param("first") openOr "").toLowerCase() + "%"), None, Full("LOWER")) :: searchParams
  }
  if ((S.param("last") openOr "").length > 0) {
    searchParams = Cmp(User.lastName, OprEnum.Like, Full("%" + (S.param("last") openOr "").toLowerCase() + "%"), None, Full("LOWER")) :: searchParams
  }
  if ((S.param("email") openOr "").length > 0) {
    searchParams = Cmp(User.email, OprEnum.Like, Full("%" + (S.param("email") openOr "").toLowerCase() + "%"), None, Full("LOWER")) :: searchParams
  }
  if ((S.param("id") openOr "").length > 0) {
    try {
	  searchParams = By(User.id, (S.param("id") openOr "").toLong) :: searchParams
    } catch {case e: NumberFormatException => {}}
  }
  if ((S.param("level") openOr "").length > 0) {
    try {
	  searchParams = By(User.level, (S.param("level") openOr "").toLong) :: searchParams
    } catch {case e: NumberFormatException => {}}
  }
  
  def sortQueryPara : QueryParam[User] = {
    val dir = if (sort._2) Ascending else Descending
	val name = headers(sort._1)._1
	if (name == "first") return OrderBy(User.firstName, dir, Empty)
	if (name == "last") return OrderBy(User.lastName, dir, Empty)
	if (name == "email") return OrderBy(User.email, dir, Empty)
	if (name == "level") return OrderBy(User.level, dir, Empty)
	return OrderBy(User.id, dir, Empty)
  }
  
  val searchParamsForPage = sortQueryPara :: (MaxRows[User](itemsPerPage) :: (StartAt[User](curPage*itemsPerPage) :: searchParams))

  override def count = User.count(searchParams: _*)
  
  override def page = User.findAll(searchParamsForPage: _*) 
  
  def addFilterParams (url: String): String  = {
    val paramsAll = List("first", "last", "email", "id", "level")
    val paramsDefined = paramsAll.filter(x => (S.param(x) openOr "").length > 0)
	val params = paramsDefined.map(x => x -> (S.param(x) openOr ""))
	
    Helpers.appendParams(url, params)
  } 
  
  override def sortedPageUrl(offset: Long, sort: (Int, Boolean)): String  = addFilterParams(super.sortedPageUrl(offset, sort))
  override def pageUrl(offset: Long): String = sortedPageUrl(offset, sort)
  
  def showuser (xhtml : NodeSeq) : NodeSeq = {
    var user = userToEdit.is
	if (user == null) {
	  try {
	    val user_id = (S.param("id") openOr "").toLong
	    user = User.findByKey(user_id) openOr null
      } catch {case e: NumberFormatException => {}}
    }
	if (user == null) {
      S.warning("No user was selected or the requested user was not found.")
      return S.redirectTo("/users/index")
    }
    
    def editSubmit() = { 
      user.save
      S.redirectTo("/users/index")
    }
    
    def firstNameField = SHtml.text(user.firstName.is, user.firstName(_))
    def lastNameField = SHtml.text(user.lastName.is, user.lastName(_))
    def emailField = SHtml.text(user.email.is, user.email(_))
    
	val levelField = SHtml.select(Array(("1", "student"), ("2", "instructor"), ("3", "admin")), Full(user.getLevel.toString), {x => user.setLevel(x.toLong)})
    
    def submitButton = SHtml.submit("Submit", editSubmit)
    
    Helpers.bind("userdisplay", xhtml,
        "firstname" -> firstNameField,
        "lastname" -> lastNameField,
        "email" -> emailField,
        "level" -> levelField,
        "submitbutton" -> submitButton)
  }
  
  def makeinstructor (xhtml : NodeSeq) : NodeSeq = {
    var user = userToEdit.is
	if (user == null) {
	  try {
	    val user_id = (S.param("id") openOr "").toLong
	    user = User.findByKey(user_id) openOr null
      } catch {case e: NumberFormatException => {}}
    }
	if (user == null) {
      S.warning("No user was selected or the requested user was not found.")
      return S.redirectTo("/users/index")
    }
	if (!user.isStudent) {
	  S.warning("The user already has all instructor rights!")
      return S.redirectTo("/users/index")
	}
	
	user.makeInstructor
	user.save()
	
	val body = <div>
	    <h3>Automata Tutor: Instructor Rights Granted</h3>
        <p>Hello {user.firstName} {user.lastName},</p>
        <p>your account ({user.email}) is now an instructor. Go ahead and 
          <a href={ Config.base_url.url.get + "/user_mgt/login"}>login</a> and create your first course!
        </p>
        <p>Your Automata Tutor Team</p>
      </div>
	
    Mailer.sendMail(
      From(Config.mail.from.get),
      Subject("Automata Tutor: Instructor Rights Granted"),
      BCC(Config.mail.contact.get),
      To(user.email.is),
      body)
	  
	  S.warning("The user was promoted to instructor.")
      return S.redirectTo("/users/index")
  }
  
  def showall(ignored : NodeSeq) : NodeSeq = {
    val users = page
        
    def userToEditLink(user : User) : NodeSeq =
    	SHtml.link("/users/edit", () => userToEdit(user), Text("Edit user"))
    	
    val userTable = TableHelper.renderTableWithComplexHeader(users,
        (<sort:first>First Name</sort:first>, (user : User) => Text(user.firstName.is)),
        (<sort:last>Last Name</sort:last>, (user : User) => Text(user.lastName.is)),
        (<sort:email>Email</sort:email>, (user : User) => Text(user.email.is)),
        (<sort:level>Role</sort:level>, (user : User) => Text(user.getLevelString)),
        (<sort:id>ID</sort:id>, (user : User) => Text(user.id.toString)),
        (Text(""), (user : User) => userToEditLink(user)),
        (Text(""), (user : User) => (new UserRenderer(user)).renderDeleteLink("/users/index")))
        
    return paginate(userTable)
  }
  
  def searchform(form : NodeSeq) : NodeSeq = {
	return Helpers.bind("filter", form,
        "first" -> SHtml.text((S.param("first") openOr ""), x => {}, "name" -> "first", "id" -> "input_first_name"),
        "last" -> SHtml.text((S.param("last") openOr ""), x => {}, "name" -> "last", "id" -> "input_last_name"),
        "email" -> SHtml.text((S.param("email") openOr ""), x => {}, "name" -> "email", "id" -> "input_email"),
        "id" -> SHtml.text((S.param("id") openOr ""), x => {}, "name" -> "id", "id" -> "input_id"),
        "level" -> SHtml.select(Array(("", ""), ("1", "student"), ("2", "instructor"), ("3", "admin")), S.param("level"), x => {}, "name" -> "level", "id" -> "input_level")
		)
  }
  
  def resetlink(ignored : NodeSeq) : NodeSeq = {
    def resetDatabase() = {
      List(Course, UserToCourse, Problem, SolutionAttempt,
        ProblemPointer, Folder,
	  DFAConstructionProblem, DFAConstructionSolutionAttempt,			// NFA/DFA/RE problems
	  NFAConstructionProblem, NFAConstructionSolutionAttempt, 
	  NFAToDFAProblem, NFAToDFASolutionAttempt, 
      RegExConstructionProblem, RegexConstructionSolutionAttempt,
	  WordsInRegExProblem, WordsInRegexSolutionAttempt,
      RegExToNFAProblem, RegExToNFASolutionAttempt,
      WordsInGrammarProblem, WordsInGrammarSolutionAttempt, 			// Grammar problems
      DescriptionToGrammarProblem, DescriptionToGrammarSolutionAttempt,
	  GrammarToCNFProblem, GrammarToCNFSolutionAttempt,
	  CYKProblem, CYKSolutionAttempt,
	  FindDerivationProblem, FindDerivationSolutionAttempt,
      PDAConstructionProblem, PDAConstructionSolutionAttempt,			// PDA problems
	  PDAWordProblem, PDAWordProblemSolutionAttempt,
      EquivalenceClassesProblem, EquivalenceClassesSolutionAttempt,		// Equivalence classes problem
	  PumpingLemmaGameProblem, PumpingLemmaGameSolutionAttempt,			// Pumpling lemma game problem
	  WhileToTMProblem, WhileToTMSolutionAttempt)
		  .map(_.bulkDelete_!!())
    }
	val onclick : JsCmd = JsRaw("return confirm('Are you sure you want reset the entire database? (This will delete everything but the users.)')") 
    	
    val resetLink = SHtml.link("/users/index", () => resetDatabase, Text("Reset Database"), "onclick" -> onclick.toJsCmd)
    	
    return resetLink
  }

}