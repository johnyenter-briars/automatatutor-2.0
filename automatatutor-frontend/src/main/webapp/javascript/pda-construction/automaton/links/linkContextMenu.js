'use strict';

import contextMenu from '../../utils/contextMenu';

const contextMenuIds = {
    remove: 'remove'
};

const newLinkContextMenu = svgContainer => contextMenu.createContextMenu(svgContainer)
    .addItem(contextMenuIds.remove, 'remove')
    .build();

export {newLinkContextMenu, contextMenuIds};