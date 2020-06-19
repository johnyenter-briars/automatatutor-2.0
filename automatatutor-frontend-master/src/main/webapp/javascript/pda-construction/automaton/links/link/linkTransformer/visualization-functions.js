'use strict';

import {Vector} from '../../../../utils/vector';
import VectorTuple from '../../../../utils/vectorTuple';
import CircularList from '../../../../utils/circularList';

/**
 * @param start start-circle of the link
 * @param end end-circle of the link
 * @return new Vector that defines the start-position of the line between the given circles,
 * so that the start-position is touching the circles's rim
 */
const calcStartPositionOfLinkBetween = (start, end) => {
    const directionVector = Vector.between(start, end);
    return start.add(directionVector.getVerticalNormalVectorIfNull().withLength(start.r));
};

const calculateScaleOfVector = vector => ({
    soThatSumWith: otherVector => ({
        hasLength: r => {
            const a = vector.x;
            const b = vector.y;
            const e = otherVector.x;
            const f = otherVector.y;
            const ae = a * e;
            const bf = b * f;
            const a2 = a * a;
            const b2 = b * b;
            const e2 = e * e;
            const f2 = f * f;
            const r2 = r * r;
            const summand1 = -2 * (ae + bf);
            const summand2 = Math.sqrt(-8 * ae * bf - 3 * b2 * f2 - 4 * a2 * f2 + 4 * a2 * r2 - 4 * b2 * e2 + 4 * b2 * r2);
            const divisor = 2 * (a2 + b2);
            return (summand1 + summand2) / divisor;
        }
    })
});

/**
 * calculates the start- and end-position of a "shared" link, that means that there is another link that is mutual
 * to this link (that means it is pointing from the end-state of this link to the start-state of this link)
 * @param start start-circle of the link
 * @param end end-circle of the link
 * @param distanceBetweenLines distance that the end points of mutual links should have
 * @return new Vectors for the start- and end-position of the link, that touch the rim of the corresponding states
 * not in the middle, but a bit along the circle, so that there is the given distance between the points of this link
 * and the points of the mutual link
 */
const calcStartAndEndPositionOfSharedLinkBetween = (start, end, distanceBetweenLines) => {
    const connectionVector = Vector.between(start, end).normalizeIfNull();
    const orthogonalVector = connectionVector.getOrthogonalVector().withLength(distanceBetweenLines / 2);
    const startScale = calculateScaleOfVector(connectionVector).soThatSumWith(orthogonalVector).hasLength(start.r);
    const startPosition = start.add(connectionVector.scale(startScale)).add(orthogonalVector);
    const endScale = calculateScaleOfVector(connectionVector).soThatSumWith(orthogonalVector).hasLength(end.r);
    const endPosition = end.subtract(connectionVector.scale(endScale)).add(orthogonalVector);
    return new VectorTuple(startPosition, endPosition);
};

/**
 * see function calcStartAndEndPositionOfSharedLinkBetween
 * @param circle
 * @param distanceBetweenLines
 * @return {{startPosition: *, endPosition: new}}
 */
const calcStartAndEndPositionOfLoopLinkOf = (circle, distanceBetweenLines) => {
    const verticalNormalVector = Vector.verticalNormalVector();
    const orthogonalVector = verticalNormalVector.getOrthogonalVector().withLength(distanceBetweenLines / 2);
    const scale = calculateScaleOfVector(verticalNormalVector).soThatSumWith(orthogonalVector).hasLength(circle.r);
    const scaledVerticalNormalVector = verticalNormalVector.scale(scale);
    const startPosition = circle.add(scaledVerticalNormalVector).add(orthogonalVector);
    const endPosition = circle.add(scaledVerticalNormalVector).subtract(orthogonalVector);
    return new VectorTuple(startPosition, endPosition);
};

/**
 * calculates the angle that a self-loop at the given state must have so that it is as much
 * distance to the other links of that state as possible
 * @param state the state of the self-loop
 * @param links all links of the automaton
 * @return number optimal angle of the self-loop
 */
const calcAngleOfSelfLink = (state, links) => {
    const startingLinks = links.filter(link => link.currentStates.startState === state).filter(link => !link.isSelfLink());
    const directionsOfStartingLinks = startingLinks.map(link => link.currentStates.endState.circle.subtract(state.circle));

    const endingLinks = links.filter(link => link.currentStates.endState === state).filter(link => !link.isSelfLink());
    const directionsOfEndingLinks = endingLinks.map(link => link.currentStates.startState.circle.subtract(state.circle));

    const directions = directionsOfStartingLinks.concat(directionsOfEndingLinks).map(v => v.normalizeIfNull());
    directions.forEach(d => d.addAngleToNegativeYAxisInDegrees());
    directions.sort((dir1, dir2) => dir1.angleInDegrees - dir2.angleInDegrees);

    const circularDirections = new CircularList(directions, new Vector(0, -1).addAngleToNegativeYAxisInDegrees());
    const angleDifferences = circularDirections.zipWithSelf(-1, (second, first) => second.angleInDegrees - first.angleInDegrees);

    //should only be applied to the first element if there is more than one element
    const normedAngleDifferences = angleDifferences.map(angle => angle + (angle < 0 ? 360 : 0));

    const indexOfMaximum = normedAngleDifferences.indexOf(Math.max(...normedAngleDifferences));
    const firstDirection = circularDirections.getOrDefault(indexOfMaximum - 1);
    const secondDirection = circularDirections.getOrDefault(indexOfMaximum);
    const firstAngle = indexOfMaximum === 0 ? firstDirection.angleInDegrees - 360 : firstDirection.angleInDegrees;
    const secondAngle = secondDirection.angleInDegrees;

    return (firstAngle + secondAngle) / 2;
};

export {calcStartPositionOfLinkBetween, calcStartAndEndPositionOfSharedLinkBetween, calcStartAndEndPositionOfLoopLinkOf, calcAngleOfSelfLink};