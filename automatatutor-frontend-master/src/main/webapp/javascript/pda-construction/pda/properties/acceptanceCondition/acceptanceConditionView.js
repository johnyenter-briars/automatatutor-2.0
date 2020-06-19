'use strict';

import * as d3 from "d3";
import AcceptanceCondition from './acceptanceCondition';
import PropertyView from "../../../automaton/properties/propertyView/propertyView";
import {propertyViewListenerInterface} from "../../../automaton/properties/propertyView/propertyViewListener";
import {callIf} from "../../../utils/functionUtils";

const title = 'Acceptance condition: ';

const AcceptanceConditionView = class extends PropertyView {
    constructor(htmlElement, acceptanceConditionValue, immutable, listener, getEnableEditing) {
        super(htmlElement, title, listener, immutable, acceptanceConditionValue, getEnableEditing);
    }

    _getDisplayValueFromPropertyValue(value) {
        return value.description;
    }

    _getEditorValueFromPropertyValue(value) {
        return value.id;
    }

    _createHint() {
        if (this._immutable) {
            this._hint = d3.select(this._container).append('label').attr('class', 'property-hint').node();
        }
        else {
            this._hint = d3.select(this._form).append('label').attr('class', 'property-hint').node();
        }
    }

    _fillHint(value) {
        this._hint.innerHTML = this._getHint(value);
    }

    _getHint(value) {
        if (value.id === AcceptanceCondition.getAllAcceptanceConditionValues().finalStateAndEmptyStack.id) {
            return '=> final state and empty stack need not be reached at once for a word!';
        }
        return '';
    }

    _createSpecificEditor(form, getEnableEditing) {
        const res = d3.select(form).append('select').node();

        d3.select(res)
            .selectAll('option')
            .data(Object.values(AcceptanceCondition.getAllAcceptanceConditionValues())).enter()
            .append('option')
            .attr('value', condition => condition.id)
            .text(condition => condition.description);

        res.onchange = callIf(getEnableEditing, () => this._listeners.callForAll(propertyViewListenerInterface.onChanged, res.value));
        return res;
    }
};

export default AcceptanceConditionView;