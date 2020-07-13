'use strict';
const removeAllSpace = str => str.replace(/\s/g, '');
const splitBySeparator = (str, separator) => str.split(separator);

/**
 * @param textInput contains the transitions, one in each row (separated by \n);
 * a transitions is structured like this: [input alphabet-symbol],[popped stack-symbol]/[stack-symbols to push];
 * lines that do not match this pattern are ignored (and removed)
 */
const parseLines = textInput => parseBySeparator(textInput, /\r\n|\r|\n/);

const parseBySeparator = (textInput, separator) => {
    const strings = splitBySeparator(textInput, separator);
    const cleanedLines = strings.map(removeAllSpace);
    return strings.filter(line => line.length > 0);
};

export {parseLines, parseBySeparator};