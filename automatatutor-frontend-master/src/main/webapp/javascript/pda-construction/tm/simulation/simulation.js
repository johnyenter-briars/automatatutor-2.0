'use strict';

import SimulationControl from "./simulationControl";
import {simulationControlListenerInterface} from "./simulationControlListener";
import {Listener} from "../../utils/listener";
import specialCharacters from "../specialCharacters";
import {TapesView, TapeView, FieldView} from "./simulationViews";

const numberOfPaddingFields = 1;
const minimumNumberOfVisibleFields = 12;

const Field = class {
    constructor(fieldNumber, svgContainer) {
        this._view = new FieldView(svgContainer, fieldNumber);
        this.value = specialCharacters.blankTapeEdit;
    }

    set value(value) {
        this._value = value;
        this._view.symbol = this._value === specialCharacters.blankTapeEdit ? specialCharacters.blankTapeDisplay : this._value;
    }

    get value() {
        return this._value;
    }

    unMark() {
        this._view.unMark();
    }

    mark() {
        this._view.mark();
    }

    remove() {
        this._view.remove();
    }
};

const Tape = class {
    constructor(width, tapeNumber, svgContainer) {
        this._view = new TapeView(svgContainer, width, tapeNumber);
        this._fields = [];
    }

    recreateFields(numberOfFields) {
        this._fields.forEach(field => field.remove());
        this._view.resetVisibleRange();
        this._fields = [];
        for (let i = 0; i < numberOfFields; i++) {
            this._fields.push(new Field(i, this._view.svgElement))
        }

        this._currentField = 0;
        this._minimumVisibleIndex = 0;
        this._maximumVisibleIndex = numberOfFields - 1;
        this._minimumIndex = 0;
        this._maximumIndex = this._maximumVisibleIndex;
    }

    get _absoluteCurrentIndex() {
        return this._currentField - this._minimumIndex;
    }

    get currentLetter() {
        return this._fields[this._absoluteCurrentIndex].value;
    }

    write(letter) {
        this._fields[this._absoluteCurrentIndex].value = letter;
    }

    _unMarkCurrentField() {
        this._fields[this._absoluteCurrentIndex].unMark();
    }

    _markCurrentField() {
        this._fields[this._absoluteCurrentIndex].mark();
    }

    _addFieldAtStart() {
        this._fields.unshift(new Field(this._minimumIndex, this._view.svgElement));
    }

    _addFieldAtEnd() {
        this._fields.push(new Field(this._maximumIndex, this._view.svgElement));
    }

    _decrementCurrentIndex() {
        if (this._currentField <= this._minimumIndex + numberOfPaddingFields) {
            this._minimumIndex -= 1;
            this._addFieldAtStart();
        }

        if (this._currentField <= this._minimumVisibleIndex + numberOfPaddingFields) {
            this._minimumVisibleIndex -= 1;
            this._maximumVisibleIndex -= 1;
            this._view.shiftVisibleRangeToLeftBy(1);
        }

        this._currentField -= 1;
    }

    _incrementCurrentIndex() {
        if (this._currentField >= this._maximumIndex - numberOfPaddingFields) {
            this._maximumIndex += 1;
            this._addFieldAtEnd();
        }

        if (this._currentField >= this._maximumVisibleIndex - numberOfPaddingFields) {
            this._minimumVisibleIndex += 1;
            this._maximumVisibleIndex += 1;
            this._view.shiftVisibleRangeToRightBy(1);
        }

        this._currentField += 1;
    }

    _fillWordWithEmptyFields(word) {
        let enrichedWord = word;
        for (let i = word.length; i < this._fields.length; i++) {
            enrichedWord += specialCharacters.blankTapeEdit;
        }
        return enrichedWord;
    }

    setWord(word) {
        if (word.length > this._fields.length) {
            throw 'the tape cannot take a longer word than the number of its fields';
        }

        const enrichedWord = this._fillWordWithEmptyFields(word);

        this._fields.forEach((field, i) => field.value = enrichedWord.charAt(i));
    }

    move(moveSymbol) {
        switch (moveSymbol) {
            case 'L': {
                this._unMarkCurrentField();
                this._decrementCurrentIndex();
                this._markCurrentField();
                break;
            }
            case 'R': {
                this._unMarkCurrentField();
                this._incrementCurrentIndex();
                this._markCurrentField();
                break;
            }
            case 'N': {
                break;
            }
            default:
                throw new Error('unknown move symbol: ' + moveSymbol);
        }
    }
};

const Tapes = class {
    constructor(width, numberOfTapes, htmlContainer, wordsToSimulateContainer) {
        this._view = new TapesView(htmlContainer, numberOfTapes, width);
        this._numberOfTapes = numberOfTapes;
        this._wordsToSimulateContainer = wordsToSimulateContainer;
        this._initializeTapes(width);
    }

    _initializeTapes(width) {
        this._tapes = [];
        for (let i = 0; i < this._numberOfTapes; i++) {
            this._tapes.push(new Tape(width, i, this._view.svgElement));
        }

        this._setWords(this._createEmptyWords());
    }

    get currentLetters() {
        return this._tapes.map(tape => tape.currentLetter);
    }

    _resetTapes(numberOfFields) {
        this._tapes.forEach(tape => tape.recreateFields(numberOfFields));
    }

    _setWords(words) {
        if (words.length !== this._numberOfTapes) {
            throw 'the number of words has to equal the number of tapes';
        }

        const maximumWordLength = words.reduce((maxSoFar, word) => word.length > maxSoFar ? word.length : maxSoFar, 0);
        this._resetTapes(Math.max(maximumWordLength, minimumNumberOfVisibleFields));
        this._tapes.forEach((tape, i) => tape.setWord(words[i]));
    }

    _createEmptyWords() {
        let words = [];
        for (let i = 0; i < this._numberOfTapes; i++) {
            words.push('');
        }
        return words;
    }

    reset() {
        //console.log(this._wordsToSimulateContainer.innerHTML);
        const wordsAsString = this._wordsToSimulateContainer.innerHTML;
        const words = wordsAsString ? wordsAsString.split(',') : this._createEmptyWords();
        this._setWords(words);
    }

    enterTransition(transition) {
        this._write(transition.getOutputLetters());
        this._move(transition.getMoveSymbols());
    }

    _write(letters) {
        this._tapes.forEach((tape, i) => tape.write(letters[i]));
    }

    _move(moveSymbols) {
        this._tapes.forEach((tape, i) => tape.move(moveSymbols[i]));
    }
};

const Simulation = class {
    constructor(htmlContainer, width, numberOfTapes, simulationControlListener, wordsToSimulateContainer) {
        this._tapes = new Tapes(width, numberOfTapes, htmlContainer, wordsToSimulateContainer);
        this._simulationControl = new SimulationControl(htmlContainer,
            this._createSimulationControlListener(), simulationControlListener);
    }

    _createSimulationControlListener() {
        const listener = new Listener(Simulation.name, simulationControlListenerInterface);
        listener.set(simulationControlListenerInterface.onResetTapesClicked, () => this.resetTapes());
        return listener;
    }

    get currentLetters() {
        return this._tapes.currentLetters;
    }

    resetTapes() {
        this._tapes.reset();
    }

    hideClickTransitionHint() {
        this._simulationControl.hideHint();
    }

    showClickTransitionHint() {
        this._simulationControl.showHint();
    }

    enterTransition(transition) {
        this._tapes.enterTransition(transition);
    }
};

export default Simulation;