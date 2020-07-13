package com.automatatutor.snippet

import scala.xml.{NodeSeq, Text, Unparsed}
import net.liftweb.http._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.{JsCmd, JsCmds}
import net.liftweb.http.js.JsCmds.cmdToString
import net.liftweb.http.js.JsCmds.jsExpToJsCmd
import com.automatatutor.model._
import net.liftweb.util.Helpers
import net.liftweb.util.Helpers.bind
import net.liftweb.util.Helpers.strToSuperArrowAssoc

import scala.collection.mutable

class Coursesidebar {
  // Dynamically creates the 'NodeSeq' representing the sidebar
  def rendersidebar(ignored: NodeSeq): NodeSeq = {
    THEO.theoCourseHierarchy.update
	THEO.theoCourseHierarchy.toHTML(THEO.showChildrenMap)
  }
}

object Coursesidebar {
  def rendersidebarjs : JsCmd = {
    val sidebarcontent : NodeSeq = (new Coursesidebar).rendersidebar(NodeSeq.Empty)
    return JsCmds.Run("document.getElementById('sidebarinterior').innerHTML = `" + (sidebarcontent).toString() + "`")
  }
}

case class ShowChildrenMap(map : mutable.Map[Int, Boolean])

object THEO {

  // Map that describes the structure of said course or more specifically
  // orders (some of) the available ProblemTypes into the three categories
  // 'Regular Languages', 'Contextfree Languages' and 'Computability'.
  val chaptersToProblemTypesMap : Map[String, List[ProblemType]] = Map(
    "Regular Languages" -> (
        ProblemType.findByName("DFA Construction") :::
        ProblemType.findByName("NFA Construction") :::
        ProblemType.findByName("RE Construction") :::
        ProblemType.findByName("RE Words") :::
        ProblemType.findByName("RE to NFA") :::
        ProblemType.findByName("Equivalence Classes") :::
        // ProblemType.findByName("NFA to DFA") :::
        ProblemType.findByName("Product Construction") :::
        ProblemType.findByName("Prod Con") :::
        ProblemType.findByName("Pumping Lemma Game") :::
        ProblemType.findByName("Minimization")
      ),
    "Contextfree Languages" -> (
        ProblemType.findByName("Grammar Construction") :::
        ProblemType.findByName("Grammar Words") :::
        ProblemType.findByName("Find Derivation") :::
        ProblemType.findByName("Chomsky Normalform") :::
        ProblemType.findByName("CYK Algorithm") :::
        ProblemType.findByName("PDA Construction") :::
        ProblemType.findByName("PDA Words")
    ),
    "Computability" -> (
        ProblemType.findByName("While to TM")
    )
  )

  // Method to build the CourseHierarchy object for "THEO".
  def buildTHEOCourseHierarchy : CourseHierarchy = {
    val topNodes : List[CourseHierarchyNode] = chaptersToProblemTypesMap.keys.map(
      chapterName =>
        InnerNode(IdGenerator.getNextId,
          chapterName,
          chaptersToProblemTypesMap.get(chapterName).get.map(leafNodeFromProblemType))
    ).toList

    return new CourseHierarchy(topNodes)
  }

  // Helper method to construct LeafNodes for each ProblemType
  private def leafNodeFromProblemType(pt : ProblemType) : LeafNode = {
    return LeafNode(IdGenerator.getNextId, pt, Problem.findAllOfType(pt).filter(pr => pr.getPosed))
  }

  val theoCourseHierarchy = buildTHEOCourseHierarchy
  var showChildrenMap = ShowChildrenMap(theoCourseHierarchy.buildIdToBooleanMap)
}

object IdGenerator {
  private var id = 0
  def getNextId : Int = {
    id = id + 1
    return id
  }
}
