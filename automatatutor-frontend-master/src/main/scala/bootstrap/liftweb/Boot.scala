package bootstrap.liftweb

import net.liftweb._
import util._
import Helpers._
import common._
import http._
import sitemap.{Menu, _}
import Loc._
import net.liftmodules.JQueryModule
import net.liftweb.http.js.jquery._
import net.liftweb.db.{DB, DefaultConnectionIdentifier, StandardDBVendor}
import net.liftweb.mapper.Schemifier
import com.automatatutor.model._
import com.automatatutor.model.problems._
import com.automatatutor.snippet.CurrentCourse
import java.io.FileInputStream

import net.liftweb.util.Mailer
import javax.mail.{Authenticator, PasswordAuthentication}
import com.automatatutor.lib.Config

/**
 * A class that's instantiated early and run.  It allows the application
 * to modify lift's environment
 */
class Boot {
  def boot {

    Mailer.authenticator = Full( new Authenticator {
      override def getPasswordAuthentication =
        new PasswordAuthentication(Config.mail.user.get, Config.mail.password.get)
    } )
  
    // where to search snippet
	LiftRules.addToPackages("com.automatatutor")

	if(!DB.jndiJdbcConnAvailable_?) {
		DB.defineConnectionManager(DefaultConnectionIdentifier, Config.db.get)
		LiftRules.unloadHooks.append(Config.db.get.closeAllConnections_!)
	}

	Schemifier.schemify(true, Schemifier.infoF _, 
	  User, Course, Problem, ProblemType,
	  SolutionAttempt, UserToCourse,
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
	  WhileToTMProblem, WhileToTMSolutionAttempt)						// TM problems

  StartupHooks.hooks map (hook => hook())

	val loggedInPredicate = If(() => User.loggedIn_?, () => RedirectResponse("/index"))
	val notLoggedInPredicate = If(() => !User.loggedIn_?, () => RedirectResponse("/main/index"))
	val courseChosenPredicate = If(() => CurrentCourse.is != null, () => {RedirectResponse("/main/index")})
	val canSuperviseCoursePredicate = If(() => User.loggedIn_? && CurrentCourse.is != null && (User.currentUser.map(CurrentCourse.is.canBeSupervisedBy(_)) openOr false), () => {RedirectResponse("/main/course/index")})
	val isAdminPredicate = If(() => User.currentUser.map(_.isAdmin) openOr false, () => {RedirectResponse("/index") })

    // Build SiteMap
    val entries = List(
      Menu.i("Home") / "index" >> notLoggedInPredicate,
	  
	  Menu.i("Main") / "main" / "index" >> loggedInPredicate submenus(
	    Menu.i("Become Instructor") / "main" / "become_instructor" >> Hidden,
	    Menu.i("Course") / "main" / "course" / "index" >> courseChosenPredicate submenus(
		  Menu.i("User List") / "main" / "course" / "users" >> canSuperviseCoursePredicate,
		  Menu.i("Grade Download XML") / "main" / "course" / "downloadXML" >> canSuperviseCoursePredicate,
		  Menu.i("Grade Download CSV") / "main" / "course" / "downloadCSV" >> canSuperviseCoursePredicate,
		  Menu.i("Create Problem") / "main" / "course" / "problems" / "create" >> Hidden,
		  Menu.i("Pose Problem") / "main" / "course" / "problems" / "pose" >> Hidden,
		  Menu.i("Preview Problem") / "main" / "course" / "problems" / "preview" >> Hidden,
		  Menu.i("Solve Problem") / "main" / "course" / "problems" / "solve" >> Hidden,
		  Menu.i("Edit Problem") / "main" / "course" / "problems" / "edit" >> Hidden,
      Menu.i("Edit Problem Access") / "main" / "course" / "problems" / "editproblemaccess" >> Hidden
		)
	  ),
	  
	  Menu.i("Autogen") / "autogen" / "index" >> loggedInPredicate submenus(
		  Menu.i("Move Autogen Problem To Course") / "autogen" / "move" >> Hidden,
    	  Menu.i("Solve Autogen Problem") / "autogen" / "practice" >> Hidden),
	  
	  Menu.i("Users") / "users" / "index" >> isAdminPredicate submenus(
    	  Menu.i("Edit User") / "users" / "edit" >> Hidden,
    	  Menu.i("Make Instructor") / "users" / "make_instructor" >> Hidden),
		  
      Menu.i("Getting Started") / "getting_started" >> Hidden,
      Menu.i("Privacy") / "privacy" >> Hidden

	) ::: User.sitemap

    // set the sitemap.  Note if you don't want access control for
    // each page, just comment this line out.
    LiftRules.setSiteMap(SiteMap(entries : _*))

    //Show the spinny image when an Ajax call starts
    LiftRules.ajaxStart =
      Full(() => LiftRules.jsArtifacts.show("ajax-loader").cmd)
    
    // Make the spinny image go away when it ends
    LiftRules.ajaxEnd =
      Full(() => LiftRules.jsArtifacts.hide("ajax-loader").cmd)

    // Force the request to be UTF-8
    LiftRules.early.append(_.setCharacterEncoding("UTF-8"))

    // Use HTML5 for rendering
    LiftRules.htmlProperties.default.set((r: Req) =>
      new Html5Properties(r.userAgent))

    //Init the jQuery module, see http://liftweb.net/jquery for more information.
    LiftRules.jsArtifacts = JQueryArtifacts
    JQueryModule.InitParam.JQuery=JQueryModule.JQuery172
    JQueryModule.init()

  }
}

trait StartupHook {
  /* Since code in the trait is executed during the instantiation of the object carrying that trait, every object that has this trait
   * registers itself in the companion object and can easily be executed during 
   */
  StartupHooks.hooks += this.onStartup

  def onStartup(): Unit
}

private object StartupHooks {
  val hooks = collection.mutable.ListBuffer[() => Unit]()
}
