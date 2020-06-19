'use strict';

const BucketMap = class {
    /**
     * @param arr array of two-element-arrays which consist of key (1st element) and value (2nd element)
     */
    constructor(arr) {
        this._map = new Map();
        arr.forEach(keyValuePair => this.set(keyValuePair[0], keyValuePair[1]));
    }

    set(key, value) {
        if (this._map.has(key)) {
            this._map.get(key).push(value);
        }
        else {
            this._map.set(key, [value]);
        }
    }

    keys() {
        return [...this._map.keys()];
    }

    get(key) {
        return this._map.get(key) || [];
    }

    values() {
        return [...this._map.values()];
    }
};

export default BucketMap;