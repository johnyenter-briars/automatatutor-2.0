'use strict';

import * as d3 from "d3";
import PropertyView from "../propertyView/propertyView";
import {propertyViewListenerInterface} from "../propertyView/propertyViewListener";
import {callIf} from "../../../utils/functionUtils";

const inputSeparator = ' ';
const displaySeparator = ', ';
const placeholder = 'separated by spaces';

const buttonTitle = 'apply';

const AlphabetView = class extends PropertyView {
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
        return '{' + value.join(displaySeparator) + '}';
    }

    _getEditorValueFromPropertyValue(value) {
        return value.join(inputSeparator);
    }

    _createSpecificEditor(form, getEnableEditing) {
        const res = d3.select(form).append('input').attr('type', 'text').attr('placeholder', placeholder).node();
        const readInput = htmlElement => htmlElement.value.split(inputSeparator).filter(v => v);
        this._applyButton = d3.select(form).append('input').attr('type', 'button').attr('value', buttonTitle)
            .on('click', callIf(getEnableEditing,
                () => this._listeners.callForAll(propertyViewListenerInterface.onChanged, readInput(this._editor))))
            .node();
        return res;
    }
};

export default AlphabetView;