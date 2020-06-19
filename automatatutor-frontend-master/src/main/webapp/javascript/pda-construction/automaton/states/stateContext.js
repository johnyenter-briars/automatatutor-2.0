'use strict';

import {newStateContextMenu} from "./stateContextMenu";
import {newStateListenersSet} from './state/stateListeners';

/**
 * describes the common properties of the states of one automaton
 */
const StateContext = class {
    /**
     * @param svgContainer svg element where the {State}s should be drawn
     * @param contextMenuSvgContainer svg container for the {ContextMenu}
     * @param getIfStateCanBeFinal function that returns if a state can be final
     * @param stateRadius radius of the circle of a {State}
     * @param getEnableEditing function returning whether the {PDA} can be edited
     * @param dimensions object with width and height defining the sizes of the area
     */
    constructor(svgContainer, contextMenuSvgContainer, getIfStateCanBeFinal, stateRadius, getEnableEditing, dimensions) {
        this._getEnableEditing = getEnableEditing;
        this._svgContainer = svgContainer;
        this._getIfStateCanBeFinal = getIfStateCanBeFinal;
        this._stateRadius = stateRadius;
        this._listeners = newStateListenersSet();
        this._contextMenu = newStateContextMenu(contextMenuSvgContainer);
        this._dimensions = dimensions;
    }

    get dimensions() {
        return this._dimensions;
    }

    get getEnableEditing() {
        return this._getEnableEditing;
    }

    get svgContainer() {
        return this._svgContainer;
    }

    get stateRadius() {
        return this._stateRadius;
    }

    get listeners() {
        return this._listeners;
    }

    get contextMenu() {
        return this._contextMenu;
    }

    get stateCanBeFinal() {
        return this._getIfStateCanBeFinal();
    }
};

export default StateContext;