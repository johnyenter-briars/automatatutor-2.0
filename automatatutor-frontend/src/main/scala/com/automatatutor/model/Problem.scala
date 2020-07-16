package com.automatatutor.model

import com.automatatutor.model.problems._
import com.automatatutor.snippet._
import com.automatatutor.snippet.problems._
import net.liftweb.common._
import net.liftweb.mapper._
import bootstrap.liftweb.StartupHook
import com.automatatutor.lib.Config
import scala.xml.XML
import scala.xml.Node
import java.util.{Calendar, Date}

class ProblemType extends LongKeyedMapper[ProblemType] with IdPK {
  def getSingleton = ProblemType

  val DFAConstructionTypeName = "DFA Construction"
  val NFAConstructionTypeName = "NFA Construction"
  val NFAToDFATypeName = "NFA to DFA"
  val EnglishToRegExTypeName = "RE Construction"
  val WordsInRegExTypeName = "RE Words"
  val PLTypeName = "Pumping Lemma Proof"
  val BuchiSolvingTypeName = "Buchi Game Solving"
  val WordsInGrammarTypeName = "Grammar Words"
  val DescriptionToGrammarTypeName = "Grammar Construction"
  val GrammarToCNFTypeName = "Chomsky Normalform"
  val CYKTypeName = "CYK Algorithm"
  val FindDerivationTypeName = "Find Derivation"
  val ProductConstructionTypeName = "Product Construction"
  val ProdConTypeName = "Prod Con"
  val MinimizationTypeName = "Minimization"
  val PDAConstructionTypeName = "PDA Construction"
  val PDAWordProblemTypeName = "PDA Words"
  val WhileToTMTypeName = "While to TM"
  val PumpingLemmaGameTypeName = "Pumping Lemma Game"
  val RegExToNFATypeName = "RE to NFA"
  val EquivalenceClassesTypeName = "Equivalence Classes"

  val knownProblemSnippets: Map[String, SpecificProblemSnippet] = Map(
    DFAConstructionTypeName -> DFAConstructionSnippet,			// NFA/DFA/RE problems
    NFAConstructionTypeName -> NFAProblemSnippet,
    NFAToDFATypeName -> NFAToDFAProblemSnippet,
    EnglishToRegExTypeName -> RegExConstructionSnippet,
    WordsInRegExTypeName -> WordsInRegExSnippet,
    RegExToNFATypeName -> RegExToNFASnippet,
    WordsInGrammarTypeName -> WordsInGrammarSnippet,			// Grammar problems
    DescriptionToGrammarTypeName -> DescriptionToGrammarSnippet,
    GrammarToCNFTypeName -> GrammarToCNFSnippet,
    CYKTypeName -> CYKProblemSnippet,
    FindDerivationTypeName -> FindDerivationSnippet,
    PDAWordProblemTypeName -> PDAWordProblemSnippet,			// PDA problems
    PDAConstructionTypeName -> PDAConstructionSnippet,
    EquivalenceClassesTypeName -> EquivalenceClassesSnippet,	// Equivalence classes problem
    PumpingLemmaGameTypeName -> PumpingLemmaGameSnippet,		// Pumpling lemma game problem
    WhileToTMTypeName -> WhileToTMSnippet)						// TM problems

  val knownProblemSingletons: Map[String, SpecificProblemSingleton] = Map(
    DFAConstructionTypeName -> DFAConstructionProblem,			// NFA/DFA/RE problems
    NFAConstructionTypeName -> NFAConstructionProblem,
    NFAToDFATypeName -> NFAToDFAProblem,
    EnglishToRegExTypeName -> RegExConstructionProblem,
    WordsInRegExTypeName -> WordsInRegExProblem,
    RegExToNFATypeName -> RegExToNFAProblem,
    WordsInGrammarTypeName -> WordsInGrammarProblem,			// Grammar problems
    DescriptionToGrammarTypeName -> DescriptionToGrammarProblem,
    GrammarToCNFTypeName -> GrammarToCNFProblem,
    CYKTypeName -> CYKProblem,
    FindDerivationTypeName -> FindDerivationProblem,
    PDAWordProblemTypeName -> PDAWordProblem,					// PDA problems
    PDAConstructionTypeName -> PDAConstructionProblem, 
    EquivalenceClassesTypeName -> EquivalenceClassesProblem,	// Equivalence classes problem
    PumpingLemmaGameTypeName -> PumpingLemmaGameProblem,		// Pumpling lemma game problem
    WhileToTMTypeName -> WhileToTMProblem)						// TM problems

  protected object problemTypeName extends MappedString(this, 200)

  def getProblemTypeName = this.problemTypeName.is
  def setProblemTypeName(problemTypeName: String) = this.problemTypeName(problemTypeName)

  def getProblemSnippet(): SpecificProblemSnippet = knownProblemSnippets(this.problemTypeName.is)
  def getSpecificProblemSingleton(): SpecificProblemSingleton = knownProblemSingletons(this.problemTypeName.is)

