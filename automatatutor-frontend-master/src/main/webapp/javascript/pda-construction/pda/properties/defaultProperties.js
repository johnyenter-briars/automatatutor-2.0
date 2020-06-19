'use strict';

import AcceptanceCondition from "./acceptanceCondition/acceptanceCondition";

const alphabet = () => ['a', 'b'];
const stackAlphabet = () => ['Z', 'Y', 'X'];
const acceptanceCondition = () => Object.keys(AcceptanceCondition.getAllAcceptanceConditionValues())[0];

export default {
    alphabet, stackAlphabet, acceptanceCondition
};