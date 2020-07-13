'use strict';

import Property from '../../automaton/properties/property';
import {propertyViewListenerInterface} from "../../automaton/properties/propertyView/propertyViewListener";
import xmlExporter from "../../utils/xmlExporter";
import NumberOfTapesView from './numberOfTapesView';
import {Listener} from "../../utils/listener";

const XML_NUMBER_OF_TAPES_ID = 'numberOfTapes';
const numberOfTapes = 'Number of tapes: ';

const NumberOfTapes = class extends Property {
    constructor(immutable, numberOfTapes) {
        super(NumberOfTapes.getName(), immutable);
        this._numberOfTapes = numberOfTapes;
    }

    get numberOfTapes() {
        return this._numberOfTapes;
    }

    exportToXml() {
        return xmlExporter.buildXmlString().addElement(XML_NUMBER_OF_TAPES_ID, this.numberOfTapes.toString()).build().build();
    }

    static getName() {
        return 'numberOfTapes';
    }

    static fromXml(xmlDoc, immutable) {
        return new NumberOfTapes(immutable, parseInt(xmlDoc.getFirstElementByTagName(XML_NUMBER_OF_TAPES_ID).firstChild.nodeValue));
    }

    _createView() {
        this._view = new NumberOfTapesView(this._htmlElement, numberOfTapes, this._numberOfTapesViewListener(),
            this.numberOfTapes, this._immutable, () => this._automaton.enableEditing);
    }

    disableEditing() {
        this._view.disableEditing();
    }

    enableEditing() {
        this._view.enableEditing();
    }

    _onNumberOfTapesChanged() {
        this._automaton.links.updateValidity();
    }

    set numberOfTapes(value) {
        this._numberOfTapes = parseInt(value);
        this._onNumberOfTapesChanged();
        this._view.changeProperty(this.numberOfTapes);
    }

    _tryToSetNumberOfTapes(newNumberOfTapesString) {
        const newNumberOfTapes = parseInt(newNumberOfTapesString);
        if (isNaN(newNumberOfTapes)) {
            this._automaton.errorHandler('The given number of tapes is not a number');
            this._view.changeProperty(this.numberOfTapes);
        }
        else if(newNumberOfTapes <= 0) {
            this._automaton.errorHandler('The given number of tapes must be greater than 0');
            this._view.changeProperty(this.numberOfTapes);
        }
        else {
            this.numberOfTapes = newNumberOfTapes;
        }
    }

    _numberOfTapesViewListener() {
        const listener = new Listener(NumberOfTapes.name, propertyViewListenerInterface);
        listener.set(propertyViewListenerInterface.onChanged, numberOfTapes => this._tryToSetNumberOfTapes(numberOfTapes));
        return listener;
    }
};

export default NumberOfTapes;