'use strict';

import * as d3 from "d3";
import PropertyView from "../../../automaton/properties/propertyView/propertyView";
import {propertyViewListenerInterface} from "../../../automaton/properties/propertyView/propertyViewListener";
import {callIf} from "../../../utils/functionUtils";

const title = 'Deterministic (DPDA): ';

const DeterminismView = class extends PropertyView {
    constructor(htmlElement, determinism, immutable, listener, getEnableEditing) {
        super(htmlElement, title, listener, immutable, determinism, getEnableEditing);
    }

    _changeEditor(value) {
        this._editor.checked = value;
    }

    _createSpecificEditor(form, getEnableEditing) {
        const res = d3.select(form).append('input').attr('type', 'checkbox').node();
        res.onchange = callIf(getEnableEditing, () => this._listeners.callForAll(propertyViewListenerInterface.onChanged, res.checked));
        return res;
    }
};

export default DeterminismView;