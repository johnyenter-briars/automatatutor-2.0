'use strict';

import SimulationView from "./simulationView";
import {newSimulationPathListenersSet, simulationPathListenerInterface} from "./simulationPathListener";
import specialCharacters from "../specialCharacters";
import {Listener} from "../../utils/listener";
import {simulationControlListenerInterface} from "./simulationControlListener";

const divName = 'div';
const pathName = 'path';
const nodeName = 'node';
const transitionName = 'transition';
const configName = 'config';
const stateName = 'state';
const wordName = 'word';
const stackName = 'stack';
const errorName = 'error';
const textName = '#text';

const nodeHasName = (node, name) => node.nodeName === name;

const assertHasName = (xmlNode, expectedName) => {
    if (!nodeHasName(xmlNode, expectedName)) {
        throw new Error(`the xml node ${xmlNode.nodeName} was not ${expectedName} as expected`);
    }
};

const assertHasNumberOfElement = (nodeList, number) => {
    if (nodeList.length !== number) {
        throw new Error(`there were found ${nodeList.length} nodes, but exact ${number} was expected`);
    }
};

const getOnly = nodeList => {
    assertHasNumberOfElement(nodeList, 1);
    return nodeList[0];
};

const assertHasMaxNumberOfElement = (nodeList, max) => {
    if (nodeList.length > max) {
        throw new Error(`there were found ${nodeList.length} nodes, but at most ${max} was expected`);
    }
};

const getTextValueInside = node => {
    if (node.childNodes.length === 0) {
        return '';
    }
    const textNode = getOnly(node.childNodes);
    assertHasName(textNode, textName);
    return textNode.nodeValue;
};

const Transition = class {
    constructor(inputLetter, inputStackSymbol, outputSymbols) {
        this._inputLetter = inputLetter;
        this._inputStackSymbol = inputStackSymbol;
        this._outputSymbols = outputSymbols;
    }

    get inputLetter() {
        return this._inputLetter;
    }

    get inputStackSymbol() {
        return this._inputStackSymbol;
    }

    get outputSymbols() {
        return this._outputSymbols;
    }

    static fromXml(xmlTransition) {
        assertHasName(xmlTransition, transitionName);
        const value = getTextValueInside(xmlTransition);
        return new Transition(value.charAt(0), value.charAt(2), value.substring(4));
    }
};

const Config = class {
    constructor(stateId, word, stack) {
        this._stateId = stateId;
        this._word = word.length === 0 ? specialCharacters.epsilonDisplay : word;
        this._stack = stack;
    }

    get stateId() {
        return this._stateId;
    }

    get word() {
        return this._word;
    }

    get stack() {
        return this._stack;
    }

    static fromXml(xmlConfig) {
        assertHasName(xmlConfig, configName);
        assertHasNumberOfElement(xmlConfig.childNodes, 3);
        const stateId = parseInt(getTextValueInside(getOnly(xmlConfig.getElementsByTagName(stateName))));
        const word = getTextValueInside(getOnly(xmlConfig.getElementsByTagName(wordName)));
        const stack = getTextValueInside(getOnly(xmlConfig.getElementsByTagName(stackName))).split('');
        return new Config(stateId, word, stack);
    }
};

const Node = class {
    constructor(transitionToHere, config) {
        this._transitionToHere = transitionToHere;
        this._config = config;
    }

    get transitionToHere() {
        return this._transitionToHere;
    }

    get config() {
        return this._config;
    }

    static fromXml(xmlNode) {
        assertHasName(xmlNode, nodeName);
        assertHasMaxNumberOfElement(xmlNode.childNodes, 2);
        const transition = xmlNode.childNodes.length === 1 ? null : Transition.fromXml(getOnly(xmlNode.getElementsByTagName(transitionName)));
        const config = Config.fromXml(getOnly(xmlNode.getElementsByTagName(configName)));
        return new Node(transition, config);
    }
};

const SimulationPathParsingResult = class {
    constructor(successful) {
        this._successful = successful;
    }

    get successful() {
        return this._successful;
    }
};

