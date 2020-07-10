package com.automatatutor.snippet

import java.text.DateFormat
import java.util.Calendar
import java.util.Date

import scala.Array.canBuildFrom
import scala.xml._
import com.automatatutor.lib._
import com.automatatutor.model._
import com.automatatutor.renderer._
import net.liftweb.common.Empty
import net.liftweb.common.Full
import net.liftweb.http._
import net.liftweb.http.SHtml.ElemAttr.pairToBasic
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.mapper.By
import net.liftweb.util.AnyVar.whatVarIs
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers.bind
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.util.SecurityHelpers
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds._
import SHtml._
import js._
import JsCmds._

import util._
import Helpers._

import scala.collection.JavaConverters._

object CurrentCourse extends SessionVar[Course](null) // SessionVar makes navigation easier
object CurrentProblemInCourse extends SessionVar[Problem](null) // SessionVar makes navigation easier
object CurrentProblemTypeInCourse extends RequestVar[ProblemType](null) // RequestVar as only needed in a single request
object CurrentFolderInCourse extends SessionVar[Folder](null)

class Coursesnippet {
  def rendereditfolderform(xhtml: NodeSeq): NodeSeq ={
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    val dateFormat: DateFormat = DateFormat.getDateTimeInstance(DateFormat.SHORT, DateFormat.SHORT)
    val now: Calendar = Calendar.getInstance()
    val oneWeekFromNow: Calendar = Calendar.getInstance();
    oneWeekFromNow.add(Calendar.WEEK_OF_YEAR, 1);

    val currentFolder = CurrentFolderInCourse.is

    var folderName = if(currentFolder.getLongDescription == null) "" else currentFolder.getLongDescription
    var startDateString: String = if(currentFolder.getStartDate == null) dateFormat.format(now.getTime()) else dateFormat.format(currentFolder.getStartDate)
    var endDateString: String = if(currentFolder.getEndDate == null) dateFormat.format(oneWeekFromNow.getTime()) else dateFormat.format(currentFolder.getEndDate)

    if(!CurrentCourse.canBeSupervisedBy(user)) return NodeSeq.Empty

    def editFolderCallback(): Unit = {
      var errors: List[String] = List()
      val startDate: Date = try {
        dateFormat.parse(startDateString)
      } catch {
        case e: Exception => {
          errors = errors ++ List(e.getMessage())
          null
        }
      }
      val endDate: Date = try {
        dateFormat.parse(endDateString)
      } catch {
        case e: Exception => {
          errors = errors ++ List(e.getMessage())
          null
        }
      }
      if (endDate.compareTo(startDate) < 0) errors = errors ++ List("The end date must not be before the start date")
      if (errors.nonEmpty) {
        S.warning(errors.head)
      } else {
        currentFolder.setLongDescription(folderName)
        currentFolder.setCreator(user)
        currentFolder.setStartDate(startDate)
        currentFolder.setEndDate(endDate)
        currentFolder.save

        S.redirectTo("/main/course/index", () => {})
      }
    }

    val deleteFolderButton = SHtml.link("/main/course/index", () => {
      val currentFolder = CurrentFolderInCourse.is

      //before deleting the folder, we must send all the problems within the folder back to their original state
      ProblemToFolder.deleteProblemsUnderFolder(currentFolder)

      currentFolder.delete_!

    }, Text("Delete Folder"), "onclick" -> JsRaw("return confirm('Are you sure you want to delete this folder?')").toJsCmd, "style" -> "color: red")

    var folderNameField = SHtml.text(folderName, folderName = _)
    val startDateField = SHtml.text(startDateString, startDateString = _)
    val endDateField = SHtml.text(endDateString, endDateString = _)

    val editFolderButton = SHtml.submit("Save changes to folder", editFolderCallback)

    Helpers.bind("editfolderform", xhtml,
      "foldernamefield" -> folderNameField,
      "startdatefield" -> startDateField,
      "enddatefield" -> endDateField,
      "editbutton" -> editFolderButton,
      "deletebutton" -> deleteFolderButton)
  }

