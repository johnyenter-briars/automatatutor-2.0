'use strict';

import * as d3 from "d3";

const makeElementCollapsible = (button, element) => {
    button.classList.add('collapsible');
    element.classList.add('collapsible-content');

    d3.select(button).on('click', function () {
        this.classList.toggle('active');
        if (element.style.maxHeight) {
            element.style.maxHeight = null;
        } else {
            element.style.maxHeight = element.scrollHeight + 'px';
        }
    });
};

export {makeElementCollapsible};