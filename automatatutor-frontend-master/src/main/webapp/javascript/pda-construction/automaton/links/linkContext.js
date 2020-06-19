'use strict';

import {newLinkContextMenu} from './linkContextMenu';
import {newLinkListenersSet} from './link/linkListeners';

/**
 * contains the common properties of the {Link}s of one automaton
 */
const LinkContext = class {
    /**
     * @param svgContainer svg-element where the {Link} should be drawn
     * @param contextMenuSvgContainer svg container for the {ContextMenu}
     * @param properties {Properties} of the {PDA}
     * @param getAllLinks returns an array with all {Link}s
     * @param getStartLink returns the {StartLink} of the {PDA}
     * @param getEnableEditing function returning whether editing the {PDA} is enabled
     * @param textArea text area for editing the transitions
     * @param Transition class that defines a single transition; it must inherit from {AbstractTransition}
     */
    constructor(svgContainer, contextMenuSvgContainer, properties, getAllLinks, getStartLink, getEnableEditing, textArea,
                Transition) {
        this._svgContainer = svgContainer;
        this._getEnableEditing = getEnableEditing;
        this._listeners = newLinkListenersSet();
        this._contextMenu = newLinkContextMenu(contextMenuSvgContainer);
        this._properties = properties;
        this._getAllLinks = () => getAllLinks().concat([getStartLink()]);
        this._checkIfMustShareStates = link => getAllLinks().filter(otherLink => link.isMutualTo(otherLink.currentStates)).length > 0;
        this._textArea = textArea;
        this._Transition = Transition;
    }

    get Transition() {
        return this._Transition;
    }

    get textArea() {
        return this._textArea;
    }

    get getEnableEditing() {
        return this._getEnableEditing;
    }

    get properties() {
        return this._properties;
    }

    get svgContainer() {
        return this._svgContainer;
    }

    /**
     * @return {ListenersSet} common listeners of the {Link}s
     */
    get listeners() {
        return this._listeners;
    }

    get contextMenu() {
        return this._contextMenu;
    }

    get getAllLinks() {
        return this._getAllLinks;
    }

    /**
     * @return {(function(*): boolean)|*} function that checks for a given {Link} if there exists another link in the
     * opposite direction
     */
    get checkIfMustShareStates() {
        return this._checkIfMustShareStates;
    }
};

export default LinkContext;