  def renderaddfolderform(xhtml: NodeSeq): NodeSeq ={
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    val dateFormat: DateFormat = DateFormat.getDateTimeInstance(DateFormat.SHORT, DateFormat.SHORT)
    val now: Calendar = Calendar.getInstance()
    val oneWeekFromNow: Calendar = Calendar.getInstance();
    oneWeekFromNow.add(Calendar.WEEK_OF_YEAR, 1);

    var folderName = ""
    var startDateString: String = dateFormat.format(now.getTime())
    var endDateString: String = dateFormat.format(oneWeekFromNow.getTime())
    val from = S.referer openOr "/main/course/index"

    if(!CurrentCourse.canBeSupervisedBy(user)) return NodeSeq.Empty

    def createFolderCallback(): Unit = {
      var errors: List[String] = List()
      val startDate: Date = try {
        dateFormat.parse(startDateString)
      } catch {
        case e: Exception => {
          errors = errors ++ List(e.getMessage())
          null
        }
      }
      val endDate: Date = try {
        dateFormat.parse(endDateString)
      } catch {
        case e: Exception => {
          errors = errors ++ List(e.getMessage())
          null
        }
      }
      if (endDate.compareTo(startDate) < 0) errors = errors ++ List("The end date must not be before the start date")
      if (!errors.isEmpty) {
        S.warning(errors.head)
      } else {
        val folder = new Folder
        folder.setCreator(user)
        folder.setLongDescription(folderName)
        folder.setCourse(CurrentCourse.is)
        folder.setStartDate(startDate)
        folder.setEndDate(endDate)
        folder.save

        S.redirectTo("/main/course/index", () => {})
      }
    }

    var folderNameField = SHtml.text(folderName, folderName = _)
    val startDateField = SHtml.text(startDateString, startDateString = _)
    val endDateField = SHtml.text(endDateString, endDateString = _)

    val createFolderButton = SHtml.submit("Create Folder", createFolderCallback)

      Helpers.bind("createfolderform", xhtml,
      "foldernamefield" -> folderNameField,
      "startdatefield" -> startDateField,
      "enddatefield" -> endDateField,
      "createbutton" -> createFolderButton)
  }

  def renderaccessedit(xhtml: NodeSeq): NodeSeq = {
    if (CurrentProblemInCourse.is == null) {
      S.warning("Please first choose a problem to solve")
      return S.redirectTo("/main/course/index")
    }
    val course: Course = CurrentCourse.is
    val problem: Problem = CurrentProblemInCourse.is

    var attempts = "0"
    var maxGrade = "10"
    def editProblem() = {
      var errors: List[String] = List()
      val numMaxGrade = try {
        if (maxGrade.toInt < 1) {
          errors = errors ++ List("Best grade must be positive")
          10
        }
        else maxGrade.toInt
      } catch {
        case e: Exception => {
          errors = errors ++ List(maxGrade + " is not an integer")
          10
        }
      }
      val numAttempts = try {
        if (attempts.toInt < 0) {
          errors = errors ++ List("Nr of attempts must not be negative")
          3
        }
        else attempts.toInt
      } catch {
        case e: Exception => {
          errors = errors ++ List(attempts + " is not an integer")
          3
        }
      }
      if (!errors.isEmpty) {
        S.warning(errors.head)
      } else {
        problem.setMaxGrade(numMaxGrade).setAllowedAttempts(numAttempts).save
        S.redirectTo("/main/course/index", () => {})
      }
    }

    val maxGradeField = SHtml.text(maxGrade, maxGrade = _)
    val attemptsField = SHtml.text(attempts, attempts = _)

    val editButton = SHtml.submit("Edit Problem", editProblem)

    Helpers.bind("renderaccesseditform", xhtml,
      "maxgradefield" -> maxGradeField,
      "attemptsfield" -> attemptsField,
      "editbutton" -> editButton)
  }


