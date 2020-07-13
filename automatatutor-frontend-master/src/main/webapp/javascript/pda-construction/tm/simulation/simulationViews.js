'use strict';

import * as d3 from "d3";

const fieldSize = 40;
const fieldPadding = 15;
const fieldNumbersFontSize = 11; //Hint: this should match the font-size for css-class "tape-numbers" in tmStyles.css

const FieldView = class {
    constructor(svgContainer, number) {
        this._svgElement = d3.select(svgContainer).append('g')
            .attr('transform', `translate(${number * (fieldSize + fieldPadding)},0)`).node();

        d3.select(this._svgElement).append('rect').attr('x', 1).attr('y', 1)
            .attr('width', fieldSize).attr('height', fieldSize).attr('class', 'tape-field');
        d3.select(this._svgElement).append('text').attr('class', 'tape-numbers') //.attr('x', fieldSize/2)
            .attr('y', -fieldNumbersFontSize/2).text(number);
        d3.select(this._svgElement).append('text').attr('class', 'tape-content')
            .attr('transform', `translate(${fieldSize / 2}, ${fieldSize / 2})`);
    }

    set symbol(value) {
        d3.select(this._svgElement).select('text.tape-content').text(value);
    }

    unMark() {
        d3.select(this._svgElement).select('rect').node().classList.remove('marked');
    }

    mark() {
        d3.select(this._svgElement).select('rect').node().classList.add('marked');
    }

    remove() {
        d3.select(this._svgElement).remove();
    }
};

const TapeView = class {
    constructor(svgContainer, width, number) {
        this._svgElement = d3.select(svgContainer).append('g').node();
        this._translateX = 0;
        this._translateY = number * (fieldSize + fieldPadding) + fieldPadding;
        this._updateTranslation();
    }

    get svgElement() {
        return this._svgElement;
    }

    _updateTranslation() {
        d3.select(this._svgElement).attr('transform', `translate(${this._translateX}, ${this._translateY})`)
    }

    shiftVisibleRangeToLeftBy(numberOfFields) {
        this._translateX += numberOfFields * (fieldSize + fieldPadding);
        this._updateTranslation();
    }

    shiftVisibleRangeToRightBy(numberOfFields) {
        this._translateX -= numberOfFields * (fieldSize + fieldPadding);
        this._updateTranslation();
    }

    resetVisibleRange() {
        this._translateX = 0;
        this._updateTranslation();
    }
};

const TapesView = class {
    constructor(htmlContainer, numberOfTapes, width) {
        this._svgElement = d3.select(htmlContainer).append('div').attr('id', 'tapeContainer')
            .append('svg')
            .attr('width', width)
            .attr('height', numberOfTapes * (fieldSize + fieldPadding) + 2 * fieldPadding)
            .append('g').node();
        this._numberOfVisibleFields = width / (fieldSize + fieldPadding);
    }

    get svgElement() {
        return this._svgElement;
    }

    get numberOfVisibleFields() {
        return this._numberOfVisibleFields;
    }
};

export {FieldView, TapeView, TapesView};