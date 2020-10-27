package com.automatatutor.snippet

import scala.Array.canBuildFrom
import scala.xml._
import com.automatatutor.lib._
import com.automatatutor.model._
import com.automatatutor.model.problems.WordsInGrammarProblem
import com.automatatutor.renderer.{CourseRenderer, ProblemRenderer}
import net.liftweb.common.Empty
import net.liftweb.common.Full
import net.liftweb.http._
import net.liftweb.http.SHtml.ElemAttr.pairToBasic
import net.liftweb.http.SHtml.ElemAttr
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.mapper.By
import net.liftweb.util.AnyVar.whatVarIs
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers._
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.util.SecurityHelpers
import net.liftweb.util.Mailer
import net.liftweb.util.Mailer._

class Mainsnippet {
  def courselist(ignored: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    if (!user.hasCourses) return Text("You are not enrolled in any course...")

    val supervisedCourses = User.currentUser.map(_.getSupervisedCourses) openOr List()
    val supervisedCoursesNodeSeq = if (!supervisedCourses.isEmpty) {
      TableHelper.renderTableWithHeader(
        supervisedCourses,
        ("Course Name", (course: Course) => new CourseRenderer(course).renderSelectLink),
        ("Participants", (course: Course) => Text(course.getParticipants.length.toString)),
        ("Password", (course: Course) => Text(course.getPassword)),
        ("Contact", (course: Course) => new CourseRenderer(course).renderContactLink),
        ("", (course: Course) => new CourseRenderer(course).renderDeleteLink))
    } else {
      NodeSeq.Empty
    }

    val participantCourses = User.currentUser.map(_.getParticipantCourses) openOr List()
    val participantCoursesNodeSeq = if (!participantCourses.isEmpty) {
      <h2>Courses</h2> ++ {
        TableHelper.renderTableWithHeader(
          participantCourses,
          ("Course Name", (course: Course) => Text(course.getName)),
          ("Contact", (course: Course) => new CourseRenderer(course).renderContactLink),
          ("", (course: Course) => new CourseRenderer(course).renderSelectButton))
      }
    } else {
      NodeSeq.Empty
    }

    return supervisedCoursesNodeSeq ++ participantCoursesNodeSeq
  }

  def createcourseform(ignored: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
    if (user.isStudent) return NodeSeq.Empty

    var name: String = ""

    def createCourse() = {
      if (name.isEmpty()) {
        S.error("Name must not be empty")
      } else if (Course.findByName(name) != Empty) {
        S.error("The name is already taken!")
      } else {
        val course: Course = Course.create.setName(name).setContact(user.email.is).setPassword(SecurityHelpers.randomString(8))
        course.setNeededLevelForSupervise(user.getLevel)
        course.save

        course.enroll(User.currentUser openOrThrowException "Lift prevents non-logged-in users from being here")

        S.redirectTo("/main/index")
      }
    }

    val defaultName = ""
    val nameField = SHtml.text(defaultName, name = _, "maxlength" -> "300")
    val submitButton = SHtml.submit("Create", createCourse)

    val template: NodeSeq = Templates(List("main", "createCourseFormTemplate")) openOr Text("Could not find template /main/createCourseFormTemplate")
    Helpers.bind("enrollmentform", template,
      "name" -> nameField,
      "createbutton" -> submitButton)
  }


  def enrollform(ignored: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
    if (user.isAdmin) return NodeSeq.Empty
    var courseId: String = ""
    var coursePassword: String = ""

    def enroll() = {
      Course.find(By(field = Course.name, value = courseId)) match {
        case Full(course) => if (course.getPassword.equals(coursePassword)) {
          course.enroll(user)
          S.notice("You successfully enrolled in course " + courseId)
        } else {
          S.error("Password " + coursePassword + " is incorrect for course " + courseId)
        }
        case _ => {
          S.error("Course with id " + courseId + " not found")
        }
      }
    }

    val courseIdField = SHtml.text("", courseId = _)
    val coursePasswordField = SHtml.text("", coursePassword = _)
    val enrollButton = SHtml.submit("Enroll", enroll)

    val template: NodeSeq = Templates(List("main", "enrollFormTemplate")) openOr Text("Could not find template /main/enrollFormTemplate")
    Helpers.bind("enrollmentform", template,
      "courseidfield" -> courseIdField,
      "coursepasswordfield" -> coursePasswordField,
      "enrollbutton" -> enrollButton)
  }


