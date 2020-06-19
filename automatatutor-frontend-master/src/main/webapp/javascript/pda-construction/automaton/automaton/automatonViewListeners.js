'use strict';

import {ListenersSet} from '../../utils/listener';

const automatonViewListenerInterface = {
    onClick: 'onClick',
};

const newAutomatonViewListenersSet = () => new ListenersSet(automatonViewListenerInterface);

export {newAutomatonViewListenersSet, automatonViewListenerInterface};