'use strict';

import * as d3 from 'd3';
import {VariableListenerWrapper} from "./listener";

const MARGIN = 10;
const FONT_SIZE = 13;
const MENU_ITEM_HEIGHT = FONT_SIZE + MARGIN;

import './contextMenuStyles.css';

/**
 * a single item in the context-menu
 */
const ContextMenuItem = class {
    /**
     * @param id identifier unique among one context-menu
     * @param name name displayed in the context-menu
     */
    constructor(id, name) {
        this.id = id;
        this._name = name;
    }

    /**
     * draws the context-menu-item
     * @param listener instance of SingleListener
     * @param index number of the context-meu-item, beginning with 0
     * @param svgContainer svg-element where this item should be put
     * @param contextMenu
     */
    show(listener, index, svgContainer, contextMenu) {
        this._svgElement = d3.select(svgContainer).append('g')
            .attr('class', 'menuItem')
            .attr('transform', `translate(0,${index * MENU_ITEM_HEIGHT})`).node();
        d3.select(this._svgElement).append('text').text(this._name)
            .style('font', `${FONT_SIZE}px`)
            .attr('dy', FONT_SIZE + MARGIN / 2)
            .attr('dx', MARGIN);

        d3.select(this._svgElement).append('rect').attr('height', MENU_ITEM_HEIGHT)
            .attr('dx', MARGIN / 2)
            .on('click', () => {
                contextMenu.variableListenerWrapper.call(listener, this.id);
                contextMenu.hide();
            });
    }

    /**
     * sets the with of the background-rectangle of the item
     * @param width
     */
    setWidth(width) {
        d3.select(this._svgElement).select('rect').attr('width', width);
    }

    /**
     * get width of the text of the item
     * @return {number}
     */
    getWidth() {
        return d3.select(this._svgElement).select('text').node().getComputedTextLength() + 2 * MARGIN;
    }
};

/**
 * menu to show on right-click on an svg-element
 */
const ContextMenu = class {
    /**
     * @param svgContainer svg-element where this context-menu should be put
     * @param items array of ContextMenuItem
     */
    constructor(svgContainer, items) {
        this._svgContainer = svgContainer;
        this._items = items;
        this._isShown = false;

        const contextMenuListenerInterface = {};
        this._items.forEach(item => contextMenuListenerInterface[item.id] = item.id);
        this._variableListenerWrapper = new VariableListenerWrapper(contextMenuListenerInterface);
    }

    get isShown() {
        return this._isShown;
    }

    get variableListenerWrapper() {
        return this._variableListenerWrapper;
    }

    /**
     * displays a context-menu with all context-menu-items whose ids are in the given set
     * @param setOfItemIdsToShow set of ids of context-menu-items, that should be shown
     * @param listener instance of SingleListener
     * @param position where to show the context-menu
     */
    showSome(setOfItemIdsToShow, listener, position) {
        if (![...setOfItemIdsToShow].every(id => this._items.some(item => item.id === id))) {
            throw Error('not all ids are valid for this context menu');
        }

        this._show(this._items.filter(item => setOfItemIdsToShow.has(item.id)), listener, position);
    }

    /**
     * displays a context-menu with all context-menu-items of this context-menu
     * @param listener see method "showSome"
     * @param position see method "showSome"
     */
    showAll(listener, position) {
        this._show(this._items, listener, position);
    }

    _show(itemsToShow, listener, position) {
        if (listener.listenerInterface !== this.variableListenerWrapper.listenerInterface) {
            throw new Error('the given listener has the wrong interface');
        }
        this._isShown = true;
        this._a = d3.select(this._svgContainer).append('a').attr('xlink:href', '#0').attr('class', 'contextmenu').node();
        this._svgElement = d3.select(this._a).append('g')
            .attr('class', 'contextMenu')
            .attr('transform', `translate(${position.x},${position.y})`).node();
        d3.select(this._svgElement).append('rect').attr('height', itemsToShow.length * MENU_ITEM_HEIGHT);
        itemsToShow.forEach((item, index) => item.show(listener, index, this._svgElement, this));
        const maxWidth = itemsToShow.reduce((acc, item) => Math.max(acc, item.getWidth()), 0);
        itemsToShow.forEach(item => item.setWidth(maxWidth));
        d3.select(this._svgElement).select('rect').attr('width', maxWidth);

        this._a.onblur = () => this.hide();
        this._a.focus();
    }

    hide() {
        this._isShown = false;
        this._a.onblur = () => {
        };
        d3.select(this._a).remove();
    }
};

/**
 * builder for a context-menu
 * @param svgContainer
 */
const createContextMenu = svgContainer => {
    const itemIds = new Set();
    const items = [];
    const builder = {
        /**
         * add an item to the context-menu
         * @param id identifier unique among one context-menu
         * @param name name displayed in the context-menu
         */
        addItem: (id, name) => {
            if (itemIds.has(id)) {
                throw new Error('The id of a menu item has to be unique');
            }
            items.push(new ContextMenuItem(id, name));
            itemIds.add(id);
            return builder;
        },
        build: () => {
            return new ContextMenu(svgContainer, items);
        }
    };
    return builder;
};

export default {createContextMenu};