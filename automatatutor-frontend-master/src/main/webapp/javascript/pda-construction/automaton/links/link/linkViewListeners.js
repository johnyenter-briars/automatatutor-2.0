'use strict';

import {ListenersSet} from '../../../utils/listener';

const linkViewListenerInterface = {
    onContextMenu: 'onContextMenu',
    onDragStarted: 'onDragStarted',
    onDragged: 'onDragged',
    onDragStopped: 'onDragStopped'
};

const newLinkViewListenersSet = () => new ListenersSet(linkViewListenerInterface);

export {newLinkViewListenersSet, linkViewListenerInterface};