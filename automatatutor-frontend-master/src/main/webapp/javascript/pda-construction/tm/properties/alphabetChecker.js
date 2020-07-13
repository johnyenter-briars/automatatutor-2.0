'use strict';

import {checkIfSymbolCanBeGeneralAlphabetSymbol, checkGeneralAlphabet} from '../../automaton/properties/generic-alphabet/alphabetChecker';
import specialCharacters from '../specialCharacters';
import {getKeySymbols} from '../transition';

const checkAlphabet = alphabet => {
    const checkResult = checkGeneralAlphabet(alphabet, 'alphabet', checkIfSymbolCanBeAlphabetSymbol);
    if (alphabet.includes(specialCharacters.blankTapeEdit)) {
        checkResult.addError(`It must not contain ${specialCharacters.blankTapeEdit}, as this is the symbol representing the blank tape symbol.`);
    }
    return checkResult;
};

const checkIfSymbolCanBeAlphabetSymbol = symbol => checkIfSymbolCanBeGeneralAlphabetSymbol(symbol,  Object.values(getKeySymbols()));

export default {checkAlphabet};
export {checkIfSymbolCanBeAlphabetSymbol};