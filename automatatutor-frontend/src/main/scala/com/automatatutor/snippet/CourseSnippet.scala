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
import net.liftweb.http.provider.HTTPCookie

import scala.collection.JavaConverters._

object CurrentCourse extends SessionVar[Course](null) // SessionVar makes navigation easier
object CurrentProblemInCourse extends SessionVar[Problem](null) // SessionVar makes navigation easier
object CurrentProblemTypeInCourse extends RequestVar[ProblemType](null) // RequestVar as only needed in a single request
object CurrentProblemPointerInCourse extends RequestVar[ProblemPointer](null)

object CurrentFolderInCourse extends SessionVar[Folder](null)

class Coursesnippet {
  def rendereditfolderform(xhtml: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    val dateFormat: DateFormat = DateFormat.getDateTimeInstance(DateFormat.SHORT, DateFormat.SHORT)
    val now: Calendar = Calendar.getInstance()
    val oneWeekFromNow: Calendar = Calendar.getInstance();
    oneWeekFromNow.add(Calendar.WEEK_OF_YEAR, 1);

    val currentFolder = CurrentFolderInCourse.is

    var folderName = if (currentFolder.getLongDescription == null) "" else currentFolder.getLongDescription
    var startDateString: String = if (currentFolder.getStartDate == null) dateFormat.format(now.getTime()) else dateFormat.format(currentFolder.getStartDate)
    var endDateString: String = if (currentFolder.getEndDate == null) dateFormat.format(oneWeekFromNow.getTime()) else dateFormat.format(currentFolder.getEndDate)

    if (!CurrentCourse.canBeSupervisedBy(user)) return NodeSeq.Empty

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

      currentFolder.delete_!
    }, Text("Delete Folder"), "onclick" -> JsRaw("return confirm('Are you sure you want to delete this folder?')").toJsCmd, "style" -> "color: red")

    val folderNameField = SHtml.text(folderName, folderName = _)
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

  def renderaddfolderform(xhtml: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    val dateFormat: DateFormat = DateFormat.getDateTimeInstance(DateFormat.SHORT, DateFormat.SHORT)
    val now: Calendar = Calendar.getInstance()
    val oneWeekFromNow: Calendar = Calendar.getInstance();
    oneWeekFromNow.add(Calendar.WEEK_OF_YEAR, 1);

    var folderName = ""
    var startDateString: String = dateFormat.format(now.getTime())
    var endDateString: String = dateFormat.format(oneWeekFromNow.getTime())

    if (!CurrentCourse.canBeSupervisedBy(user)) return NodeSeq.Empty

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

    val folderNameField = SHtml.text(folderName, folderName = _)
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
    if (CurrentProblemPointerInCourse.is == null) {
      S.warning("Please first choose a problem to edit")
      return S.redirectTo("/main/course/index")
    }
    val course: Course = CurrentCourse.is
    val problem: ProblemPointer = CurrentProblemPointerInCourse.is

    var attempts = problem.getAllowedAttemptsString
    var maxGrade = problem.getMaxGrade.toString

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

    def editAccessButton(problem: ProblemPointer): NodeSeq = {
      SHtml.link(
        "/main/course/problems/editproblemaccess",
        () => {
          CurrentProblemPointerInCourse(problem)
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
        "/main/course/index",
        () => {
          folder.setPosed(true).save
        },
        Text("No"), "title" -> "Pose this folder!")
    }

    def solveButton(problem: ProblemPointer): NodeSeq = {
      SHtml.link(
        "/main/course/problems/solve",
        () => {
          CurrentProblemPointerInCourse(problem)
        },
        <button type='button'>Solve</button>)
    }

    def getCollapsibleElemAttributes(folder: Folder) = List(("class", "collapsible_tr collapsible_" + folder.getFolderID), ("style", "display: none"))

    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    val folders = CurrentCourse.getFoldersForUser(user)
    if (folders.isEmpty) return Text("There are no folders in this course") ++
      SHtml.link("/main/course/folders/create", () => {}, <button type="button">Create a folder</button>)

    if (CurrentCourse.canBeSupervisedBy(user)) {
      (<div>
        {folders.map(folder => {
          TableHelper.renderTableWithHeader(
            List(folder),
            ("Folder Name", (folder: Folder) => Text(folder.getLongDescription)),
            ("Posed", (folder: Folder) => poseUnposeLink(folder)),
            ("Start Date", (folder: Folder) => Text(folder.getStartDate.toString)),
            ("End Date", (folder: Folder) => Text(folder.getEndDate.toString)),
            ("Expand", (folder: Folder) => expandButton(folder)),
            ("Edit", (folder: Folder) => editFolderButton(folder))
          )
            .theSeq.++(
            TableHelper.renderTableWithHeaderPlusAttributes(
              folder.getProblemPointersUnderFolder, getCollapsibleElemAttributes(folder),
              ("Problem Description", (problem: ProblemPointer) => Text(problem.getShortDescription)),
              ("Type", (problem: ProblemPointer) => Text(problem.getTypeName)),
              ("Attempts", (problem: ProblemPointer) => Text(problem.getAllowedAttemptsString)),
              ("Max Grade", (problem: ProblemPointer) => Text(problem.getMaxGrade.toString)),
              ("Edit Access", (problem: ProblemPointer) => editAccessButton(problem)),
              ("", (problem: ProblemPointer) => solveButton(problem))
            )
          )
        })}
      </div>
        ++
        SHtml.link("/main/course/folders/create", () => {}, <button type="button">Create a folder</button>)
        )
    }
    else {
      //logged in user is a student
      <div>
        {folders.map(folder => {

        if (!folder.isOpen) {
          NodeSeq.Empty
        }
        else {
          TableHelper.renderTableWithHeader(
            List(folder),
            ("Folder Name", (folder: Folder) => Text(folder.getLongDescription)),
            ("Start Date", (folder: Folder) => Text(folder.getStartDate.toString)),
            ("End Date", (folder: Folder) => Text(folder.getEndDate.toString)),
            ("Expand", (folder: Folder) => expandButton(folder))
          )
            .theSeq.++(
            TableHelper.renderTableWithHeaderPlusAttributes(
              folder.getProblemPointersUnderFolder, getCollapsibleElemAttributes(folder),
              ("Problem Description", (problem: ProblemPointer) => Text(problem.getShortDescription)),
              ("Type", (problem: ProblemPointer) => Text(problem.getTypeName)),
              ("Attempts Remaining", (problem: ProblemPointer) => Text(problem.getNumberAttemptsRemaining(user).toString)),
              ("Your Highest Grade", (problem: ProblemPointer) => Text(problem.getGrade(user).toString)),
              ("", (problem: ProblemPointer) => solveButton(problem))
            )
          )
        }
      })}
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


    return <h2>Manage Course</h2> ++ gradesCsvLink ++
      Unparsed("&emsp;") ++ gradesXmlLink ++
      Unparsed("&emsp;") ++ userLink ++
      Unparsed("&emsp;") ++ exportLink ++
      Unparsed("&emsp;") ++ importLink
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
        //NOTE: These following two lines assume that you only want to count the grades/attempts of questions
        //which are CURRENTLY posed
        //If a student solves a question, but then its containing folder is unposed, that grade will not be accounted for
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
    if (CurrentProblemPointerInCourse.is == null) {
      S.warning("Please first choose a problem to solve")
      return S.redirectTo("/main/course/index")
    }

    val problemPointer = CurrentProblemPointerInCourse.is
    val problem: Problem = problemPointer.getProblem
    val problemSnippet: SpecificProblemSnippet = problem.getProblemType.getProblemSnippet

    val lastAttempt = SolutionAttempt.getLatestAttempt(user, problemPointer)

    var lastGrade = 0

    def recordSolutionAttempt(grade: Int, dateTime: Date): SolutionAttempt = {
      lastGrade = grade

      if (dateTime.compareTo(problemPointer.getFolder.getEndDate) > 0 ||
        dateTime.compareTo(problemPointer.getFolder.getStartDate) < 0) return null
      // otherwise: create SolutionAttempt
      val solutionAttempt = new SolutionAttempt
      solutionAttempt.problempointerId(problemPointer).userId(user).dateTime(dateTime).grade(grade).save

      solutionAttempt
    }

    def returnFunc(problem: Problem) = {
      S.redirectTo("/main/course/index")
    }

    def remainingAttempts(): Int = {
      problemPointer.getNumberAttemptsRemaining(user).toInt
    }

    def bestGrade(): Int = {
      problemPointer.getGrade(user)
    }

    //If the user is the admin, don't even bother recording an attempt
    if (user.isAdmin)
      return problemSnippet.renderSolve(problem, problemPointer.getMaxGrade, Empty, (grade, date) => SolutionAttempt, returnFunc, () => 1, () => 0)

    problemSnippet.renderSolve(problem, problemPointer.getMaxGrade, lastAttempt, recordSolutionAttempt, returnFunc, remainingAttempts, bestGrade)
  }

  def renderexportforcourse(ignored: NodeSeq): NodeSeq = {
    val course = CurrentCourse.is

    //create export xml
    var xml = NodeSeq.Empty
    course.getProblems.foreach(
      (problemPointer) => {
        xml = xml ++ problemPointer.getProblem.toXML
      })
    xml = <exported>
      {xml}
    </exported>

    val returnLink = SHtml.link("/main/course/index", () => {}, <button type='button'>Return to course</button>)
    val downloadLink = DownloadHelper.renderXmlDownloadLink(xml, "Download all problems as an XML file.", <button type='button'>Download</button>)
    val exportOutputField = SHtml.textarea(xml.toString, (input) => {}, "cols" -> "80", "rows" -> "40")

    return downloadLink ++ new Unparsed("<br><br>") ++ returnLink ++ new Unparsed("<br><br>") ++ exportOutputField
  }

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
}

object Courses {
}
