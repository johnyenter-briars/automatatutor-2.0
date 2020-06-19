'use strict';

import Automaton from '../automaton/automaton/automaton.js';
import pdaNotes from './pdaNotes';
import defaultProperties from "./properties/defaultProperties";
import defaultSettings from "../automaton/automaton/defaultSettings";
import XmlDocument from '../utils/xmlDocument';
import {simulationControlListenerInterface} from "./simulation/simulationControlListener";
import {tryToCreateSimulationPathFromXml} from "./simulation/simulationPath";
import {simulationPathListenerInterface} from "./simulation/simulationPathListener";
import {Listener} from "../utils/listener";
import SimulationControl from "./simulation/simulationControl";
import AcceptanceCondition from "./properties/acceptanceCondition/acceptanceCondition";
import Alphabet from './properties/alphabet/alphabet';
import StackAlphabet from './properties/stackAlphabet/stackAlphabet';
import Determinism from './properties/determinism/determinism';
import {Transition} from "./transition";

import './pdaStyles.css';

const PDA = class {
    constructor(svgCanvas, dimensions, stateRadius, alphabet, stackAlphabet, acceptanceCondition,
                determinism, errorHandler) {

        this._automaton = new Automaton(svgCanvas, dimensions, stateRadius, errorHandler,
            () => acceptanceCondition.acceptanceConditionValue.allowFinalStates, pdaNotes, Transition);

        this._automaton.addProperty(alphabet);
        this._automaton.addProperty(stackAlphabet);
        this._automaton.addProperty(acceptanceCondition);
        this._automaton.addProperty(determinism);

        this._automaton.links.addListener(determinism.linksListener());

        this._errorHandler = errorHandler;

        this._simulationContainer = this._automaton.addContainer();

        this._simulationControl = new SimulationControl(svgCanvas, this._createSimulationControlListener());
    }

    exportToXml() {
        return this._automaton.exportToXml();
    }

    isValid() {
        return this._automaton.isValid();
    }

    /**
     * starts a simulation of the current pda for the given path; during a simulation, editing the pda is not possible
     * @param simulationPathXml xml string with the path or an error; structure of the xml-string: every node stands for
     * a specific point in the simulation process and contains the configuration at that point and the transition,
     * that lead to that config; so the first node does not have such a transition
     <div>
     <path>
     <node>
     <config>
     <state>0</state>
     <word>a</word>
     <stack>Z</stack>
     </config>
     </node>
     <node>
     <transition>a,Z/</transition>
     <config>
     <state>1</state>
     <word/>
     <stack/>
     </config>
     </node>
     </path>
     </div>
     */
    startSimulation(simulationPathXml) {
        this.endSimulation();
        const path = tryToCreateSimulationPathFromXml(simulationPathXml, this._automaton.svgContainer,
            this._simulationContainer,
            this._automaton.dimensions, this._createSimulationPathListener());
        if (path.successful) {
            this._automaton.disableEditingTemporary();
            this._simulationControl.addListener(path.createSimulationControlListener());
            this._simulationControl.show();

            this._simulationPath = path;
        }
        else {
            this._errorHandler(path.errorMsg);
        }
    }

    _onSimulationStateChanged(stateId) {
        this._automaton.unMarkLinks();
        this._automaton.markOnlyState(stateId);
    }

    _createSimulationPathListener() {
        const listener = new Listener(PDA.name, simulationPathListenerInterface);
        listener.set(simulationPathListenerInterface.onStateChanged, stateId => this._onSimulationStateChanged(stateId));
        listener.set(simulationPathListenerInterface.onTransitionEntered,
            (fromStateId, toStateId, transition) => this._automaton.markOnlyLink(fromStateId, toStateId, transition));
        return listener;
    }

    _createSimulationControlListener() {
        const listener = new Listener(PDA.name, simulationControlListenerInterface);
        listener.set(simulationControlListenerInterface.onSimulationEndClicked, () => this.endSimulation());
        return listener;
    }

    endSimulation() {
        if (this._simulationPath) {
            this._simulationPath.remove();
            this._automaton.unMarkLinks();
            this._automaton.unMarkStates();
            this._simulationControl.hide();
            this._automaton.enableEditingAgain();
            this._simulationPath = null;
        }
    }

    /**
     * creates a mutable pda with default properties, which are mutable. This method is for initializing a PDA for
     * creating one
     * @param svgCanvas
     * @return {PDA}
     */
    static createMutablePDAWithDefaultProperties(svgCanvas) {
        const alphabet = new Alphabet(false, defaultProperties.alphabet());
        const stackAlphabet = new StackAlphabet(false, defaultProperties.stackAlphabet());
        const acceptanceCondition = new AcceptanceCondition(false, defaultProperties.acceptanceCondition());
        const determinism = new Determinism(false, false);

        return new PDA(svgCanvas,
            defaultSettings.dimensions(), defaultSettings.stateRadius(),
            alphabet, stackAlphabet, acceptanceCondition, determinism,
            defaultSettings.errorHandler());
    }

    /**
     * creates a mutable pda from xml, whose properties are mutable. This method is for loading a pda from xml in
     * order to edit it
     * @param xmlString
     * @param svgCanvas
     * @return {PDA}
     */
    static createMutablePDAFromXMLWithMutableProperties(xmlString, svgCanvas) {
        const xmlDoc = new XmlDocument(xmlString);
        const res = Automaton.parseAutomatonSettingsFromXml(xmlDoc);

        const alphabet = Alphabet.fromXml(xmlDoc, false);
        const stackAlphabet = StackAlphabet.fromXml(xmlDoc, false);
        const acceptanceCondition = AcceptanceCondition.fromXml(xmlDoc, false);
        const determinism = Determinism.fromXml(xmlDoc, false);

        const pda = new PDA(svgCanvas,
            res.dimensions, res.stateRadius,
            alphabet, stackAlphabet, acceptanceCondition, determinism,
            defaultSettings.errorHandler());
        pda._automaton.addStatesFromXml(xmlDoc);
        pda._automaton.addLinksFromXml(xmlDoc);
        return pda;
    }

    /**
     * creates an immutable pda from the given xml-string, whose properties are immutable. This method
     * is for creating a completely readonly pda
     * @param xmlString
     * @param svgCanvas
     * @return {PDA}
     */
    static createImmutablePDAFromXMLWithImmutableProperties(xmlString, svgCanvas) {
        const xmlDoc = new XmlDocument(xmlString);
        const res = Automaton.parseAutomatonSettingsFromXml(xmlDoc);

        const alphabet = Alphabet.fromXml(xmlDoc, true);
        const stackAlphabet = StackAlphabet.fromXml(xmlDoc, true);
        const acceptanceCondition = AcceptanceCondition.fromXml(xmlDoc, true);
        const determinism = Determinism.fromXml(xmlDoc, true);

        const pda = new PDA(svgCanvas,
            res.dimensions, res.stateRadius,
            alphabet, stackAlphabet, acceptanceCondition, determinism,
            defaultSettings.errorHandler());
        pda._automaton.addStatesFromXml(xmlDoc);
        pda._automaton.addLinksFromXml(xmlDoc);
        pda._automaton.disableEditing();
        return pda;
    }

    /**
     * creates an immutable pda with the properties of the pda in the given xml-string, whose properties are immutable.
     * @param xmlString
     * @param svgCanvas
     * @param xmlPdaAttempt
     * @return {PDA}
     */
    static createMutablePDAFromXMLWithImmutableProperties(xmlString, svgCanvas, xmlPdaAttempt = null) {
        const xmlDoc = new XmlDocument(xmlString);
        const res = Automaton.parseAutomatonSettingsFromXml(xmlDoc);

        const alphabet = Alphabet.fromXml(xmlDoc, true);
        const stackAlphabet = StackAlphabet.fromXml(xmlDoc, true);
        const acceptanceCondition = AcceptanceCondition.fromXml(xmlDoc, true);
        const determinism = Determinism.fromXml(xmlDoc, true);

        const pda = new PDA(svgCanvas,
            res.dimensions, res.stateRadius,
            alphabet, stackAlphabet, acceptanceCondition, determinism,
            defaultSettings.errorHandler());
        if (xmlPdaAttempt) {
            const attemptDoc = new XmlDocument(xmlPdaAttempt);
            pda._automaton.addStatesFromXml(attemptDoc);
            pda._automaton.addLinksFromXml(attemptDoc);
        }
        return pda;
    }

    /**
     * creates an immutable pda with the properties of the pda in the given xml-string, whose properties are immutable.
     * @param xmlString
     * @param svgCanvas
     * @param xmlPdaAttempt
     * @return {PDA}
     */
    static createMutablePDAFromXMLWithOnlyStackAlphabetMutable(xmlString, svgCanvas, xmlPdaAttempt = null) {
        const xmlDoc = new XmlDocument(xmlString);
        const res = Automaton.parseAutomatonSettingsFromXml(xmlDoc);
        let attemptDoc;
        let attempt;
        if (xmlPdaAttempt) {
            attemptDoc = new XmlDocument(xmlPdaAttempt);
            attempt = Automaton.parseAutomatonSettingsFromXml(attemptDoc);
        }

        const alphabet = Alphabet.fromXml(xmlDoc, true);
        const stackAlphabet = xmlPdaAttempt ? StackAlphabet.fromXml(attemptDoc, false) : new StackAlphabet(false, defaultProperties.stackAlphabet());
        const acceptanceCondition = AcceptanceCondition.fromXml(xmlDoc, true);
        const determinism = Determinism.fromXml(xmlDoc, true);

        const pda = new PDA(svgCanvas, res.dimensions, res.stateRadius,
            alphabet, stackAlphabet, acceptanceCondition, determinism,
            defaultSettings.errorHandler());
        if (xmlPdaAttempt) {
            pda._automaton.addStatesFromXml(attemptDoc);
            pda._automaton.addLinksFromXml(attemptDoc);
        }
        return pda;
    }
};

export {PDA};