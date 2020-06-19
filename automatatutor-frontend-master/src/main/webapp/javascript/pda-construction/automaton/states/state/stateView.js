'use strict';

import {callIf} from "../../../utils/functionUtils";
import {Vector} from "../../../utils/vector";
import * as d3 from "d3";
import {newStateViewListenersSet, stateViewListenerInterface} from "./stateViewListener";

const HOVER_AREA_WITH = 30;

const StateView = class {
    constructor(svgContainer, stateId, circle, getEnableEditing, listener) {
        this._svgContainer = svgContainer;
        this._listeners = newStateViewListenersSet();
        this._listeners.add(listener);
        this._createSvgElement(svgContainer, stateId, circle, getEnableEditing);
    }

    _createSvgElement(svgContainer, stateId, circle, getEnableEditing) {
        this._svgElement = d3.select(svgContainer).append('g').attr('class', 'state').node();

        let hoverAreaHovered = false;

        d3.select(this._svgElement).append('circle').attr('r', circle.r + HOVER_AREA_WITH).attr('class', 'hoverArea')
            .on('mouseover', () => hoverAreaHovered = true)
            .on('mouseout', () => hoverAreaHovered = false)
            .append('title').text('drag from here to create transitions');

        const visibleCircleContainer = d3.select(this._svgElement).append('g')
            .on('contextmenu', callIf(getEnableEditing, () => {
                d3.event.preventDefault();
                this._listeners.callForAll(stateViewListenerInterface.onContextMenu, Vector.fromArray(d3.mouse(this._svgContainer)));
            }))
            .on('dblclick', callIf(getEnableEditing, () => this._listeners.callForAll(stateViewListenerInterface.onDblClick)))
            .on('mouseover', callIf(getEnableEditing, () => this._listeners.callForAll(stateViewListenerInterface.onMouseOver)))
            .on('mouseout', callIf(getEnableEditing, () => this._listeners.callForAll(stateViewListenerInterface.onMouseOut)))
            .node();

        d3.select(visibleCircleContainer).append('circle').attr('class', 'state').attr('r', circle.r);
        d3.select(visibleCircleContainer).append('circle').attr('class', 'finalMarker').attr('r', circle.r - 10);

        let creatingNewLink = false;

        d3.select(this._svgElement).call(d3.drag()
            .on('start', () => {
                if (hoverAreaHovered) {
                    callIf(getEnableEditing, () => {
                        creatingNewLink = true;
                        this._listeners.callForAll(stateViewListenerInterface.onNewLinkCreationStarted, new Vector(d3.event.x, d3.event.y));
                    })();
                }
            })
            .on('drag', () => {
                if (creatingNewLink) {
                    callIf(getEnableEditing, () =>
                        this._listeners.callForAll(stateViewListenerInterface.onNewLinkCreationDragged, new Vector(d3.event.x, d3.event.y)))();
                }
                else {
                    this._listeners.callForAll(stateViewListenerInterface.onDrag, new Vector(d3.event.x, d3.event.y));
                }
            })
            .on('end', () => {
                if (creatingNewLink) {
                    callIf(getEnableEditing, () =>
                        this._listeners.callForAll(stateViewListenerInterface.onNewLinkCreationFinished))();
                    creatingNewLink = false;
                }
            }));
        const textElement = d3.select(this._svgElement).append('text').attr('class', 'state-id').text(stateId);
        textElement.attr('dx', -textElement.node().getComputedTextLength() / 2);
        this.updatePosition(circle);
    }

    updatePosition(circle) {
        d3.select(this._svgElement).attr('transform', `translate(${circle.x},${circle.y})`)
    }

    remove() {
        d3.select(this._svgElement).remove();
    }

    /**
     * sets the css-class of the svg-element according to the final-state of the state
     */
    updateFinalMarker(isFinal) {
        const cssClass = isFinal ? 'final' : 'nonFinal';
        d3.select(this._svgElement).attr('class', `state ${cssClass}`);
    }

    mark() {
        this._svgElement.classList.add('marked');
    }

    unMark() {
        this._svgElement.classList.remove('marked');
    }
};

export default StateView;