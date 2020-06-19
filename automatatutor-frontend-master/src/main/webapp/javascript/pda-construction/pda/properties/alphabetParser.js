'use strict';

import alphabetChecker from './alphabetChecker';
import specialCharacters from '../specialCharacters';

/**
 * checks the given alphabet, copies it into a new array and adds the input epsilon-symbol
 * @param alphabet
 * @return a copy of the given alphabet with added input epsilon-symbol
 */
const parseAlphabet = alphabet => {
    let checkResult = alphabetChecker.checkAlphabet(alphabet);
    if (checkResult.isCorrect) {
        const alphabetCopy = alphabet.slice();
        alphabetCopy.push(specialCharacters.epsilonEdit);
        return alphabetCopy;
    }
    else {
        throw checkResult.messages;
    }
};

/**
 * checks the given stack-alphabet
 * @param stackAlphabet
 * @return a copy of the given stack-alphabet
 */
const parseStackAlphabet = stackAlphabet => {
    let checkResult = alphabetChecker.checkStackAlphabet(stackAlphabet);
    if (checkResult.isCorrect) {
        return stackAlphabet.slice();
    }
    else {
        throw checkResult.messages;
    }
};

export default {parseAlphabet, parseStackAlphabet};