package com.automatatutor.snippet

import java.text.{DateFormat, SimpleDateFormat}
import java.util.Calendar
import java.util.Date

import scala.xml._
import com.automatatutor.lib.DownloadHelper
import com.automatatutor.model._
import com.automatatutor.renderer._
import net.liftweb.common.Empty
import net.liftweb.common.Full
import net.liftweb.http._
import net.liftweb.http.SHtml.ElemAttr.pairToBasic
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.util.AnyVar.whatVarIs
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.http.js.JsCmd
import JsCmds._

import com.automatatutor.model.{Exercise, Problem, ProblemType}
import com.automatatutor.lib.{UploadHelper, UploadTarget, UploadTargetEnum, TableHelper}
import scala.collection.mutable.ListBuffer
import scala.xml


object CurrentCourse extends SessionVar[Course](null) // SessionVar makes navigation easier
object CurrentProblemInCourse extends SessionVar[Problem](null) // SessionVar makes navigation easier
object CurrentProblemTypeInCourse extends RequestVar[ProblemType](null) // RequestVar as only needed in a single request
object CurrentExerciseInCourse extends RequestVar[Exercise](null)
object CurrentBatchExercisesInCourse extends SessionVar[ListBuffer[Exercise]](null)
object CurrentFolderInCourse extends SessionVar[Folder](null)

class Coursesnippet {

  if(CurrentBatchExercisesInCourse.is == null) CurrentBatchExercisesInCourse(new ListBuffer[Exercise])

  def renderfolders(ignored: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    val folders = CurrentCourse.getFoldersForUser(user)


    if (CurrentCourse.canBeSupervisedBy(user)) {
      if (folders.isEmpty) return Text("There are no folders in this course")

      <div>
        {
          TableHelper.renderTableWithHeader(
            folders,
            ("Folder Name", (folder: Folder) => new FolderRenderer(folder).renderSelectLink),
            ("Visible", (folder: Folder) => Text(folder.getVisible.toString)),
            ("Start Date", (folder: Folder) => Text(folder.getStartDateString)),
            ("End Date", (folder: Folder) => Text(folder.getEndDateString)),
            ("Number of Problems", (folder: Folder) => Text(folder.getExercisesUnderFolder.length.toString)),
            ("", (folder: Folder) => new FolderRenderer(folder).renderDeleteLink)
          )
        }
      </div>
    }
    else {
      //logged in user is a student
      if (folders.isEmpty) return Text("There are no folders in this course! Please tell your instructor to add some.")

      <div>
        {
          TableHelper.renderTableWithHeader(
            folders,
            ("Folder Name", (folder: Folder) => Text(folder.getLongDescription)),
            ("Start Date", (folder: Folder) => Text(folder.getStartDate.toString)),
            ("End Date", (folder: Folder) => Text(folder.getEndDate.toString)),
            ("Number of Problems", (folder: Folder) => Text(folder.getExercisesUnderFolder.length.toString)),
            ("", (folder: Folder) => new FolderRenderer(folder).renderSelectButton)
          )
        }
      </div>
    }
  }

  def supervisorsection(ignored: NodeSeq): NodeSeq = {
    val course = CurrentCourse.is

    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
    if (!CurrentCourse.canBeSupervisedBy(user)) return NodeSeq.Empty

    val gradesCsvLink = SHtml.link("/main/course/downloadCSV", () => {}, Text("Grades (as .csv)"))
    val gradesXmlLink = SHtml.link("/main/course/downloadXML", () => {}, Text("Grades (as .xml)"))
    val userLink = SHtml.link("/main/course/users", () => {}, Text("Users"))
    val exportLink = SHtml.link("/main/course/export", () => {}, Text("Export Problems"))
    val importLink = SHtml.link("/main/course/import", () => {}, Text("Import Problems"))


    <h2>Manage Course</h2> ++
      Unparsed("&emsp;") ++
        DownloadHelper.renderZipDownloadLink(course.getName + "_Grades", course.renderFoldersForGradeZip, DownloadHelper.CsvFile, Text("Course Grades")) ++
      Unparsed("&emsp;") ++
      userLink ++
      Unparsed("&emsp;") ++
      DownloadHelper.renderZipDownloadLink(course.getName + "_Folders", course.renderFoldersProblems, DownloadHelper.XmlFile, Text("Export Folders"))
  }

