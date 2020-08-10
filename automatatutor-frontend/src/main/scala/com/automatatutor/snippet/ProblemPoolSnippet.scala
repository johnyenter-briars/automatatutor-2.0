package com.automatatutor.snippet

import scala.Array.canBuildFrom
import scala.xml._
import com.automatatutor.lib._
import com.automatatutor.model._
import com.automatatutor.renderer.{CourseRenderer, ProblemRenderer}
import com.sun.tracing.Probe
import net.liftweb.common.{Box, Empty, EmptyBox, Full}
import net.liftweb.http
import net.liftweb.http.{S, SHtml, _}
import net.liftweb.http.SHtml.ElemAttr.pairToBasic
import net.liftweb.http.SHtml.ElemAttr
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.{JsCmd, JsCmds}
import net.liftweb.mapper.{Ascending, BaseOwnedMappedField, By, Cmp, Descending, MaxRows, OprEnum, OrderBy, QueryParam, StartAt}
import net.liftweb.util.AnyVar.whatVarIs
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers._
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.util.SecurityHelpers
import net.liftweb.util.Mailer
import net.liftweb.util.Mailer._

import scala.collection.mutable.ListBuffer

//TODO 7/17/2020 don't need to use this sessionvar, there's another one that lives in AutogenSnippet that does something similar
object CurrentEditableProblem extends SessionVar[Problem](null)
object BatchProblems extends SessionVar[ListBuffer[Problem]](null)
object PreviousPage extends SessionVar[String](null)

class Problempoolsnippet extends{

  if(BatchProblems.is == null) BatchProblems(new ListBuffer[Problem])

  def renderbatchlist(xhtml: NodeSeq): NodeSeq = {
    <h2>Currently Sending problems:</h2> ++
    <form>
      {
        TableHelper.renderTableWithHeader(
        BatchProblems.is.toList,
        ("", (problem: Problem) => Text(problem.getShortDescription)))
      }
      {
        //TODO 7/25/2020 temporary fix. the checkboxs should retain their "clickness" on the previous page
        //and then which which ever ones are clicked are
        SHtml.button("Deselect Problems", ()=>{BatchProblems.is.clear()})
      }
    </form>

  }

  def renderbatchsend(xhtml: NodeSeq): NodeSeq = {
    if (BatchProblems.is == null) {
      S.warning("Please first select multiple pronblems to edit!")
      return S.redirectTo("/main/problempool/index")
    }

    val user: User = User.currentUser openOrThrowException "Lift only allows logged-in-users here";
    val supervisedCourses = user.getSupervisedCourses
    val currentProblems = BatchProblems.is.toList

    //Keep a list of all the problem pointers we are going to send over to the multiple courses/folders
    //This is done because two different components need to be able to change attributes of the PPs at different times
    var problemPointersToSend = new ListBuffer[ProblemPointer]
    var attempts = "10"
    var maxGrade = "10"

    def sendProblems() = {
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
        problemPointersToSend.foreach((problemPointer: ProblemPointer) => {
          problemPointer.setMaxGrade(numMaxGrade).setAllowedAttempts(numAttempts).save
        })

        //Clear out BatchProblems for reuse
        BatchProblems.is.clear()
        S.redirectTo("/main/problempool/index", () => {})
      }
    }

    def selectionCallback(folderIDS: List[String], course: Course): Unit = {
      folderIDS.foreach((folderID: String) =>{
        val folder = Folder.findByID(folderID)

        currentProblems.foreach((problem: Problem) => {
          var problemPointer = new ProblemPointer
          problemPointer.setCourse(course).setFolder(folder).setProblem(problem)
          problemPointersToSend += problemPointer
        })
      })
    }

    val maxGradeField = SHtml.text(maxGrade, maxGrade = _)
    val attemptsField = SHtml.text(attempts, attempts = _)
    val sendButton = SHtml.submit("Send Problems", sendProblems)

    val courseTable = TableHelper.renderTableWithHeader(
      supervisedCourses,
      ("Course Name", (course: Course) => Text(course.getName)),
      ("Folder Select", (course: Course) => {

        val folderOptions = Folder.findAllByCourse(course)
          .map(f => (f.getFolderID.toString -> f.getLongDescription.toString))

        SHtml.multiSelect(folderOptions, List(), selectionCallback(_, course))
      }))


