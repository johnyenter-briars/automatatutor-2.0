'use strict';

const XmlDocument = class {
  constructor(xmlString) {
      const parser = new DOMParser();
      this._xmlDoc = parser.parseFromString(xmlString, 'text/xml');
  }

  getFirstElementByTagName(tagName) {
      return this._xmlDoc.getElementsByTagName(tagName)[0];
  }

  getArrayOfChildrenContents(tagName) {
      return this.getArrayOfChildren(tagName).map(child => child.firstChild.nodeValue);
  }

  getArrayOfChildren(tagName) {
      return Array.from(this.getFirstElementByTagName(tagName).childNodes);
  }
};

export default XmlDocument;