  def renderxmldownloadlink(ignored: NodeSeq): NodeSeq = {
    val course = CurrentCourse.is

    val downloadXmlLink = DownloadHelper.renderXmlDownloadLink(course.renderGradesXml, "grades", Text("Grades.xml"))

    return downloadXmlLink
  }

  def rendercoursecsvdownloadlink(ignored: NodeSeq): NodeSeq = {
    val course = CurrentCourse.is

    val downloadCsvLink = DownloadHelper.renderCsvDownloadLink(course.renderGradesCsv, "grades", Text("Grades.csv"))

    return downloadCsvLink
  }

  def userlist(ignored: NodeSeq): NodeSeq = {
    val course = CurrentCourse.is

    def dismissLink(user: User): NodeSeq = {
      val onclick: JsCmd = JsRaw("return confirm('Are you sure you want to dismiss this user?')")
      return SHtml.link("/main/course/users", () => {
        course.dismiss(user)
      }, Text("dismiss"), "onclick" -> onclick.toJsCmd)
    }

    val supervisorList = if (course.hasSupervisors) {
      <h2>Supervisors</h2> ++ TableHelper.renderTableWithHeader(course.getSupervisors,
        ("First Name", (user: User) => Text(user.firstName.is)),
        ("Last Name", (user: User) => Text(user.lastName.is)),
        ("Email", (user: User) => Text(user.email.is)),
        ("", (user: User) => dismissLink(user)))
    } else {
      NodeSeq.Empty
    }

    val participantList = if (course.hasParticipants) {
      <h2>Participants</h2> ++ TableHelper.renderTableWithHeader(course.getParticipants,
        ("First Name", (user: User) => Text(user.firstName.is)),
        ("Last Name", (user: User) => Text(user.lastName.is)),
        ("Email", (user: User) => Text(user.email.is)),
        //NOTE: These following two line assumes that you only want to count the grades/attempts of questions
        //which are CURRENTLY visible
        //If a student solves a question, but then its containing folder is made invisible, that grade will not be accounted for
        ("Total Attempts", (user: User) => Text(course.getVisibleExercises.map(_.getNumberAttempts(user)).sum.toString)),
        ("Possible Points", (user: User) => Text(course.getPossiblePoints.toString)),
        ("Total Points", (user: User) => Text(course.getTotalPoints(user).toString)),
        ("Average Grade", (user: User) => Text(course.getAverageGrade(user).toString + "%")),
        ("", (user: User) => dismissLink(user)))
    } else {
      <h2>Participants</h2> ++ Text("There are no paticipants yet.")
    }

    return supervisorList ++ participantList
  }

  def rendersolve(ignored: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users on here"
    if (CurrentExerciseInCourse.is == null) {
      S.warning("Please first choose a problem to solve")
      return S.redirectTo("/main/course/index")
    }

    val exercise = CurrentExerciseInCourse.is
    val problem: Problem = exercise.getProblem
    val problemSnippet: SpecificProblemSnippet = problem.getProblemType.getProblemSnippet

    val lastAttempt = SolutionAttempt.getLatestAttempt(user, exercise)

    var lastGrade = 0

    def recordSolutionAttempt(grade: Int, dateTime: Date): SolutionAttempt = {
      lastGrade = grade

      if (dateTime.compareTo(exercise.getFolder.getEndDate) > 0 ||
        dateTime.compareTo(exercise.getFolder.getStartDate) < 0) return null
      // otherwise: create SolutionAttempt
      val solutionAttempt = new SolutionAttempt
      solutionAttempt.exerciseId(exercise).userId(user).dateTime(dateTime).grade(grade).save

      solutionAttempt
    }

    def returnFunc(problem: Problem) = {
      S.redirectTo("/main/course/index")
    }

    def remainingAttempts(): Int = {
      exercise.getNumberAttemptsRemaining(user).toInt
    }

    def bestGrade(): Int = {
      exercise.getHighestAttempt(user)
    }

    //If the user is the admin or instructor, don't even bother recording an attempt
    if (user.isAdmin || user.isInstructor)
      return problemSnippet.renderSolve(problem, exercise.getMaxGrade, Empty, (grade, date) => SolutionAttempt, returnFunc, () => 1, () => 0)

    problemSnippet.renderSolve(problem, exercise.getMaxGrade, lastAttempt, recordSolutionAttempt, returnFunc, remainingAttempts, bestGrade)
  }

