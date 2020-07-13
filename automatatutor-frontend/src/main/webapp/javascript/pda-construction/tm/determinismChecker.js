'use strict';

import BucketMap from '../utils/bucketMap';

/**
 * checks if the given array of transitions violates the constraints of a deterministic multi tape TM
 * and changes the {isValidConcerningDeterminism}-property of each transition according to that
 * @param transitions array of {Transition}s
 */
const checkDeterminismOfTransitions = transitions => {
    const allTransitions = new BucketMap(transitions.map(t => [t.getInputLetters().join(';'), t]));

    allTransitions.values().forEach(transitions => {
        const areValid = transitions.length <= 1;
        transitions.forEach(t => t.isValidConcerningDeterminism = areValid);
    });
};

export default {checkDeterminismOfTransitions: checkDeterminismOfTransitions};