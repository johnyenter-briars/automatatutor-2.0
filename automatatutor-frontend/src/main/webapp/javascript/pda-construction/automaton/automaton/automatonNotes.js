'use strict';

import * as d3 from 'd3';
import {makeElementCollapsible} from "../../utils/collapsibleView";

const INDENT = '-';

const AutomatonNotes = class {
    constructor(svgCanvas, notes) {
        this._htmlElement = d3.select(svgCanvas).append('div').attr('class', 'notes').node();
        const button = d3.select(this._htmlElement).append('button').text('Hints').node();
        const contentContainer = d3.select(this._htmlElement).append('div').node();
        const notesContent = d3.select(contentContainer).append('div').attr('id', 'notes-content').node();
        makeElementCollapsible(button, contentContainer);
        d3.select(notesContent).selectAll('li.note').data(notes).enter().append('li').attr('class', 'note').text(note => note.title)
            .append('ul').attr('class', 'explanations').selectAll('li').data(note => note.explanations).enter()
            .append('li').attr('class', 'explanation').text(expl => expl);
    }

    remove() {
        d3.select(this._htmlElement).remove();
    }
};

export default AutomatonNotes;