  def renderexportforcourse(ignored: NodeSeq): NodeSeq = {
    val course = CurrentCourse.is

    //create export xml
    var xml = NodeSeq.Empty
    course.getExercises.foreach(
      (exercise) => {
        xml = xml ++ exercise.getProblem.toXML
      })
    xml = <exported>
      {xml}
    </exported>

    val returnLink = SHtml.link("/main/course/index", () => {}, <button type='button'>Return to course</button>)
    val downloadLink = DownloadHelper.renderXmlDownloadLink(xml, "Download all problems as an XML file.", <button type='button'>Download</button>)
    val exportOutputField = SHtml.textarea(xml.toString, (input) => {}, "cols" -> "80", "rows" -> "40")

    return downloadLink ++ new Unparsed("<br><br>") ++ returnLink ++ new Unparsed("<br><br>") ++ exportOutputField
  }

  //TODO 8/19/2020 change this back to be an import for the course itself
  def renderimport(ignored: NodeSeq): NodeSeq = {
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
      S.redirectTo("/main/course/index")
    }

    val importForm = <form method="post">
      {SHtml.textarea("", importProblems(_), "cols" -> "80", "rows" -> "40", "placeholder" -> "please copy exported xml data here")}<input type="submit" value="Import"/>
    </form>
    val returnLink = SHtml.link("/main/course/index", () => {}, <button type='button'>Return to course</button>)

    return importForm ++ new Unparsed("<br><br>") ++ returnLink
  }

  def rendernavigation(ignored: NodeSeq): NodeSeq = {
    val overviewButton = SHtml.link("/main/course/index", () => {}, <button type='button' class='navButton'>Course Overview</button>)
    return overviewButton
  }

  def renderbacktocoursebutton(xhtml: NodeSeq): NodeSeq = {
    SHtml.link("/main/course/index", () => {}, <button type='button'>Go back to Course Overview</button>)
  }

  def renderbacktofolderbutton(xhtml: NodeSeq): NodeSeq = {
    SHtml.link("/main/course/folder/index", () => {}, <button type='button'>Go back to Course Overview</button>)
  }

  def rendercoursename(xhtml: NodeSeq): NodeSeq = {
    if (CurrentCourse.is == null) {
      S.warning("You have not selected a course")
      return S.redirectTo("/main/index")
    }
    <h3>{CurrentCourse.is.getName}</h3>
  }

  def addfolderbutton(xhtml: NodeSeq): NodeSeq = {
    if (CurrentCourse.is == null) {
      S.warning("You have not selected a course")
      return S.redirectTo("/main/index")
    }
    val user = User.currentUser openOrThrowException "Lift only allows logged in users on here"

    <div>
        <div style="display: flex">
          <h3 style="margin-bottom: 0.5em; margin-right: 0.5em">Problem Folders</h3>
          <br></br>
          {
            if(CurrentCourse.canBeSupervisedBy(user)){
                <button type="button" id="add-folder_button_1" class="modal-button far fa-plus-square" />
            }
            else{
              NodeSeq.Empty
            }
          }

        </div>

      <div id="add-folder_modal_1" class="modal">
        <div class="modal-content">
          <div class="modal-header">
            <span class="close" id="add-folder_span_1">&times;</span>
            <h3>Add a Folder</h3>
          </div>
          <div class="modal-body">
            {
              FolderSnippetObj.getAddFolderForm(xhtml)
            }
          </div>
        </div>
      </div>
    </div>
  }

  def rendercourseimportbutton(xhtml: NodeSeq): NodeSeq = {
    if (CurrentCourse.is == null) {
      S.warning("Please first choose a course")
      return S.redirectTo("/main/course/index")
    }

    new UploadHelper(new UploadTarget(UploadTargetEnum.Course, CurrentCourse.is)).fileUploadToCourseForm(xhtml)
  }

}

object Courses {
}
