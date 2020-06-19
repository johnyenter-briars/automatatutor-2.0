'use strict';

import alphabetParser from './alphabetParser';
import alphabetCheckers from './alphabetChecker';
import AlphabetView from "../../automaton/properties/generic-alphabet/alphabetView";
import {Listener} from "../../utils/listener";
import Property from '../../automaton/properties/property';
import {propertyViewListenerInterface} from "../../automaton/properties/propertyView/propertyViewListener";
import xmlExporter from "../../utils/xmlExporter";

const XML_ALPHABET_ID = 'alphabet';
const XML_SYMBOL_ID = 'symbol';
const alphabet = 'Alphabet: ';

const Alphabet = class extends Property {
    constructor(immutable, alphabet) {
        super(Alphabet.getName(), immutable);
        this._alphabet = alphabetParser.parseAlphabet(alphabet);
    }

    exportToXml() {
        return xmlExporter.buildXmlString().addElement(XML_ALPHABET_ID,
            xmlExporter.buildXmlStringFromArray(this.alphabetWithoutEmptyTape, XML_SYMBOL_ID)).build().build();
    }

    static getName() {
        return 'alphabet';
    }

    static fromXml(xmlDoc, immutable) {
        return new Alphabet(immutable, xmlDoc.getArrayOfChildrenContents(XML_ALPHABET_ID));
    }

    _createView() {
        this._view = new AlphabetView(this._htmlElement, alphabet, this._alphabetViewListener(),
            this.alphabetWithoutEmptyTape, this._immutable, () => this._automaton.enableEditing);
    }

    disableEditing() {
        this._view.disableEditing();
    }

    enableEditing() {
        this._view.enableEditing();
    }

    get alphabet() {
        return this._alphabet;
    }

    get alphabetWithoutEmptyTape() {
        return this._alphabet.slice(0, this._alphabet.length - 1);
    }

    _onAlphabetChanged() {
        this._automaton.links.updateValidity();
    }

    set alphabet(value) {
        this._alphabet = alphabetParser.parseAlphabet(value);
        this._onAlphabetChanged();
        this._view.changeProperty(this.alphabetWithoutEmptyTape);
    }

    _tryToSetAlphabet(newAlphabet) {
        const res = alphabetCheckers.checkAlphabet(newAlphabet);
        if (res.isCorrect) {
            this.alphabet = newAlphabet;
        }
        else {
            this._automaton.errorHandler(res.message);
            this._view.changeProperty(this.alphabetWithoutEmptyTape);
        }
    }

    _alphabetViewListener() {
        const listener = new Listener(Alphabet.name, propertyViewListenerInterface);
        listener.set(propertyViewListenerInterface.onChanged, alphabet => this._tryToSetAlphabet(alphabet));
        return listener;
    }
};

export default Alphabet;