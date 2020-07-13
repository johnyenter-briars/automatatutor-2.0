'use strict';

const flatten = arr => [].concat.apply([], arr);

const maximumOfArray = (arr, propertyFun) => arr.reduce((maxSoFar, element) => Math.max(maxSoFar, propertyFun(element)), 0);

export {flatten, maximumOfArray};