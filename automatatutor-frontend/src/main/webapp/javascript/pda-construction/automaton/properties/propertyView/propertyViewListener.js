'use strict';

import {ListenersSet} from '../../../utils/listener';

const propertyViewListenerInterface = {
    onChanged: 'onChanged',
};

const newPropertyViewListenersSet = () => new ListenersSet(propertyViewListenerInterface);

export {newPropertyViewListenersSet, propertyViewListenerInterface};