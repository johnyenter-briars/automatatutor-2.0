'use strict';

import {checkIfSymbolCanBeAlphabetSymbol} from "./properties/alphabetChecker";
import {AbstractTransition} from "../automaton/links/link/transitionGroup/transition";
import specialCharacters from "./specialCharacters";
import xmlExporter from "../utils/xmlExporter";
import {parseBySeparator} from "../utils/textParser";

const getKeySymbols = () => ({writeSeparator: '/', moveSeparator: ','});
const keySymbols = getKeySymbols();

const moveSymbols = [specialCharacters.rightMove, specialCharacters.leftMove, specialCharacters.noMove];

const XML_TRANSITION_ID = 'transition';

const transitionRegex = (() => {
    const anySymbol = '.';
    const startSymbol = '^';
    const endSymbol = '$';
    const anyNumber = '*';
    const startOneOf = '[';
    const endOneOf = ']';
    const tapeTransitionPattern =
        anySymbol
        + keySymbols.writeSeparator
        + anySymbol
        + keySymbols.moveSeparator
        + startOneOf + moveSymbols.join('') + endOneOf;
    const pattern =
        startSymbol
        + tapeTransitionPattern
        + (specialCharacters.tapeTransitionSeparator + tapeTransitionPattern) + anyNumber
        + endSymbol;
    return new RegExp(pattern);
})();


const getDisplayingStringOfTapeLetter = tapeLetter =>
    tapeLetter === specialCharacters.blankTapeEdit ? specialCharacters.blankTapeDisplay : tapeLetter;

const TapeTransition = class {
    constructor(tapeTransitionString) {
        this.inputLetter = TapeTransition._getInputSymbol(tapeTransitionString);
        this.outputLetter = TapeTransition._getOutputSymbol(tapeTransitionString);
        this.moveSymbol = TapeTransition._getMoveSymbol(tapeTransitionString);
    }

    equals(tapeTransitionLike) {
        return this.inputLetter === tapeTransitionLike.inputLetter
            && this.outputLetter === tapeTransitionLike.outputLetter
            && this.moveSymbol === tapeTransitionLike.moveSymbol;
    }

    displayingString() {
        return TapeTransition._format(getDisplayingStringOfTapeLetter(this.inputLetter),
            getDisplayingStringOfTapeLetter(this.outputLetter), this.moveSymbol);
    }

    toString() {
        return TapeTransition._format(this.inputLetter, this.outputLetter, this.moveSymbol);
    }

    static _format(readSymbol, writeSymbol, moveSymbol) {
        return readSymbol + keySymbols.writeSeparator + writeSymbol + keySymbols.moveSeparator + moveSymbol;
    }

    static _getInputSymbol(transitionString) {
        return transitionString.charAt(0);
    }

    static _getOutputSymbol(transitionString) {
        return transitionString.charAt(2);
    }

    static _getMoveSymbol(transitionString) {
        return transitionString.charAt(4);
    }
};

/**
 * single transition from a {State} to another
 * @type {Transition}
 */
const Transition = class extends AbstractTransition {
    /**
     * @param transitionString must have the structure that the toString()-method defines
     * @param properties {Properties} of the {TuringMachine}
     */
    constructor(transitionString, properties) {
        super();
        this._isValidConcerningDeterminism = true;
        const tapeTransitionStrings = parseBySeparator(transitionString, specialCharacters.tapeTransitionSeparator);
        this._tapeTransitions = tapeTransitionStrings.map(tapeTransitionString => new TapeTransition(tapeTransitionString));
        this._properties = properties;
        this.updateValidity();
    }

    get isValidConcerningDeterminism() {
        return this._isValidConcerningDeterminism;
    }

    set isValidConcerningDeterminism(value) {
        this._isValidConcerningDeterminism = value;
        this._updateView();
    }

    getInputLetters() {
        return this._tapeTransitions.map(tapeTransition => tapeTransition.inputLetter);
    }

    getOutputLetters() {
        return this._tapeTransitions.map(tapeTransition => tapeTransition.outputLetter);
    }

    getMoveSymbols() {
        return this._tapeTransitions.map(tapeTransition => tapeTransition.moveSymbol);
    }

    makeClickAble(onClick) {
        this.mark();
        this._view.setOnClick(() => onClick(this));
    }

    makeUnClickAble() {
        this.unMark();
        this._view.removeOnClick();
    }

    _updateView() {
        if (this._view) {
            const classes = (this._isValidConcerningAlphabet ? 'valid-symbols' : 'invalid-symbols')
                + (this._isValidConcerningDeterminism ? ' valid' : ' invalid')
            + (this._isValidConcerningNumberOfTapes ? ' valid-number-tapes' : ' invalid-number-tapes');
            this._view.setCssClasses(classes);
        }
    }

    /**
     * @param transitionString
     * @return {*}
     */
    static representsTransition(transitionString) {
        if (transitionRegex.test(transitionString)) {
            const tapeTransitionStrings = parseBySeparator(transitionString, specialCharacters.tapeTransitionSeparator);
            const inputSymbolsAreAdmissible = tapeTransitionStrings.every(s => checkIfSymbolCanBeAlphabetSymbol(TapeTransition._getInputSymbol(s)).isCorrect);
            const outputSymbolsAreAdmissible = tapeTransitionStrings.every(s => checkIfSymbolCanBeAlphabetSymbol(TapeTransition._getOutputSymbol(s)).isCorrect);
            return inputSymbolsAreAdmissible && outputSymbolsAreAdmissible;
        }
        return false;
    }

    isValid() {
        return this._isValidConcerningAlphabet && this._isValidConcerningNumberOfTapes && this._isValidConcerningDeterminism;
    }

    /**
     * checks whether this Transition equals another object, that has to have the three attributes
     * {inputLetter}, {inputStackSymbol}, {outputSymbols}
     * @param transition
     * @return {boolean} if this transition equals the given object
     */
    equals(transition) {
        return this._tapeTransitions.every((tapeTransition, i) => tapeTransition.equals(transition._tapeTransitions[i]));
    }

    exportToXml() {
        return xmlExporter.buildXmlString().addElement(XML_TRANSITION_ID, this.toString())
            .build()
            .build();
    }

    displayingString() {
        return Transition._format(this._tapeTransitions.map(t => t.displayingString()));
    }

    toString() {
        return Transition._format(this._tapeTransitions.map(t => t.toString()));
    }

    updateValidity() {
        const alphabet = this._properties.getProperty('alphabet').alphabet;
        this._isValidConcerningAlphabet = this._tapeTransitions.every(t => alphabet.includes(t.inputLetter)
            && alphabet.includes(t.outputLetter) && moveSymbols.includes(t.moveSymbol));
        this._isValidConcerningNumberOfTapes =
            this._properties.getProperty('numberOfTapes').numberOfTapes === this._tapeTransitions.length;
        this._updateView();
    }

    static createFromXml(xmlElement, properties) {
        if (xmlElement.tagName === XML_TRANSITION_ID) {
            return new Transition(xmlElement.firstChild.nodeValue, properties);
        }
        throw `the tag-name ${xmlElement.tagName} was not ${XML_TRANSITION_ID} as required`;
    }

    static _format(tapeTransitionDisplayingStrings) {
        return tapeTransitionDisplayingStrings.join(specialCharacters.tapeTransitionSeparator);
    }
};

export {getKeySymbols, Transition};