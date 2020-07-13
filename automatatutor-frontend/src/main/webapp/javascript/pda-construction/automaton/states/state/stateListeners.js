'use strict';

import {ListenersSet} from '../../../utils/listener';

const stateListenerInterface = {
    onStateMoved: 'onStateMoved',
    onStateRemoved: 'onStateRemoved',
    onDragStarted: 'onDragStarted',
    onDragged: 'onDragged',
    onDragStopped: 'onDragStopped',
    onStateHovered: 'onStateHovered',
    onStateHoveredOut: 'onStateHoveredOut'
};

const newStateListenersSet = () => new ListenersSet(stateListenerInterface);

export {newStateListenersSet, stateListenerInterface};
