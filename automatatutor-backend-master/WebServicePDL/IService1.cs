using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

using System.ServiceModel;

namespace WebServicePDL
{
    [ServiceContract(Namespace = "http://automatagrader.com/")]
    public interface IService1
    {
        [OperationContract(Name = "CheckArithLanguageDescription")]
        XElement CheckArithLanguageDescription(XElement languageDesc, XElement constraintDesc, XElement alphabet, XElement pumpingString);
        [OperationContract(Name = "CheckEquivalentRegexp")]
        XElement CheckEquivalentRegexp(XElement correctRegex, XElement equivRegex, XElement alphabet);
        [OperationContract(Name = "CheckGrammar")]
        XElement CheckGrammar(XElement grammar);
        [OperationContract(Name = "CheckRegexp")]
        XElement CheckRegexp(XElement regexDesc, XElement alphabet);
        [OperationContract(Name = "CheckWhileFormat")]
        XElement CheckWhileFormat(XElement program);
        [OperationContract(Name = "ComputeCYKFeedback")]
        XElement ComputeCYKFeedback(XElement grammar, XElement word, XElement attempt, XElement maxGrade);
        [OperationContract(Name = "ComputeEquivalencyClassTwoWordsFeedback")]
        XElement ComputeEquivalencyClassTwoWordsFeedback(XElement regex, XElement alphabet, XElement firstWord, XElement secondWord);
        [OperationContract(Name = "ComputeEquivalentShortestFeedback")]
        XElement ComputeEquivalentShortestFeedback(XElement regex, XElement alphabet, XElement representative, XElement shortest, XElement maxGrade);
        [OperationContract(Name = "ComputeEquivalentWordsFeedback")]
        XElement ComputeEquivalentWordsFeedback(XElement regex, XElement alphabet, XElement representative, XElement wordsIn, XElement maxGrade);
        [OperationContract(Name = "ComputeFeedbackDynamicRegexpEditDistance")]
        XElement ComputeFeedbackDynamicRegexpEditDistance(XElement regexCorrectDesc, XElement equivRegex, XElement regexAttemptDesc, XElement alphabet, XElement maxGrade);
        [OperationContract(Name = "ComputeFeedbackMinimization")]
        XElement ComputeFeedbackMinimization(XElement dfaDesc, XElement minimizationTableAttempt, XElement dfaAttemptDesc, XElement maxGrade, XElement feedbackLevel, XElement enableFeedbacks);
        [OperationContract(Name = "ComputeFeedbackNfaToDfa")]
        XElement ComputeFeedbackNfaToDfa(XElement nfaCorrectDesc, XElement dfaAttemptDesc, XElement maxGrade);
        [OperationContract(Name = "ComputeFeedbackNfaToDfaOld")]
        XElement ComputeFeedbackNfaToDfaOld(XElement nfaCorrectDesc, XElement dfaAttemptDesc, XElement maxGrade);
        [OperationContract(Name = "ComputeFeedbackNFAXML")]
        XElement ComputeFeedbackNFAXML(XElement nfaCorrectDesc, XElement nfaAttemptDesc, XElement maxGrade, XElement feedbackLevel, XElement enabledFeedbacks, XElement userId);
        [OperationContract(Name = "ComputeFeedbackPDAConstruction")]
        XElement ComputeFeedbackPDAConstruction(XElement xmlPdaCorrect, XElement xmlPdaAttempt, XElement xmlGiveStackAlphabet, XElement xmlMaxGrade);
        [OperationContract(Name = "ComputeFeedbackPDAWordProblem")]
        XElement ComputeFeedbackPDAWordProblem(XElement xmlPda, XElement xmlWordsInLanguage, XElement xmlWordsNotInLanguage, XElement xmlMaxGrade);
        [OperationContract(Name = "ComputeFeedbackProductConstruction")]
        XElement ComputeFeedbackProductConstruction(XElement dfaDescList, XElement dfaAttemptDesc, XElement booleanOperation, XElement maxGrade, XElement feedbackLevel, XElement enabledFeedbacks);
        [OperationContract(Name = "ComputeFeedbackRegexp")]
        XElement ComputeFeedbackRegexp(XElement regexCorrectDesc, XElement regexAttemptDesc, XElement alphabet, XElement feedbackLevel, XElement enabledFeedbacks, XElement maxGrade);
        [OperationContract(Name = "ComputeFeedbackRegexToNfa")]
        XElement ComputeFeedbackRegexToNfa(XElement regex, XElement alphabet, XElement attemptNfa, XElement maxGrade);
        [OperationContract(Name = "ComputeFeedbackWhileToTM")]
        XElement ComputeFeedbackWhileToTM(XElement correctProgram, XElement attemptTM, XElement maxGrade);
        [OperationContract(Name = "ComputeFeedbackXML")]
        XElement ComputeFeedbackXML(XElement dfaCorrectDesc, XElement dfaAttemptDesc, XElement maxGrade, XElement feedbackLevel, XElement enabledFeedbacks);
        [OperationContract(Name = "ComputeFindDerivationFeedback")]
        XElement ComputeFindDerivationFeedback(XElement grammar, XElement word, XElement derivation, XElement maxGrade, XElement derivationType);
        [OperationContract(Name = "ComputeGrammarEqualityFeedback")]
        XElement ComputeGrammarEqualityFeedback(XElement solution, XElement attempt, XElement maxGrade, XElement checkEmptyWord);
        [OperationContract(Name = "ComputeSameEquivalencyClassFeedback")]
        XElement ComputeSameEquivalencyClassFeedback(XElement regex, XElement alphabet, XElement firstWord, XElement secondWord, XElement notEquivalent, XElement reason, XElement maxGrade);
        [OperationContract(Name = "ComputeWordsInGrammarFeedback")]
        XElement ComputeWordsInGrammarFeedback(XElement grammar, XElement wordsIn, XElement wordsOut, XElement maxGrade);
        [OperationContract(Name = "ComputeWordsInRegexpFeedback")]
        XElement ComputeWordsInRegexpFeedback(XElement regEx, XElement wordsIn, XElement wordsOut, XElement maxGrade);
        [OperationContract(Name = "GenerateProblem")]
        XElement GenerateProblem(XElement type, XElement minQual);
        [OperationContract(Name = "GenerateProblemBestIn")]
        XElement GenerateProblemBestIn(XElement type, XElement minDiff, XElement maxDiff);
        [OperationContract(Name = "GenerateProblemHardest")]
        XElement GenerateProblemHardest(XElement type, XElement minQual);
        [OperationContract(Name = "GenerateStringSplits")]
        XElement GenerateStringSplits(XElement languageDesc, XElement constraintDesc, XElement alphabet, XElement pumpingString);
        [OperationContract(Name = "GetPumpingLemmaFeedback")]
        XElement GetPumpingLemmaFeedback(XElement languageDesc, XElement constraintDesc, XElement alphabet, XElement pumpingString, XElement pumpingNumbers);
        [OperationContract(Name = "isCNF")]
        XElement isCNF(XElement grammar);
        [OperationContract(Name = "PLGNfaToDfa")]
        XElement PLGNfaToDfa(XElement automaton);
        [OperationContract(Name = "PLGNonRegularCheckI")]
        XElement PLGNonRegularCheckI(XElement alphabet, XElement symbolicString, XElement constraints, XElement start, XElement mid, XElement end, XElement i);
        [OperationContract(Name = "PLGNonRegularCheckValidity")]
        XElement PLGNonRegularCheckValidity(XElement alphabet, XElement symbolicString, XElement constraints, XElement unpumpableWord);
        [OperationContract(Name = "PLGNonRegularCheckWordGetSplit")]
        XElement PLGNonRegularCheckWordGetSplit(XElement alphabet, XElement symbolicString, XElement constraints, XElement n, XElement word);
        [OperationContract(Name = "PLGNonRegularGetI")]
        XElement PLGNonRegularGetI(XElement alphabet, XElement symbolicString, XElement constraints, XElement start, XElement mid, XElement end);
        [OperationContract(Name = "PLGNonRegularGetN")]
        XElement PLGNonRegularGetN(XElement max);
        [OperationContract(Name = "PLGNonRegularGetWord")]
        XElement PLGNonRegularGetWord(XElement alphabet, XElement symbolicString, XElement constraints, XElement n, XElement unpumpableWord);
        [OperationContract(Name = "PLGRegularCheckI")]
        XElement PLGRegularCheckI(XElement automaton, XElement start, XElement mid, XElement end, XElement i);
        [OperationContract(Name = "PLGRegularCheckWordGetSplit")]
        XElement PLGRegularCheckWordGetSplit(XElement automaton, XElement n, XElement word);
        [OperationContract(Name = "PLGRegularGetDFAFromSymbolicString")]
        XElement PLGRegularGetDFAFromSymbolicString(XElement alphabet, XElement symbolicString, XElement constraints);
        [OperationContract(Name = "PLGRegularGetI")]
        XElement PLGRegularGetI(XElement automaton, XElement start, XElement mid, XElement end);
        [OperationContract(Name = "PLGRegularGetN")]
        XElement PLGRegularGetN(XElement automaton);
        [OperationContract(Name = "PLGRegularGetWord")]
        XElement PLGRegularGetWord(XElement automaton, XElement n);
        [OperationContract(Name = "SimulateWordInPDA")]
        XElement SimulateWordInPDA(XElement xmlPda, XElement xmlWord);
    }
}