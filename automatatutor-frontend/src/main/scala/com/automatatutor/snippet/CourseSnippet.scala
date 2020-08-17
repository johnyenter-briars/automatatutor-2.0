package com.automatatutor.snippet

import java.text.{DateFormat, SimpleDateFormat}
import java.util.Calendar
import java.util.Date
import scala.xml._
import com.automatatutor.lib._
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
import scala.collection.mutable.ListBuffer

object CurrentCourse extends SessionVar[Course](null) // SessionVar makes navigation easier
object CurrentProblemInCourse extends SessionVar[Problem](null) // SessionVar makes navigation easier
object CurrentProblemTypeInCourse extends RequestVar[ProblemType](null) // RequestVar as only needed in a single request
object CurrentExerciseInCourse extends RequestVar[Exercise](null)
object CurrentBatchExercisesInCourse extends SessionVar[ListBuffer[Exercise]](null)
object CurrentFolderInCourse extends SessionVar[Folder](null)

class Coursesnippet {

  if(CurrentBatchExercisesInCourse.is == null) CurrentBatchExercisesInCourse(new ListBuffer[Exercise])

  def rendereditfolderform(xhtml: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    val currentFolder = CurrentFolderInCourse.is

    val dateFormat = currentFolder.getDateFormat
    var folderName = if (currentFolder.getLongDescription == null) "" else currentFolder.getLongDescription
    var startDateString: String = currentFolder.getStartDateString
    var endDateString: String = currentFolder.getEndDateString

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
      if (endDate != null && startDate != null && endDate.compareTo(startDate) < 0) errors = errors ++ List("The end date must not be before the start date")
      if (errors.nonEmpty) {
        S.warning(errors.head)
      } else {
        currentFolder.setLongDescription(folderName)
        currentFolder.setCreator(user)
        currentFolder.setStartDate(startDate)
        currentFolder.setEndDate(endDate)
        currentFolder.save

        S.redirectTo("/main/course/folders/index", () => {})
      }
    }

    val deleteFolderButton = SHtml.link("/main/course/index", () => {
      val currentFolder = CurrentFolderInCourse.is

      currentFolder.delete_!
    }, Text("Delete Folder"), "onclick" -> JsRaw(
      "return confirm('Are you sure you want to delete this folder? If you do, all student grades will be lost!')")
      .toJsCmd, "style" -> "color: red")

    val deleteProblems = SHtml.link("/main/course/folders/index", () => {
      val currentFolder = CurrentFolderInCourse.is

      currentFolder.getExercisesUnderFolder.foreach(_.delete_!)
    }, Text("Delete Problems in this Folder"), "onclick" -> JsRaw(
      "return confirm('Are you sure you want to delete all problems in this folder? If you do, all student grades will be lost!')")
      .toJsCmd, "style" -> "color: red")

    val folderNameField = SHtml.text(folderName, folderName = _)
    val startDateField = SHtml.text(startDateString, startDateString = _)
    val endDateField = SHtml.text(endDateString, endDateString = _)
    val visibleLabel = <strong>{currentFolder.getVisible.toString}</strong>

    val postFolderButton = SHtml.button(
      if(currentFolder.getVisible) "Remove Visibility" else "Make Visible",
      () => {currentFolder.setVisible(!currentFolder.getVisible).save})

    val editFolderButton = SHtml.submit("Save changes to folder", editFolderCallback)

