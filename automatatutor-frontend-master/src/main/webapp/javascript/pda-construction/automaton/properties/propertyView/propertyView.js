'use strict';

import * as d3 from "d3";
import {newPropertyViewListenersSet} from "./propertyViewListener";

const PropertyView = class {
    constructor(htmlParentElement, title, listener, immutable, initPropertyValue, getEnableEditing) {
        this._immutable = immutable;
        this._listeners = newPropertyViewListenersSet();
        this._listeners.add(listener);
        this._createView(htmlParentElement, title, getEnableEditing);
        this.changeProperty(initPropertyValue);
    }

    disableEditing() {
        if (!this._immutable) {
            this._editor.disabled = true;
            this._disableApplyButton();
        }
    }

    enableEditing() {
        if (!this._immutable) {
            this._editor.disabled = false;
            this._enableApplyButton();
        }
    }

    _disableApplyButton(){
    }

    _enableApplyButton() {
    }

    changeProperty(value) {
        if (this._immutable) {
            this._changeDisplay(this._getDisplayValueFromPropertyValue(value));
            this._fillHint(value);
        }
        else {
            this._changeEditor(this._getEditorValueFromPropertyValue(value));
            this._fillHint(value);
        }
    }

    _getDisplayValueFromPropertyValue(value) {
        return value;
    }

    _getEditorValueFromPropertyValue(value) {
        return value;
    }

    _changeDisplay(value) {
        this._display.innerHTML = value;
    }

    _changeEditor(value) {
        this._editor.value = value;
    }

    _createDisplay(htmlParentElement, title) {
        this._container = d3.select(htmlParentElement)
            .append('li').append('div').attr('class', 'property-display').node();
        d3.select(this._container).append('label').attr('class', 'title').text(title);
        this._display = d3.select(this._container).append('div').node();
    }

    _createHint(){
    }

    _fillHint(){
    }

    _createSpecificEditor(form) {
        throw Error('not implemented');
    }

    _createEditor(htmlParentElement, title, getEnableEditing) {
        this._form = d3.select(htmlParentElement)
            .append('li').append('form').attr('class', 'property-display').node();
        d3.select(this._form).append('label').attr('class', 'title').text(title);
        this._editor = this._createSpecificEditor(this._form, getEnableEditing);
    }

    _createView(htmlParentElement, title, getEnableEditing) {
        if (this._immutable) {
            this._createDisplay(htmlParentElement, title);
        }
        else {
            this._createEditor(htmlParentElement, title, getEnableEditing);
        }
        this._createHint();
    }
};

export default PropertyView;