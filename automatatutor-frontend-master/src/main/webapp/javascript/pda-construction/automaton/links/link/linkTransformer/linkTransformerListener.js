'use strict';

import {ListenersSet} from '../../../../utils/listener';

const linkTransformerListenerInterface = {
    onMiddlePositionChanged: 'onMiddlePositionChanged',
    onPathChanged: 'onPathChanged',
    onAngleChanged: 'onAngleChanged',
    onRotationCenterChanged: 'onRotationCenterChanged'
};

const newLinkTransformerListenersSet = () => new ListenersSet(linkTransformerListenerInterface);

export {newLinkTransformerListenersSet, linkTransformerListenerInterface};