'use strict';

import LinksView from './linksView';
import {createNewLinkSet} from './link/link';
import LinkContext from './linkContext';
import StateTuple from '../states/stateTuple';
import xmlExporter from '../../utils/xmlExporter';
import {Listener} from "../../utils/listener";
import {linkListenerInterface} from "./link/linkListeners";
import {stateListenerInterface} from "../states/state/stateListeners";
import {linksListenerInterface, newLinksListenersSet} from "./linksListener";
import {flatten} from "../../utils/arrayUtils";

const XML_LINKS_ID = 'links';

/**
 * stores all {Link} of one {PDA}
 */
const Links = class {
    /**
     * @param svgContainer where to draw the {Link}s
     * @param automatonContainer div with the svg of the automaton
     * @param contextMenuSvgContainer svg container for the {ContextMenu}
     * @param properties {Properties} of the {PDA}
     * @param TemporaryState {TemporaryState}
     * @param getEnableEditing return whether the {PDA} should be able to be edited
     * @param getStartLink return the {StartLink} of the {PDA}
     * @param Transition class that defines a single transition; it must inherit from {AbstractTransition}
     */
    constructor(svgContainer, automatonContainer, contextMenuSvgContainer, properties, TemporaryState, getEnableEditing,
                getStartLink, Transition) {
        this._linksView = new LinksView(svgContainer, automatonContainer);
        this._links = new Set();
        this._TemporaryState = TemporaryState;
        this._listeners = newLinksListenersSet();
        this._createLinkContext(svgContainer, contextMenuSvgContainer, properties, getEnableEditing, getStartLink, Transition);
        this._linkContext.listeners.add(this._linkListener());
    }

    addListener(listener) {
        this._listeners.add(listener);
    }

    makeAllTransitionsUnClickAble() {
        this._links.forEach(link => link.makeAllTransitionsUnClickAble());
    }

    makeTransitionsClickAbleStartingAt(state, filter, onClick, onNoTransitionFound) {
        const linksStartingAtState = this.allLinks.filter(link => link.states.startState === state);
        const clickCallback = (link, transition) => {
            this._links.forEach(link => link.makeAllTransitionsUnClickAble());
            onClick(link, transition);
        };
        const results = linksStartingAtState.map(link => link.makeTransitionsClickAble(filter, clickCallback));

        if (!results.some(res => res)) {
            onNoTransitionFound();
        }
    }

    unMarkAll() {
        this._links.forEach(link => link.unMark());
    }

    /**
     * marks the given {Transition} between the {State}s with the given ids in a color
     * @param fromStateId
     * @param toStateId
     * @param transition
     */
    mark(fromStateId, toStateId, transition) {
        const links = this.allLinks.filter(link => link.isBetween(fromStateId, toStateId));
        if (links.length !== 1) {
            throw new Error('there exists no link between the states with the given ids');
        }
        links[0].mark(transition);
    }

    isContextMenuShownAtTheMoment() {
        return this._linkContext.contextMenu.isShown;
    }

    isAnyLinkEditedAtTheMoment() {
        return this.allLinks.some(link => link.isEditedAtTheMoment());
    }

    get allLinks() {
        return [...this._links];
    }

    updateValidity() {
        this.allLinks.forEach(link => link.updateValidity());
    }

    allAreValid() {
        return this.allLinks.every(link => link.isValid());
    }

    exportToXml() {
        return xmlExporter.buildXmlString().addElement(XML_LINKS_ID,
            xmlExporter.joinXmlElements(Array.from(this._links.values()).map(link => link.exportToXml())))
            .build()
            .build();
    }

    addLinksFromXml(xmlDoc, getStateById) {
        xmlDoc.getArrayOfChildren(XML_LINKS_ID).forEach(xmlLink =>
            this._addLink(this._LinkClasses.Link.createFromXml(xmlLink, getStateById)));
    }

    _createLinkContext(svgContainer, contextMenuSvgContainer, properties, getEnableEditing, getStartLink, Transition) {
        this._linkContext = new LinkContext(svgContainer, contextMenuSvgContainer, properties, () => this.allLinks,
            getStartLink, getEnableEditing, this._linksView.textArea, Transition);
        this._LinkClasses = createNewLinkSet(this._linkContext);
    }

    _updatePathOfMutualLink(stateTuple) {
        if (stateTuple.startState !== stateTuple.endState) {
            const linksToUpdate = this.allLinks.filter(link => link.isMutualTo(stateTuple));
            if (linksToUpdate.length > 1) {
                throw 'there should be only one link being mutual to e specific one';
            }
            linksToUpdate.forEach(link => link.updatePath());
        }
    }

    _updateAngleFor(link) {
        const linksWithCommonState = this.allLinks.filter(otherLink =>
            otherLink.currentStates.contains(link.currentStates.startState) ||
            otherLink.currentStates.contains(link.currentStates.endState) ||
            otherLink.currentStates.contains(link.states.startState) ||
            otherLink.currentStates.contains(link.states.endState));
        linksWithCommonState.forEach(otherLink => otherLink.updateAngle());
    }

    _updateAngleForStateTuple(stateTuple) {
        const linksWithCommonState = this.allLinks.filter(otherLink =>
            otherLink.currentStates.contains(stateTuple.startState) ||
            otherLink.currentStates.contains(stateTuple.endState));
        linksWithCommonState.forEach(otherLink => otherLink.updateAngle());
    }

    _onLinkMoved(position, newLink) {
        newLink.link.changeCurrentState(newLink.stateSelector, this._stateHoveredOver || newLink.temporaryEndState);
        newLink.temporaryEndState.move(position);
    }

    _removeLink(link) {
        this._links.delete(link);
        this._updatePathOfMutualLink(link.currentStates);
        this._updateAngleFor(link);
        this._listeners.callForAll(linksListenerInterface.onLinkChanged, link.states.startState);
    }

    _addLink(link) {
        this._links.add(link);
        this._updatePathOfMutualLink(link.currentStates);
        this._updateAngleFor(link);
        this._listeners.callForAll(linksListenerInterface.onLinkChanged, link.states.startState);
    }

    _checkIfLinkAlreadyExists(link) {
        const map = new Map(this.allLinks.filter(l => l !== link).map(l => [l.getId(), l]));
        const id = link.getId();
        if (map.has(id)) {
            map.get(id).mergeWith(link);
            link.remove();
        }
    }

    //TODO: when taking the hovered state, merge the link with the potential existing one provisionally!!
    //strategy: create new merge-link, which is removed when the dragging goes on
    _startDraggingLink(link, stateSelector, position, draggedLink) {
        draggedLink.link = link;
        draggedLink.stateSelector = stateSelector;
        draggedLink.temporaryEndState = new this._TemporaryState(position.x, position.y);
        link.changeCurrentState(stateSelector, draggedLink.temporaryEndState);
    }

    _finishDraggingLink(draggedLink) {
        this._onLinkMoved(draggedLink.temporaryEndState.circle, draggedLink);
        draggedLink.link.takeCurrentState(draggedLink.stateSelector);
        draggedLink.temporaryEndState = null;
        draggedLink.link = null;
        draggedLink.stateSelector = null;
        draggedLink.stateSelector = null;
    }

    _startCreatingNewLink(state, position, newLink) {
        newLink.temporaryEndState = new this._TemporaryState(position.x, position.y);
        newLink.link = new this._LinkClasses.Link(state, newLink.temporaryEndState, false);
        this._addLink(newLink.link);
    }

    _finishCreatingNewLink(newLink) {
        this._onLinkMoved(newLink.temporaryEndState.circle, newLink);
        newLink.link.takeCurrentState(StateTuple.getEndStateSelector());
        newLink.temporaryEndState = null;
        newLink.link = null;
    }

    _onEndsStatesOfLinkChanged(link, oldStartState) {
        this._checkIfLinkAlreadyExists(link);
        this._listeners.callForAll(linksListenerInterface.onLinkChanged, link.states.startState);
        this._listeners.callForAll(linksListenerInterface.onLinkChanged, oldStartState);
    }

    _onCurrentEndStatesOfLinkChanged(newStateTuple, oldStateTuple) {
        this._updateAngleForStateTuple(oldStateTuple);
        this._updatePathOfMutualLink(newStateTuple);
        this._updatePathOfMutualLink(oldStateTuple);
    }

    _linkListener() {
        const draggedLink = {
            link: null,
            stateSelector: null,
            temporaryEndState: null
        };

        const listener = new Listener(Links.name, linkListenerInterface);
        listener.set(linkListenerInterface.onLinkRemoved, link => this._removeLink(link));
        listener.set(linkListenerInterface.onLinkPositionChanged, link => this._updateAngleFor(link));
        listener.set(linkListenerInterface.onEndStatesChanged,
            (link, oldStartState) => this._onEndsStatesOfLinkChanged(link, oldStartState));
        listener.set(linkListenerInterface.onCurrentEndStatesChanged,
            (newStateTuple, oldStateTuple) => this._onCurrentEndStatesOfLinkChanged(newStateTuple, oldStateTuple));
        listener.set(linkListenerInterface.onDragStarted,
            (link, stateSelector, position) => this._startDraggingLink(link, stateSelector, position, draggedLink));
        listener.set(linkListenerInterface.onDragged,
            position => this._onLinkMoved(position, draggedLink));
        listener.set(linkListenerInterface.onDragStopped,
            () => this._finishDraggingLink(draggedLink));
        listener.set(linkListenerInterface.onTransitionsChanged,
            link => this._listeners.callForAll(linksListenerInterface.onLinkChanged, link.states.startState));
        return listener;
    }

    stateListener() {
        const newLink = {
            link: null,
            stateSelector: StateTuple.getEndStateSelector(),
            temporaryEndState: null
        };
        const getLinksOfState = state => this.allLinks.filter(link => link.currentStates.contains(state));
        
        const listener = new Listener(Links.name, stateListenerInterface);
        listener.set(stateListenerInterface.onStateMoved, state => getLinksOfState(state).forEach(link => link.updatePositions()));
        listener.set(stateListenerInterface.onStateRemoved, state => getLinksOfState(state).forEach(link => link.remove()));
        listener.set(stateListenerInterface.onDragStarted, (state, position) => this._startCreatingNewLink(state, position, newLink));
        listener.set(stateListenerInterface.onDragged, position => this._onLinkMoved(position, newLink));
        listener.set(stateListenerInterface.onDragStopped, () => this._finishCreatingNewLink(newLink));
        listener.set(stateListenerInterface.onStateHovered, state => this._stateHoveredOver = state);
        listener.set(stateListenerInterface.onStateHoveredOut, () => this._stateHoveredOver = null);
        return listener;
    }
};

export default Links;