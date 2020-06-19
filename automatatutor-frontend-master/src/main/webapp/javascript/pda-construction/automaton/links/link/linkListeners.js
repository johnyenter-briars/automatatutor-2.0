'use strict';

import {ListenersSet} from '../../../utils/listener';

const linkListenerInterface = {
    onLinkRemoved: 'onLinkRemoved',
    onLinkPositionChanged: 'onLinkPositionChanged',
    onEndStatesChanged: 'onEndStatesChanged',
    onCurrentEndStatesChanged: 'onCurrentEndStatesChanged',
    onDragStarted: 'onDragStarted',
    onDragged: 'onDragged',
    onDragStopped: 'onDragStopped',
    onTransitionsChanged: 'onTransitionsChanged',
};

const newLinkListenersSet = () => new ListenersSet(linkListenerInterface);

export {newLinkListenersSet, linkListenerInterface};