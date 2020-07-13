'use strict';

import {Vector} from "../../../../utils/vector";
import * as d3 from "d3";
import {callIf} from "../../../../utils/functionUtils";
import {parseLines} from "../../../../utils/textParser";
import {maximumOfArray} from "../../../../utils/arrayUtils";
import {newTransitionGroupViewListenersSet, transitionGroupViewListenerInterface} from "./transitionGroupViewListener";

const FONT_SIZE = 13;
const MIN_WIDTH_OF_LINE = 100;
const MIN_NUM_LINES = 3;

const TransitionGroupView = class {
    constructor(svgContainer, textArea, getTransitionsToDisplay, getTransitionsToEdit, getEnableEditing, listener) {
        this._getTransitionsToDisplay = getTransitionsToDisplay;
        this._getTransitionsToEdit = getTransitionsToEdit;
        this._textInputField = textArea;
        this._listeners = newTransitionGroupViewListenersSet();
        this._listeners.add(listener);
        this._position = new Vector(0, 0);
        this._angle = 0;
        this._createSvgElements(svgContainer, getEnableEditing);
    }

    _createSvgElements(svgContainer, getEnableEditing) {
        this._svgElement = d3.select(svgContainer).append('g').attr('class', 'nonedit').node();
        this._textDisplayField = d3.select(this._svgElement).append('g').attr('class', 'display')
            .append('text')
            .attr('text-anchor', 'start').node();
        d3.select(this._textDisplayField).append('title').text('double click to edit');
        d3.select(this._textDisplayField).on('dblclick', callIf(getEnableEditing, () => this._startEditing()));
    }

    _startEditing() {
        this._isEdited = true;
        d3.select(this._svgElement).attr('class', 'edit');
        this._getTransitionsToDisplay().forEach(transition => transition.removeView());

        this._positionAndResizeTextInputField();
        this._fillTextInputFieldWithTransitions();
        this._showTextInputField();
    }

    _positionAndResizeTextInputField() {
        const width = this._getEditorWidth();
        const height = this._getEditorHeight();
        const positionOfUpperLeftCorner = new Vector(this._position.x - width/2, this._position.y - height/2);
        this._textInputField.style.width = width + 'px';
        this._textInputField.style.height= height + 'px';
        this._textInputField.style.left = positionOfUpperLeftCorner.x + 'px';
        this._textInputField.style.top = positionOfUpperLeftCorner.y + 'px';
    }

    _fillTextInputFieldWithTransitions() {
        this._textInputField.value = this._getTransitionsToEdit().map(transition => transition.toString()).join('\n');
    }

    _showTextInputField() {
        this._textInputField.style.display = 'inline-block';
        this._textInputField.onblur = () => this._finishEditing();
        this._textInputField.focus();
    }

    _hideTextInputField() {
        this._textInputField.onblur = null;
        this._textInputField.style.display = 'none';
    }

    get isEdited() {
        return this._isEdited;
    }

    _finishEditing() {
        this._hideTextInputField();
        this._isEdited = false;
        d3.select(this._svgElement).attr('class', 'nonedit');
        this._listeners.callForAll(transitionGroupViewListenerInterface.onTransitionsChanged, parseLines(this._textInputField.value));
    }

    updateOnTransitionsChanged() {
        this._redrawTransitions();
        this._updateTransformations();
    }

    _redrawTransitions() {
        d3.select(this._svgElement).select('g.display').select('text').selectAll('tspan').remove();
        this._getTransitionsToDisplay().forEach((transition, i) =>
            transition.createView(d3.select(this._svgElement).select('g.display').select('text').node(), i));
    }

    _updateTransformations() {
        const positionOfDisplayUpperLeftCorner = new Vector(this._position.x - this._getWidth() / 2,
            this._position.y - this._getHeight() / 2);
        d3.select(this._svgElement).select('g.display').attr('transform',
            `translate(${positionOfDisplayUpperLeftCorner.x},${positionOfDisplayUpperLeftCorner.y})`);
    }

    _getWidth() {
        return maximumOfArray(this._getTransitionsToDisplay(), transition => transition.getWidth());
    }

    _getHeight() {
        return d3.select(this._svgElement).select('g.display').node().getBBox().height;
    }

    _getEditorWidth() {
        const drawnWidth = maximumOfArray(this._getTransitionsToEdit(), transition => transition.getWidth());
        return Math.max(MIN_WIDTH_OF_LINE, drawnWidth);
    }

    _getEditorHeight() {
        const rows = Math.max(MIN_NUM_LINES, this._getTransitionsToEdit().length + 2);
        return rows * FONT_SIZE;
    }

    getDiagonal() {
        return Math.sqrt(this._getHeight() * this._getHeight() + this._getWidth() * this._getWidth());
    }

    _updatePosition(position) {
        this._position = position;
        this._updateTransformations();
    }
};

export default TransitionGroupView;