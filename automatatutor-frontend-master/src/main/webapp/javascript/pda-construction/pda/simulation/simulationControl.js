'use strict';

import * as d3 from "d3";
import {newSimulationControlListenersSet, simulationControlListenerInterface} from "./simulationControlListener";

const placeholder = 'Enter the word to simulate';

const SimulationControl = class {
    constructor(htmlContainer, listener) {
        this._listeners = newSimulationControlListenersSet();
        this._listeners.add(listener);
        this._createView(htmlContainer);
    }

    addListener(listener) {
        this._listeners.add(listener);
    }

    _createView(htmlContainer) {
        this._htmlElement = d3.select(htmlContainer).append('div').attr('class', 'hide').node();
        const controlArea = d3.select(this._htmlElement).append('div').attr('id', 'control-area').node();

        const form = d3.select(controlArea).append('form').node();

        const div = d3.select(form).append('div').attr('id', 'control-buttons').node();
        d3.select(div).append('input').attr('type', 'button').attr('value', '|<')
            .on('click', () => this._listeners.callForAll(simulationControlListenerInterface.onToBeginClicked));
        d3.select(div).append('input').attr('type', 'button').attr('value', '<')
            .on('click', () => this._listeners.callForAll(simulationControlListenerInterface.onStepBackClicked));
        d3.select(div).append('input').attr('type', 'button').attr('value', '>')
            .on('click', () => this._listeners.callForAll(simulationControlListenerInterface.onStepForwardClicked));
        d3.select(div).append('input').attr('type', 'button').attr('value', '>|')
            .on('click', () => this._listeners.callForAll(simulationControlListenerInterface.onToEndClicked));

        d3.select(form).append('input').attr('type', 'button').attr('value', 'End simulation')
            .on('click', () => this._listeners.callForAll(simulationControlListenerInterface.onSimulationEndClicked));
    }

    show() {
        d3.select(this._htmlElement).attr('class', '');
    }

    hide() {
        d3.select(this._htmlElement).attr('class', 'hide');
    }
};

export default SimulationControl;