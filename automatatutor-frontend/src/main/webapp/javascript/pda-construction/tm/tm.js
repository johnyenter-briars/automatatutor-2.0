'use strict';

import {Transition} from "./transition";
import tmNotes from "./tmNotes";
import Automaton from "../automaton/automaton/automaton";
import defaultSettings from "../automaton/automaton/defaultSettings";
import Alphabet from "./properties/alphabet";
import NumberOfTapes from "./properties/numberOfTapes";
import Simulation from "./simulation/simulation";
import {simulationControlListenerInterface} from "./simulation/simulationControlListener";
import {Listener} from "../utils/listener";
import {flatten} from "../utils/arrayUtils";
import determinismChecker from "./determinismChecker";
import {linksListenerInterface} from "../automaton/links/linksListener";

import './tmStyles.css';
import {callIf} from "../utils/functionUtils";
import XmlDocument from "../utils/xmlDocument";

const TuringMachine = class {
    constructor(svgCanvas, dimensions, stateRadius, alphabet, numberOfTapes, errorHandler, wordsToSimulateContainer) {
        this._automaton = new Automaton(svgCanvas, dimensions, stateRadius, errorHandler,
            () => true, tmNotes, Transition);

        this._automaton.addProperty(alphabet);
        this._automaton.addProperty(numberOfTapes);

        this._automaton.links.addListener(this._createLinksListener());

        this._errorHandler = errorHandler;

        this._simulation = new Simulation(svgCanvas, dimensions.width, numberOfTapes.numberOfTapes,
            this._createSimulationControlListener(), wordsToSimulateContainer);
        this._resetCurrentSimulationState();

        this._isSimulationAllowed = false;
    }

    allowSimulation() {
        this._isSimulationAllowed = true;
    }

    _createLinksListener() {
        const listener = new Listener(TuringMachine.name, linksListenerInterface);
        listener.set(linksListenerInterface.onLinkChanged, startState => {
            const linksBelongingToState = this._automaton.links.allLinks.filter(link => link.states.startState === startState);
            const transitionsBelongingToState = flatten(linksBelongingToState.map(link => link.transitions));
            determinismChecker.checkDeterminismOfTransitions(transitionsBelongingToState);
            linksBelongingToState.forEach(link => link.updateViewValidity());
        });
        return listener;
    }

    _createSimulationControlListener() {
        const simulationNotAllowedError = 'Submit a solution first';
        const listener = new Listener(Simulation.name, simulationControlListenerInterface);
        listener.set(simulationControlListenerInterface.onResetTapesClicked,
            callIf(() => this._isSimulationAllowed, () => this.resetTapes(),
                () => this._errorHandler(simulationNotAllowedError)));
        listener.set(simulationControlListenerInterface.onStepClicked,
            callIf(() => this._isSimulationAllowed, () => this._step(),
                () => this._errorHandler(simulationNotAllowedError)));
        return listener;
    }

    _resetCurrentSimulationState() {
        this._changeCurrentSimulationState(this._automaton.states.initialState);
    }

    _changeCurrentSimulationState(value) {
        this._currentSimulationState = value;
        this._automaton._states.markOnly(this._currentSimulationState.id);
    }

    resetTapes() {
        this._simulation.resetTapes();
        this._resetCurrentSimulationState();

        this._simulation.hideClickTransitionHint();

        this._automaton.links.makeAllTransitionsUnClickAble();
        this._automaton.enableEditingAgain();
    }

    _step() {
        if (this.isValid()) {
            this._automaton.disableEditingTemporary();

            const currentLetters = this._simulation.currentLetters;

            this._simulation.showClickTransitionHint();
            this._automaton.links.makeTransitionsClickAbleStartingAt(this._currentSimulationState,
                transition => transition.getInputLetters().every((letter, i) => letter === currentLetters[i]),
                (link, transition) => {
                    this._changeCurrentSimulationState(link.states.endState);
                    this._simulation.enterTransition(transition);
                    this._automaton.enableEditingAgain();
                    this._simulation.hideClickTransitionHint();
                },
                () => {
                    this._errorHandler('There is no transition that can be entered');
                    this._automaton.enableEditingAgain();
                });
        }
        else {
            this._errorHandler('The TM is not valid.');
        }
    }

    exportToXml() {
        return this._automaton.exportToXml();
    }

    isValid() {
        return this._automaton.isValid();
    }

    static createMutableTMWithImmutableProperties(alphabet, numberOfTapes, svgCanvas, wordsToSimulateContainer, xmlTM = null) {
        const alphabetProperty = new Alphabet(true, alphabet);
        const numberOfTapesProperty = new NumberOfTapes(true, parseInt(numberOfTapes));

        const tm = new TuringMachine(svgCanvas, defaultSettings.dimensions(), defaultSettings.stateRadius(),
            alphabetProperty, numberOfTapesProperty,
            defaultSettings.errorHandler(), wordsToSimulateContainer);
        if (xmlTM) {
            const attemptDoc = new XmlDocument(xmlTM);
            tm._automaton.addStatesFromXml(attemptDoc);
            tm._automaton.addLinksFromXml(attemptDoc);
            tm._resetCurrentSimulationState();
            tm.allowSimulation();
        }
        return tm;
    }
};

export {TuringMachine};