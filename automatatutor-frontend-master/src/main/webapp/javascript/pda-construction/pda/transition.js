'use strict';

import {checkIfSymbolCanBeAlphabetSymbol, checkIfSymbolCanBeStackAlphabetSymbol} from "./properties/alphabetChecker";
import {AbstractTransition} from "../automaton/links/link/transitionGroup/transition";
import specialCharacters from "./specialCharacters";
import xmlExporter from "../utils/xmlExporter";

const getKeySymbols = () => ({readSeparator: ',', writeSeparator: '/'});
const keySymbols = getKeySymbols();

const XML_TRANSITION_ID = 'transition';

const transitionRegex = (() => {
    const anySymbol = '.';
    const startSymbol = '^';
    const endSymbol = '$';
    const anyNumber = '*';
    const pattern =
        startSymbol
        + anySymbol
        + keySymbols.readSeparator
        + anySymbol
        + keySymbols.writeSeparator
        + anySymbol + anyNumber
        + endSymbol;
    return new RegExp(pattern);
})();

/**
 * single transition from a {State} to another
 * @type {Transition}
 */
const Transition = class extends AbstractTransition {
    /**
     * @param transitionString must have the structure that the toString()-method defines
     * @param properties {Properties} of the {PDA}
     */
    constructor(transitionString, properties) {
        super();
        this._isValidConcerningDeterminism = true;
        this.inputLetter = Transition._getInputSymbol(transitionString);
        this.inputStackSymbol = Transition._getInputStackSymbol(transitionString);
        this.outputSymbols = Transition._getOutputSymbols(transitionString);
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

    _updateView() {
        if (this._view) {
            const classes = (this._isValidConcerningAlphabets ? 'valid-symbols' : 'invalid-symbols')
                + (this._isValidConcerningDeterminism ? ' valid' : ' invalid');
            this._view.setCssClasses(classes);
        }
    }

    /**
     * @param transitionString
     * @return {*}
     */
    static representsTransition(transitionString) {
        if (transitionRegex.test(transitionString)) {
            const inputSymbolIsAdmissible = checkIfSymbolCanBeAlphabetSymbol(Transition._getInputSymbol(transitionString)).isCorrect;
            const inputStackSymbolIsAdmissible = checkIfSymbolCanBeStackAlphabetSymbol(Transition._getInputStackSymbol(transitionString)).isCorrect;
            const outputStackSymbolsAreAdmissible = Transition._getOutputSymbols(transitionString).split('').every(stackSymbol => checkIfSymbolCanBeStackAlphabetSymbol(stackSymbol).isCorrect);
            return inputSymbolIsAdmissible && inputStackSymbolIsAdmissible && outputStackSymbolsAreAdmissible;
        }
        return false;
    }

    isValid() {
        return this._isValidConcerningDeterminism && this._isValidConcerningAlphabets;
    }

    /**
     * checks whether this Transition equals another object, that has to have the three attributes
     * {inputLetter}, {inputStackSymbol}, {outputSymbols}
     * @param transitionLike
     * @return {boolean} if this transition equals the given object
     */
    equals(transitionLike) {
        return this.inputLetter === transitionLike.inputLetter
            && this.inputStackSymbol === transitionLike.inputStackSymbol
            && this.outputSymbols === transitionLike.outputSymbols;
    }

    exportToXml() {
        return xmlExporter.buildXmlString().addElement(XML_TRANSITION_ID, this.toString())
            .build()
            .build();
    }

    displayingString() {
        return Transition._format(this._getDisplayingStringOfInputLetter(), this.inputStackSymbol, this.outputSymbols);
    }

    toString() {
        return Transition._format(this.inputLetter, this.inputStackSymbol, this.outputSymbols);
    }

    static _getInputSymbol(transitionString) {
        return transitionString.charAt(0);
    }

    static _getInputStackSymbol(transitionString) {
        return transitionString.charAt(2);
    }

    static _getOutputSymbols(transitionString) {
        return transitionString.substring(4);
    }

    updateValidity() {
        this._isValidConcerningAlphabets = this._properties.getProperty('alphabet').alphabet.includes(this.inputLetter)
            && this._properties.getProperty('stackAlphabet').stackAlphabet.includes(this.inputStackSymbol)
            && this.outputSymbols.split('').every(stackSymbol => this._properties.getProperty('stackAlphabet')
                .stackAlphabet.includes(stackSymbol));
        this._updateView();
    }


    static createFromXml(xmlElement, properties) {
        if (xmlElement.tagName === XML_TRANSITION_ID) {
            return new Transition(xmlElement.firstChild.nodeValue, properties);
        }
        throw `the tag-name ${xmlElement.tagName} was not ${XML_TRANSITION_ID} as required`;
    }

    _getDisplayingStringOfInputLetter() {
        return this.inputLetter === specialCharacters.epsilonEdit ? specialCharacters.epsilonDisplay : this.inputLetter;
    }


    static _format(readSymbol, readStackSymbol, writtenStackSymbols) {
        return readSymbol + keySymbols.readSeparator + readStackSymbol + keySymbols.writeSeparator + writtenStackSymbols;
    }

    getInputPart() {
        return `${this.inputLetter},${this.inputStackSymbol}`;
    }
};

export {getKeySymbols, Transition};