  def showfolders(ignored: NodeSeq): NodeSeq = {

    def expandButton(folder: Folder): NodeSeq = {
      SHtml.button("expand/collapse", null, "class" -> "btn_collapse", "id" -> ("btn_collapse" + folder.getFolderID))
    }

    def previewButton(problem: Problem): NodeSeq = {
      SHtml.link(
        "/main/course/problems/preview",
        () => {
          CurrentProblemInCourse(problem)
        },
        <button type='button'>Preview</button>)
    }

    def editButton(problem: Problem): NodeSeq = {
      SHtml.link(
        "/main/course/problems/edit",
        () => {
          CurrentProblemInCourse(problem)
        },
        <button type='button'>Edit problem</button>)
    }

    def editFolderButton(folder: Folder): NodeSeq = {
      SHtml.link(
        "/main/course/folders/edit",
        () => {
          CurrentFolderInCourse(folder)
        },
        <button type='button'>Edit Folder</button>)
    }

    def editAccessButton(problem: Problem): NodeSeq = {
      SHtml.link(
        "/main/course/problems/editproblemaccess",
        () => {
          CurrentProblemInCourse(problem)
        },
        <button type='button'>Edit Access</button>)
    }

    def poseUnposeLink(folder: Folder): NodeSeq = {
      if (folder.getPosed) return SHtml.link(
        "/main/course/index",
        () => {
          folder.setPosed(false).save
        },
        Text("Yes"), "title" -> "Click to unpose!")
      else return SHtml.link(
        "/main/course/problems/pose",
        () => {
          folder.setPosed(true).save
        },
        Text("No"), "title" -> "Pose this folder!")
    }

    def solveButton(problem: Problem): NodeSeq = {
      SHtml.link(
        "/main/course/problems/solve",
        () => {
          CurrentProblemInCourse(problem)
        },
        <button type='button'>Solve</button>)
    }

    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
    var folders = CurrentCourse.getFoldersForUser(user)
    if (folders.isEmpty) return Text("There are no folders in this course")
    var problemsUnderFolder = folders.map(folder => folder.getProblemsUnderFolder)

    if (CurrentCourse.canBeSupervisedBy(user)) {
      return (
        <table>
          <tr>
            <td>
              <b>Description</b>
            </td>
            <td>
              <b>Posed</b>
            </td>
            <td>
              <b>Start Date</b>
            </td>
            <td>
              <b>End Date</b>
            </td>
            <td></td>
          </tr>{folders.map(folder => {
          <div>
            <tr>
              <td>
                {folder.getLongDescription}
              </td>
              <td>
                {poseUnposeLink(folder)}
              </td>
              <td>
                {folder.getStartDate}
              </td>
              <td>
                {folder.getEndDate}
              </td>
              <td>
                {expandButton(folder)}
              </td>
              <td>
                {editFolderButton(folder)}
              </td>
            </tr>
            <tr class={"collapsable_tr collapsable_" + folder.getFolderID} style="display: none">
              <td></td> <td>
              <b>Description</b>
            </td> <td>
              <b>Problem Type</b>
            </td>
              <td>
                <b>Attempts</b>
              </td> <td>
              <b>Max Grade</b>
            </td> <td></td>
            </tr>{folder.getProblemsUnderFolder.map(problem => {
            <tr class={"collapsable_tr collapsable_" + folder.getFolderID} style="display: none">
              <td></td>
              <td>
                {problem.getShortDescription}
              </td>
              <td>
                {problem.getTypeName}
              </td>
              <td>
                {problem.getAllowedAttemptsString}
              </td>
              <td>
                {problem.getMaxGrade.toString}
              </td>
              <td>
                {editButton(problem)}
              </td>
              <td>
                {editAccessButton(problem)}
              </td>
              <td>
                {previewButton(problem)}
              </td>
            </tr>
          })}
          </div>
        })}
        </table> ++ SHtml.link("/main/course/folders/create", () => {},
          <button type="button">Create a folder</button>)
        )
    }
    else {
      return <table>
        <tr>
          <td>
            <b>Description</b>
          </td> <td></td>
        </tr>{folders.map(folder => {
          if(folder.getEndDate.compareTo(Calendar.getInstance().getTime) < 0) <div></div> else
        <tr>
          <td>
            {folder.getLongDescription}
          </td>
          <td>
            {expandButton(folder)}
          </td>
        </tr>
          <tr class={"collapsable_tr collapsable_" + folder.getFolderID} style="display: none">
            <td></td> <td>
            <b>Description</b>
          </td> <td>
            <b>Problem Type</b>
          </td>
            <td>
              <b>Attempts</b>
            </td> <td>
            <b>Max Grade</b>
          </td> <td></td>
          </tr> ++ {folder.getProblemsUnderFolder.map(problem => {
        <tr class={"collapsable_tr collapsable_" + folder.getFolderID} style="display: none">
          <td></td>
          <td>
            {problem.getShortDescription}
          </td>
          <td>
            {problem.getTypeName}
          </td>
          <td>
            {problem.getAllowedAttemptsString}
          </td>
          <td>
            {problem.getMaxGrade.toString}
          </td>
          <td>
            {solveButton(problem)}
          </td>
        </tr>
      })}
      })}
      </table>
    }
  }


