'use strict';

import alphabetParser from '../alphabetParser';
import alphabetCheckers from '../alphabetChecker';
import AlphabetView from "../../../automaton/properties/generic-alphabet/alphabetView";
import {Listener} from "../../../utils/listener";
import Property from '../../../automaton/properties/property';
import {propertyViewListenerInterface} from '../../../automaton/properties/propertyView/propertyViewListener';
import xmlExporter from "../../../utils/xmlExporter";

const XML_STACK_ALPHABET_ID = 'stackAlphabet';
const XML_SYMBOL_ID = 'symbol';
const stackAlphabet = 'Stack alphabet (the first symbol is the initial one): ';

const StackAlphabet = class extends Property{
    constructor(immutable, stackAlphabet) {
        super(StackAlphabet.getName(), immutable);
        this._stackAlphabet = alphabetParser.parseStackAlphabet(stackAlphabet);
    }

    _createView() {
        this._view = new AlphabetView(this._htmlElement, stackAlphabet,
            this._viewListener(), this.stackAlphabet, this._immutable, () => this._automaton.enableEditing);
    }

    static getName() {
        return 'stackAlphabet';
    }

    static fromXml(xmlDoc, immutable) {
        return new StackAlphabet(immutable, xmlDoc.getArrayOfChildrenContents(XML_STACK_ALPHABET_ID));
    }

    exportToXml() {
        return xmlExporter.buildXmlString().addElement(XML_STACK_ALPHABET_ID,
            xmlExporter.buildXmlStringFromArray(this.stackAlphabet, XML_SYMBOL_ID)).build().build();
    }

    disableEditing() {
        this._view.disableEditing();
    }

    enableEditing() {
        this._view.enableEditing();
    }

    _onStackAlphabetChanged() {
        this._automaton.links.updateValidity();
    }

    get stackAlphabet() {
        return this._stackAlphabet;
    }

    set stackAlphabet(value) {
        this._stackAlphabet = alphabetParser.parseStackAlphabet(value);
        this._onStackAlphabetChanged();
        this._view.changeProperty(this.stackAlphabet);
    }

    _tryToSetStackAlphabet(newStackAlphabet) {
        const res = alphabetCheckers.checkStackAlphabet(newStackAlphabet);
        if (res.isCorrect) {
            this.stackAlphabet = newStackAlphabet;
        }
        else {
            this._automaton.errorHandler(res.message);
            this._view.changeProperty(this.stackAlphabet);
        }
    }

    _viewListener() {
        const listener = new Listener(StackAlphabet.name, propertyViewListenerInterface);
        listener.set(propertyViewListenerInterface.onChanged, stackAlphabet => this._tryToSetStackAlphabet(stackAlphabet));
        return listener;
    }
};

export default StackAlphabet;