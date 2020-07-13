'use strict';

import {ListenersSet} from '../../utils/listener';

const simulationControlListenerInterface = {
    onToBeginClicked: 'onToBeginClicked',
    onToEndClicked: 'onToEndClicked',
    onStepForwardClicked: 'onStepForwardClicked',
    onStepBackClicked: 'onStepBackClicked',
    onSimulationEndClicked: 'onSimulationEndClicked'
};

const newSimulationControlListenersSet = () => new ListenersSet(simulationControlListenerInterface);

export {newSimulationControlListenersSet, simulationControlListenerInterface};