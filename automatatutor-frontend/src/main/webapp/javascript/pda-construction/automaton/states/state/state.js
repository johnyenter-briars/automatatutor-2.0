'use strict';

import {Vector, Circle} from '../../../utils/vector';
import {stateListenerInterface} from './stateListeners';
import {contextMenuIds, getItemIdsForState} from '../stateContextMenu';
import xmlExporter from '../../../utils/xmlExporter';
import {StartLink} from '../../links/link/link';
import StateView from "./stateView";
import {Listener} from "../../../utils/listener";
import {stateViewListenerInterface} from "./stateViewListener";

const XML_INNER_STATE_ID = 'innerState';
const XML_INITIAL_STATE_ID = 'initialState';
const XML_ATTR_ID = 'id';
const XML_ATTR_X = 'x';
const XML_ATTR_Y = 'y';
const XML_ATTR_IS_FINAL = 'isFinal';

/**
 * abstract class that represents a state of the automaton
 */
const AbstractState = class {
    constructor(circle) {
        this._circle = circle;
    }

    get circle() {
        return this._circle;
    }

    /**
     * @return whether the state exists
     */
    isEmpty() {
        return false;
    }

    /**
     * @return if the state is not permanently shown
     * (a reason for not showing a state permanently is for example the creation of a new link)
     */
    isTemporary() {
        return false;
    }
};

/**
 * State that is permanently part of the automaton but not shown (used for the start-side of the initial link)
 */
const InvisibleState = class extends AbstractState {
    constructor(position) {
        super(Circle.fromPosition(position, 0));
    }

    changePosition(position) {
        this._circle = this._circle.changePosition(position);
    }
};

/**
 * Placeholder for a state that does not exist yet, but will exist in the future
 */
const EmptyState = class extends AbstractState {
    constructor() {
        super(new Circle(0, 0, 0));
    }

    isEmpty() {
        return true;
    }
};

/**
 * initialize the State-classes with their {StateContext}
 * @param stateContext {StateContext} that is used for all states created with the returned classes
 */
