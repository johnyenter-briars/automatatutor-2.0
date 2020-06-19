'use strict';

import contextMenu from '../../utils/contextMenu';

const contextMenuIds = {
    remove: 'remove',
    nonFinal: 'nonFinal',
    final: 'final'
};

const newStateContextMenu = svgContainer => contextMenu.createContextMenu(svgContainer)
    .addItem(contextMenuIds.remove, 'remove')
    .addItem(contextMenuIds.nonFinal, 'make non-final')
    .addItem(contextMenuIds.final, 'make final')
    .build();

const getItemIdsForState = (state, stateCanBeFinal) => {
    const itemIds = new Set();
    if (!state.isInitial) {
        itemIds.add(contextMenuIds.remove);
    }
    if (stateCanBeFinal) {
        itemIds.add(state._isFinal ? contextMenuIds.nonFinal : contextMenuIds.final);
    }
    return itemIds;
};

export {newStateContextMenu, getItemIdsForState, contextMenuIds};