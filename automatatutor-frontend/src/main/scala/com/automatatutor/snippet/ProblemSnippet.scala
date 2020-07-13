package com.automatatutor.snippet

import java.text.DateFormat
import java.util.{Calendar, Date}

import scala.xml._
import com.automatatutor.lib._
import com.automatatutor.model._
import com.automatatutor.renderer.ProblemRenderer
import net.liftweb.common._
import net.liftweb.http._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.util.{Helpers, PassThru}
import net.liftweb.util.Helpers.bind
import net.liftweb.util.Helpers.strToSuperArrowAssoc
import net.liftweb.util.AnyVar.whatVarIs

class Problemsnippet {
  
}

trait SpecificProblemSnippet {
  /**
   * Should produce a NodeSeq that allows the user to create a new problem of
   *  the type. This NodeSeq also has to handle creation of the unspecific
   *  {@link Problem}.
   */
  def renderCreate(
    createUnspecificProb: (String, String) => Problem,
    returnFunc:       Problem => Unit): NodeSeq

  /**
   * Should produce a NodeSeq that allows the user to edit the problem
   *  associated with the given unspecific problem.
   */
  def renderEdit: Box[((Problem, Problem => Unit) => NodeSeq)]

  /**
   * Should produce a NodeSeq that allows the user a try to solve the problem
   *  associated with the given unspecific problem. The function
   *  recordSolutionAttempt must be called once for every solution attempt
   *  and expects the grade of the attempt (which must be <= maxGrade) and the
   *  time the attempt was made. After finishing the solution attempt, the
   *  snippet should send the user back to the overview of problems in the
   *  set by calling returnToSet
   */
  def renderSolve(problem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                  recordSolutionAttempt: (Int, Date) => SolutionAttempt,
                  returnFunc:            Problem => Unit, attemptsLeft: () => Int, bestGrade: () => Int): NodeSeq

  /**
   * Is called before the given unspecific problem is deleted from the database.
   *  This method should delete everything associated with the given unspecific
   *  problem from the database
   */
  def onDelete(problem: Problem): Unit
}