const createNewStateSet = stateContext => {

    /**
     * State that is not permanently part of the automaton (used for creating a new link)
     */
    const TemporaryState = class extends InvisibleState {
        constructor(x, y) {
            super(new Vector(x, y));
        }

        isTemporary() {
            return true;
        }

        move(position) {
            this._circle = this._circle.changePositionBounded(position, stateContext.dimensions);
            stateContext.listeners.callForAll(stateListenerInterface.onStateMoved, this);
        }
    };

    /**
     * State of the automaton
     */
    const State = class extends AbstractState {
        /**
         * @param id unique id of the {State}
         * @param x x coordinate
         * @param y y coordinate
         * @param isFinal if the state is a final state
         */
        constructor(id, x, y, isFinal = false) {
            super(new Circle(x, y, stateContext.stateRadius));
            this._id = id;
            this._view = new StateView(stateContext.svgContainer, this._id, this._circle, stateContext.getEnableEditing, this._getViewListener());
            this._markFinalState(isFinal);
        }

        get id() {
            return this._id;
        }

        _getContextMenuListener() {
            const listener = new Listener(State.name, stateContext.contextMenu.variableListenerWrapper.listenerInterface);
            listener.set(contextMenuIds.remove, () => this.remove());
            listener.set(contextMenuIds.nonFinal, () => this.tryToUnMarkAsFinal());
            listener.set(contextMenuIds.final, () => this.tryToMarkAsFinal());
            return listener;
        }

        _getViewListener() {
            const listener = new Listener(State.name, stateViewListenerInterface);
            listener.set(stateViewListenerInterface.onContextMenu, position => stateContext.contextMenu.showSome(getItemIdsForState(this, stateContext.stateCanBeFinal), this._getContextMenuListener(), position));
            listener.set(stateViewListenerInterface.onDblClick, () => this.tryToSwapFinalState());
            listener.set(stateViewListenerInterface.onMouseOver, () => stateContext.listeners.callForAll(stateListenerInterface.onStateHovered, this));
            listener.set(stateViewListenerInterface.onMouseOut, () => stateContext.listeners.callForAll(stateListenerInterface.onStateHoveredOut, this));
            listener.set(stateViewListenerInterface.onNewLinkCreationStarted, position => stateContext.listeners.callForAll(stateListenerInterface.onDragStarted, this, position));
            listener.set(stateViewListenerInterface.onNewLinkCreationDragged, position => stateContext.listeners.callForAll(stateListenerInterface.onDragged, position));
            listener.set(stateViewListenerInterface.onDrag, position => this.move(position));
            listener.set(stateViewListenerInterface.onNewLinkCreationFinished, () => stateContext.listeners.callForAll(stateListenerInterface.onDragStopped));
            return listener;
        }

        exportToXml(xmlName) {
            return xmlExporter.buildXmlString().addElement(xmlName)
                .addAttr(XML_ATTR_ID, this._id)
                .addAttr(XML_ATTR_X, this._circle.x)
                .addAttr(XML_ATTR_Y, this._circle.y)
                .addAttr(XML_ATTR_IS_FINAL, this._isFinal)
                .build()
                .build();
        }

        static createFromXml(xmlElement) {
            const id = parseInt(xmlElement.getAttribute(XML_ATTR_ID));
            const x = Number.parseFloat(xmlElement.getAttribute(XML_ATTR_X));
            const y = Number.parseFloat(xmlElement.getAttribute(XML_ATTR_Y));
            const isFinal = xmlElement.getAttribute(XML_ATTR_IS_FINAL) === 'true';
            if (xmlElement.tagName === XML_INITIAL_STATE_ID) {
                return new InitialState(id, x, y, isFinal);
            }
            else if (xmlElement.tagName === XML_INNER_STATE_ID) {
                return new InnerState(id, x, y, isFinal);
            }
            throw `${xmlElement.tagName} is not a valid xml-tag-name for a state`;
        }

        remove() {
            this._view.remove();
            stateContext.listeners.callForAll(stateListenerInterface.onStateRemoved, this);
        }

        move(position) {
            this._circle = this._circle.changePositionBounded(position, stateContext.dimensions);
            this._view.updatePosition(this._circle);
            stateContext.listeners.callForAll(stateListenerInterface.onStateMoved, this);
        }

        tryToMarkAsFinal() {
            this._tryToMarkFinalState(true);
        }

        tryToUnMarkAsFinal() {
            this._tryToMarkFinalState(false);
        }

        tryToSwapFinalState() {
            this._tryToMarkFinalState(!this._isFinal);
        }

        _tryToMarkFinalState(isFinal) {
            if (stateContext.stateCanBeFinal) {
                this._markFinalState(isFinal);
            }
        }

        _markFinalState(isFinal) {
            this._isFinal = isFinal;
            this.updateFinalMarker();
        }

        updateFinalMarker() {
            this._view.updateFinalMarker(stateContext.stateCanBeFinal && this._isFinal);
        }

        mark() {
            this._view.mark();
        }

        unMark() {
            this._view.unMark();
        }
    };

    /**
     * the initial state of the automaton
     */
    const InitialState = class extends State {
        constructor(id, x, y, isFinal = false) {
            super(id, x, y, isFinal);
            this.isInitial = true;
            this._startLink = new StartLink(this, stateContext.svgContainer);
        }

        get startLink() {
            return this._startLink;
        }

        move(position) {
            super.move(position);
            this._startLink.updatePositions();
        }

        remove() {
            super.remove();
            this._startLink.remove();
        }

        exportToXml() {
            return super.exportToXml(XML_INITIAL_STATE_ID);
        }
    };

    /**
     * "normal" state of the automaton
     */
    const InnerState = class extends State {
        constructor(id, x, y, isFinal = false) {
            super(id, x, y, isFinal);
        }

        exportToXml() {
            return super.exportToXml(XML_INNER_STATE_ID);
        }
    };

    return {
        TemporaryState, InitialState, InnerState, State
    };
};

export {createNewStateSet, EmptyState, InvisibleState};