  def personalsection(ignored: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    val autogenButton = SHtml.link("/autogen/index", () => (), <button type='button'>Autogenerate a Problem</button>)
    val problemPoolButton = SHtml.link("/main/problempool/index", () => (), <button type='button'>Problem Pool</button>)
    val usersButton = SHtml.link("/users/index", () => (), <button type='button'>User List</button>)
    val editProfilButton = SHtml.link("/user_mgt/change_password", () => (), <button type='button'>Change Password</button>)
    val becomeInstructorButton = SHtml.link("/main/become_instructor", () => {}, <button type='button'>Become Instructor</button>)

    val exportLink = SHtml.link("/main/export", () => {}, Text("Export All Problems From Database"))
    val importLink = SHtml.link("/main/import", () => {}, Text("Import Problems Into Database"))

    if (user.isAdmin) return autogenButton ++ problemPoolButton ++ usersButton ++  Unparsed("&emsp;") ++ exportLink ++  Unparsed("&emsp;") ++ importLink
    if (user.isInstructor) return autogenButton ++ problemPoolButton ++ editProfilButton ++ Unparsed("&emsp;") ++ exportLink ++  Unparsed("&emsp;") ++ importLink
    return autogenButton ++ editProfilButton ++ becomeInstructorButton
  }


  def becomeinstructorform(ignored: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
    if (!user.isStudent) return Text("You are already an instructor...")
    var institution: String = ""
    var website: String = ""
    var message: String = ""

    def sendRequest() = {
      val body = <div>
        <h3>Automata Tutor: Request for Instructor Rights</h3>
        <ul>
          <li>
            <strong>Name:</strong>{user.firstName}{user.lastName}
          </li>
          <li>
            <strong>Email:</strong>{user.email}
          </li>
          <li>
            <strong>Institution:</strong>{institution}
          </li>
          <li>
            <strong>Website:</strong>{website}
          </li>
          <li>
            <strong>Message:</strong>{message}
          </li>
        </ul>
        First
        <a href={Config.base_url.url.get + "/user_mgt/login"}>login</a>
        and
        <a href={Config.base_url.url.get + "/users/edit?id=" + user.id}>inspect</a>
        the user.
        To grant instructor rights and send an automatic email, click
        <a href={Config.base_url.url.get + "/users/make_instructor?id=" + user.id}>here</a>
        .
      </div>

      Mailer.sendMail(
        From(Config.mail.from.get),
        Subject("Automata Tutor: Instructor Request"),
        To(Config.mail.contact.get),
        body)

      val body2 = <div>
        <h3>Automata Tutor: Request for Instructor Rights</h3>
        <p>Hello
          {user.firstName}{user.lastName}
          ,</p>
        <p>we recieved your request for instructor rights.
          We will check it as soon as possible.
          You will be informed by email as soon as you are granted the rights.
        </p>
        <p>Thank you for your patience.</p>
        <p>Your Automata Tutor Team</p>
      </div>

      Mailer.sendMail(
        From(Config.mail.from.get),
        Subject("Automata Tutor: Instructor Request"),
        To(user.email.is),
        body2)

      S.warning("Your request was send successfully! You sould recieve an answer soon...")
      S.redirectTo("/main/index")
    }

    val institutionField = SHtml.text("", institution = _)
    val websiteField = SHtml.text("", website = _)
    val messageField = SHtml.textarea("", message = _, "rows" -> "4", "cols" -> "50")
    val sendButton = SHtml.submit("Send", sendRequest)

    val template: NodeSeq = Templates(List("main", "becomeInstructorFormTemplate")) openOr Text("Could not find template /main/becomeInstructorFormTemplate")
    Helpers.bind("becomeinstructorform", template,
      "institution" -> institutionField,
      "website" -> websiteField,
      "message" -> messageField,
      "sendbutton" -> sendButton)
  }