  def getSpecificProblem(generalProblem: Problem): SpecificProblem[_] = this.getSpecificProblemSingleton().findByGeneralProblem(generalProblem)
}

object ProblemType extends ProblemType with LongKeyedMetaMapper[ProblemType] with StartupHook {

  def onStartup = knownProblemSnippets.map(entry => assertExists(entry._1))

  def assertExists(typeName: String): Unit = if (!exists(typeName)) { ProblemType.create.problemTypeName(typeName).save }
  def exists(typeName: String): Boolean = !findAll(By(ProblemType.problemTypeName, typeName)).isEmpty

  def findByName(typeName: String): List[ProblemType] = findAll(By(ProblemType.problemTypeName, typeName))
}

class Problem extends LongKeyedMapper[Problem] with IdPK {
  def getSingleton = Problem

  protected object problemType extends MappedLongForeignKey(this, ProblemType)
  protected object createdBy extends MappedLongForeignKey(this, User)
  protected object shortDescription extends MappedText(this)
  protected object longDescription extends MappedText(this)
  
  // "posed" information
//  protected object courseId extends MappedLongForeignKey(this, Course)
//  protected object isPosed extends MappedBoolean(this)
//  protected object allowedAttempts extends MappedLong(this)
//  protected object maxGrade extends MappedLong(this)
//  protected object startDate extends MappedDateTime(this)
//  protected object endDate extends MappedDateTime(this)

  def getProblemID: Long = this.id.is

  def getProblemType = this.problemType.obj openOrThrowException "Every Problem must have a ProblemType"
  def setProblemType(problemType: ProblemType) = this.problemType(problemType)

  def getTypeName(): String = (problemType.obj.map(_.getProblemTypeName)) openOr ""

  def getCreator: User = this.createdBy.obj openOrThrowException "Every Problem must have a CreatedBy"
  def setCreator(creator: User) = this.createdBy(creator)

  def getShortDescription = this.shortDescription.is
  def setShortDescription(description: String) = this.shortDescription(description)

  def getLongDescription = this.longDescription.is
  def setLongDescription(description: String) = this.longDescription(description)

  def getCourse : Box[Course] =
    throw new NotImplementedError("Problems are no longer tied directly to courses. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")
  def setCourse ( course : Course ) =
    throw new NotImplementedError("Problems are no longer tied directly to courses. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")
  def setCourse ( course: Box[Course] ) =
    throw new NotImplementedError("Problems are no longer tied directly to courses. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")

  def getPosed: Boolean =
    throw new NotImplementedError("Problems are no longer posed directly to courses. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")
  def setPosed(posed: Boolean) =
    throw new NotImplementedError("Problems are no longer posed directly to courses. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")

  def getAllowedAttempts: Long =
    throw new NotImplementedError("Problems no longer have allowed attempts. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")
  def getAllowedAttemptsString: String =
    throw new NotImplementedError("Problems no longer have allowed attempts. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")
  def setAllowedAttempts(attempts: Long) =
    throw new NotImplementedError("Problems no longer have allowed attempts. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")

  def getMaxGrade: Long =
    throw new NotImplementedError("Problems no longer have a maxgrade. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")
  def setMaxGrade(maxGrade: Long) =
    throw new NotImplementedError("Problems no longer have a maxgrade. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")

  def getStartDate: Date =
    throw new NotImplementedError("Problems no longer have a startdate. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")
  def setStartDate(startDate: Date) =
    throw new NotImplementedError("Problems no longer have a startdate. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")

  def getEndDate: Date =
    throw new NotImplementedError("Problems no longer have a enddate. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")
  def setEndDate(endDate: Date) =
    throw new NotImplementedError("Problems no longer have a enddate. ProblemLinks are the objects" +
      "which live under courses and reference problem objects")

  //TODO 7/15/2020 figure out what to do with this
//  def shareWithUserByEmail(email: String): Boolean = {
//    val otherUser = User.findByEmail(email) match {
//      case Full(user) => user
//      case _          => return false
//    }
//
//    val copiedGeneralProblem = new Problem
//    copiedGeneralProblem.problemType(this.problemType.get)
//    copiedGeneralProblem.createdBy(otherUser)
//    copiedGeneralProblem.shortDescription(this.shortDescription.get)
//    copiedGeneralProblem.longDescription(this.longDescription.get)
//    copiedGeneralProblem.save()
//
//    val copiedSpecificProblem: SpecificProblem[_] = this.problemType.obj.openOrThrowException("Every problem must have an associated type").getSpecificProblem(this).copy()
//    copiedSpecificProblem.setGeneralProblem(copiedGeneralProblem)
//    copiedSpecificProblem.save()
//
//    return true
//  }

