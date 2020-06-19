'use strict';

import * as d3 from 'd3';

const LinksView = class {
    constructor(svgContainer, automatonContainer) {
        d3.select(svgContainer).append('defs').append('marker')
            .attr('id', 'arrow')
            .attr('refX', 8)
            .attr('refY', 3)
            .attr('markerWidth', 10)
            .attr('markerHeight', 10)
            .attr('orient', 'auto')
            .attr('markerUnits', 'strokeWidth')
            .append('path')
            .attr('d', 'M0,0 L0,6 L9,3 z');

        this._textArea = d3.select(automatonContainer).append('textarea')
            .attr('id', 'transition-input')
            .style('display', 'none')
            .node();

        this._textArea.addEventListener('input', () => {
            this._textArea.style.height = 'auto';
            this._textArea.style.height = (this._textArea.scrollHeight) + 'px';
        });
    }

    get textArea() {
        return this._textArea;
    }
};

export default LinksView;