'use strict';

import {newLinkTransformerListenersSet, linkTransformerListenerInterface} from './linkTransformerListener';
import {
    calcStartPositionOfLinkBetween,
    calcStartAndEndPositionOfSharedLinkBetween,
    calcStartAndEndPositionOfLoopLinkOf,
    calcAngleOfSelfLink
} from './visualization-functions';
import {Vector} from '../../../../utils/vector';
import VectorTuple from '../../../../utils/vectorTuple';
import {Listener} from "../../../../utils/listener";
import {transitionGroupListenerInterface} from "../transitionGroup/transitionGroupListener";

const MINIMUM_SHARED_PATH_DIAMETER = 50;
const DISTANCE_BETWEEN_LINES = 15;
const MINIMUM_LOOP_DIAMETER = 100;

/**
 * represent a specific geometrical form of a {Link}
 * @type {LinkForm}
 */
const LinkForm = class {
    constructor(name) {
        this._name = name;
    }

    get name() {
        return this._name;
    }

    static name() {
        throw 'not implemented';
    }

    set linkTransformer(value) {
        this._linkTransformer = value;
    }

    /**
     * updates the end positions of the form
     */
    updatePositions() {
        throw 'not implemented';
    }

    /**
     * updates the angle of the form (not every form uses an angle)
     */
    updateAngle() {
        this._linkTransformer.angle = 0;
    }

    /**
     * updates the form according to a changed diameter of the {TransitionGroup}; not used by every form
     */
    onDiameterChanged() {
    }
};

/**
 * represent the form of a {Link} as a straight line
 * @type {StraightForm}
 */
const StraightForm = class extends LinkForm {
    constructor() {
        super(StraightForm.name);
    }

    static name() {
        return 'straight';
    }

    updatePositions(circleTuple) {
        const start = calcStartPositionOfLinkBetween(circleTuple.start, circleTuple.end);
        const end = calcStartPositionOfLinkBetween(circleTuple.end, circleTuple.start);
        this._linkTransformer.positionTuple = new VectorTuple(start, end);
        this._linkTransformer.middlePosition = start.add(end).scale(0.5);
        this._linkTransformer.path = `M ${start.x} ${start.y} L ${end.x} ${end.y}`;
    }
};

/**
 * represents the form of a {Link} as a curved line; this is used, when for a specific {Link} exists another link
 * in the opposite direction
 * @type {CurvedForm}
 */
const CurvedForm = class extends LinkForm {
    constructor() {
        super(CurvedForm.name);
    }

    static get name() {
        return 'shared';
    }

    updatePositions(circleTuple) {
        this._linkTransformer.positionTuple =
            calcStartAndEndPositionOfSharedLinkBetween(circleTuple.start, circleTuple.end, DISTANCE_BETWEEN_LINES);
        this._updatePath();
    }

    _updatePath() {
        const start = this._linkTransformer.positionTuple.start;
        const end = this._linkTransformer.positionTuple.end;
        const diameter = Math.max(this._linkTransformer.diameter / 2 + 20, MINIMUM_SHARED_PATH_DIAMETER);
        const middlePointOfLine = start.add(end).scale(0.5);
        const connectionVector = end.subtract(start);
        const orthogonalVector = connectionVector.getOrthogonalVector().withLength(2 * diameter);
        const refPoint = middlePointOfLine.add(orthogonalVector);
        this._linkTransformer.middlePosition = middlePointOfLine.add(orthogonalVector.withLength(diameter));
        this._linkTransformer.path = `M ${start.x} ${start.y} Q ${refPoint.x} ${refPoint.y} ${end.x} ${end.y}`;
    }

    onDiameterChanged() {
        this._updatePath();
    }
};

/**
 * represents the form for a self {Link}
 * @type {LoopForm}
 */
