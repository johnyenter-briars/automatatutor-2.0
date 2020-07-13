'use strict';

import {createNewStateSet} from './state/state';
import StateContext from './stateContext';
import xmlExporter from '../../utils/xmlExporter';
import {Listener} from "../../utils/listener";
import {stateListenerInterface} from "./state/stateListeners";

const XML_STATES_ID = 'nodes';

const INITIAL_STATE_ID = 0;

const getSmallestPossibleStateIndex = stateIndexArray => {
    stateIndexArray.sort((i, j) => i - j);
    const jumpingValue = stateIndexArray.find((index, position) => position < index);
    return jumpingValue === undefined ? stateIndexArray.length : stateIndexArray.indexOf(jumpingValue);
};

/**
 * contains all {State}s of the {PDA}
 * @type {States}
 */
const States = class {
    /**
     * @param svgContainer svg element where the {State}s should be drawn
     * @param contextMenuSvgContainer svg element for the {ContextMenu}
     * @param getIfStateCanBeFinal function that returns if states can be final
     * @param stateRadius radius of the circle of a {State}
     * @param dimensions object with width and height of the {PDA} area
     * @param getEnableEditing function returning whether the {PDA} is editable
     */
    constructor(svgContainer, contextMenuSvgContainer, getIfStateCanBeFinal, stateRadius, dimensions, getEnableEditing) {
        this._states = new Map();
        this._createStateContext(svgContainer, contextMenuSvgContainer, getIfStateCanBeFinal, stateRadius, getEnableEditing, dimensions);
        this.addStateListener(this._stateListener());
        this._initialState = new this._StateClasses.InitialState(INITIAL_STATE_ID, dimensions.width / 4, dimensions.height / 2);
        this._states.set(this._initialState.id, this._initialState);
    }

    markOnly(stateId) {
        this.unMarkAll();
        this._states.get(stateId).mark();
    }

    unMarkAll() {
        Array.from(this._states.values()).forEach(state => state.unMark());
    }

    isContextMenuShownAtTheMoment() {
        return this._stateContext.contextMenu.isShown;
    }

    getStateById(stateId) {
        return this._states.get(stateId);
    }

    exportToXml() {
        return xmlExporter.buildXmlString().addElement(XML_STATES_ID,
            xmlExporter.joinXmlElements(Array.from(this._states.values()).map(state => state.exportToXml())))
            .build()
            .build();
    }

    addStatesFromXml(xmlDoc) {
        xmlDoc.getArrayOfChildren(XML_STATES_ID).forEach(xmlState => {
            const state = this._StateClasses.State.createFromXml(xmlState);
            if (state.isInitial && state.id !== INITIAL_STATE_ID) {
                throw `an initial state has to have id ${INITIAL_STATE_ID} (${typeof INITIAL_STATE_ID}), but found ${state.id} (${typeof state.id})`;
            }
            if (!state.isInitial && state.id === INITIAL_STATE_ID) {
                throw `a state with id ${INITIAL_STATE_ID} has to be initial`;
            }
            if (this._states.has(state.id)) {
                this._states.get(state.id).remove();
            }
            this._states.set(state.id, state);
            if (state.isInitial) {
                this._initialState = state;
            }
        });
    }

    get initialState() {
        return this._initialState;
    }

    get TemporaryState() {
        return this._StateClasses.TemporaryState;
    }

    get states() {
        return [...this._states.values()];
    }

    _createStateContext(svgContainer, contextMenuSvgContainer, getIfStateCanBeFinal, stateRadius, getEnableEditing, dimensions) {
        this._stateContext = new StateContext(svgContainer, contextMenuSvgContainer, getIfStateCanBeFinal, stateRadius, getEnableEditing, dimensions);
        this._StateClasses = createNewStateSet(this._stateContext)
    }

    addNewState(position) {
        const newId = getSmallestPossibleStateIndex(Array.from(this._states.keys()));
        const newState = new this._StateClasses.InnerState(newId, position.x, position.y);
        this._states.set(newState.id, newState);
    }

    addStateListener(listener) {
        this._stateContext.listeners.add(listener);
    }

    updateAllFinalMarker() {
        Array.from(this._states.values()).forEach(state => state.updateFinalMarker());
    }

    _stateListener() {
        const listener = new Listener(States.name, stateListenerInterface);
        listener.set(stateListenerInterface.onStateRemoved, state => this._states.delete(state.id));
        return listener;
    }
};

export default States;