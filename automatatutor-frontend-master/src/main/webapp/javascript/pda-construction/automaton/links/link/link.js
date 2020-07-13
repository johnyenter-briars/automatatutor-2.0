'use strict';

import {Vector} from '../../../utils/vector';
import {linkListenerInterface} from './linkListeners';
import {EmptyState, InvisibleState} from '../../states/state/state';
import StateTuple from '../../states/stateTuple';
import {LinkTransformer, StraightForm, CurvedForm, LoopForm} from './linkTransformer/linkTransformer';
import TransitionGroup from './transitionGroup/transitionGroup';
import xmlExporter from '../../../utils/xmlExporter';
import LinkView from './linkView';
import {Listener} from "../../../utils/listener";
import {contextMenuIds} from "../linkContextMenu";
import {transitionGroupListenerInterface} from "./transitionGroup/transitionGroupListener";
import {linkViewListenerInterface} from "./linkViewListeners";

const START_LINK_LENGTH = 60;

const getStartPosition = endCircle => endCircle.shiftLeft(START_LINK_LENGTH + endCircle.r);

const XML_LINK_ID = 'link';
const XML_ATTR_START = 'start';
const XML_ATTR_END = 'end';

const AbstractLink = class {
    get currentStates() {
        throw 'not implemented';
    }

    isSelfLink() {
        throw 'not implemented';
    }
};

/**
 * the link to the {InitialState}
 */
const StartLink = class extends AbstractLink {
    constructor(initialState, svgContainer) {
        super();
        this._states = new StateTuple(new InvisibleState(getStartPosition(initialState.circle)), initialState);
        this._currentStates = new StateTuple(this._states.startState, initialState);
        this._view = new LinkView(svgContainer);
        this._linkTransformer = new LinkTransformer(0, this._view.getLinkTransformerListener()).addForm(new StraightForm());
        this._linkTransformer.setForm(StraightForm.name, this._currentStates.getCircleTuple());
    }

    get currentStates() {
        return this._currentStates;
    }

    isSelfLink() {
        return false;
    }

    remove() {
        this._view.remove();
    }

    updatePositions() {
        this._currentStates.startState.changePosition(getStartPosition(this._currentStates.endState.circle));
        this._linkTransformer.updatePositions(this._currentStates.getCircleTuple());
    }
};

/**
 * initialize the Link-classes with their LinkContext
 * @param linkContext {LinkContext} that is used for all links created with the returned classes
 */
