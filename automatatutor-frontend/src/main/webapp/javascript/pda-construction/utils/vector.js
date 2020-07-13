'use strict';

const angleInDegreesToRad = angleInDeg => (angleInDeg / 180) * Math.PI;

/**
 * two-dimensional immutable vector
 */
const Vector = class {
    constructor(x, y) {
        this.x = x;
        this.y = y;
    }

    shiftLeft(offset) {
        return new Vector(this.x - offset, this.y);
    }

    static fromArray(arr) {
        return new Vector(arr[0], arr[1]);
    }

    static fromObject(obj) {
        return new Vector(obj.x, obj.y);
    }

    static createVectorWithInclinationToVerticalNormal(angleInDegrees) {
        const angleInRad = angleInDegreesToRad(angleInDegrees);
        //y is negative, because the angle is applied to the vertical normal vector (whose direction is negative)
        return new Vector(Math.sin(angleInRad), -Math.cos(angleInRad));
    }

    /**
     * @return new vector with length 1 that point into the same direction as this vector
     */
    getNormalizedVector() {
        return this.withLength(1);
    }

    /**
     * @return new vector that is orthogonal to this vector
     */
    getOrthogonalVector() {
        return new Vector(this.y, -this.x);
    }

    /**
     * adds a property "angleInDegrees" to this vector, that stores the angle between this vector and the negative y-axis
     * @return this vector
     */
    addAngleToNegativeYAxisInDegrees() {
        const normalizedVector = this.getNormalizedVector();
        const angleInRadian = Math.asin(normalizedVector.x);
        this.angleInDegrees = (angleInRadian / Math.PI) * 180;
        if (normalizedVector.x < 0 && normalizedVector.y <= 0) {
            this.angleInDegrees = 360 + this.angleInDegrees;
        }
        else if (normalizedVector.x < 0 && normalizedVector.y > 0) {
            this.angleInDegrees = 180 - this.angleInDegrees;
        }
        else if (normalizedVector.x >= 0 && normalizedVector.y > 0) {
            this.angleInDegrees = 180 - this.angleInDegrees;
        }
        return this;
    }

    /**
     * @param vector other vector
     * @return new Vector from the sum of this and the given other vector
     */
    add(vector) {
        return new Vector(this.x + vector.x, this.y + vector.y);
    }

    /**
     *
     * @param vector other vector
     * @return new Vector from the difference of this and the given other vector
     */
    subtract(vector) {
        return new Vector(this.x - vector.x, this.y - vector.y);
    }

    /**
     * @param length required length of this vector
     * @return new Vector with the given length that points into the same direction as this vector
     */
    withLength(length) {
        const scale = length / this.getLength();
        return this.scale(scale);
    }

    /**
     * @param scale
     * @return new Vector that represents this vector which is scaled by the given scale
     */
    scale(scale) {
        return new Vector(scale * this.x, scale * this.y);
    }

    getLength() {
        return Math.sqrt(this.x * this.x + this.y * this.y);
    }

    /**
     * @param startVector
     * @param endVector
     * @return new Vector, that points from the startVector to the endVector
     */
    static between(startVector, endVector) {
        return new Vector(endVector.x - startVector.x, endVector.y - startVector.y);
    }

    normalizeIfNull() {
        if (this.x === 0 && this.y === 0){
            return new Vector(0, 1);
        }
        return this;
    }

    clone() {
        return new Vector(this.x, this.y);
    }

    /**
     * @return the vertical normal vector (vector that point into negative y-direction with length 1)
     * if this vector is null, otherwise returns this vector
     */
    getVerticalNormalVectorIfNull() {
        return this.getLength() === 0 ? Vector.verticalNormalVector() : this;
    }

    equals(otherVector) {
        return this.x === otherVector.x && this.y === otherVector.y;
    }

    /**
     * @return new Vector that points into negative y-direction with length 1
     */
    static verticalNormalVector() {
        return new Vector(0, -1);
    }
};

const getBoundedValue = (value, lower, upper) => Math.max(lower, Math.min(upper, value));

const Circle = class extends Vector {
    constructor(x, y, r) {
        super(x, y);
        this.r = r;
    }

    changePosition(position) {
        return Circle.fromPosition(position, this.r);
    }

    changePositionBounded(position, dimensions) {
        const x = getBoundedValue(position.x, this.r, dimensions.width - this.r);
        const y = getBoundedValue(position.y, this.r, dimensions.height - this.r);
        return new Circle(x, y, this.r);
    }

    static fromPosition(position, r) {
        return new Circle(position.x, position.y, r);
    }
};

export {Vector, Circle};