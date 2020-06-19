'use strict';

const callIf = (getCondition, fun, otherwiseFun = () => void 0) => () => {
    if (getCondition()) {
        fun();
    }
    else {
        otherwiseFun();
    }
};

export {callIf};