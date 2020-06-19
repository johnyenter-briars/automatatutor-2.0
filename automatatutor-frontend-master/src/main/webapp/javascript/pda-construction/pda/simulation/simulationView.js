'use strict';

import * as d3 from "d3";

const stackWidth = 35;
const stackSymbolHeight = 20;

const wordPaddingRight = 15;
const paddingLeftRight = 5;
const paddingTopBottom = 20;
const minWordHeight = 20;
const titleHeight = 35;

const WordView = class {
    constructor(pdaSvgElement, dimensions) {
        this._wordContainer = d3.select(pdaSvgElement).append('g')
            .attr('transform', `translate(${dimensions.width},${dimensions.height})`).node();
        this._wordText = d3.select(this._wordContainer).append('text').attr('class', 'word').node();
        d3.select(this._wordText).append('tspan').text('Remaining word: ');
        this._firstLetter = d3.select(this._wordText).append('tspan').node();
        this._otherLetters = d3.select(this._wordText).append('tspan').node();
    }

    remove() {
        d3.select(this._wordContainer).remove();
    }

    changeWord(word) {
        d3.select(this._firstLetter).text(word.charAt(0));
        d3.select(this._otherLetters).text(word.substring(1));
        this._moveWordViewContainerToCorrectPosition();
        this._unMarkFirstLetter();
    }

    _moveWordViewContainerToCorrectPosition() {
        const box = this._wordText.getBBox();
        this._height = box.height;
        const width = box.width;
        d3.select(this._wordText).attr('transform', `translate(${-(width + wordPaddingRight)},${-paddingTopBottom})`);
    }

    get height() {
        return this._height;
    }

    markFirstLetter() {
        d3.select(this._firstLetter).attr('class', 'marked');
    }

    _unMarkFirstLetter() {
        d3.select(this._firstLetter).attr('class', '');
    }
};

const getPointSoThatElementInMiddle = (outerLength, innerLength) => (outerLength - innerLength) / 2;

const StackSymbolView = class {
    constructor(svgSimulationViewContainer, stackSymbol, index, isTitle = false) {
        const height = isTitle ? titleHeight : stackSymbolHeight;
        const width = isTitle ? getStackColumnWidth() : stackWidth;

        this._index = index;
        this._stackSymbolViewContainer = d3.select(svgSimulationViewContainer).append('g')
            .attr('transform', `translate(0, ${-index * height})`).node();
        d3.select(this._stackSymbolViewContainer).append('rect').attr('class', isTitle ? 'title' : '')
            .attr('width', width)
            .attr('height', height)
            .attr('y', -height);
        const text = d3.select(this._stackSymbolViewContainer).append('text').text(stackSymbol).node();
        const box = text.getBBox();
        d3.select(text)
            .attr('x', getPointSoThatElementInMiddle(width, box.width))
            .attr('y', -getPointSoThatElementInMiddle(height, box.height));
    }

    remove() {
        d3.select(this._stackSymbolViewContainer).remove();
    }

    markOnlyIfIndexAtLeastAsHigh(minIndex) {
        if (this._index >= minIndex) {
            this._mark();
        }
        else {
            this._unMark();
        }
    }

    _mark() {
        d3.select(this._stackSymbolViewContainer).attr('class', 'marked');
    }

    _unMark() {
        d3.select(this._stackSymbolViewContainer).attr('class', '');
    }
};

const getStackColumnWidth = () => stackWidth + 2 * paddingLeftRight;

const StackView = class {
    constructor(containerOfSvgs, dimensions) {
        this._scrollableDiv = d3.select(containerOfSvgs).append('div')
            .attr('id', 'stackcontainer').node();
        this._maxHeight = dimensions.height;
        this._svg = d3.select(this._scrollableDiv).append('svg').attr('id', 'stacksvg')
            .attr('width', getStackColumnWidth()).node();
        this._stackViewContainer = d3.select(this._svg).append('g').node();

        this.height = dimensions.height;

        new StackSymbolView(this._stackViewContainer, 'Stack', 0, true);

        this._actualStackContainer = d3.select(this._stackViewContainer).append('g').attr('transform',
            `translate(${paddingLeftRight},${-titleHeight})`).node();
        this._stackSymbolViews = [];
    }

    changeStack(newStack, numberOfNewSymbols) {
        const newStackReverse = newStack.slice().reverse();
        const count = newStackReverse.length;
        const minMarkedIndex = count - numberOfNewSymbols;

        this._stackSymbolViews.forEach(v => v.remove());

        this.height = Math.max(this._maxHeight, StackView._getComputedHeight(count));

        this._stackSymbolViews = newStackReverse.map((s, i) => new StackSymbolView(this._actualStackContainer, s, i));

        this._stackSymbolViews.forEach(v => v.markOnlyIfIndexAtLeastAsHigh(minMarkedIndex));
    }

    static _getComputedHeight(numberOfStackSymbols) {
        return numberOfStackSymbols * (stackSymbolHeight + 1) + 2 * paddingTopBottom;
    }

    set height(value) {
        this._height = value;
        d3.select(this._svg).attr('height', this._height);
        d3.select(this._stackViewContainer)
            .attr('transform', `translate(0,${this._height})`);

        const maxHeightOfDiv = this._height > this._maxHeight ? this._maxHeight : this._maxHeight + 5;
        this._scrollableDiv.style.height = maxHeightOfDiv + 'px';
    }

    markOnlyTopMost() {
        const lastIndex = this._stackSymbolViews.length - 1;
        this._stackSymbolViews.forEach(v => v.markOnlyIfIndexAtLeastAsHigh(lastIndex));
    }

    remove() {
        d3.select(this._scrollableDiv).remove();
    }
};

const SimulationView = class {
    constructor(containerOfSvgs, pdaSvgElement, dimensions) {
        this._wordView = new WordView(pdaSvgElement, dimensions);
        this._stackView = new StackView(containerOfSvgs, dimensions);
    }

    static getStackWidth() {
        return stackWidth;
    }

    changeConfig(config, numberOfNewStackSymbols) {
        this._wordView.changeWord(config.word);
        this._stackView.changeStack(config.stack, numberOfNewStackSymbols);
    }

    prepareConfigChange(inputLetterIsEpsilon) {
        if (!inputLetterIsEpsilon) {
            this._wordView.markFirstLetter();
        }
        this._stackView.markOnlyTopMost();
    }

    remove() {
        this._wordView.remove();
        this._stackView.remove();
    }
};

export default SimulationView;