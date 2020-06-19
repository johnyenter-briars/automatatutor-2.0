'use strict';

import VectorTuple from '../../utils/vectorTuple';

const StateTuple = class {
    constructor(startState, endState) {
        this._startState = startState;
        this._endState = endState;
    }

    get startState() {
        return this._startState;
    }

    get endState() {
        return this._endState;
    }

    set startState(value) {
        this._startState = value;
    }

    set endState(value) {
        this._endState = value;
    }

    static getStartStateSelector() {
        return 'startState';
    }

    static getEndStateSelector() {
        return 'endState';
    }

    getCircleTuple() {
        return new VectorTuple(this.startState.circle, this.endState.circle);
    }

    clone() {
        return new StateTuple(this.startState, this.endState);
    }

    contains(state) {
        return this.startState === state || this.endState === state;
    }
};

export default StateTuple;