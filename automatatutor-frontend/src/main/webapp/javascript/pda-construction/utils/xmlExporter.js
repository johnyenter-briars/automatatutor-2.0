'use strict';

const OPENING_START_MARKER = '<';
const CLOSING_START_MARKER = '</';
const END_MARKER = '>';
const LINE_BREAK = '\n';

const createOpeningTag = (title, attributes) => {
    let xmlAttributes = attributes.map(attr => `${attr.key}="${attr.value}"`).join(' ');
    xmlAttributes = xmlAttributes.length > 0 ? ' ' + xmlAttributes: xmlAttributes;
    return OPENING_START_MARKER + title + xmlAttributes + END_MARKER;
};
const createClosingTag = title => CLOSING_START_MARKER + title + END_MARKER;

const buildXmlString = () => {
    const xmlElements = [];
    const elementsBuilder = {
        addElement: (name, content) => {
            const attributes = [];
            const elementBuilder = {
                addAttr: (key, value) => {
                    attributes.push({key, value});
                    return elementBuilder;
                },
                build: () => {
                    const openingTag = createOpeningTag(name, attributes);
                    const closingTag = createClosingTag(name);
                    content = content || '';
                    let xmlElement;
                    if (content.startsWith('<')) {
                        xmlElement = openingTag + LINE_BREAK
                            + content + LINE_BREAK
                            + closingTag;
                    }
                    else {
                        xmlElement = openingTag + content + closingTag;
                    }
                    xmlElements.push(xmlElement);
                    return elementsBuilder;
                }
            };
            return elementBuilder;
        },
        build: () => xmlElements.join(LINE_BREAK)
    };
    return elementsBuilder;
};

const buildXmlStringFromArray = (arr, elementName) =>
    arr.reduce((acc, element) => acc.addElement(elementName, element).build(), buildXmlString()).build();

const joinXmlElements = xmlElements => xmlElements.filter(el => el).join(LINE_BREAK);

export default {
    buildXmlString, buildXmlStringFromArray, joinXmlElements
}