package com.automatatutor.snippet

import scala.xml.{Elem, NodeSeq}

class Blockcanvas {
  def renderblockhints(xhtml: NodeSeq): NodeSeq = {
    val returnSeq =
      <div class="notes">
        <button class="collapsible">HELP: Block Canvas Tutorial</button>
        <div class="collapsible-content">
          <div id="notes-content">
            <ul class="notes">
              <li class="note">States:</li>
              <ul class="sublist">
                <li>Add: Click anywhere on the canvas or right-click ➞ 'Add state'</li>
                <li>Select: Click on state. Selected states are marked in a lighter shade</li>
                <li>Remove: Right click on state ➞ 'Remove' </li>
                <li>Final: Double click or right-click on state ➞ 'Toggle final' </li>
                <li>Initial: The initial state cannot be removed and no further initial states can be added </li>
              </ul>
              <li class="note">Transitions:</li>
              <ul class="sublist">
                <li>Add: Drag form the symbols of the halo and release on the same or another state </li>
                <li>Select: Click on arrow. Selected transition are marked by a dashed line</li>
                <li>Remove: Right click on arrow ➞ 'Remove entire edge' or right click on label ➞ 'Remove label' </li>
                <li>Edit: Drag from label to reposition </li>
                <li>Rotate: Right click on arrow ➞ 'Rotate edge' </li>
              </ul>
              <li class="note">Keyboard shortcuts (only on selected items):</li>
              <ul class="sublist">
                <li>F key: make state final </li>
                <li>Backspace or Delete key: remove state or (entire) transition</li>
              </ul>
              <li class="note">Macro states:</li>
              <ul class="sublist">
                <li>Add: Right-click ➞ 'Add Block State' ➞ give the block a lablel, in the form of a regular expression, which corresponds to the automaton it represents ➞ press Enter </li>
                <li>Reuse: Right-click the macro state in the toolbar ➞ 'Add' or just add a new one with the same name </li>
                <li>Define: To edit the automaton of the macro state, double click it or right-click it ➞ 'Expand' or right-click it in the toolbar ➞ 'Edit'</li>
              </ul>
              <li class="note">Macro state view (bounding box): </li>
              <ul class="sublist">
                <li>'Return' button: goes back to previous macro state</li>
                <li>'Home' button: goes back to the main (initial) automaton </li>
                <li>'Reset' button: clears the automaton of the macro state</li>
                <li>Stack trace (left): provides an overview of the current position of the macro state. One can return to a state in the stack trace by clicking on it</li>
              </ul>
              <li class="note">Toolbar (bottom):</li>
              <ul class="sublist">
                <li>Every macro state ever used is added to the toolbar to make reusing it easier </li>
                <li>Scroll to reveal hidden macro states</li>
                <li>Right-click to reveal options</li>
                <li>Double click to expand macro state</li>
              </ul>
            </ul>
          </div>
        </div>
        <script type="text/javascript" src="/javascript/collapsible.js"> </script>
      </div>

    return returnSeq
  }

  def rendernfahints(xhtml: NodeSeq): NodeSeq = {
    val returnSeq =
      <div class="notes">
        <button class="collapsible">HELP: NFA Canvas Tutorial</button>
        <div class="collapsible-content">
          <div id="notes-content">
            <ul class="notes">
              <li class="note">States:</li>
              <ul class="sublist">
                <li>Add: Click anywhere on the canvas or right-click ➞ 'Add state'</li>
                <li>Select: Click on state. Selected states are marked in a lighter shade</li>
                <li>Remove: Right click on state ➞ 'Remove' </li>
                <li>Final: Double click or right-click on state ➞ 'Toggle final' </li>
                <li>Initial: The initial state cannot be removed and no further initial states can be added </li>
              </ul>
              <li class="note">Transitions:</li>
              <ul class="sublist">
                <li>Add: Drag form the symbols of the halo and release on the same or another state </li>
                <li>Select: Click on arrow. Selected transition are marked by a dashed line</li>
                <li>Remove: Right click on arrow ➞ 'Remove entire edge' or right click on label ➞ 'Remove label' </li>
                <li>Edit: Drag from label to reposition </li>
                <li>Rotate: Right click on arrow ➞ 'Rotate edge' </li>
              </ul>
              <li class="note">Keyboard shortcuts (only on selected items):</li>
              <ul class="sublist">
                <li>F key: make state final </li>
                <li>Backspace or Delete key: remove state or (entire) transition</li>
              </ul>
            </ul>
          </div>
        </div>
        <script type="text/javascript" src="/javascript/collapsible.js"> </script>
      </div>

    return returnSeq
  }

  def renderdfahints(xhtml: NodeSeq): NodeSeq = {
    val returnSeq =
      <div class="notes">
        <button class="collapsible">HELP: DFA Canvas Tutorial</button>
        <div class="collapsible-content">
          <div id="notes-content">
            <ul class="notes">
              <li class="note">States:</li>
              <ul class="sublist">
                <li>Add: Click anywhere on the canvas or right-click ➞ 'Add state'</li>
                <li>Select: Click on state. Selected states are marked in a lighter shade</li>
                <li>Remove: Right click on state ➞ 'Remove' </li>
                <li>Final: Double click or right-click on state ➞ 'Toggle final' </li>
                <li>Initial: The initial state cannot be removed and no further initial states can be added </li>
              </ul>
              <li class="note">Transitions:</li>
              <ul class="sublist">
                <li>Edit: Drag from label of the arrow and release on target state </li>
                <li>Add/Remove: Transitions cannot be removed, just repositioned </li>
                <li>Rotate: Right click on arrow ➞ 'Rotate edge' </li>
              </ul>
              <li class="note">Keyboard shortcuts (only on selected items):</li>
              <ul class="sublist">
                <li>F key: make state final </li>
                <li>Backspace or Delete key: remove state</li>
              </ul>
            </ul>
          </div>
        </div>
        <script type="text/javascript" src="/javascript/collapsible.js"> </script>
      </div>

    return returnSeq
  }
}
