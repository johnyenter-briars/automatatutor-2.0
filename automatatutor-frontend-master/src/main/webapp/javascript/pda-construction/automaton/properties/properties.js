'use strict';

import PropertiesView from './propertiesView';
import xmlExporter from "../../utils/xmlExporter";

const XML_PROPERTIES_ID = 'properties';

const Properties = class {
    constructor(automaton, canvas) {
        this._automaton = automaton;
        this._view = new PropertiesView(canvas);
        this._allProperties = new Map();
    }

    exportToXml() {
        return xmlExporter.buildXmlString().addElement(XML_PROPERTIES_ID,
            xmlExporter.joinXmlElements(Array.from(this._allProperties.values()).map(property => property.exportToXml())))
            .build().build();
    }

    addProperty(property) {
        this._allProperties.set(property.name, property);
        property.attachToAutomaton(this._automaton, this._view.htmlElement);
    }

    getProperty(name) {
        return this._allProperties.get(name);
    }

    disableEditing() {
        [...this._allProperties.values()].forEach(property => property.disableEditing());
    }

    enableEditing() {
        [...this._allProperties.values()].forEach(property => property.enableEditing());
    }
};

export default Properties;