package com.automatatutor.model

import net.liftweb.mapper._
import net.liftweb.common.Box
import net.liftweb.common.Empty
import net.liftweb.common.Full

class SolutionAttempt extends LongKeyedMapper[SolutionAttempt] with IdPK {
	def getSingleton = SolutionAttempt

	object dateTime extends MappedDateTime(this)
	object userId extends MappedLongForeignKey(this, User) // CARE: might have been deleted
	object problempointerId extends MappedLongForeignKey(this, ProblemPointer) // CARE: might have been deleted
	object grade extends MappedInt(this)
}

object SolutionAttempt extends SolutionAttempt with LongKeyedMetaMapper[SolutionAttempt] {
	def getLatestAttempt(user : User, problem : ProblemPointer) : Box[SolutionAttempt] = {
	  val allAttempts = this.findAll(By(SolutionAttempt.userId, user), By(SolutionAttempt.problempointerId, problem))
	  return if (allAttempts.isEmpty) { Empty } else { Full(allAttempts.maxBy(attempt => attempt.dateTime.is.getTime())) }
	}
}


/***************
 * Grammar Problems
 */
class WordsInGrammarSolutionAttempt extends LongKeyedMapper[WordsInGrammarSolutionAttempt] with IdPK {
	def getSingleton = WordsInGrammarSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptWordsIn extends MappedText(this)
	object attemptWordsOut extends MappedText(this)
}

