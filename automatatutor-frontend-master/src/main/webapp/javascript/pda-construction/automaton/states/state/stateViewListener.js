'use strict';

import {ListenersSet} from '../../../utils/listener';

const stateViewListenerInterface = {
    onContextMenu: 'onContextMenu',
    onDblClick: 'onDblClick',
    onMouseOver: 'onMouseOver',
    onMouseOut: 'onMouseOut',
    onDrag: 'onDrag',
    onNewLinkCreationStarted: 'onNewLinkCreationStarted',
    onNewLinkCreationDragged: 'onNewLinkCreationDragged',
    onNewLinkCreationFinished: 'onNewLinkCreationFinished',
};

const newStateViewListenersSet = () => new ListenersSet(stateViewListenerInterface);

export {newStateViewListenersSet, stateViewListenerInterface};