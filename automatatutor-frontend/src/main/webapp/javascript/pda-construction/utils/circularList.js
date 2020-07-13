'use strict';

const mod = (number, n) => ((number % n) + n) % n;

/**
 * immutable circular list, where an index higher than the number of elements is applied to the beginning of the list again
 */
const CircularList = class {
    /**
     * @param arr values in the list
     * @param defaultValue is returned, if the list is empty
     */
    constructor(arr, defaultValue) {
        this._array = Array.from(arr);
        this._defaultValue = defaultValue;
    }

    map(fun) {
        new CircularList(this._array.map(fun));
    }

    getOrDefault(index) {
        if (this._array.length === 0) {
            return this._defaultValue;
        }
        return this._array[mod(index, this._array.length)];
    }

    /**
     * zips this array with itself shifted by the given number
     * @param shiftNumber 0 means, that the same elements are combined; can be negative
     * @param zipFunction function that takes two elements and returns anything
     * @return array with the calculated values of the zipFunction
     */
    zipWithSelf(shiftNumber, zipFunction) {
        shiftNumber = mod(shiftNumber, this._array.length);
        const otherArray = this._array.slice(shiftNumber).concat(this._array.slice(0, shiftNumber));
        return this._array.map((el, i) => zipFunction(el, otherArray[i]));
    }
};

export default CircularList;