const createNewLinkSet = linkContext => {

    /**
     * represents a link between two {State}s: a link combines all transitions from a specific state to another one
     */
    const Link = class extends AbstractLink {
        /**
         * @param startState permanent start {State} of the {Link} (can be changed)
         * @param currentEndState provisional end {State} of the {Link}
         */
        constructor(startState, currentEndState) {
            super();
            this._states = new StateTuple(startState, new EmptyState());
            this._currentStates = new StateTuple(startState, currentEndState);
            this._view = new LinkView(linkContext.svgContainer, linkContext.getEnableEditing, this._getViewListener());
            //using svgElement so that rotation is applied to transitionGroup
            this._transitionGroup = new TransitionGroup(linkContext.Transition, this._view.svgElement,
                linkContext.textArea, linkContext.properties, linkContext.getEnableEditing,
                this._getTransitionGroupListener());

            this._linkTransformer = new LinkTransformer(this._transitionGroup.view.getDiagonal(),
                this._view.getLinkTransformerListener(), this._transitionGroup.getLinkTransformerListener())
                .addForm(new StraightForm())
                .addForm(new CurvedForm())
                .addForm(new LoopForm(() => this._currentStates.startState, linkContext.getAllLinks));
            this.updatePath();
            this._transitionGroup.listeners().add(this._linkTransformer.getTransitionGroupListener());
        }

        get transitions() {
            return this._transitionGroup.transitions;
        }

        mark(transition) {
            this._view.mark();
            this._transitionGroup.mark(transition);
        }

        unMark() {
            this._view.unMark();
            this._transitionGroup.unMark();
        }

        makeTransitionsClickAble(filter, onClick) {
            const filteredTransitions = this._transitionGroup.transitions.filter(filter);
            filteredTransitions.forEach(transition => transition.makeClickAble(transition => onClick(this, transition)));
            return filteredTransitions.length > 0;
        }

        makeAllTransitionsUnClickAble() {
            this._transitionGroup.transitions.forEach(transition => transition.makeUnClickAble());
        }

        _getContextMenuListener() {
            const listener = new Listener(Link.name, linkContext.contextMenu.variableListenerWrapper.listenerInterface);
            listener.set(contextMenuIds.remove, () => this.remove());
            return listener;
        }

        isEditedAtTheMoment() {
            return this._transitionGroup.isEditedAtTheMoment();
        }

        updateValidity() {
            this._transitionGroup.updateValidity();
            this.updateViewValidity();
        }

        isBetween(fromId, toId) {
            return this._states.startState.id === fromId && this._states.endState.id === toId;
        }

        get states() {
            return this._states;
        }

        get currentStates() {
            return this._currentStates;
        }

        isMutualTo(stateTuple) {
            return stateTuple.startState === this._currentStates.endState
                && stateTuple.endState === this._currentStates.startState;
        }

        getId() {
            return `${this._states.startState.id}->${this._states.endState.id}`;
        }

        isValid() {
            return this._hasAtLeastOneTransition && this._transitionGroup.isValid();
        }

        exportToXml() {
            return xmlExporter.buildXmlString().addElement(XML_LINK_ID, this._transitionGroup.exportToXml())
                .addAttr(XML_ATTR_START, this._states.startState.id)
                .addAttr(XML_ATTR_END, this._states.endState.id)
                .build()
                .build();
        }

        static createFromXml(xmlElement, getStateById) {
            const start = parseInt(xmlElement.getAttribute(XML_ATTR_START));
            const end = parseInt(xmlElement.getAttribute(XML_ATTR_END));
            if (xmlElement.tagName === XML_LINK_ID) {
                const link = new Link(getStateById(start), getStateById(end));
                link.takeCurrentState(StateTuple.getEndStateSelector());
                link._transitionGroup.addTransitionsFromXml(xmlElement);
                return link;
            }
            throw `${xmlElement.tagName} is not a valid xml-tag-name for a state`;
        }

        remove() {
            this._view.remove();
            linkContext.listeners.callForAll(linkListenerInterface.onLinkRemoved, this);
        }

        _checkTransitionsNumber(transitionsNumber) {
            this._hasAtLeastOneTransition = transitionsNumber !== 0;
            this.updateViewValidity();
        }

        updateViewValidity() {
            this._view.updateValidityMarker(this.isValid());
        }

        _onTransitionsChanged(transitionsNumber) {
            this._checkTransitionsNumber(transitionsNumber);
            linkContext.listeners.callForAll(linkListenerInterface.onTransitionsChanged, this);
        }

        _getTransitionGroupListener() {
            const listener = new Listener(Link.name, transitionGroupListenerInterface);
            listener.set(transitionGroupListenerInterface.onTransitionsChanged,
                transitionsNumber => this._onTransitionsChanged(transitionsNumber));
            return listener;
        }

        _onContextMenu(position) {
            linkContext.contextMenu.showAll(this._getContextMenuListener(), position);
        }

        _onDragStarted(position) {
            const distanceToStart = Vector.between(position, this._linkTransformer.positionTuple.start).getLength();
            const distanceToEnd = Vector.between(position, this._linkTransformer.positionTuple.end).getLength();
            const stateSelector = distanceToEnd <= distanceToStart ? StateTuple.getEndStateSelector() : StateTuple.getStartStateSelector();
            linkContext.listeners.callForAll(linkListenerInterface.onDragStarted, this, stateSelector, position);
        }

        _onDragged(position) {
            linkContext.listeners.callForAll(linkListenerInterface.onDragged, position)
        }

        _onDragStopped() {
            linkContext.listeners.callForAll(linkListenerInterface.onDragStopped)
        }

        _getViewListener() {
            const listener = new Listener(Link.name, linkViewListenerInterface);
            listener.set(linkViewListenerInterface.onContextMenu, position => this._onContextMenu(position));
            listener.set(linkViewListenerInterface.onDragStarted, position => this._onDragStarted(position));
            listener.set(linkViewListenerInterface.onDragged, position => this._onDragged(position));
            listener.set(linkViewListenerInterface.onDragStopped, () => this._onDragStopped());
            return listener;
        }

        isSelfLink() {
            return this._currentStates.startState === this._currentStates.endState;
        }

        _getCurrentLinkForm() {
            if (this.isSelfLink()) {
                return LoopForm.name;
            }
            if (linkContext.checkIfMustShareStates(this)) {
                return CurvedForm.name;
            }
            return StraightForm.name;
        }

        /**
         * updates the end positions of the {Link} according to the end {State}s' positions
         */
        updatePositions() {
            this._linkTransformer.updatePositions(this._currentStates.getCircleTuple());
            linkContext.listeners.callForAll(linkListenerInterface.onLinkPositionChanged, this);
        }

        /**
         * updates the path (that means the form) of the {Link} according to if it is self link or if there exists
         * another link in the opposite direction or not
         */
        updatePath() {
            this._linkTransformer.setForm(this._getCurrentLinkForm(), this._currentStates.getCircleTuple());
            linkContext.listeners.callForAll(linkListenerInterface.onLinkPositionChanged, this);
        }

        updateAngle() {
            this._linkTransformer.updateAngle();
        }

        /**
         * sets the {State} of the current states, which is defined by the state-selector, to the given {State};
         * updates the path (only if the state has really changed)
         * @param stateSelector string which state should be changed
         * @param newState {State}
         */
        changeCurrentState(stateSelector, newState) {
            if (this._currentStates[stateSelector] !== newState) {
                const currentStatesBefore = this._currentStates.clone();
                this._currentStates[stateSelector] = newState;
                this.updatePath();
                linkContext.listeners.callForAll(linkListenerInterface.onCurrentEndStatesChanged, this._currentStates, currentStatesBefore);
            }
        }

        /**
         * takes the current {State} defined by the state-selector for the {Link}'s permanent {State};
         * if the {State} that should be taken over is empty (that means the link is being created), the {Link} is removed;
         * if the {State} that should be taken over is only temporary but not empty, the "old" end {State}is kept
         * @param stateSelector the {State} that should be taken over
         */
        takeCurrentState(stateSelector) {
            if (this._currentStates[stateSelector].isTemporary()) {
                if (this._states[stateSelector].isEmpty()) {
                    this.remove();
                }
                else {
                    this.changeCurrentState(stateSelector, this._states[stateSelector]);
                }
            }
            else {
                const oldStartState = this._states.startState;
                this._states[stateSelector] = this._currentStates[stateSelector];
                linkContext.listeners.callForAll(linkListenerInterface.onEndStatesChanged, this, oldStartState);
            }
        }

        mergeWith(otherLink) {
            this._transitionGroup.add(otherLink._transitionGroup);
            otherLink.remove();
        }
    };

    return {
        Link
    };
};

export {createNewLinkSet, StartLink};