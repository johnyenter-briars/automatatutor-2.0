'use strict';

import * as d3 from "d3";
import {newSimulationControlListenersSet, simulationControlListenerInterface} from "./simulationControlListener";

const SimulationControl = class {
    constructor(htmlContainer, ...listeners) {
        this._listeners = newSimulationControlListenersSet();
        listeners.forEach(listener => this._listeners.add(listener));
        this._createView(htmlContainer);
    }

    _createView(htmlContainer) {
        this._htmlElement = d3.select(htmlContainer).append('div').attr('id', 'control-area').node();

        const stepButton = d3.select(this._htmlElement).append('button')
            .on('click', () => this._listeners.callForAll(simulationControlListenerInterface.onStepClicked)).node();
        stepButton.innerHTML = 'Step';

        this._hint = d3.select(this._htmlElement).append('text').node();

        const resetButton = d3.select(this._htmlElement).append('button')
            .on('click', () => this._listeners.callForAll(simulationControlListenerInterface.onResetTapesClicked)).node();
        resetButton.innerHTML = 'Reset tapes';
    }

    showHint() {
        d3.select(this._hint).text('--> now click on a transition');
    }

    hideHint() {
        d3.select(this._hint).text('');
    }
};

export default SimulationControl;