const SimulationPath = class extends SimulationPathParsingResult {
    constructor(nodePath, containerOfSvgs, pdaSvgElement, dimensions, listener, stopMessage) {
        super(true);
        this._nodePath = nodePath;
        this._stopMessage = stopMessage;
        this._currentNodeIndex = 0;
        this._view = new SimulationView(containerOfSvgs, pdaSvgElement, dimensions);
        this._nextStepIsPreparing = true;
        this._listeners = newSimulationPathListenersSet();
        this._listeners.add(listener);
        this._tryToChangeNode(this._currentNodeIndex);
    }

    get nodePath() {
        return this._nodePath;
    }

    step() {
        if (this._nextStepIsPreparing) {
            this._nextStepIsPreparing = false;
            this._prepareStep();
        }
        else {
            this._nextStepIsPreparing = true;
            this._stepForward();
        }
    }

    _stepForward() {
        this._tryToChangeNode(this._currentNodeIndex + 1);
    }

    _getMaxIndex() {
        return this._nodePath.length - 1;
    }

    _tryToChangeNode(targetIndex) {
        if (targetIndex >= 0 && targetIndex <= this._getMaxIndex()) {
            this._currentNodeIndex = targetIndex;
            const config = this._nodePath[this._currentNodeIndex].config;
            this._listeners.callForAll(simulationPathListenerInterface.onStateChanged, config.stateId);

            const stackLengthBefore = targetIndex === 0 ? 1 : this._nodePath[targetIndex - 1].config.stack.length;
            this._view.changeConfig(config, config.stack.length - stackLengthBefore + 1);
        }
        else if (targetIndex > this._getMaxIndex()) {
            this._showStopMessage();
        }
    }

    _showStopMessage() {
        if (this._stopMessage !== null) {
            alert(this._stopMessage);
        }
    }

    _prepareStep() {
        if (this._currentNodeIndex < this._getMaxIndex()) {
            const currentState = this._nodePath[this._currentNodeIndex].config.stateId;
            const nextState = this._nodePath[this._currentNodeIndex + 1].config.stateId;
            const transition = this._nodePath[this._currentNodeIndex + 1].transitionToHere;
            this._listeners.callForAll(simulationPathListenerInterface.onTransitionEntered, currentState, nextState, transition);
            this._view.prepareConfigChange(transition.inputLetter === specialCharacters.epsilonEdit);
        }
        else {
            this._showStopMessage();
        }
    }

    stepBack() {
        this._tryToChangeNode(this._currentNodeIndex - 1);
        this._nextStepIsPreparing = true;
    }

    toBegin() {
        this._tryToChangeNode(0);
        this._nextStepIsPreparing = true;
    }

    toEnd() {
        this._tryToChangeNode(this._getMaxIndex());
    }

    remove() {
        this._view.remove();
    }

    createSimulationControlListener() {
        const listener = new Listener(SimulationPath.name, simulationControlListenerInterface);
        listener.set(simulationControlListenerInterface.onToBeginClicked, () => this.toBegin());
        listener.set(simulationControlListenerInterface.onToEndClicked, () => this.toEnd());
        listener.set(simulationControlListenerInterface.onStepBackClicked, () => this.stepBack());
        listener.set(simulationControlListenerInterface.onStepForwardClicked, () => this.step());
        return listener;
    }

    static FromXml(xmlSimulationPath, containerOfSvgs, pdaSvgElement, dimensions, listener) {
        assertHasName(xmlSimulationPath, pathName);
        const stopMessage = xmlSimulationPath.getAttribute('stop');
        const nodePath = Array.from(xmlSimulationPath.childNodes).map(xmlNode => Node.fromXml(xmlNode));
        return new SimulationPath(nodePath, containerOfSvgs, pdaSvgElement, dimensions, listener, stopMessage);
    }
};

const SimulationError = class extends SimulationPathParsingResult {
    constructor(errorMsg) {
        super(false);
        this._errorMsg = errorMsg;
    }

    get errorMsg() {
        return this._errorMsg;
    }
};

const tryToCreateSimulationPathFromXml = (simulationPathXmlString, containerOfSvgs, pdaSvgElement, dimensions, listener) => {
    const parser = new window.DOMParser();
    const xmlDoc = parser.parseFromString(simulationPathXmlString, 'text/xml');

    const div = xmlDoc.documentElement;
    assertHasName(div, divName);
    assertHasNumberOfElement(div.childNodes, 1);
    const root = div.childNodes[0];
    if (nodeHasName(root, errorName)) {
        return new SimulationError(root.childNodes[0].nodeValue);
    }
    return SimulationPath.FromXml(root, containerOfSvgs, pdaSvgElement, dimensions, listener);
};

export {tryToCreateSimulationPathFromXml};