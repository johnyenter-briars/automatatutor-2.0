'use strict';

import {flatten} from '../utils/arrayUtils';
import BucketMap from '../utils/bucketMap';
import specialCharacters from './specialCharacters';

const EPSILON = specialCharacters.epsilonEdit;

/**
 * checks if the given array of transitions violates the constraints of a deterministic PDA
 * and changes the {isValidConcerningDeterminism}-property of each transition according to that
 * @param transitions array of {Transition}s
 */
const checkDeterminismOfTransitions = transitions => {
    const allEpsilonTransitions = new BucketMap(transitions.filter(t => t.inputLetter === EPSILON).map(t => [t.inputStackSymbol, t]));
    const allNonEpsilonTransitions = new BucketMap(transitions.filter(t => t.inputLetter !== EPSILON).map(t => [t.getInputPart(), t]));
    flatten(allEpsilonTransitions.values()).forEach(t => t.isValidConcerningDeterminism = true);

    allNonEpsilonTransitions.values().forEach(transitions => {
        const epsilonTransitions = allEpsilonTransitions.get(transitions[0].inputStackSymbol);
        const areValid = transitions.length + epsilonTransitions.length <= 1;
        transitions.forEach(t => t.isValidConcerningDeterminism = areValid);
        if (!areValid) {
            //mark only if false, otherwise keep existing marking of this check
            epsilonTransitions.forEach(t => t.isValidConcerningDeterminism = false);
        }
    });

    allEpsilonTransitions.values().forEach(transitions => {
        const areValid = transitions.length <= 1;
        if (!areValid) {
            //mark only if false, otherwise keep existing marking of this check
            transitions.forEach(t => t.isValidConcerningDeterminism = false);
        }
    });
};

export default {checkDeterminismOfTransitions: checkDeterminismOfTransitions};