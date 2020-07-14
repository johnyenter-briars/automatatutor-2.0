package com.automatatutor.snippet

import java.text.DateFormat
import java.util.Calendar

import com.automatatutor.model._
import com.automatatutor.renderer.ProblemRenderer
import net.liftweb.util.Helpers.strToSuperArrowAssoc

import scala.xml.NodeSeq
import scala.xml.Text
import net.liftweb.http.{RedirectResponse, S, SHtml}
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.{JsCmd, JsCmds}
import net.liftweb.http.js.JsCmds.cmdToString
import net.liftweb.http.js.JsCmds.jsExpToJsCmd
import net.liftweb.mapper.By
import net.liftweb.sitemap.Loc.If

import scala.xml.NodeSeq
import scala.collection.mutable

class CourseHierarchy(var topNodes : List[CourseHierarchyNode]) {

  // Helper method to execute the JavaScript command built by 'func'
  // after the regular SHtml.ajaxCall
  def ajaxCallString(cmd:String, func: String => JsCmd) : String = {
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw(cmd), func)
    return ajaxCall.toString().substring(6, ajaxCall.toString().length - 2)
  }

  // Turns this CourseHierarchy into HTML applying the 'toHTML' function below
  // to every CourseHierarchyNode in the 'topNodes' list.
  def toHTML(showChildrenMap : ShowChildrenMap) : NodeSeq = {
      <div>{topNodes.map(node => toHTML(node, showChildrenMap)).foldLeft(NodeSeq.Empty)((x, y) => x ++ y)}</div>
  }

  // Turns a given CourseHierarchyNode to HTML creating a button to toggle its children
  // as well as a button to create a new problem of a certain type for the LeafNodes
  // which hold the ProblemTypes.
  private def toHTML(chn: CourseHierarchyNode, showChildrenMap : ShowChildrenMap) : NodeSeq = {
    val isSupervisor = CurrentCourse.canBeSupervisedBy(User.currentUser_!)

    // Helper method to set the current displayed/not displayed state
    // of the current node in the passed map 'showChildren' so that this state
    // is available upon re-rendering the sidebar.
    // This method is bound to the toggle button for the successor list.
    // Input format for 'str': <'id' as string> + " " + <'children of that id currently shown' as string>
    // E.g. str == "17 false" if children of node 17 are currently not shown.
    def setShowChildren(str: String) : Unit = {
      val split = str.split("\\s")
      val id = split(0).toInt
      val show = split(1).toBoolean
      showChildrenMap.map += (id -> show)
      return ()
    }

    // Returns an SHtml.ajaxCall redirecting to the create view for a given problem type 'pType'
    def ajaxCallToCreate(pType : ProblemType) : JsCmd = {
      def linkToCreate(ignored : String) = {
        S.redirectTo("/main/course/problems/create", () => {CurrentProblemTypeInCourse(pType)})
      }
      return SHtml.ajaxCall("", linkToCreate(_))
    }

    // Helper method that creates the HTML for the toggling of lists.
    // 'buttonContent' is displayed on the toggle button
    // 'otherContent' is displayed next to the toggle button
    // 'successorList' is what will be possible to toggle
    def mkCollapsibleList(id : Int,
                          className : String,
                          buttonContent : NodeSeq,
                          otherContent : NodeSeq,
                          successorList : List[NodeSeq]) : NodeSeq = {
      val idStr = "" + id
      val style = if (showChildrenMap.map.get(id).get) "" else "display:none"
      val styleInverted = if (!showChildrenMap.map.get(id).get) "" else "display:none"
      val onClickFunction = "toggleItem(" + id + ")"
      val ajaxCall : JsCmd = SHtml.ajaxCall(
        JsRaw({onClickFunction}), setShowChildren(_)
      )
      val toggleArrowRight = <div id={idStr + "right"} style={styleInverted} class="toggleArrow">&#x25B7;</div>
      val toggleArrowDown = <div id={idStr + "down"} style={style} class="toggleArrow">&#x25BD;</div>
	  
	  val empty = successorList.length == 0
	  val toggleArrows = if (!empty) toggleArrowRight ++ toggleArrowDown else NodeSeq.Empty
	  val classes = if (empty) "toggleButton emptySection" else "toggleButton nonemptySection"
	  
      val toggleButton = <button type="button" class={classes} onclick={ajaxCall}>{toggleArrows ++ buttonContent}</button>
      val succHTMLList = <div id={idStr} style={style}>{successorList}</div>
      return <div class={className}>{toggleButton ++ otherContent}</div> ++ succHTMLList
    }

    chn match {
      case InnerNode(id, name, succ)
      => {
        val buttonContent = Text(name)
        val successorList = succ.map(node => toHTML(node, showChildrenMap))
        return mkCollapsibleList(id, "structureNode", buttonContent, NodeSeq.Empty, successorList)
      }
      case LeafNode(id, pType, pList)
      => {
        val buttonContent = Text(pType.getProblemTypeName)
        val otherContent = if (isSupervisor)
          <button type="button" class="createButton" onclick={ajaxCallToCreate(pType)}>+</button>
          //<span class="createLink">{SHtml.link("/problems/create", () => ChosenProblemType(pType), Text("+"))}</span>
        else
          NodeSeq.Empty
        val successorList = pList.map(problem => toHTML(problem))
        return mkCollapsibleList(id, "problemTypeNode", buttonContent, otherContent, successorList)
      }
    }
  }

  // Turns a problem into HTML with links to solve/edit/pose/delete it.
  private def toHTML(problem : Problem) : NodeSeq = {
    val isPosed = problem.getPosed
    val isSupervisor = CurrentCourse.canBeSupervisedBy(User.currentUser_!)
  
    // Method that is called once an exercise is clicked.
    // If 'problem' has a PosedProblem associated to it, that is in the current course,
    // then the standard solve view is loaded
    // otherwise ...TODO
    def show(ignored: String) : Unit = {
      if (isSupervisor)
        S.redirectTo("/main/course/problems/preview", () => {CurrentProblemInCourse(problem)})
      else
        S.redirectTo("/main/course/problems/solve", () => {CurrentProblemInCourse(problem)})
    }

    // Called upon pressing the pose/unpose-button
    def poseButtonCall(ignored: String) : JsCmd = {
      if (isPosed)
        unpose
      else
        pose

      return Coursesidebar.rendersidebarjs
    }

    // Method that creates a new PosedProblem referencing 'problem' to this course
    def pose = {
      S.redirectTo("/main/course/problems/pose", () => CurrentProblemInCourse(problem))
    }

    // Method that removes any PosedProblems referencing 'problem' from this course
    def unpose = {
      problem.setPosed(false).setStartDate(null).setEndDate(null).save
	  S.redirectTo("/main/course/index", () => {})
    }
 
    // Method that deletes a problem
    def delete(ignored : String) = {
      val target = "/main/course/index"
      def function() = { problem.getProblemType.getProblemSnippet().onDelete(problem); problem.delete_! }
      val onclick : JsCmd = if(problem.canBeDeleted) {
        JsRaw("return confirm('Are you sure you want to delete this problem?')")
      } else  {
        JsCmds.Alert("Cannot delete this problem:\n" + problem.getDeletePreventers.mkString("\n")) & JsRaw("return false")
      }
      S.redirectTo(target, function)
    }

    val ajaxCallShow : JsCmd = SHtml.ajaxCall("", show(_))
    val ajaxCallPose : String = ajaxCallString("", poseButtonCall(_))
    val ajaxCallDelete : JsCmd = SHtml.ajaxCall("", delete(_))
	val mainButtonText = if (problem.getShortDescription.length > 0) problem.getShortDescription else "_"
    var mainButton = <button type="button" class="mainButton" onclick={ajaxCallShow}>{mainButtonText}</button>
	if (CurrentProblemInCourse.is != null && CurrentProblemInCourse.is.getProblemID == problem.getProblemID)
	  mainButton = <button type="button" class="mainButton selectedButton" onclick={ajaxCallShow}>{mainButtonText}</button>
	
    val poseButtonContent = if (isPosed) "Unpose" else "Pose"
    val poseButton = <button type="button" class="poseButton" onclick={ajaxCallPose}>{poseButtonContent}</button>
    val deleteLink = if (isPosed)
      NodeSeq.Empty
    else
      <button type="button" class="deleteButton" onclick={ajaxCallDelete}>x</button>

    val secundaryClass = if(isPosed) "red" else ""

    return <div class="problemNode">{mainButton}</div>
  }

  // Builds a map from each node ID in this hierarchy to a boolean value
  // (that can e.g. be interpreted as a show children value)
  def buildIdToBooleanMap : mutable.Map[Int, Boolean] = {
    topNodes.foldLeft(mutable.Map.empty[Int, Boolean])(
      (map, node) => map ++ buildIdToBooleanMap(node)
    )
  }
  private def buildIdToBooleanMap(node: CourseHierarchyNode) : mutable.Map[Int, Boolean] = node match {
    case InnerNode(id,_,succ)
    => succ.foldLeft(mutable.Map(id -> false))(
      (map, child) => map ++ buildIdToBooleanMap(child)
    )
    case LeafNode(id,_,_)
      => mutable.Map(id -> false)
  }

  // Loads all problems into CourseHierarchy that are visible for current user
  def update : Unit = {
    topNodes = topNodes.map(update)
    return ()
  }
  private def update(chn : CourseHierarchyNode) : CourseHierarchyNode = chn match {
    case InnerNode(id,name,succ)
      => {
      InnerNode(id,name,succ.map(update))
    }
    case LeafNode(id,pt,_)
      => {
	  val user = User.currentUser openOrThrowException "Lift only allows logged in users here"
	  LeafNode(id,pt,CurrentCourse.getProblemsForUser(user).filter(problem => problem.getProblemType == pt))
    }
  }
}

// Representation of course hierarchy
abstract class CourseHierarchyNode

case class InnerNode(id : Int,
                     name : String,
                     successors : List[CourseHierarchyNode])
  extends CourseHierarchyNode

case class LeafNode(id : Int,
                    problemType : ProblemType,
                    problems : List[Problem])
  extends CourseHierarchyNode
