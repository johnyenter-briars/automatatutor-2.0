'use strict';

import {ListenersSet} from '../../utils/listener';

const simulationPathListenerInterface = {
    onStateChanged: 'onStateChanged',
    onTransitionEntered: 'onTransitionEntered'
};

const newSimulationPathListenersSet = () => new ListenersSet(simulationPathListenerInterface);

export {newSimulationPathListenersSet, simulationPathListenerInterface};