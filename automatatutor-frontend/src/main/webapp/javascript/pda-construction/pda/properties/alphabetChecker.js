'use strict';

import {checkIfSymbolCanBeGeneralAlphabetSymbol, checkGeneralAlphabet} from '../../automaton/properties/generic-alphabet/alphabetChecker';
import specialCharacters from '../specialCharacters';
import {getKeySymbols} from '../transition';

const checkAlphabet = alphabet => {
    const checkResult = checkGeneralAlphabet(alphabet, 'alphabet', checkIfSymbolCanBeAlphabetSymbol);
    if (alphabet.includes(specialCharacters.epsilonEdit)) {
        checkResult.addError(`It must not contain ${specialCharacters.epsilonEdit}, as this is the symbol representing epsilon.`);
    }
    return checkResult;
};

const checkStackAlphabet = stackAlphabet => {
    const checkResult = checkGeneralAlphabet(stackAlphabet, 'stack-alphabet', checkIfSymbolCanBeStackAlphabetSymbol);
    if (stackAlphabet.length === 0) {
        checkResult.addError('It has to have at least one element.');
    }
    return checkResult;
};

const checkIfSymbolCanBeStackAlphabetSymbol = symbol => checkIfSymbolCanBeGeneralAlphabetSymbol(symbol, Object.values(getKeySymbols()));
const checkIfSymbolCanBeAlphabetSymbol = symbol => checkIfSymbolCanBeGeneralAlphabetSymbol(symbol, Object.values(getKeySymbols()));

export default {checkAlphabet, checkStackAlphabet};
export {checkIfSymbolCanBeAlphabetSymbol, checkIfSymbolCanBeStackAlphabetSymbol};