'use strict';

const Property = class {
    constructor(name, immutable) {
        this._name = name;
        this._immutable = immutable;
    }

    attachToAutomaton(automaton, htmlElement) {
        this._automaton = automaton;
        this._htmlElement = htmlElement;
        this._createView();
    }

    get name() {
        return this._name;
    }

    exportToXml() {
        throw new Error('not implemented');
    }

    _createView() {
        throw new Error('not implemented');
    }

    disableEditing() {
        throw new Error('not implemented');
    }

    enableEditing() {
        throw new Error('not implemented');
    }
};

export default Property;