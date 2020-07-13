'use strict';

import * as d3 from "d3";
import specialCharacters from "../specialCharacters";

const tapeHeight = 50;
const fieldSize = 40;
const fieldPadding = 10;
const numberOfVisibleFields = 13;

const TapeView = class {
    constructor(svgContainer, width, number) {
        this._svgElement = d3.select(svgContainer).append('g').attr('transform', `translate(0,${number * (tapeHeight + fieldPadding)})`).node();
        this._groups = [];
        this._min = 0;
        this._max = 0;
        for (let i = this._min; i < numberOfVisibleFields; i++) {
            this._addField(i);
        }
    }

    _addField(i) {
        const group = d3.select(this._svgElement).append('g').attr('transform', `translate(${i * (fieldSize + fieldPadding)},0)`);
        this._groups.push(group);
        group.append('rect')
            .attr('x', 1).attr('y', 1)
            .attr('width', fieldSize).attr('height', fieldSize).attr('class', 'tape-field');
        group.append('text').attr('class', 'tape-content')
            .attr('transform', `translate(${fieldSize / 2}, ${fieldSize / 2})`)
            .text(specialCharacters.blankTapeDisplay);

        if (i < this._min) {
            this._min = i;
        }

        if (i > this._max) {
            this._max = i;
        }
    }

    expandTo(numberOfField) {
        for (let i = this._max + 1; i < numberOfField; i++) {
            this._addField(i);
        }
    }

    mark(index) {
        d3.select(this._groups[index]).select('rect').node().classList.add('marked');
    }

    unmark(index) {
        d3.select(this._groups[index]).select('rect').node().classList.remove('marked');
    }

    setWord(word) {
        this._word = word;
        if (word.length > numberOfVisibleFields) {
            throw 'this should not occur';
        }
        [...word].forEach((symbol, i) => this._groups[i].select('text').text(symbol));
    }

    resetWord() {
        this._groups.forEach(group => group.select('text').text(specialCharacters.blankTapeDisplay));
    }

    getCurrentSymbol(currentIndex) {
        return d3.select(this._groups[currentIndex]).select('text').text();
    }
};

const TapesView = class {
    constructor(htmlContainer, numberOfTapes, width) {
        this._svgElement = d3.select(htmlContainer).append('div').attr('id', 'tapeContainer')
            .append('svg').attr('width', width).attr('height', (tapeHeight + fieldPadding) * numberOfTapes + 2 * fieldPadding)
            .append('g').node();
        this._tapes = [];
        for (let i = 0; i < numberOfTapes; i++) {
            this._tapes.push(new TapeView(this._svgElement, width, i));
        }
        this._currentIndex = 0;
        this._minimumVisibleIndex = 0;
        this._maximumVisibleIndex = numberOfVisibleFields - 1;

        this.translateX = 0;
        this._currentTapeLength = numberOfVisibleFields;
    }

    get currentSymbols() {
        return this._tapes.map(tape => tape.getCurrentSymbol(this._currentIndex));
    }

    setWords(words) {
        const maximumWordLength = words.reduce((maxSoFar, word) => word.length > maxSoFar ? word.length : maxSoFar, 0);
        if (maximumWordLength > this._currentTapeLength) {
            this._tapes.forEach(tape => tape.expandTo(maximumWordLength));
            this._currentTapeLength = maximumWordLength;
        }
        this._tapes.forEach((tape, i) => tape.setWord(words[i]));

        this.translateX = 0;
        this._currentIndex = 0;
        this._minimumVisibleIndex = 0;
        this._maximumVisibleIndex = numberOfVisibleFields - 1;
    }

    set translateX(value) {
        this._translateX = value;
        d3.select(this._svgElement).attr('transform', `translate(${value},0)`);
    }

    move(direction) {
        this._tapes.forEach(tape => tape.unmark(this._currentIndex));
        if (direction === 'R') {
            if (this._currentIndex >= this._maximumVisibleIndex - 1) {
                this.translateX = this._translateX - (fieldSize + fieldPadding);
                this._maximumVisibleIndex += 1;
                this._minimumVisibleIndex -= 1;
            }
            this._currentIndex += 1;
        }
        else if (direction === 'L') {
            if (this._currentIndex <= this._minimumVisibleIndex + 1) {
                this.translateX = this._translateX + (fieldSize + fieldPadding);
                this._maximumVisibleIndex -= 1;
                this._minimumVisibleIndex += 1;
            }
            this._currentIndex -= 1;
        }
        this._tapes.forEach(tape => tape.mark(this._currentIndex));
    }

    resetWords() {
        this._tapes.forEach(tape => tape.resetWord());
        this._tapes.forEach(tape => tape.unmark(this._currentIndex));
    }
};

export {TapesView};