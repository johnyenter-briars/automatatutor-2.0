'use strict';

import {ListenersSet} from '../../utils/listener';

const simulationControlListenerInterface = {
    onStepClicked: 'onStepClicked',
    onResetTapesClicked: 'onResetTapesClicked'
};

const newSimulationControlListenersSet = () => new ListenersSet(simulationControlListenerInterface);

export {newSimulationControlListenersSet, simulationControlListenerInterface};