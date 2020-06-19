'use strict';

import {ListenersSet} from '../../utils/listener';

const linksListenerInterface = {
    onLinkChanged: 'onLinkChanged',
};

const newLinksListenersSet = () => new ListenersSet(linksListenerInterface);

export {newLinksListenersSet, linksListenerInterface};