  def toXML: Node = {
    val specificProblem: SpecificProblem[_] = this.problemType.obj.openOrThrowException("Every problem must have an associated type").getSpecificProblem(this)
    return <problem>
             <typeName>{ this.getTypeName }</typeName>
             <shortDescription>{ this.getShortDescription }</shortDescription>
             <longDescription>{ this.getLongDescription }</longDescription>
             <specificProblem>{ specificProblem.toXML }</specificProblem>
           </problem>
  }
  
  def canBeDeleted : Boolean = true
  def getDeletePreventers : Seq[String] = List()

  override def delete_! : Boolean = {
    if (!canBeDeleted) {
      return false
    } else {
      ProblemPointer.deleteByReferencedProblem(this)
      val specificDel = this.getProblemType.getSpecificProblemSingleton.deleteByGeneralProblem(this)
      val superDel = super.delete_!
      return specificDel & superDel
    }
  }
  
//  def getAttempts(user: User): Seq[SolutionAttempt] = {
//    SolutionAttempt.findAll(
//      By(SolutionAttempt.userId, user),
//      By(SolutionAttempt.problemId, this))
//  }
//
//  def getNumberAttempts(user: User): Int = {
//    this.getAttempts(user).size
//  }
//
//  def getNumberAttemptsRemaining(user: User): Long = {
//    if (this.allowedAttempts == 0) return 99
//    else return this.allowedAttempts.is - this.getNumberAttempts(user)
//  }
//  def getNumberAttemptsRemainingString(user: User): String = {
//    if (this.allowedAttempts == 0) return "∞"
//    else return this.getNumberAttemptsRemaining(user).toString
//  }
//
//  def getGrade(user: User): Int = {
//    val grades = this.getAttempts(user).map(_.grade.is)
//    if (grades.isEmpty) {
//      0
//    } else {
//      grades.max
//    }
//  }
//
//  def getTimeToExpirationInMs : Long = {
//    val nowTimestamp = Calendar.getInstance().getTime().getTime()
//    val endDateTimestamp = this.endDate.is.getTime()
//    return endDateTimestamp - nowTimestamp
//  }
//  def getTimeToExpirationString : String = {
//    val ms = this.getTimeToExpirationInMs
//	if (ms < 0) return "ended"
//
//	val msPerSecond = 1000
//    val msPerMinute = 60 * msPerSecond
//    val msPerHour = 60 * msPerMinute
//    val msPerDay = 24 * msPerHour
//
//    val days = ms / msPerDay
//    val hours = (ms % msPerDay) / msPerHour
//    val minutes = (ms % msPerHour) / msPerMinute
//    val seconds = (ms % msPerMinute) / msPerSecond
//
//    return (days + " days, " + hours + ":" + minutes + ":" + seconds + " hours")
//  }
//
//  /**
//    * A problem is defined as closed if either the user has used all attempts
//    * or if they have reached the maximal possible grade
//    */
//  def isOpen(user: User): Boolean = {
//    val allowedAttempts = this.allowedAttempts.is
//    val takenAttempts = this.getNumberAttempts(user)
//    val maxGrade = this.maxGrade.is
//    val userGrade = this.getGrade(user)
//
//    return takenAttempts < allowedAttempts && userGrade < maxGrade
//  }
}

object Problem extends Problem with LongKeyedMetaMapper[Problem] {
  def findAllByCreator(creator: User): List[Problem] = findAll(By(Problem.createdBy, creator))

  def deleteByCreator(creator: User) : Unit = this.bulkDelete_!!(By(Problem.createdBy, creator))

  def findAllOfType(problemType: ProblemType) : List[Problem] = findAll(By(Problem.problemType, problemType))

  def fromXML(xml: Node): Box[Problem] = {
    //find matching specific type
    val matchingTypes = ProblemType.findByName((xml \ "typeName").text)
    if (matchingTypes.isEmpty) return Empty
    val specificType = matchingTypes.head
    //generate general problem
    val generalProblem = new Problem
    generalProblem.problemType(specificType)
    generalProblem.createdBy(User.currentUser)
    generalProblem.shortDescription((xml \ "shortDescription").text)
    generalProblem.longDescription((xml \ "longDescription").text)
    generalProblem.save()
    //build specific problem
    val specificProblem = specificType.getSpecificProblemSingleton().fromXML(generalProblem, (xml \ "specificProblem" \ "_").head)
    if (specificProblem == Empty) { generalProblem.delete_! }
    return Full(generalProblem)
  }
}

abstract trait SpecificProblem[T] {
  /// Does not save the modified problem. Caller has to do that manually by calling save()
  def setGeneralProblem(newProblem: Problem): T
  def save(): Boolean
  def copy(): SpecificProblem[T]
  def toXML: Node
}

abstract trait SpecificProblemSingleton {
  def fromXML(generalProblem: Problem, xml: Node): Box[SpecificProblem[_]]
  def findByGeneralProblem(generalProblem: Problem): SpecificProblem[_]
  def deleteByGeneralProblem(generalProblem: Problem): Boolean
}
