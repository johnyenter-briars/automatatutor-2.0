'use strict';

const CheckResult = class {
    constructor() {
        this._isCorrect = true;
        this._messages = [];
    }

    get isCorrect() {
        return this._isCorrect;
    }

    addError(message) {
        this._isCorrect = false;
        this._messages.push(message);
    }
};

const SymbolCheckResult = class extends CheckResult {
    constructor() {
        super();
    }
};

const AlphabetCheckResult = class extends CheckResult {
    constructor(symbolCheckResults, alphabetTitle) {
        super();
        this._alphabetTitle = alphabetTitle;
        this._isCorrect = symbolCheckResults.every(checkResult => checkResult.isCorrect);
        this._messages = [].concat.apply([], symbolCheckResults.map(checkResult => checkResult._messages));
    }

    get isCorrect() {
        return this._isCorrect;
    }

    get message() {
        return this._alphabetTitle + ': ' + this._messages.join(' ');
    }
};

const checkGeneralAlphabet = (alphabet, alphabetTitle, checkSymbol) => {
    const symbolCheckResults = alphabet.map(symbol => checkSymbol(symbol));
    const checkResult = new AlphabetCheckResult(symbolCheckResults, alphabetTitle);
    if (Array.from(new Set(alphabet).values()).length < alphabet.length) {
        checkResult.addError(`It contains at least one symbol twice.`);
    }
    return checkResult;
};

const checkIfSymbolCanBeGeneralAlphabetSymbol = (symbol, keySymbols) => {
    const checkResult = new SymbolCheckResult();
    const correctLength = symbol.length === 1;
    if (!correctLength) {
        checkResult.addError(`The symbol ${symbol} is a string, but only characters are allowed.`);
    }
    keySymbols.forEach(keySymbol => {
        if (symbol === keySymbol) {
            checkResult.addError(`The symbol ${symbol} is not allowed, as it is a key-symbol in a transition`);
        }
    });
    return checkResult;
};

export {checkGeneralAlphabet, checkIfSymbolCanBeGeneralAlphabetSymbol};