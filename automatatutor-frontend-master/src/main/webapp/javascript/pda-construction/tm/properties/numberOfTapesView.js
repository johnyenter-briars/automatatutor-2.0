'use strict';

import * as d3 from "d3";
import PropertyView from "../../automaton/properties/propertyView/propertyView";
import {propertyViewListenerInterface} from "../../automaton/properties/propertyView/propertyViewListener";
import {callIf} from "../../utils/functionUtils";

const buttonTitle = 'apply';

const NumberOfTapesView = class extends PropertyView {
    constructor(htmlParentElement, title, listener, alphabet, immutable, getEnableEditing) {
        super(htmlParentElement, title, listener, immutable, alphabet, getEnableEditing);
    }

    _disableApplyButton(){
        this._applyButton.disabled = true;
    }

    _enableApplyButton() {
        this._applyButton.disabled = false;
    }

    _getDisplayValueFromPropertyValue(value) {
        return value.toString();
    }

    _getEditorValueFromPropertyValue(value) {
        return value.toString();
    }

    _createSpecificEditor(form, getEnableEditing) {
        const res = d3.select(form).append('input').attr('type', 'text').node();
        const readInput = htmlElement => htmlElement.value;
        this._applyButton = d3.select(form).append('input').attr('type', 'button').attr('value', buttonTitle)
            .on('click', callIf(getEnableEditing,
                () => this._listeners.callForAll(propertyViewListenerInterface.onChanged, readInput(this._editor))))
            .node();
        return res;
    }
};

export default NumberOfTapesView;