    Helpers.bind("editfolderform", xhtml,
      "foldernamefield" -> folderNameField,
      "startdatefield" -> startDateField,
      "enddatefield" -> endDateField,
      "visiblelabel" -> visibleLabel,
      "posebutton" -> postFolderButton,
      "editbutton" -> editFolderButton,
      "deletebutton" -> deleteFolderButton,
      "deleteproblemsbutton" -> deleteProblems)
  }

  def renderaddfolderform(xhtml: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    val dateFormat = new SimpleDateFormat("EEE, MMM d, K:mm ''yy")
    val now: Calendar = Calendar.getInstance()
    val oneWeekFromNow: Calendar = Calendar.getInstance()
    oneWeekFromNow.add(Calendar.WEEK_OF_YEAR, 1)

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

  def renderaddproblems(xhtml: NodeSeq): NodeSeq = {
    new FolderRenderer(CurrentFolderInCourse.is).renderAddProblemsButton
  }

  def renderexerciseedit(xhtml: NodeSeq, problem: Exercise): NodeSeq = {
    if (CurrentBatchExercisesInCourse.is == null) {
      S.warning("Please first choose problems to batch edit")
      return S.redirectTo("/main/course/folders/index")
    }

    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    //set to default cause we're working with multiple problems
    var attempts = "10"
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
      if (errors.nonEmpty) {
        S.warning(errors.head)
      } else {
        problem.setMaxGrade(numMaxGrade).setAllowedAttempts(numAttempts).save
      }
    }

    val maxGradeField = SHtml.text(maxGrade, maxGrade = _)
    val attemptsField = SHtml.text(attempts, attempts = _)
    val editButton = SHtml.submit("Edit Problems", editProblem)

    val onClick: JsCmd = JsRaw(
      "return confirm('Are you sure you want to delete these problems from the folder? " +
        "If you do, all student grades on these problems will be lost!')")

    val deleteButton = SHtml.button("Delete", ()=>{
      problem.delete_!
    }, "onclick" -> onClick.toJsCmd,
      "style" -> "color: red")

    Helpers.bind("renderbatcheditform", xhtml,
      "maxgradefield" -> maxGradeField,
      "attemptsfield" -> attemptsField,
      "editbutton" -> editButton,
      "deletebutton" -> deleteButton
    )
  }

  def renderexercises(xhtml: NodeSeq): NodeSeq = {
    if (CurrentFolderInCourse.is == null) {
      S.warning("Please first choose a folder")
      return S.redirectTo("/main/course/index")
    }

    CurrentBatchExercisesInCourse.is.clear()

    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
    val folder = CurrentFolderInCourse.is
    val exercises = folder.getExercisesUnderFolder

    def checkBoxForProblem(p: Exercise): NodeSeq = {
      SHtml.checkbox(false, (chosen: Boolean) => {
        if(chosen) CurrentBatchExercisesInCourse.is += p
      }, "class"->"checkbox")
    }

    def renderAccessModal(problem: Exercise): NodeSeq = {

      <div>
        <button type="button" id={"edit_access-modal-button_" + problem.getExerciseID} class="modal-button">Edit Access</button>

        <div id={"edit_access-modal_" + problem.getExerciseID} class="modal">

          <div class="modal-content">
            <div class="modal-header">
              <span class="close" id={"edit_access-span_" + problem.getExerciseID}>&times;</span>
              <h3>Edit an Exercise</h3>
            </div>
            <div class="modal-body">
              {
                this.renderexerciseedit(xhtml, problem)
              }
            </div>
          </div>
        </div>
      </div>
    }

    if(CurrentCourse.canBeSupervisedBy(user)){
      <form>
        {
          TableHelper.renderTableWithHeader(
            exercises,
            ("", (problem: Exercise) => checkBoxForProblem(problem)),
            ("Problem Name", (problem: Exercise) => Text(problem.getShortDescription)),
            ("Type", (problem: Exercise) => Text(problem.getTypeName)),
            ("Max Attempts", (problem: Exercise) => Text(problem.getAllowedAttemptsString)),
            ("Max Grade", (problem: Exercise) => Text(problem.getMaxGrade.toString)),
            ("Avg Grade/Avg Attempts", (problem: Exercise) => new ExerciseRenderer(problem).renderExerciseStats),
            ("Edit Access", (problem: Exercise) => renderAccessModal(problem)),
            ("Edit/View Referenced Problems", (problem: Exercise) =>
              new ExerciseRenderer(problem).renderReferencedProblemButton("/main/course/folders/index")),
            ("", (problem: Exercise) => new ExerciseRenderer(problem).renderSolveButton),
            ("", (problem: Exercise) => new ExerciseRenderer(problem).renderDeleteLink)
          ).theSeq ++ SHtml.button(
            "Batch Edit",
            () => {
              PreviousPage("/main/course/folders/index")
              S.redirectTo("/main/course/problems/batchedit")}
          )
        }
      </form>
    }
    else{
      //logged in user is a student
      TableHelper.renderTableWithHeader(
        exercises,
        ("Problem Description", (problem: Exercise) => Text(problem.getShortDescription)),
        ("Type", (problem: Exercise) => Text(problem.getTypeName)),
        ("Your Attempts", (problem: Exercise) => Text(problem.getAttempts(user).length.toString)),
        ("Max Attempts", (problem: Exercise) => Text(problem.getAllowedAttemptsString)),
        ("Your Highest Grade", (problem: Exercise) => Text(problem.getHighestAttempt(user).toString)),
        ("Max Grade", (problem: Exercise) => Text(problem.getMaxGrade.toString)),
        ("", (problem: Exercise) => new ExerciseRenderer(problem).renderSolveButton)
      )
    }
  }

  def renderbatchedit(xhtml: NodeSeq): NodeSeq = {
    if (CurrentBatchExercisesInCourse.is == null) {
      S.warning("Please first choose problems to batch edit")
      return S.redirectTo("/main/course/folders/index")
    }

    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    //set to default cause we're working with multiple problems
    var attempts = "10"
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
        CurrentBatchExercisesInCourse.is.foreach((exercise: Exercise) => {
          exercise.setMaxGrade(numMaxGrade).setAllowedAttempts(numAttempts).save
        })
      }
    }

    val maxGradeField = SHtml.text(maxGrade, maxGrade = _)
    val attemptsField = SHtml.text(attempts, attempts = _)
    val editButton = SHtml.submit("Edit Problems", editProblem)

    val onClick: JsCmd = JsRaw(
      "return confirm('Are you sure you want to delete these problems from the folder? " +
        "If you do, all student grades on these problems will be lost!')")

    val deleteButton = SHtml.button("Delete", ()=>{
      CurrentBatchExercisesInCourse.is.foreach((exercise: Exercise) => {
        exercise.delete_!
      })
    }, "onclick" -> onClick.toJsCmd,
      "style" -> "color: red")

    Helpers.bind("renderbatcheditform", xhtml,
      "maxgradefield" -> maxGradeField,
      "attemptsfield" -> attemptsField,
      "editbutton" -> editButton,
      "deletebutton" -> deleteButton
      )
  }

  def rendercancelbatchedit(xhtml: NodeSeq): NodeSeq = {
    val redirectTarget = if(PreviousPage.is == null) "/main/course/index" else PreviousPage.is
    SHtml.link(redirectTarget, ()=>{PreviousPage(null)}, <button type="button">Cancel</button>)
  }

  def renderbatchmove(xhtml: NodeSeq): NodeSeq = {
    if (CurrentBatchExercisesInCourse.is == null) {
      S.warning("Please first choose problems to batch edit")
      return S.redirectTo("/main/course/folders/index")
    }

    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
    val supervisedCourses = user.getSupervisedCourses
    val currentProblems = CurrentBatchExercisesInCourse.is.toList

    def moveProblems() = {
      CurrentBatchExercisesInCourse.is.clear()
      S.redirectTo("/main/course/folders/index", () => {})
    }

    def selectionCallback(folderIDS: List[String], course: Course): Unit = {
      folderIDS.foreach((folderID: String) =>{
        val folder = Folder.findByID(folderID)

        currentProblems.foreach((exercise: Exercise) => {
          exercise.setFolder(folder).setCourse(course).save
        })
      })
    }

    val moveButton = SHtml.submit("Move Problems", moveProblems)

    val courseTable = TableHelper.renderTableWithHeader(
      supervisedCourses,
      ("Course Name", (course: Course) => Text(course.getName)),
      ("Folder Select", (course: Course) => {

        val folderOptions = Folder.findAllByCourse(course)
          .map(f => (f.getFolderID.toString -> f.getLongDescription.toString))

        SHtml.multiSelect(folderOptions, List(), selectionCallback(_, course))
      }))

    Helpers.bind("renderbatchmoveform", xhtml,
      "movebutton" -> moveButton,
      "courseselecttable" -> courseTable)
  }

  def renderbatchlist(xhtml: NodeSeq): NodeSeq = {
    <h3>Currently Editing problems:</h3> ++
      <form>
        {
          TableHelper.renderTableWithHeader(
            CurrentBatchExercisesInCourse.is.toList,
            ("", (problem: Exercise) => Text(problem.getShortDescription)))
        }
      </form>
  }

  def renderfolders(ignored: NodeSeq): NodeSeq = {
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"

    val folders = CurrentCourse.getFoldersForUser(user)
    if (folders.isEmpty) return Text("There are no folders in this course") ++
      SHtml.link("/main/course/folders/create", () => {}, <button type="button">Create a folder</button>)

    if (CurrentCourse.canBeSupervisedBy(user)) {
      (<div>
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
        ++
        SHtml.link("/main/course/folders/create", () => {}, <button type="button">Create a folder</button>)
        )
    }
    else {
      //logged in user is a student
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

  def foldersupervisorsection(ignored: NodeSeq): NodeSeq = {
    if (CurrentFolderInCourse.is == null) {
      S.warning("Please first choose a folder")
      return S.redirectTo("/main/course/index")
    }

    val course = CurrentCourse.is
    val folder = CurrentFolderInCourse.is
    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
    if (!course.canBeSupervisedBy(user)) return NodeSeq.Empty

    val userLink = SHtml.link("/main/course/folders/users", () => {}, Text("Users"))

    <h2>Manage Folder</h2> ++
      DownloadHelper.renderCsvDownloadLink(
        folder.renderGradesCsv,
        s"${folder.getLongDescription}_Grades",
        Text(s"FolderGrades_${folder.getLongDescription}.csv")) ++
      Unparsed("&emsp;") ++ userLink
  }

  def folderuserlist(ignored: NodeSeq): NodeSeq = {
    val course = CurrentCourse.is
    val folder = CurrentFolderInCourse.is

    val participantList = if (course.hasParticipants) {
      TableHelper.renderTableWithHeader(course.getParticipants,
        ("First Name", (user: User) => Text(user.firstName.is)),
        ("Last Name", (user: User) => Text(user.lastName.is)),
        ("Email", (user: User) => Text(user.email.is)),
        ("Possible Points", (user: User) => Text(folder.getPossiblePoints.toString)),
        ("Achieved Points", (user: User) => Text(folder.getAchievedPoints(user).toString)),
        ("Average Grade/Num Attempts", (user: User) =>
          Text(folder.getOverallGrade(user).toString + "%/" + folder.getNumAttemptsAcrossAllProblems(user)))
      )
    } else {
      Text("There are no participants yet.")
    }

    <h3>Users of Folder: </h3> ++ participantList
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
        DownloadHelper.renderZipDownloadLink(course.getName, course.renderFoldersForZip, Text("Grades")) ++
      Unparsed("&emsp;") ++ userLink ++
      Unparsed("&emsp;") ++ exportLink ++
      Unparsed("&emsp;") ++ importLink
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
        //NOTE: These following two lines assume that you only want to count the grades/attempts of questions
        //which are CURRENTLY posed
        //If a student solves a question, but then its containing folder is unposed, that grade will not be accounted for
        ("Total Attempts", (user: User) => Text(course.getVisibleExercises.map(_.getNumberAttempts(user)).sum.toString)),
        ("Total Points", (user: User) => Text(course.getVisibleExercises.map(_.getHighestAttempt(user)).sum.toString)),
        ("Average Grade", (user: User) => new CourseRenderer(course).renderAverageGrade(user)),
        ("", (user: User) => dismissLink(user)))
    } else {
      <h2>Participants</h2> ++ Text("There are no paticipants yet.")
    }

    return supervisorList ++ participantList
  }

  //TODO 8/7/2020 make a reference to problem create
  def rendercreate(ignored: NodeSeq): NodeSeq = {
    if (CurrentProblemTypeInCourse.is == null) {
      S.warning("You have not selected a problem type")
      return S.redirectTo("/main/course/index")
    }

    val problemType = CurrentProblemTypeInCourse.is

    def createUnspecificProb(shortDesc: String, longDesc: String): Problem = {
      val createdBy: User = User.currentUser openOrThrowException "Lift protects this page against non-logged-in users"

      val unspecificProblem: Problem = Problem.create.setCreator(createdBy)
      unspecificProblem.setName(shortDesc).setDescription(longDesc).setProblemType(problemType)
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

  //TODO 8/14/2020 do we need this?
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

  //TODO 8/14/2020 do we need this?
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

}

object Courses {
}
