'use strict';

import AcceptanceConditionView from "./acceptanceConditionView";
import {Listener} from "../../../utils/listener";
import {propertyViewListenerInterface} from "../../../automaton/properties/propertyView/propertyViewListener";
import Property from '../../../automaton/properties/property';
import xmlExporter from "../../../utils/xmlExporter";

const XML_ACCEPTANCE_CONDITION_ID = 'acceptanceCondition';

const AcceptanceCondition = class extends Property {
    constructor(immutable, acceptanceConditionId) {
        super(AcceptanceCondition.getName(), immutable);
        this._acceptanceConditionValue = getAcceptanceConditionValueById(acceptanceConditionId);
    }

    _createView() {
        this._view = new AcceptanceConditionView(this._htmlElement, this.acceptanceConditionValue,
            this._immutable, this._viewListener(), () => this._automaton.enableEditing);
    }

    exportToXml() {
        return xmlExporter.buildXmlString().addElement(XML_ACCEPTANCE_CONDITION_ID,
            this.acceptanceConditionValue.id).build().build();
    }

    static fromXml(xmlDoc, immutable) {
        return new AcceptanceCondition(immutable, xmlDoc.getFirstElementByTagName(XML_ACCEPTANCE_CONDITION_ID).firstChild.nodeValue);
    }

    static getName() {
        return 'acceptanceCondition';
    }

    disableEditing() {
        this._view.disableEditing();
    }

    enableEditing() {
        this._view.enableEditing();
    }

    _onAcceptanceConditionChanged() {
        this._automaton.states.updateAllFinalMarker();
    }

    set acceptanceConditionValue(acceptanceConditionId) {
        this._acceptanceConditionValue = getAcceptanceConditionValueById(acceptanceConditionId);
        this._onAcceptanceConditionChanged();
        this._view.changeProperty(this.acceptanceConditionValue);
    }

    get acceptanceConditionValue() {
        return this._acceptanceConditionValue;
    }

    static getAllAcceptanceConditionValues() {
        return getAcceptanceConditionValues();
    }

    _viewListener() {
        const listener = new Listener(AcceptanceCondition.name, propertyViewListenerInterface);
        listener.set(propertyViewListenerInterface.onChanged, id => this.acceptanceConditionValue = id);
        return listener;
    }
};

/**
 * represents the acceptance condition of a PDA
 */
const AcceptanceConditionValue = class {
    constructor(id, allowFinalStates, description) {
        this._id = id;
        this._allowFinalStates = allowFinalStates;
        this._description = description;
    }

    get id() {
        return this._id;
    }

    get allowFinalStates() {
        return this._allowFinalStates;
    }

    get description() {
        return this._description;
    }
};

const getAcceptanceConditionValues = () => {
    return {
        finalState: new AcceptanceConditionValue('finalState', true, 'final state'),
        emptyStack: new AcceptanceConditionValue('emptyStack', false, 'empty stack'),
        finalStateAndEmptyStack: new AcceptanceConditionValue('finalStateAndEmptyStack', true, 'final state AND empty stack')
    };
};

const acceptanceConditionValues = getAcceptanceConditionValues();

const getAcceptanceConditionValueById = id => {
    if (Object.keys(acceptanceConditionValues).includes(id)) {
        return acceptanceConditionValues[id];
    }
    else {
        throw new Error('invalid acceptance condition id');
    }
};

export default AcceptanceCondition;