object WordsInGrammarSolutionAttempt extends WordsInGrammarSolutionAttempt with LongKeyedMetaMapper[WordsInGrammarSolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : WordsInGrammarSolutionAttempt = {
		return this.find(By(WordsInGrammarSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a words in grammar attempt"
	}
}

class DescriptionToGrammarSolutionAttempt extends LongKeyedMapper[DescriptionToGrammarSolutionAttempt] with IdPK {
	def getSingleton = DescriptionToGrammarSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptGrammar extends MappedText(this)
}

object DescriptionToGrammarSolutionAttempt extends DescriptionToGrammarSolutionAttempt with LongKeyedMetaMapper[DescriptionToGrammarSolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : DescriptionToGrammarSolutionAttempt = {
		return this.find(By(DescriptionToGrammarSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a description to grammar attempt"
	}
}

class GrammarToCNFSolutionAttempt extends LongKeyedMapper[GrammarToCNFSolutionAttempt] with IdPK {
	def getSingleton = GrammarToCNFSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptGrammar extends MappedText(this)
}

object GrammarToCNFSolutionAttempt extends GrammarToCNFSolutionAttempt with LongKeyedMetaMapper[GrammarToCNFSolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : GrammarToCNFSolutionAttempt = {
		return this.find(By(GrammarToCNFSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a grammar to CNF attempt"
	}
}

class CYKSolutionAttempt extends LongKeyedMapper[CYKSolutionAttempt] with IdPK {
	def getSingleton = CYKSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attempt extends MappedText(this)
}

object CYKSolutionAttempt extends CYKSolutionAttempt with LongKeyedMetaMapper[CYKSolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : CYKSolutionAttempt = {
		return this.find(By(CYKSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a CYK attempt"
	}
}

class FindDerivationSolutionAttempt extends LongKeyedMapper[FindDerivationSolutionAttempt] with IdPK {
	def getSingleton = FindDerivationSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attempt extends MappedText(this)
}

object FindDerivationSolutionAttempt extends FindDerivationSolutionAttempt with LongKeyedMetaMapper[FindDerivationSolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : FindDerivationSolutionAttempt = {
		return this.find(By(FindDerivationSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a FindDerivation attempt"
	}
}



/***************
 * PDA Problems
 */
class PDAConstructionSolutionAttempt extends LongKeyedMapper[PDAConstructionSolutionAttempt] with IdPK {
	def getSingleton = PDAConstructionSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptAutomaton extends MappedText(this)
}

object PDAConstructionSolutionAttempt extends PDAConstructionSolutionAttempt with LongKeyedMetaMapper[PDAConstructionSolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : PDAConstructionSolutionAttempt = {
		return this.find(By(PDAConstructionSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a PDA construction attempt"
	}
}

class PDAWordProblemSolutionAttempt extends LongKeyedMapper[PDAWordProblemSolutionAttempt] with IdPK {
	def getSingleton = PDAWordProblemSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptWordsInLanguage extends MappedText(this)
	object attemptWordsNotInLanguage extends MappedText(this)
}

object PDAWordProblemSolutionAttempt extends PDAWordProblemSolutionAttempt with LongKeyedMetaMapper[PDAWordProblemSolutionAttempt] {

}



/***************
 * NFA/DFA/RE Problems
 */
class DFAConstructionSolutionAttempt extends LongKeyedMapper[DFAConstructionSolutionAttempt] with IdPK {
	def getSingleton = DFAConstructionSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptAutomaton extends MappedText(this)
}

object DFAConstructionSolutionAttempt extends DFAConstructionSolutionAttempt with LongKeyedMetaMapper[DFAConstructionSolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : DFAConstructionSolutionAttempt = {
		return this.find(By(DFAConstructionSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a DFA construction attempt"
	}
}
 
class NFAConstructionSolutionAttempt extends LongKeyedMapper[NFAConstructionSolutionAttempt] with IdPK {
	def getSingleton = NFAConstructionSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptAutomaton extends MappedText(this)
}

object NFAConstructionSolutionAttempt extends NFAConstructionSolutionAttempt with LongKeyedMetaMapper[NFAConstructionSolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : NFAConstructionSolutionAttempt = {
		return this.find(By(NFAConstructionSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a NFA construction attempt"
	}
}

class NFAToDFASolutionAttempt extends LongKeyedMapper[NFAToDFASolutionAttempt] with IdPK {
	def getSingleton = NFAToDFASolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptAutomaton extends MappedText(this)
}

object NFAToDFASolutionAttempt extends NFAToDFASolutionAttempt with LongKeyedMetaMapper[NFAToDFASolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : NFAToDFASolutionAttempt = {
		return this.find(By(NFAToDFASolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a NFA to DFA construction attempt"
	}
}

class RegexConstructionSolutionAttempt extends LongKeyedMapper[RegexConstructionSolutionAttempt] with IdPK {
	def getSingleton = RegexConstructionSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptRegex extends MappedText(this)
}

object RegexConstructionSolutionAttempt extends RegexConstructionSolutionAttempt with LongKeyedMetaMapper[RegexConstructionSolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : RegexConstructionSolutionAttempt = {
		return this.find(By(RegexConstructionSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a RegEx construction attempt"
	}
}

class WordsInRegexSolutionAttempt extends LongKeyedMapper[WordsInRegexSolutionAttempt] with IdPK {
	override def getSingleton = WordsInRegexSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptWordsIn extends MappedText(this)
	object attemptWordsOut extends MappedText(this)
}

object WordsInRegexSolutionAttempt extends WordsInRegexSolutionAttempt with LongKeyedMetaMapper[WordsInRegexSolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt: SolutionAttempt) : WordsInRegexSolutionAttempt = {
		this.find(By(WordsInRegexSolutionAttempt.solutionAttemptId, generalAttempt)).openOrThrowException("Must only be called if we are sure that the general attempt also has a RegEx construction attempt")
	}
}

class RegExToNFASolutionAttempt extends LongKeyedMapper[RegExToNFASolutionAttempt] with IdPK {
	def getSingleton = RegExToNFASolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptAutomaton extends MappedText(this)
}

object RegExToNFASolutionAttempt extends RegExToNFASolutionAttempt with LongKeyedMetaMapper[RegExToNFASolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : RegExToNFASolutionAttempt = {
		return this.find(By(RegExToNFASolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a RegEx to NFA attempt"
	}
}


/***************
	* Equivalence Classes
	*/
class EquivalenceClassesSolutionAttempt extends LongKeyedMapper[EquivalenceClassesSolutionAttempt] with IdPK {
	def getSingleton = EquivalenceClassesSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	// Some fields may remain empty, depending on the problem type
	object attemptWordsIn extends MappedText(this)
	object areEquivalent extends MappedInt(this)
	object reason extends  MappedText(this)
	object representative extends MappedText(this)
}

object EquivalenceClassesSolutionAttempt extends EquivalenceClassesSolutionAttempt with LongKeyedMetaMapper[EquivalenceClassesSolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : EquivalenceClassesSolutionAttempt = {
		return this.find(By(EquivalenceClassesSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a Equivalence class attempt"
	}
}


/***************
	* Pumping lemma game
	*/
class PumpingLemmaGameSolutionAttempt extends LongKeyedMapper[PumpingLemmaGameSolutionAttempt] with IdPK {
	def getSingleton = PumpingLemmaGameSolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object choiceRegular extends MappedText(this)
	object choiceN extends MappedText(this)
	object choiceWord extends MappedText(this)
	object choiceSplit extends MappedText(this)
	object choiceI extends MappedText(this)
	object win extends MappedText(this)
	object userId extends MappedLongForeignKey(this, User)
	object problemId extends MappedLongForeignKey(this, Problem)
	object dateTime extends MappedDateTime(this)
}

object PumpingLemmaGameSolutionAttempt extends PumpingLemmaGameSolutionAttempt with LongKeyedMetaMapper[PumpingLemmaGameSolutionAttempt] {
	def getByGeneralAttempt(generalAttempt: SolutionAttempt): PumpingLemmaGameSolutionAttempt = {
		return this.find(By(PumpingLemmaGameSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a PumpingLemmaGame attempt"
	}
}



/***************
	* TM problems
	*/
class WhileToTMSolutionAttempt extends LongKeyedMapper[WhileToTMSolutionAttempt] with IdPK {
    def getSingleton = WhileToTMSolutionAttempt

    object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
    object attemptTM extends MappedText(this)
}

object WhileToTMSolutionAttempt extends WhileToTMSolutionAttempt with LongKeyedMetaMapper[WhileToTMSolutionAttempt] {
    def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : WhileToTMSolutionAttempt = {
        return this.find(By(WhileToTMSolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a WhileToTM attempt"
    }
}
