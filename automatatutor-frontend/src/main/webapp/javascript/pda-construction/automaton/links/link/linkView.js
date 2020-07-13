'use strict';

import {newLinkViewListenersSet, linkViewListenerInterface} from './linkViewListeners';
import * as d3 from 'd3';
import {Vector} from '../../../utils/vector';
import {callIf} from '../../../utils/functionUtils';
import {Listener} from "../../../utils/listener";
import {linkTransformerListenerInterface} from "./linkTransformer/linkTransformerListener";

const LinkView = class {
    constructor(svgContainer, getEnableEditing, ...listeners) {
        this._svgContainer = svgContainer;
        this._svgElement = d3.select(this._svgContainer).append('g').node();
        this._pathContainer = d3.select(this._svgElement).append('g').node();
        d3.select(this._pathContainer).append('path').attr('class', 'arrow');
        d3.select(this._pathContainer).append('path').attr('class', 'hoverArea').append('title').text('drag me from both ends');
        this.updateValidityMarker(true);
        this._listeners = newLinkViewListenersSet();
        this._listeners.addAll([...listeners]);
        if (!this._listeners.isEmpty()) {
            this._applyInteractionListener(getEnableEditing);
        }
    }

    mark() {
        this._svgElement.classList.add('marked');
    }

    unMark() {
        this._svgElement.classList.remove('marked');
    }

    _applyInteractionListener(getEnableEditing) {
        d3.select(this._pathContainer).select('path.hoverArea')
            .on('contextmenu', callIf(getEnableEditing, () => {
                d3.event.preventDefault();
                this._listeners.callForAll(linkViewListenerInterface.onContextMenu, Vector.fromArray(d3.mouse(this._svgContainer)))
            }))
            .call(d3.drag()
                .on('start', callIf(getEnableEditing, () => this._listeners.callForAll(linkViewListenerInterface.onDragStarted, Vector.fromObject(d3.event))))
                .on('drag', callIf(getEnableEditing, () => this._listeners.callForAll(linkViewListenerInterface.onDragged, new Vector(d3.event.x, d3.event.y))))
                .on('end', callIf(getEnableEditing, () => this._listeners.callForAll(linkViewListenerInterface.onDragStopped))));
    }

    remove() {
        d3.select(this._svgElement).remove();
    }

    updateValidityMarker(isValid) {
        d3.select(this._svgElement).attr('class', isValid ? 'valid' : 'invalid');
    }

    get svgElement() {
        return this._svgElement;
    }

    _updateRotation(angle, rotationCenter) {
        d3.select(this._pathContainer).attr('transform', `rotate(${angle} ${rotationCenter.x}, ${rotationCenter.y})`);
    }

    getLinkTransformerListener() {
        const listener = new Listener(LinkView.name, linkTransformerListenerInterface);
        listener.set(linkTransformerListenerInterface.onAngleChanged, (angle, rotationCenter) => this._updateRotation(angle, rotationCenter));
        listener.set(linkTransformerListenerInterface.onRotationCenterChanged, (angle, rotationCenter) => this._updateRotation(angle, rotationCenter));
        listener.set(linkTransformerListenerInterface.onPathChanged, path => d3.select(this._pathContainer).selectAll('path').attr('d', path));
        return listener;
    }
};

export default LinkView;