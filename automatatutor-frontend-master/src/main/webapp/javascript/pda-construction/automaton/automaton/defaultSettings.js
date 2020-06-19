'use strict';

const dimensions = () => ({height: 540, width: 670});
const stateRadius = () => 30;
const errorHandler = () => msg => alert(msg);

export default {
    dimensions, stateRadius, errorHandler
};