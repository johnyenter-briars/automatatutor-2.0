'use strict';

import TransitionView from "./transitionView";

const TRANSITION_DESCRIPTION = '(double click to edit)';

const AbstractTransition = class {
    constructor() {
    }

    isDefault() {
        return false;
    }

    createView(svgTextContainer, rowNumber) {
        this._view = new TransitionView(svgTextContainer, rowNumber, this.displayingString());
        this._updateView();
    }

    unMark() {
        this._view.unMark();
    }

    mark() {
        this._view.mark();
    }

    removeView() {
        this._view.remove();
    }

    getWidth() {
        return this._view.getWidth();
    }

    /**
     * maybe override
     */
    _updateView() {
    }


    /**
     * override
     */
    updateValidity() {
    }

    /**
     * override
     * checks if the given string represents a transition, that means if it has the format that is used
     * by the {toString()} method
     */
    static representsTransition() {
    }

    /**
     * override
     */
    isValid() {
        throw new Error('not implemented');
    }

    /**
     * override
     */
    equals() {
        throw new Error('not implemented');
    }

    /**
     * override
     */
    exportToXml() {
        throw new Error('not implemented');
    }

    /**
     * override
     */
    displayingString() {
        throw new Error('not implemented');
    }

    /**
     * override
     */
    toString() {
        throw new Error('not implemented');
    }
};

/**
 * represent the transition with an explanation how to edit the transitions of link
 * @type {DefaultTransition}
 */
const DefaultTransition = class extends AbstractTransition {
    constructor() {
        super();
        this.description = TRANSITION_DESCRIPTION;
    }

    displayingString() {
        return this.toString();
    }

    toString() {
        return this.description;
    }

    isDefault() {
        return true;
    }
};

export {DefaultTransition, AbstractTransition};
