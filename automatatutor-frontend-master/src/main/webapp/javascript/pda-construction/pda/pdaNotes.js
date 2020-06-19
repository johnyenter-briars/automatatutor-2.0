'use strict';

const NOTES = [
    {
        title: 'Make state final: double click or use context-menu',
        explanations: []
    },
    {
        title: 'Create new transition: hover over circle, drag from appearing outer circle to target state',
        explanations: []
    },
    {
        title: 'Edit transitions: ',
        explanations: [
            'double click on transition label',
            'enter one transition per line',
            'click anywhere else to finish editing'
        ]
    },
    {
        title: 'Transition format: "a,X/Y", where',
        explanations: [
            '"a" is alphabet-symbol or epsilon --> epsilon is represented by E',
            '"Y" is arbitrary sequence of stack symbols (if the transition only pops from the stack, "Y" is left empty)',
            'example: reading alphabet-symbol "b" with stack "->XXZ" and following a transition "b,X/YX" results in the stack "->YXXZ"'
            'example: reading alphabet-symbol "b" with stack "->XXZ" and following a transition "b,X/" results in the stack "->XZ"'
            'transition of wrong format are ignored'
        ]
    },
    {
        title: 'Error marking of transitions',
        explanations: [
            'line is marked red, if any error occurs in the transitions',
            'transition is marked red, if it contains symbols that are not in the alphabet respectively stack-alphabet',
            'transition is marked green, if it violates the determinism-condition',
            'lines white --> all right'
        ]
    },
    {
        title: 'Delete transition or state: use context-menu',
        explanations: []
    },
    {
        title: 'Move transitions to another state',
        explanations: [
            'drag the line on the side you want to move to another state',
            'if there are already transitions, both are merged'
        ]
    }
];

export default NOTES;