    Helpers.bind("renderbatchsendform", xhtml,
      "maxgradefield" -> maxGradeField,
      "attemptsfield" -> attemptsField,
      "sendbutton" -> sendButton,
      "courseselecttable" -> courseTable
    )
  }

  def renderlocationtree(xhtml: NodeSeq): NodeSeq = {
    val user: User = User.currentUser openOrThrowException "Lift only allows logged-in-users here"
    val supervisedCourses = user.getSupervisedCourses

    val selectedFolders = new ListBuffer[Folder]
    def checkBoxForFolder(folder: Folder): NodeSeq = {
      SHtml.checkbox(false, (chosen: Boolean) => {
        if(chosen) selectedFolders += folder
      })
    }

    <ul id="myUL">
      {
        supervisedCourses.map(course => {
          <li><span class="caret">{course.getName}</span>
            <ul class="nested">
              {
                course.getFolders.map(f => {

                  <li>{f.getLongDescription}{checkBoxForFolder(f)}</li>
                })
              }
            </ul>
          </li>
        })
      }
    </ul> ++
    <form>
    {
      SHtml.button("Send Problems", () => {
        selectedFolders.foreach(folder => {
          BatchProblems.is.foreach(problem => {
            val problemPointer = new ProblemPointer
            problemPointer.setCourse(folder.getCourse.get)
              .setProblem(problem)
              .setFolder(folder)
              //TODO 8/10/2020 add a method by which the user can set these settings on first transfer
              .setMaxGrade(10)
              .setAllowedAttempts(10)
              .save
          })
        })
        S.redirectTo("/main/problempool/index")
      })
    }
    </form>
  }

  def renderproblempool(xhtml: NodeSeq): NodeSeq ={
    val user: User = User.currentUser openOrThrowException "Lift only allows logged-in-users here"
    val usersProblems = Problem.findAllByCreator(user)

    if (usersProblems.isEmpty) return Text("You currently have no autogenerated problems")

    def editProblemButton(problem: Problem): NodeSeq = {
      if (user.hasSupervisedCourses) {
        SHtml.link(
          "/main/problempool/edit",
          () => {
            CurrentEditableProblem(problem)
          },
          <button type='button'>Edit</button>)
      } else {
        NodeSeq.Empty
      }
    }

    def sendButton(problem: Problem): NodeSeq = {
      if (user.hasSupervisedCourses) {
        SHtml.link(
          "/main/problempool/send",
          () => {
            CurrentEditableProblem(problem)
          },
          <button type='button'>Send To Course</button>)
      } else {
        NodeSeq.Empty
      }
    }

    def checkBoxForProblem(potentialProblem: Problem): NodeSeq = {
      SHtml.checkbox(false, (chosen: Boolean) => {
        if(chosen) BatchProblems.is += potentialProblem
      }, "class" -> "checkbox")
    }

    //Clear out BatchProblems to avoid over collection of problems if the user goes back and forth between pages
    BatchProblems.is.clear()

    val tableID = "problemPoolTable"
    
    val table = <form>
      {
        (TableHelper.renderTableWithHeader(
          tableID,
          usersProblems,
          List(3, 10.5, 21, 5, 13, 13.5, 5, 5.3, 10, 4),
          Map("Problem Stats" -> "Average grade a student gets on this problem/Average attempts per student"),
          ("", (problem: Problem) => checkBoxForProblem(problem)),
          ("Description", (problem: Problem) => Text(problem.getShortDescription)),
          ("Long Description", (problem: Problem) => Text(problem.getLongDescription)),
          ("Problem Stats", (problem: Problem) => new ProblemRenderer(problem).renderProblemStats),
          ("Problem Type", (problem: Problem) => Text(problem.getTypeName)),
          ("Instances", (problem: Problem) => new ProblemRenderer(problem).renderProblemInstances),
          ("", (problem: Problem) => editProblemButton(problem)),
          ("", (problem: Problem) => SHtml.link(
            "/main/problempool/practice",
            () => {
              CurrentEditableProblem(problem)
            },
            <button type='button'>Solve</button>)),
          ("", (problem: Problem) => sendButton(problem)),
          ("", (problem: Problem) => {
            new ProblemRenderer(problem).renderDeleteLink("/main/problempool/index")
          })
        )
        ++ {
          <div>
            <button type="button" id="batch_send-modal-button" class="modal-button">Batch Send</button>

            <div id="batch_send-modal" class="modal">

              <div class="modal-content">
                <div class="modal-header">
                  <span class="close">&times;</span>
                  <h3>Send Problems to Folders</h3>
                </div>
                <div class="modal-body">
                  {
                    this.renderlocationtree(xhtml)
                  }
                </div>
              </div>
            </div>
          </div>
        }
        ++ <br></br>
        ++ SHtml.button("Delete Selected", ()=>{
          BatchProblems.is.foreach(_.delete_!)
        }, "onclick" -> JsRaw("return confirm('Are you sure you want to delete the selected problems?')").toJsCmd))
      }
    </form>

    val descriptionFilterID = "input_desc"
    val descriptionFilter = SHtml.text("", (x: String)=>{},
      "onkeyup"->s"filterTableRows('$tableID', 'Description', '$descriptionFilterID');", "id"->descriptionFilterID)

    val problemTypeFilterID = "input_pt"
    val problemTypeFilter = SHtml.text("", (x: String)=>{},
      "onkeyup"->s"filterTableRows('$tableID', 'Problem Type', '$problemTypeFilterID');", "id"->problemTypeFilterID)

    val longdescriptionFilterID = "input_long_desc"
    val longdescriptionFilter = SHtml.text("", (x: String)=>{},
      "onkeyup"->s"filterTableRows('$tableID', 'Long Description', '$longdescriptionFilterID');", "id"->longdescriptionFilterID)

    Helpers.bind("problempool", xhtml,
      "problemlist" -> table,
      "descriptionfilter" -> descriptionFilter,
      "problemtypefilter" -> problemTypeFilter,
      "longdescriptionfilter" -> longdescriptionFilter
    )
  }

  def renderbackbutton(xhtml: NodeSeq): NodeSeq = {
    val redirectTarget = if(PreviousPage.is == null) "/main/problempool/index" else PreviousPage.is
    SHtml.link(redirectTarget, ()=>{PreviousPage(null)}, <button>Cancel</button>)
  }

  def renderproblemedit(ignored: NodeSeq): NodeSeq ={
    if (CurrentEditableProblem.is == null) {
      S.warning("Please first choose a problem to edit")
      return S.redirectTo("/main/problempool/index")
    }

    val problem : Problem = CurrentEditableProblem.is
    val problemSnippet: SpecificProblemSnippet = problem.getProblemType.getProblemSnippet

    def returnFunc(ignored : Problem) = {
      //TODO 7/27/2020 need a better way of conditional setting the redirect page based on previous page
      val redirectTarget = if(PreviousPage.is == null) "/main/problempool/index" else PreviousPage.is
      PreviousPage(null)
      S.redirectTo(redirectTarget)
    }

    problemSnippet.renderEdit match {
      case Full(renderFunc) => renderFunc(problem, returnFunc)
      case Empty            =>
        S.error("Editing not implemented for this problem type"); S.redirectTo("/main/course/index")
      case _                => S.error("Error when retrieving editing function"); S.redirectTo("/main/course/index")
    }

  }

  def renderproblemstatistics(ignored: NodeSeq): NodeSeq = {
    if (CurrentEditableProblem.is == null) {
      S.warning("Please first choose a problem to edit")
      return S.redirectTo("/main/problempool/index")
    }
    val problem : Problem = CurrentEditableProblem.is

    new ProblemRenderer(problem).renderProblemInstancesTable
  }

  def renderdeletegrades(ignored: NodeSeq): NodeSeq = {
    if (CurrentEditableProblem.is == null) {
      S.warning("Please first choose a problem to edit")
      return S.redirectTo("/main/problempool/index")
    }

    <form>
      {
        val onClick = JsRaw("return confirm('Are you sure you want to delete all the user grades for this problem?')")

        SHtml.link(
          "/main/problempool/index",
          () => {
            SolutionAttempt.deleteAllByProblem(CurrentEditableProblem.is)
          },
          Text("Delete ALL User Grades"),
          "onclick" -> onClick.toJsCmd,
          "style" -> "color: red")
      }
    </form>
  }

  def renderpractice(ignored: NodeSeq): NodeSeq = {
    if (CurrentEditableProblem == null) {
      S.warning("Please first choose a problem")
      return S.redirectTo("/autogen/index")
    }

    val problem: Problem = CurrentEditableProblem.is
    val problemSnippet: SpecificProblemSnippet = problem.getProblemType.getProblemSnippet

    def returnFunc(problem: Problem) = {
      S.redirectTo("/main/problempool/index")
    }

    val returnLink = SHtml.link("/main/problempool/index", () => {}, Text("Return"))

    problemSnippet.renderSolve(problem, 10, Empty,
      (grade, date) => SolutionAttempt, returnFunc, () => 1, () => 0) ++ returnLink
  }

  //TODO 7/17/2020 Have capability to delete a problempointer from a folder
  //also need to have finer control over sending a problem and taking it back
  //also what if that folder already has that problem in it?
  //Better error handling overall
  def sendtocourseform(xhtml: NodeSeq): NodeSeq = {
    if (CurrentEditableProblem.is == null) {
      S.warning("Please first choose a problem to send")
      return S.redirectTo("/main/problempool/index")
    }

    def selectionCallback(folderIDS: List[String], course: Course): Unit = {
      val user: User = User.currentUser openOrThrowException "Lift only allows logged-in-users here";
      val courses = user.getSupervisedCourses

      folderIDS.foreach((folderID: String) => {
        val folder = Folder.findByID(folderID)
        val problem = CurrentEditableProblem.is
        val problemPointer = new ProblemPointer
        problemPointer.setProblem(problem)
          .setFolder(folder)
          //TODO 7/15/2020 add a method by which the user can set these settings on first transfer
          .setAllowedAttempts(10)
          .setMaxGrade(10)
          .setCourse(folder.getCourse)
          .save
      })
    }

    val user: User = User.currentUser openOrThrowException "Lift only allows logged-in-users here";
    val supervisedCourses = user.getSupervisedCourses

    val cancelButton = SHtml.link("/main/problempool/index", () => {}, <button type='button'>Cancel</button>)

    <form action="/main/problempool/send">
      <p>Please select a folder within
        <strong>ONE</strong>
        course, to transfer the problem to</p>
      <p>Leave the rest as "----"</p>
      {(TableHelper.renderTableWithHeader(
        supervisedCourses,
        ("Name", (course: Course) => Text(course.getName)),
        ("", (course: Course) => {

          val folderOptions = Folder.findAllByCourse(course)
            .map(f => (f.getFolderID.toString -> f.getLongDescription.toString))

          SHtml.multiSelect(folderOptions, List(), selectionCallback(_, course))
        }))
        ++ SHtml.button("Send", () => {}, "type" -> "submit")
        ++ cancelButton
        )}
    </form>
  }

  def renderproblemoptions(xhtml: NodeSeq): NodeSeq = {
    if (CurrentFolderInCourse.is == null) {
      S.warning("Please first choose a folder to send problems to")
      return S.redirectTo("/main/course/folders/index")
    }

    def problemsAreIdentical(problem1: Problem, problem2: Problem): Boolean = {
      problem1.getProblemID == problem2.getProblemID
    }

    val user: User = User.currentUser openOrThrowException "Lift only allows logged-in-users here";
    val folder = CurrentFolderInCourse.is
    //Only show the problems which are not in the current folder
    val problems = Problem.findAllByCreator(user).filterNot(folder.getProblemsUnderFolder.contains(_))

    def checkBoxForProblem(potentialProblem: Problem): NodeSeq = {

      SHtml.checkbox(false, (chosen: Boolean) => {
        if(chosen){
          //TODO 7/21/2020 refactor this to make less ugly and more responsive. Something like returning a JSCmd to alert the user
          //If a ProblemPointer with the same problem already exists with the folder don't add it
          var isDuplicate: Boolean = false
          ProblemPointer.findAllByFolder(folder).map(_.getProblem).foreach(problem =>{
            if(problemsAreIdentical(problem, potentialProblem)) isDuplicate = true
          })

          if(!isDuplicate){
            val problemPointer = new ProblemPointer
            problemPointer.setProblem(potentialProblem)
              .setFolder(folder)
              //TODO 7/17/2020 add a method by which the user can set these settings on first transfer
              .setAllowedAttempts(10)
              .setMaxGrade(10)
              .setCourse(folder.getCourse)
              .save
          }
        }
      })
    }
    val tableID = "problemOptionsTable"

    val table = <form>
      {
        TableHelper.renderTableWithHeaderPlusID(
          tableID,
          problems,
          ("Description", (problem: Problem) => Text(problem.getShortDescription)),
          ("Problem Type", (problem: Problem) => Text(problem.getTypeName)),
          ("Click to add problem", (problem: Problem) => checkBoxForProblem(problem))
        )
      }
      {
        SHtml.button("Add Problems", () => {
          S.redirectTo("/main/course/folders/index")
        })
      }
    </form>

    val descriptionFilterID = "input_desc"
    val descriptionFilter = SHtml.text("", (x: String)=>{},
      "onkeyup"->s"filterTableRows('$tableID', 'Description', '$descriptionFilterID');", "id"->descriptionFilterID)

    val problemTypeFilterID = "input_pt"
    val problemTypeFilter = SHtml.text("", (x: String)=>{},
      "onkeyup"->s"filterTableRows('$tableID', 'Problem Type', '$problemTypeFilterID');", "id"->problemTypeFilterID)

    val longdescriptionFilterID = "input_long_desc"
    val longdescriptionFilter = SHtml.text("", (x: String)=>{},
      "onkeyup"->s"filterTableRows('$tableID', 'Long Description', '$longdescriptionFilterID');", "id"->longdescriptionFilterID)

    Helpers.bind("problemoptions", xhtml,
      "problemlist" -> table,
      "descriptionfilter" -> descriptionFilter,
      "problemtypefilter" -> problemTypeFilter,
      "longdescriptionfilter" -> longdescriptionFilter
    )
  }
}

object Problempoolsnippet{
}