'use strict';

import * as d3 from 'd3';
import {Vector} from '../../utils/vector';
import {newAutomatonViewListenersSet, automatonViewListenerInterface} from './automatonViewListeners';
import AutomatonNotes from './automatonNotes';
import {callIf} from '../../utils/functionUtils';

const AutomatonView = class {
    constructor(svgCanvas, notes, dimensions, listener, getEnableEditing) {
        this._automatonNotes = new AutomatonNotes(svgCanvas, notes);
        this._svgContainer = d3.select(svgCanvas).append('div').attr('id', 'svg-container').node();
        this._automatonContainer = d3.select(this._svgContainer).append('div').attr('id', 'automaton-container').node();
        this._svg = d3.select(this._automatonContainer).append('svg').attr('class', 'editable').attr('height', dimensions.height).attr('width', dimensions.width).node();
        this._clickReceiver = d3.select(this._svg).append('rect').attr('id', 'clickReceiver')
            .attr('width', dimensions.width).attr('height', dimensions.height).node();
        //hint: order of creating the groups is important, as the mouseover-events of the states are not always fired otherwise
        this._linkSvgContainer = d3.select(this._svg).append('g').node();
        this._stateSvgContainer = d3.select(this._svg).append('g').node();
        this._contextMenuSvgContainer = d3.select(this._svg).append('g').node();
        this._listeners = newAutomatonViewListenersSet();
        this._listeners.add(listener);
        d3.select(this._clickReceiver).on('mousedown', callIf(getEnableEditing,
            () => this._listeners.callForAll(automatonViewListenerInterface.onClick, Vector.fromArray(d3.mouse(this._clickReceiver)))));
    }

    addContainer() {
        return d3.select(this._svg).append('g').node();
    }

    remove() {
        this._automatonNotes.remove();
        d3.select(this._automatonContainer).remove();
    }

    disableEditing() {
        this.disableEditingTemporary();
        this._automatonNotes.remove();
    }

    disableEditingTemporary() {
        d3.select(this._svg).attr('class', '');
    }

    enableEditing() {
        d3.select(this._svg).attr('class', 'editable');
    }

    get svgContainer() {
        return this._svgContainer;
    }

    get automatonContainer() {
        return this._automatonContainer;
    }

    get linkSvgContainer() {
        return this._linkSvgContainer;
    }

    get stateSvgContainer() {
        return this._stateSvgContainer;
    }

    get contextMenuSvgContainer() {
        return this._contextMenuSvgContainer;
    }
};

export default AutomatonView;