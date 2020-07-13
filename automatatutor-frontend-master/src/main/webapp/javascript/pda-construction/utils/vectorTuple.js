'use strict';

import {Vector} from './vector';

const VectorTuple = class {
    constructor(start, end) {
        if (!(start instanceof Vector && end instanceof Vector)) {
            throw `start and end have to be vectors but found ${start} and ${end}`;
        }
        this._start = start;
        this._end = end;
    }

    get start() {
        return this._start;
    }

    get end() {
        return this._end;
    }
};

export default VectorTuple;