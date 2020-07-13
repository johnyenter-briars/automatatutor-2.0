'use strict';

import DeterminismView from "./determinismView";
import {Listener} from "../../../utils/listener";
import {propertyViewListenerInterface} from "../../../automaton/properties/propertyView/propertyViewListener";
import Property from '../../../automaton/properties/property';
import xmlExporter from "../../../utils/xmlExporter";
import {linksListenerInterface} from "../../../automaton/links/linksListener";
import {flatten} from "../../../utils/arrayUtils";
import determinismChecker from '../../determinismChecker';

const XML_DETERMINISM_ID = 'determinism';

const Determinism = class extends Property {
    constructor(immutable, isDeterministic) {
        super(Determinism.getName(), immutable);
        this._isDeterministic = isDeterministic;
    }

    _updateDeterministicValidityOfLinksStartingAt(state) {
        //TODO: maybe already with currentStates
        const linksBelongingToState = this._automaton.links.allLinks.filter(link => link.states.startState === state);
        const transitionsBelongingToState = flatten(linksBelongingToState.map(link => link.transitions));
        determinismChecker.checkDeterminismOfTransitions(transitionsBelongingToState);
        linksBelongingToState.forEach(link => link.updateViewValidity());
    };

    linksListener() {
        const listener = new Listener(Determinism.name, linksListenerInterface);
        listener.set(linksListenerInterface.onLinkChanged, startState => {
            if (this._isDeterministic) {
                this._updateDeterministicValidityOfLinksStartingAt(startState);
            }
        });
        return listener;
    }

    _createView() {
        this._view = new DeterminismView(this._htmlElement, this.isDeterministic,
            this._immutable, this._viewListener(), () => this._automaton.enableEditing);
    }

    exportToXml() {
        return xmlExporter.buildXmlString().addElement(XML_DETERMINISM_ID, this.isDeterministic.toString()).build().build();
    }

    static fromXml(xmlDoc, immutable) {
        return new Determinism(immutable, xmlDoc.getFirstElementByTagName(XML_DETERMINISM_ID).firstChild.nodeValue === 'true');
    }

    static getName() {
        return 'determinism';
    }

    disableEditing() {
        this._view.disableEditing();
    }

    enableEditing() {
        this._view.enableEditing();
    }

    _onDeterminismChanged() {
        if (this._isDeterministic) {
            this._automaton.states.states.forEach(state => this._updateDeterministicValidityOfLinksStartingAt(state));
        }
        else {
            this._automaton.links.allLinks.forEach(link => link.transitions.forEach(t => t.isValidConcerningDeterminism = true));
            this._automaton.links.allLinks.forEach(link => link.updateViewValidity());
        }
    }

    get isDeterministic() {
        return this._isDeterministic;
    }

    set isDeterministic(value) {
        this._isDeterministic = value;
        this._onDeterminismChanged();
        this._view.changeProperty(this.isDeterministic);
    }

    _viewListener() {
        const listener = new Listener(Determinism.name, propertyViewListenerInterface);
        listener.set(propertyViewListenerInterface.onChanged, isDeterministic => this.isDeterministic = isDeterministic);
        return listener;
    }
};

export default Determinism;