  def rendernavigation(ignored: NodeSeq): NodeSeq = {
    val homeButton = SHtml.link("/main/index", () => {}, <button type='button' class='navButton'>Home</button>)
    val logoutButton = SHtml.link("/index", () => {
      User.logUserOut(); S.redirectTo("/index")
    }, <button type='button' class='navButton'>Logout</button>)
    val loginButton = SHtml.link("/user_mgt/login", () => {}, <button type='button' class='navButton' autofocus='autofocus'>Login</button>)
    val registerButton = SHtml.link("/user_mgt/sign_up", () => {}, <button type='button' class='navButton'>Register</button>)

    if (User.loggedIn_?) return homeButton ++ logoutButton
    else return homeButton ++ registerButton ++ loginButton
  }

  def renderaddcoursebutton(xhtml: NodeSeq): NodeSeq = {
    <div>
      <div style="display: flex">
        <h2 style="margin-bottom: 0.5em; margin-right: 0.5em">Supervised Courses</h2>
        <br></br>
        <button type="button" id="add-course_button_1" class="modal-button far fa-plus-square"/>
      </div>

      <div id="add-course_modal_1" class="modal">

        <div class="modal-content">
          <div class="modal-header">
            <span class="close" id="add-course_span_1">
              &times;
            </span>
          </div>
          <div class="modal-body">
            {this.createcourseform(xhtml)}
          </div>
        </div>
      </div>
    </div>
  }

  def renderexportfordatabase(ignored: NodeSeq): NodeSeq = {
    //create export xml
    var xml = NodeSeq.Empty
    Problem.findAll().foreach(
      (problem: Problem) => {
        xml = xml ++ problem.toXML
      })
    xml = <exported>
      {xml}
    </exported>

    val returnLink = SHtml.link("/main/index", () => {}, <button type='button'>Return to Main</button>)
    val downloadLink = DownloadHelper.renderXmlDownloadLink(xml, "AT_DB_Problems", <button type='button'>Download</button>)
    val exportOutputField = SHtml.textarea(xml.toString, (input) => {}, "cols" -> "80", "rows" -> "40")

    downloadLink ++ new Unparsed("<br><br>") ++ returnLink ++ new Unparsed("<br><br>") ++ exportOutputField
  }

  def renderimportfordatabase(ignored: NodeSeq): NodeSeq = {
    val course = CurrentCourse.is

    def importProblems(importing: String) = {
      var imported = 0
      var failed = 0

      val xml = XML.loadString(importing)
      (xml \ "_").foreach(
        (problemXML) => {
          val problem = Problem.fromXML(problemXML)
          if (problem != Empty) {
            imported += 1
          }
          else {
            failed += 1
          }
        })
      //give feedback
      if (imported > 0) {
        S.notice("Successfully imported " + imported.toString + " problems")
      }
      if (failed > 0) {
        S.error("Could not import " + failed.toString + " problems")
      }
      S.redirectTo("/main/index")
    }

    val importForm = <form method="post">
      {SHtml.textarea("", importProblems(_), "cols" -> "80", "rows" -> "40", "placeholder" -> "please copy exported xml data here")}<input type="submit" value="Import"/>
    </form>
    val returnLink = SHtml.link("/main/index", () => {}, <button type='button'>Return to course</button>)

    importForm ++ new Unparsed("<br><br>") ++ returnLink
  }
}

object Mainsnippet {
}