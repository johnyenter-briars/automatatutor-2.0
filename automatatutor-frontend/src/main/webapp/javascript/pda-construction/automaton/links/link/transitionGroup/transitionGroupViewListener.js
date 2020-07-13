'use strict';

import {ListenersSet} from '../../../../utils/listener';

const transitionGroupViewListenerInterface = {
    onTransitionsChanged: 'onTransitionsChanged'
};

const newTransitionGroupViewListenersSet = () => new ListenersSet(transitionGroupViewListenerInterface);

export {newTransitionGroupViewListenersSet, transitionGroupViewListenerInterface};