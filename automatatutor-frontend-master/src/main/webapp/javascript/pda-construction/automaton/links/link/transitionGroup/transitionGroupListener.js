'use strict';

import {ListenersSet} from '../../../../utils/listener';

const transitionGroupListenerInterface = {
    onTransitionsChanged: 'onTransitionsChanged',
    onDiameterChanged: 'onDiameterChanged'
};

const newTransitionGroupListenersSet = () => new ListenersSet(transitionGroupListenerInterface);

export {newTransitionGroupListenersSet, transitionGroupListenerInterface};