  def showproblems(ignored: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
    var problems = CurrentCourse.getProblemsForUser(user)

    if (problems.isEmpty) return Text("There are no problems in this course")

    if (CurrentCourse.canBeSupervisedBy(user)) {
      def poseUnposeLink(problem: Problem): NodeSeq = {
        if (problem.getPosed) return SHtml.link(
          "/main/course/index",
          () => {
            problem.setPosed(false).setStartDate(null).setEndDate(null).save
          },
          Text("Yes"), "title" -> "Click to unpose!")
        else return SHtml.link(
          "/main/course/problems/pose",
          () => {
            CurrentProblemInCourse(problem)
          },
          Text("No"), "title" -> "Pose this problem!")
      }

      def previewButton(problem: Problem): NodeSeq = {
        SHtml.link(
          "/main/course/problems/preview",
          () => {
            CurrentProblemInCourse(problem)
          },
          <button type='button'>Preview</button>)
      }

      return TableHelper.renderTableWithHeader(
        problems,
        ("Description", (problem: Problem) => Text(problem.getShortDescription)),
        ("Problem Type", (problem: Problem) => Text(problem.getTypeName)),
        ("Posed?", poseUnposeLink(_)),
        ("Start", (problem: Problem) => if (problem.getPosed) Text(problem.getStartDate.toString) else Text("-")),
        ("End", (problem: Problem) => if (problem.getPosed) Text(problem.getEndDate.toString) else Text("-")),
        ("Attempts", (problem: Problem) => if (problem.getPosed) Text(problem.getAllowedAttemptsString) else Text("-")),
        ("Max Grade", (problem: Problem) => if (problem.getPosed) Text(problem.getMaxGrade.toString) else Text("-")),
        ("", previewButton(_)))
    } else {
      def solveButton(problem: Problem): NodeSeq = {
        SHtml.link(
          "/main/course/problems/solve",
          () => {
            CurrentProblemInCourse(problem)
          },
          <button type='button'>Solve</button>)
      }

      return TableHelper.renderTableWithHeader(
        problems,
        ("Description", (problem: Problem) => Text(problem.getShortDescription)),
        ("Problem Type", (problem: Problem) => Text(problem.getTypeName)),
        ("Grade / Max. Grade", (problem: Problem) => Text(problem.getGrade(user).toString + " / " + problem.getMaxGrade.toString)),
        ("Remaining Attempts", (problem: Problem) => Text(problem.getNumberAttemptsRemainingString(user))),
        ("Ends in", (problem: Problem) => Text(problem.getTimeToExpirationString)),
        ("", solveButton(_)))
    }
  }


  def supervisorsection(ignored: NodeSeq): NodeSeq = {
    val course = CurrentCourse.is

    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
    if (!CurrentCourse.canBeSupervisedBy(user)) return NodeSeq.Empty

    val gradesCsvLink = SHtml.link("/main/course/downloadCSV", () => {}, Text("Grades (as .csv)"))
    val gradesXmlLink = SHtml.link("/main/course/downloadXML", () => {}, Text("Grades (as .xml)"))
    val userLink = SHtml.link("/main/course/users", () => {}, Text("Users"))

    return <h2>Manage Course</h2> ++ gradesCsvLink ++ Unparsed("&emsp;") ++ gradesXmlLink ++ Unparsed("&emsp;") ++ userLink
  }

