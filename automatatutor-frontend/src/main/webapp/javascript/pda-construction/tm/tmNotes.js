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
            'double click on transitions',
            'enter one transition per line',
            'click anywhere else to finish editing'
        ]
    },
    {
        title: 'Transition format: "x1/y1,U1|x2/y2,U2|...", where',
        explanations: [
            '"x1", "y1", "x2", "y2" and so on are alphabet-symbols or the empty tape symbol ' +
            '--> the empty tape symbol is represented by E',
            '"U1", "U2" and so on are either R, L or N',
            '"x1/y1,U1" defines the behaviour on the first tape, "x2/y2,U2" on the second tape and so on'
        ]
    },
    {
        title: 'Error marking of transitions',
        explanations: [
            'line is marked red, if any error occurs in the transitions',
            'transition is marked red, if it contains invalid symbols',
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