const LoopForm = class extends LinkForm {
    constructor(getState, getAllLinks) {
        super(LoopForm.name);
        this._calcAngle = () => calcAngleOfSelfLink(getState(), getAllLinks());
    }

    static get name() {
        return 'loop';
    }

    updatePositions(circleTuple) {
        this._linkTransformer.positionTuple =
            calcStartAndEndPositionOfLoopLinkOf(circleTuple.start, DISTANCE_BETWEEN_LINES);
        this._updatePath();
        this._r = circleTuple.start.r;
        this._updateMiddlePosition();
    }

    updateAngle() {
        this._linkTransformer.angle = this._calcAngle();
        this._updateMiddlePosition();
    }

    _updatePath() {
        const pos = this._linkTransformer.positionTuple.start;
        this._diameter = Math.max(this._linkTransformer.diameter / 2 + 20, MINIMUM_LOOP_DIAMETER);
        const d = this._diameter;
        const c = DISTANCE_BETWEEN_LINES;
        this._linkTransformer.path =
            `M ${pos.x} ${pos.y} q ${-d / 2 + c / 2} ${-d} ${c / 2} ${-d} q ${d / 2} 0 ${c / 2} ${d}`;
    }

    _updateMiddlePosition() {
        const shiftDirection = Vector.createVectorWithInclinationToVerticalNormal(this._linkTransformer.angle);
        const shift = shiftDirection.withLength(this._diameter + this._r);
        this._linkTransformer.middlePosition = this._linkTransformer.rotationCenter.add(shift);
    }

    onDiameterChanged() {
        this._updatePath();
    }
};

/**
 * represents a collection of all {LinkForm}s, that a specific {Link} can have,
 * and contains the current form of this {Link}
 * @type {LinkTransformer}
 */
const LinkTransformer = class {
    constructor(diameter, ...listeners) {
        this._listeners = newLinkTransformerListenersSet();
        this._listeners.addAll([...listeners]);
        this._diameter = diameter;
        this._angle = 0;
        this._allForms = new Map();
    }

    /**
     * adds a {LinkForm} to this transformer, so that the {Link} with this transformer can have this form
     * @param linkForm
     * @return {LinkTransformer}
     */
    addForm(linkForm) {
        if (!(linkForm instanceof LinkForm)) {
            throw 'the added link form has to extends from class LinkForm';
        }
        linkForm.linkTransformer = this;
        this._allForms.set(linkForm.name, linkForm);
        return this;
    }

    set positionTuple(value) {
        this._positionTuple = value;
    }

    get positionTuple() {
        return this._positionTuple;
    }

    set middlePosition(value) {
        this._middlePosition = value;
        this._listeners.callForAll(linkTransformerListenerInterface.onMiddlePositionChanged, this._middlePosition);
    }

    set path(value) {
        this._path = value;
        this._listeners.callForAll(linkTransformerListenerInterface.onPathChanged, this._path);
    }

    set angle(value) {
        this._angle = value;
        this._listeners.callForAll(linkTransformerListenerInterface.onAngleChanged, this._angle, this._rotationCenter);
    }

    set rotationCenter(value) {
        this._rotationCenter = value;
        this._listeners.callForAll(linkTransformerListenerInterface.onRotationCenterChanged, this._angle, this._rotationCenter);
    }

    get diameter() {
        return this._diameter;
    }

    get angle() {
        return this._angle;
    }

    get rotationCenter() {
        return this._rotationCenter;
    }

    /**
     * sets the current form of the {Link}
     * @param formName
     * @param circleTuple
     */
    setForm(formName, circleTuple) {
        if (this._allForms.has(formName)) {
            this._form = formName;
            this.updatePositions(circleTuple);
            this.updateAngle();
        }
        else {
            throw `${formName} is not a correct form`;
        }
    }

    updatePositions(circleTuple) {
        this.rotationCenter = circleTuple.start;
        this._allForms.get(this._form).updatePositions(circleTuple);
    }

    updateAngle() {
        this._allForms.get(this._form).updateAngle();
    }

    _onDiameterChanged(diameter) {
        this._diameter = diameter;
        this._allForms.get(this._form).onDiameterChanged();
    }

    getTransitionGroupListener() {
        const listener = new Listener(LinkTransformer.name, transitionGroupListenerInterface);
        listener.set(transitionGroupListenerInterface.onDiameterChanged, diameter => this._onDiameterChanged(diameter));
        return listener;
    }
};

export {LinkTransformer, StraightForm, CurvedForm, LoopForm};