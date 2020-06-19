package com.automatatutor.snippet

import scala.xml.{Elem, NodeSeq}

class Whileprogram {
  def renderhints(xhtml: NodeSeq): NodeSeq = {
    val returnSeq =
      <div class="notes">
        <button class="collapsible">While Program Syntax</button>
        <div class="collapsible-content">
          <div id="notes-content">
            <ul class="notes">
              <li class="note">Var: x_0, x_1...</li>
              <li class="note">Val: any number</li>
              <li class="note">Operand: Val | Var</li>
              <li class="note">Assign: Operand [(+|-) Operand]</li>
              <li class="note">Comp: ==, &lt;=, &gt;=, &lt;,  &gt;, !=</li>
              <li class="note">Cond: Operand Comp Operand</li>
              <li class="note">If: if Cond then Prog [else Prog] endif</li>
              <li class="note">While: while Cond then Prog endwhile</li>
              <li class="note">Prog: (If|While|Assign)+</li>
            </ul>
          </div>
        </div>
        <script type="text/javascript" src="/javascript/collapsible.js"> </script>
      </div>

    return returnSeq
  }
}
