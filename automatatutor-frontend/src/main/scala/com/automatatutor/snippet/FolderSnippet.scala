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

class Foldersnippet {

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

  def renderaddproblems(xhtml: NodeSeq): NodeSeq = {
    new FolderRenderer(CurrentFolderInCourse.is).renderAddProblemsButton
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
        )
        }
        <div>
          <button type="button" id="edit_access-modal-button_batch" class="modal-button">Batch Edit</button>
          {
          SHtml.button("Export Problems", () => {
            //If batch problem is empty, export all problems
            if(CurrentBatchExercisesInCourse.is.toList.isEmpty){
              DownloadHelper.offerZipDownloadToUser(folder.getLongDescription + "_Problems", folder.getExercisesUnderFolder.map(_.getProblem).map((problem: Problem) => {
                (problem.getName, <exported> {problem.toXML}</exported>.toString())
              }), DownloadHelper.XmlFile, DownloadHelper.ZipFile)
            }
            else{
              DownloadHelper.offerZipDownloadToUser(folder.getLongDescription + "_Problems", CurrentBatchExercisesInCourse.is.toList.map(_.getProblem).map((problem: Problem) => {
                (problem.getName, <exported> {problem.toXML}</exported>.toString())
              }), DownloadHelper.XmlFile, DownloadHelper.ZipFile)
            }
          })
          }
          <div id="edit_access-modal_batch" class="modal">

            <div class="modal-content">
              <div class="modal-header">
                <span class="close" id="edit_access-span_batch">&times;</span>
                <h3>Batch Edit Exercises</h3>
              </div>
              <div class="modal-body">
                {
                this.renderexerciseedit(xhtml, null)
                }
              </div>
            </div>
          </div>
        </div>
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

  def renderfolerimportbutton(xhtml: NodeSeq): NodeSeq = {
    if (CurrentFolderInCourse.is == null) {
      S.warning("Please first choose a folder")
      return S.redirectTo("/main/course/index")
    }

    new UploadHelper(new UploadTarget(UploadTargetEnum.Folder, CurrentFolderInCourse.is)).fileUploadForm(xhtml)
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

    <h2>Manage Folder</h2> ++
      Unparsed("&emsp;") ++
      DownloadHelper.renderCsvDownloadLink(
        folder.renderGradesCsv,
        s"${folder.getLongDescription}_Grades",
        Text(s"FolderGrades_${folder.getLongDescription}.csv"))
  }

  def renderaddfolderform(xhtml: NodeSeq): NodeSeq = {
    if (CurrentCourse.is == null) {
      S.warning("You have not selected a course")
      return S.redirectTo("/main/index")
    }

    val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
    if(!CurrentCourse.is.canBeSupervisedBy(user)) return NodeSeq.Empty

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

  def renderexerciseedit(xhtml: NodeSeq, exercise: Exercise): NodeSeq = {
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
        if(exercise == null){
          CurrentBatchExercisesInCourse.is.foreach((exercise: Exercise) =>{
            exercise.setMaxGrade(numMaxGrade).setAllowedAttempts(numAttempts).save
          })
        }
        else{
          exercise.setMaxGrade(numMaxGrade).setAllowedAttempts(numAttempts).save
        }
      }
    }

    val maxGradeField = SHtml.text(maxGrade, maxGrade = _)
    val attemptsField = SHtml.text(attempts, attempts = _)
    val editButton = SHtml.submit("Edit Selected Exercises", editProblem)

    val onClick: JsCmd = JsRaw(
      "return confirm('Are you sure you want to delete these problems from the folder? " +
        "If you do, all student grades on these problems will be lost!')")

    val deleteButton = SHtml.button("Delete", ()=>{
      if(exercise == null){
        CurrentBatchExercisesInCourse.is.foreach(_.delete_!)
      }else{
        exercise.delete_!
      }
    }, "onclick" -> onClick.toJsCmd,
      "style" -> "color: red")

    Helpers.bind("renderbatcheditform", xhtml,
      "maxgradefield" -> maxGradeField,
      "attemptsfield" -> attemptsField,
      "editbutton" -> editButton,
      "deletebutton" -> deleteButton
    )
  }

}

object FolderSnippetObj extends Foldersnippet {

  def getAddFolderForm(xhtml: NodeSeq): NodeSeq = this.renderaddfolderform(xhtml)
}
