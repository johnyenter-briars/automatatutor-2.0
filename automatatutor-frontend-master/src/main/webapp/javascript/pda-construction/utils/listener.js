'use strict';

const getErrorMsg = fun => `${fun} is not a function of this listener-interface`;

const VariableListenerWrapper = class {
    constructor(listenerInterface) {
        this._listenerInterface = listenerInterface;
    }

    get listenerInterface() {
        return this._listenerInterface;
    }

    /**
     * invokes the given function for the listener with the given parameters
     * @param listener listener object with this interface
     * @param fun string with the name of the function to be called
     * @param params parameters to call the function with
     */
    call(listener, fun, ...params) {
        if (Object.values(this._listenerInterface).includes(fun)) {
            if (typeof listener.listenerFunctions[fun] === 'function') {
                listener.listenerFunctions[fun](...params)
            }
        }
        else {
            throw new Error(getErrorMsg(fun));
        }
    }
};

/**
 * stores the listeners of a certain object that implement all the same interface
 */
const ListenersSet = class {
    /**
     * @param listenerInterface interface that these listeners have to implement
     */
    constructor(listenerInterface) {
        this._listenerInterface = listenerInterface;
        this._listeners = new Map();
    }

    isEmpty() {
        return this._listeners.size === 0;
    }

    /**
     * add a new listener
     * @param listener object with the required properties "id" (a unique id among the listeners here) and
     * "listenerFunctions" (object with those listener-functions of this interface which are needed,
     * not all functions have to be realized)
     */
    add(listener) {
        if (this._listenerInterface === listener.listenerInterface) {
            this._listeners.set(listener.id, listener.listenerFunctions);
        }
        else {
            throw new Error('the given listener has not this interface');
        }
    }

    /**
     * add array of listeners
     * @param listenerArray array of listener-objects with the required properties "id" (a unique id among the listeners here) and
     * "listenerFunctions" (object with those listener-functions of this interface which are needed,
     * not all functions have to be realized)
     */
    addAll(listenerArray) {
        listenerArray.forEach(listener => this.add(listener));
    }

    /**
     * removes the listener with the given id
     * @param id
     */
    remove(id) {
        this._listeners.delete(id);
    }

    /**
     * invokes the given function for all listeners here with the given parameters
     * @param fun string with the name of the function to be called
     * @param params parameters to call the function with
     */
    callForAll(fun, ...params) {
        if (Object.values(this._listenerInterface).includes(fun)) {
            Array.from(this._listeners.values()).forEach(listener => {
                if (typeof listener[fun] === 'function') {
                    listener[fun](...params)
                }
            });
        }
        else {
            throw new Error(getErrorMsg(fun));
        }
    }
};

const Listener = class {
    constructor(id, listenerInterface) {
        this._id = id;
        this._listenerInterface = listenerInterface;
        this._listenerFunctions = {};
    }

    get id() {
        return this._id;
    }

    get listenerInterface() {
        return this._listenerInterface;
    }

    get listenerFunctions() {
        return this._listenerFunctions;
    }

    set(functionName, fun) {
        if (Object.values(this._listenerInterface).includes(functionName)) {
            this._listenerFunctions[functionName] = fun;
        }
        else {
            throw new Error('the listener interface of this listener does not have the given function');
        }
    }
};

export {ListenersSet, VariableListenerWrapper, Listener};