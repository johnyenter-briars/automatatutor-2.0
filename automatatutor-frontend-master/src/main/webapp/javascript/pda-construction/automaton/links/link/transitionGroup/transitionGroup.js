'use strict';

import {newTransitionGroupListenersSet, transitionGroupListenerInterface} from './transitionGroupListener';
import {DefaultTransition} from './transition';
import xmlExporter from '../../../../utils/xmlExporter';
import TransitionGroupView from "./transitionGroupView";
import {transitionGroupViewListenerInterface} from "./transitionGroupViewListener";
import {Listener} from "../../../../utils/listener";
import {linkTransformerListenerInterface} from "../linkTransformer/linkTransformerListener";

const XML_TRANSITION_GROUP_ID = 'transitionGroup';

const removeDuplicates = (list, getId) => Array.from(new Map(list.map(element => [getId(element), element])).values());

/**
 * contains all {Transition}s of a {Link}
 * @type {TransitionGroup}
 */
const TransitionGroup = class {
    /**
     * @param Transition class that defines a single transition; it must inherit from {AbstractTransition}
     * @param svgContainer svg element where the {Transition}s should be displayed
     * @param textArea text area for editing the transitions
     * @param properties {Properties} of the {PDA}
     * @param getEnableEditing function returning whether the {PDA} can be edited
     * @param listener {Listener}
     */
    constructor(Transition, svgContainer, textArea, properties, getEnableEditing, listener) {
        this._Transition = Transition;
        this._properties = properties;
        this._listeners = newTransitionGroupListenersSet();
        this._listeners.add(listener);
        this._transitions = [];

        this._view = new TransitionGroupView(svgContainer, textArea, () => this._getVisibleTransitions(), () => this._transitions,
            getEnableEditing, this._getViewListener());

        this._defaultTransition = new DefaultTransition();
        this._setTransitions([]);
    }

    get transitions() {
        return this._transitions;
    }

    _getViewListener() {
        const listener = new Listener(TransitionGroup.name, transitionGroupViewListenerInterface);
        listener.set(transitionGroupViewListenerInterface.onTransitionsChanged, transitionStrings => this._setTransitionsByStrings(transitionStrings));
        return listener;
    }

    unMark() {
        this._transitions.forEach(transition => transition.unMark());
    }

    mark(transition) {
        const transitions = this._transitions.filter(t => t.equals(transition));
        if (transitions.length === 0) {
            throw new Error('the given transition was not found');
        }
        transitions[0].mark();
    }

    isEditedAtTheMoment() {
        return this._view.isEdited;
    }

    updateValidity() {
        this._transitions.forEach(transition => transition.updateValidity());
    }

    get view() {
        return this._view;
    }

    isValid() {
        return this._transitions.every(transition => transition.isValid());
    }

    exportToXml() {
        return xmlExporter.buildXmlString().addElement(XML_TRANSITION_GROUP_ID,
            xmlExporter.joinXmlElements(this._transitions.map(transition => transition.exportToXml())))
            .build()
            .build();
    }

    addTransitionsFromXml(xmlElement) {
        const xmlTransitionGroup = xmlElement.firstChild;
        if (xmlTransitionGroup.tagName === XML_TRANSITION_GROUP_ID) {
            this._setTransitions(Array.from(xmlTransitionGroup.childNodes).map(xmlTransition => this._Transition.createFromXml(xmlTransition, this._properties)));
        }
        else {
            throw `the tag-name ${xmlTransitionGroup.tagName} was not ${XML_TRANSITION_GROUP_ID} as required`;
        }
    }

    listeners() {
        return this._listeners;
    }

    add(otherTransitionGroup) {
        this._setTransitions(this._transitions.concat(otherTransitionGroup._transitions));
    }

    _getVisibleTransitions() {
        return this._transitions.length === 0 ? [this._defaultTransition] : this._transitions;
    }

    _setTransitionsByStrings(transitionStrings) {
        this._setTransitions(transitionStrings.filter(s => this._Transition.representsTransition(s)).map(s => new this._Transition(s, this._properties)));
    }

    _setTransitions(transitions) {
        this._transitions = removeDuplicates(transitions, transition => transition.toString());
        this._view.updateOnTransitionsChanged();
        this._listeners.callForAll(transitionGroupListenerInterface.onTransitionsChanged, transitions.length);
        this._listeners.callForAll(transitionGroupListenerInterface.onDiameterChanged, this._view.getDiagonal());
    }

    getLinkTransformerListener() {
        const listener = new Listener(TransitionGroup.name, linkTransformerListenerInterface);
        listener.set(linkTransformerListenerInterface.onMiddlePositionChanged, position => this._view._updatePosition(position));
        return listener;
    }
};

export default TransitionGroup;