  def renderxmldownloadlink(ignored: NodeSeq): NodeSeq = {
    val course = CurrentCourse.is

    val downloadXmlLink = DownloadHelper.renderXmlDownloadLink(course.renderGradesXml, "grades", Text("Grades.xml"))

    return downloadXmlLink
  }

  def rendercsvdownloadlink(ignored: NodeSeq): NodeSeq = {
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
        ("Attempts", (user: User) => Text(course.getPosedProblems.map(_.getNumberAttempts(user)).sum.toString)),
        ("Points", (user: User) => Text(course.getPosedProblems.map(_.getGrade(user)).sum.toString)),
        ("", (user: User) => dismissLink(user)))
    } else {
      <h2>Participants</h2> ++ Text("There are no paticipants yet.")
    }

    return supervisorList ++ participantList
  }


  def rendercreate(ignored: NodeSeq): NodeSeq = {
    if (CurrentProblemTypeInCourse.is == null) {
      S.warning("You have not selected a problem type")
      return S.redirectTo("/main/course/index")
    }

    val problemType = CurrentProblemTypeInCourse.is

    def createUnspecificProb(shortDesc: String, longDesc: String): Problem = {
      val createdBy: User = User.currentUser openOrThrowException "Lift protects this page against non-logged-in users"

      val unspecificProblem: Problem = Problem.create.setCreator(createdBy)
      unspecificProblem.setShortDescription(shortDesc).setLongDescription(longDesc).setProblemType(problemType)
      unspecificProblem.setCourse(CurrentCourse)
      unspecificProblem.save

      return unspecificProblem
    }

    def returnFunc(problem: Problem) = {
      CurrentProblemInCourse(problem)
      S.redirectTo("/main/course/problems/preview")
    }

    return problemType.getProblemSnippet().renderCreate(createUnspecificProb, returnFunc)
  }


  def renderedit(ignored: NodeSeq): NodeSeq = {
    if (CurrentProblemInCourse.is == null) {
      S.warning("Please first choose a problem to edit")
      return S.redirectTo("/main/course/index")
    }

    val problem: Problem = CurrentProblemInCourse.is
    val problemSnippet: SpecificProblemSnippet = problem.getProblemType.getProblemSnippet

    def returnFunc(ignored: Problem) = {
      CurrentProblemInCourse(problem)
      S.redirectTo("/main/course/problems/preview")
    }

    return problemSnippet.renderEdit match {
      case Full(renderFunc) => renderFunc(problem, returnFunc)
      case Empty =>
        S.error("Editing not implemented for this problem type"); S.redirectTo("/main/course/index")
      case _ => S.error("Error when retrieving editing function"); S.redirectTo("/main/course/index")
    }
  }


  def rendersolve(ignored: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users on here"
    if (CurrentProblemInCourse.is == null) {
      S.warning("Please first choose a problem to solve")
      return S.redirectTo("/main/course/index")
    }

    val problem: Problem = CurrentProblemInCourse.is
    val problemSnippet: SpecificProblemSnippet = problem.getProblemType.getProblemSnippet

    val lastAttempt = SolutionAttempt.getLatestAttempt(user, problem)

    var lastGrade = 0

    def recordSolutionAttempt(grade: Int, dateTime: Date): SolutionAttempt = {
      lastGrade = grade
      // check that attempt is in allowed time frame
      if (dateTime.compareTo(problem.getEndDate) > 0 || dateTime.compareTo(problem.getStartDate) < 0) return null
      // otherwise: create SolulationAttempt
      val solutionAttempt = SolutionAttempt.create.problemId(problem).userId(user)
      solutionAttempt.dateTime(dateTime).grade(grade).save
      return solutionAttempt
    }

    def returnFunc(problem: Problem) = {
      S.redirectTo("/main/course/index")
    }

    def remainingAttempts(): Int = {
      problem.getNumberAttemptsRemaining(user).toInt
    }

    def bestGrade(): Int = {
      problem.getGrade(user)
    }

    problemSnippet.renderSolve(problem, CurrentProblemInCourse.getMaxGrade, lastAttempt, recordSolutionAttempt, returnFunc, remainingAttempts, bestGrade)
  }


  def renderpreview(ignored: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users on here"
    if (CurrentProblemInCourse.is == null) {
      S.warning("Please first choose a problem to preview")
      return S.redirectTo("/main/course/index")
    }

    val problem: Problem = CurrentProblemInCourse.is
    val problemSnippet: SpecificProblemSnippet = problem.getProblemType.getProblemSnippet

    def returnFunc(problem: Problem) = {
      S.redirectTo("/main/course/index")
    }

    //var editLink = SHtml.link("/main/course/problems/edit", () => {CurrentProblemInCourse(problem)}, Text("Edit"))
    val editLink = SHtml.link("/main/course/problems/edit", () => {
      CurrentProblemInCourse(problem)
    }, <button type='button'>Edit</button>)

    return problemSnippet.renderSolve(problem, 10, Empty,
      (grade, date) => SolutionAttempt, returnFunc, () => 1, () => 0) ++ editLink
  }


  def renderpose(xhtml: NodeSeq): NodeSeq = {
    if (CurrentProblemInCourse.is == null) {
      S.warning("Please first choose a problem to solve")
      return S.redirectTo("/main/course/index")
    }
    val course: Course = CurrentCourse.is
    val problem: Problem = CurrentProblemInCourse.is

    val dateFormat: DateFormat = DateFormat.getDateTimeInstance(DateFormat.SHORT, DateFormat.SHORT)
    val now: Calendar = Calendar.getInstance()
    val oneWeekFromNow: Calendar = Calendar.getInstance();
    oneWeekFromNow.add(Calendar.WEEK_OF_YEAR, 1);

    var startDateString: String = dateFormat.format(now.getTime())
    var endDateString: String = dateFormat.format(oneWeekFromNow.getTime())
    var attempts = "0"
    var maxGrade = "10"

    def poseProblem() = {
      var errors: List[String] = List()
      val startDate: Date = try {
        dateFormat.parse(startDateString)
      } catch {
        case e: Exception => {
          errors = errors ++ List(e.getMessage())
          null
        }
      }
      val endDate: Date = try {
        dateFormat.parse(endDateString)
      } catch {
        case e: Exception => {
          errors = errors ++ List(e.getMessage())
          null
        }
      }
      if (endDate.compareTo(startDate) < 0) errors = errors ++ List("The end date must not be before the start date")
      val numMaxGrade = try {
        if (maxGrade.toInt < 1) {
          errors = errors ++ List("Best grade must be positive")
          10
        }
        else maxGrade.toInt
      } catch {
        case e: Exception => {
          errors = errors ++ List(maxGrade + " is not an integer")
          10
        }
      }
      val numAttempts = try {
        if (attempts.toInt < 0) {
          errors = errors ++ List("Nr of attempts must not be negative")
          3
        }
        else attempts.toInt
      } catch {
        case e: Exception => {
          errors = errors ++ List(attempts + " is not an integer")
          3
        }
      }
      if (!errors.isEmpty) {
        S.warning(errors.head)
      } else {
        problem.setPosed(true).setStartDate(startDate).setEndDate(endDate).setMaxGrade(numMaxGrade).setAllowedAttempts(numAttempts).save
        S.redirectTo("/main/course/index", () => {})
      }
    }

    val startDateField = SHtml.text(startDateString, startDateString = _)
    val endDateField = SHtml.text(endDateString, endDateString = _)
    val maxGradeField = SHtml.text(maxGrade, maxGrade = _)
    val attemptsField = SHtml.text(attempts, attempts = _)

    val poseButton = SHtml.submit("Pose Problem", poseProblem)

    Helpers.bind("poseproblemform", xhtml,
      "startdatefield" -> startDateField,
      "enddatefield" -> endDateField,
      "maxgradefield" -> maxGradeField,
      "attemptsfield" -> attemptsField,
      "posebutton" -> poseButton)
  }

  def rendernavigation(ignored: NodeSeq): NodeSeq = {
    val overviewButton = SHtml.link("/main/course/index", () => {}, <button type='button' class='navButton'>Course Overview</button>)
    return overviewButton
  }
}

object Courses {
}
