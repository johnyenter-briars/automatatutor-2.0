'use strict';

import AutomatonView from './automatonView';
import Links from '../links/links';
import States from '../states/states';
import xmlExporter from '../../utils/xmlExporter';
import Properties from "../properties/properties";

const XML_AUTOMATON_ID = 'automaton';
const XML_ATTR_WIDTH = 'width';
const XML_ATTR_HEIGHT = 'height';
const XML_ATTR_STATE_RADIUS = 'nodeRadius';



import './automatonStyles.css';
import {Listener} from "../../utils/listener";
import {automatonViewListenerInterface} from "./automatonViewListeners";

/**
 * only one instance of automaton may be created for one init-call
 */
const Automaton = class {
    /**
     *
     * @param svgCanvas html element (usually a div), where all the html-elements of the automaton can be inserted
     * @param dimensions object with width and height of the field for creating the automaton
     * @param stateRadius radius of the states of the automaton
     * @param errorHandler function that takes a string error message for handling errors
     * @param getIfStateCanBeFinal function that returns if state can be final
     * checks if they are deterministic and changes properties of the transitions according to this
     * @param notes [{title: String, explanations: [String]}] with explanations
     * @param Transition class that defines a single transition; it must inherit from {AbstractTransition}
     */
    constructor(svgCanvas, dimensions, stateRadius, errorHandler, getIfStateCanBeFinal, notes, Transition) {
        this._canvas = svgCanvas;
        this._dimensions = dimensions;
        this._errorHandler = errorHandler;

        this._enableEditing = true;
        const getEnableEditing = () => this._enableEditing;

        this._properties = new Properties(this, svgCanvas);

        this._view = new AutomatonView(svgCanvas, notes, dimensions, this._getViewListener(), getEnableEditing);

        this._states = new States(this._view.stateSvgContainer, this._view.contextMenuSvgContainer, getIfStateCanBeFinal, stateRadius, dimensions, getEnableEditing);
        this._links = new Links(this._view.linkSvgContainer, this._view.automatonContainer, this._view.contextMenuSvgContainer, this._properties,
            this._states.TemporaryState, getEnableEditing, () => this._states.initialState.startLink, Transition);
        this._states.addStateListener(this._links.stateListener());
    }

    addStatesFromXml(xmlDoc) {
        this._states.addStatesFromXml(xmlDoc);
    }

    addLinksFromXml(xmlDoc) {
        this._links.addLinksFromXml(xmlDoc, stateId => this._states.getStateById(stateId));
    }

    addProperty(property) {
        this._properties.addProperty(property);
    }

    get states() {
        return this._states;
    }

    get links() {
        return this._links;
    }

    get properties() {
        return this._properties;
    }

    get errorHandler() {
        return this._errorHandler;
    }

    get enableEditing() {
        return this._enableEditing;
    }

    get dimensions() {
        return this._dimensions;
    }

    addContainer() {
        return this._view.addContainer();
    }

    get svgContainer() {
        return this._view.svgContainer;
    }

    disableEditing() {
        this._enableEditing = false;
        this._view.disableEditing();
    }

    disableEditingTemporary() {
        this._enableEditing = false;
        this._properties.disableEditing();
        this._view.disableEditingTemporary();
    }

    enableEditingAgain() {
        this._enableEditing = true;
        this._properties.enableEditing();
        this._view.enableEditing();
    }

    unMarkLinks() {
        this._links.unMarkAll();
    }

    unMarkStates() {
        this._states.unMarkAll();
    }

    markOnlyState(stateId) {
        this._states.markOnly(stateId);
    }

    markOnlyLink(fromStateId, toStateId, transition) {
        this._links.mark(fromStateId, toStateId, transition);
    }

    isValid() {
        return this._links.allAreValid();
    }

    _onClick(position) {
        if (!this._links.isAnyLinkEditedAtTheMoment() && !this._states.isContextMenuShownAtTheMoment() && !this._links.isContextMenuShownAtTheMoment()) {
            this._states.addNewState(position);
        }
    }

    _getViewListener() {
        const listener = new Listener(Automaton.name, automatonViewListenerInterface);
        listener.set(automatonViewListenerInterface.onClick, position => this._onClick(position));
        return listener;
    }

    exportToXml() {
        if (!this.isValid()) {
            throw new Error('the automaton has at least one invalid link');
        }

        const xmlProperties = this._properties.exportToXml();
        const xmlStates = this._states.exportToXml();
        const xmlLinks = this._links.exportToXml();
        const content = xmlExporter.joinXmlElements([xmlProperties, xmlStates, xmlLinks]);
        return xmlExporter.buildXmlString()
            .addElement(XML_AUTOMATON_ID, content)
            .addAttr(XML_ATTR_WIDTH, this._dimensions.width)
            .addAttr(XML_ATTR_HEIGHT, this._dimensions.height)
            .addAttr(XML_ATTR_STATE_RADIUS, this._states._stateContext.stateRadius)
            .build()
            .build();
    }

    static parseAutomatonSettingsFromXml(xmlDoc) {
        const xmlAutomaton = xmlDoc.getFirstElementByTagName(XML_AUTOMATON_ID);
        const dimensions = {
            width: Number.parseFloat(xmlAutomaton.getAttribute(XML_ATTR_WIDTH)),
            height: Number.parseFloat(xmlAutomaton.getAttribute(XML_ATTR_HEIGHT))
        };
        const stateRadius = Number.parseFloat(xmlAutomaton.getAttribute(XML_ATTR_STATE_RADIUS));
        return {
            dimensions, stateRadius
        }
    }
};

export default Automaton;