'use strict';

import * as d3 from "d3";

const TransitionView = class{
    constructor(svgTextContainer, rowNumber, displayingString) {
        this._svgElement = d3.select(svgTextContainer).append('tspan').text(displayingString).attr('x', 0).node();
        if (rowNumber > 0) {
            d3.select(this._svgElement).attr('dy', '1em');
        }
    }

    setCssClasses(classes) {
        d3.select(this._svgElement).attr('class', classes);
    }

    remove() {
        d3.select(this._svgElement).remove();
    }

    setOnClick(onClick) {
        this._svgElement.classList.add('pointer');
        this._svgElement.onclick = onClick;
    }

    removeOnClick() {
        this._svgElement.classList.remove('pointer');
        this._svgElement.onclick = null;
    }

    getWidth() {
        return this._svgElement.getComputedTextLength();
    }

    unMark() {
        this._svgElement.classList.remove('marked');
    }

    mark() {
        this._svgElement.classList.add('marked');
    }
};

export default TransitionView;