'use strict';

import * as d3 from 'd3';

const PropertiesView = class {
    constructor(svgCanvas) {
        this._htmlElement = d3.select(svgCanvas).append('div').attr('class', 'bordered').append('ul').attr('class', 'circle').node();
    }

    get htmlElement() {
        return this._htmlElement;
    }
};

export default PropertiesView;