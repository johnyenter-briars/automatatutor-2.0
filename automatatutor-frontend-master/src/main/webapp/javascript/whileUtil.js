const Global = {
    variables : new Set(),
    programXml : null
}

class Tuple {
    constructor(first, second){
        this.first = first;
        this.second = second;
    }
}

class GeneralNode {
    constructor(program){
        this.program = program.trim();
    }
    // return:
    // - traversed has start token bot not end token
    // - rst
    findEnd(rst, startToken, endToken, getFrom, includeLast){
        const inf = 1000000000;
        let counter = 1;
        let traversed = rst.substring(0, rst.indexOf(startToken) + startToken.length);
        rst = rst.substring(rst.indexOf(startToken) + startToken.length);
        while(counter > 0){
            const firstBeg = rst.indexOf(startToken) === -1 ? inf : rst.indexOf(startToken);
            const firstEnd = rst.indexOf(endToken) === -1 ? inf : rst.indexOf(endToken);
            if(firstBeg < firstEnd){
                counter++;
                traversed += rst.substring(0, firstBeg + startToken.length);
                rst = rst.substring(firstBeg + startToken.length)
            }
            else{
                counter--;
                traversed += rst.substring(0, firstEnd + endToken.length);
                rst = rst.substring(firstEnd + endToken.length);
            }
        }
        traversed = traversed.substring(traversed.indexOf(getFrom) + getFrom.length, traversed.length - endToken.length);
        // in case end token ist the same as the start token for a subsequent call
        if(includeLast)
            rst = endToken + rst;
        return new Tuple(traversed, rst);
    }
    toXML(){
        console.log("NOT IMPLEMENTED!");
    }
    isValid(){
        try{
            this.toXML();
        }
        catch(err){
            return false;
        }
        return true;
    }
}

class OperandNode extends GeneralNode {
    constructor(program){
        super(program);
    }
    toXML(){
        if(this.program.match(/\d+/) === null)
            throw `Error in line: ${this.program.substring(0, 20) + (this.program.length > 20 ? "" : "...")}\nOperand must contain a digit`;
        const nr = this.program.match(/\d+/)[0];
        if(this.program != nr && this.program != 'x_' + nr)
            throw `Error in line: ${this.program.substring(0, 20) + (this.program.length > 20 ? "" : "...")}\nOperand must be of the form x_c or c, where c is a number`;
        if(this.program.match(/[a-zA-Z]/) === null)
            return $($.parseXML(`<const>${nr}</const>`)).children().get(0);
        Global.variables.add(nr);
        return $($.parseXML(`<var>${nr}</var>`)).children().get(0);
    }
}

class ArithNode extends GeneralNode {
    constructor(program){
        super(program);
        this.map = new Map([['+', 'plus'], ['-', 'minus']])
    }
    toXML(){

        if(this.program.includes('=') === false)
            throw `Error in line: ${this.program.substring(0, 10) + (this.program.length > 10 ? "" : "...")}\nMissing assignment operator`;

        // No operators
        const testNode = new OperandNode(this.program.substring(this.program.indexOf('=') + 1));

        let lhs = new OperandNode(this.program.substring(0,this.program.indexOf('=')));
        let first = testNode;
        let operator = '+';
        let sec = new OperandNode("0");

        if(testNode.isValid() === false){
            if(this.program.match(/[\+\-]/) === null){
                throw `Error in line: ${this.program.substring(0, 10) + (this.program.length > 10 ? "" : "...")}\nInvalid arithmetic operator. Allowed operators are '+' and '-'.\nTIP: Try '+ 0' if satisfactory`;
            }
            operator = this.program.match(/[\+\-]/)[0];
            first = new OperandNode(this.program.substring(this.program.indexOf('=') + 1, this.program.indexOf(operator)));
            if(this.program.substring(this.program.indexOf(operator) + 1).trim().length === 0)
                throw `Error in line: ${this.program.substring(0, 10) + (this.program.length > 10 ? "" : "...")}\nMissing second argument.\nTIP: Try '+ 0' if satisfactory`;
            sec = new OperandNode(this.program.substring(this.program.indexOf(operator) + 1));
        }
        let ret = $.parseXML("<arith/>");
        $(ret).children().append(lhs.toXML());
        $(ret).children().append(first.toXML());
        $(ret).children().append(sec.toXML());
        $(ret).children().attr("op", this.map.get(operator));
        return $(ret).children().get(0);
    }
}

class ComparisonNode extends GeneralNode {
    constructor(program){
        super(program);
        this.map = new Map([['=', 'eq'], ['!=', 'neq'], ['<', 'l'], ['>', 'g'], ['<=', 'leq'], ['>=', 'geq']])
    }
    toXML(){

        const lhs = new OperandNode(this.program.match(/\w+/)[0]);
        let operator;
        if(this.program.match(/[\!\<\>\=]/).length > 0)
            operator = this.program.match(/\!\=|\<\=|\>\=|\=\=/)[0];
        else if(this.program.match(/[\<\>]/) != null)
            operator = this.program.match(/[\<\>]/)[0];
        else
            throw `Error in line: ${this.program.substring(0, 20) + (this.program.length > 20 ? "" : "...")}\nInvalid comparison operator`;
        const rhs = new OperandNode(this.program.substring(this.program.indexOf(operator) + operator.length));

        let ret = $.parseXML("<compare/>");
        $(ret).children().append(lhs.toXML());
        $(ret).children().append(rhs.toXML());
        $(ret).children().attr("op", this.map.get(operator));
        return $(ret).children().get(0);
    }
}

class IfNode extends GeneralNode {
    constructor(program){
        super(program);
    }
    toXML(){
        if(this.program.includes("then") === false)
            throw `Error in line: ${this.program.substring(0, 20) + (this.program.length > 20 ? "" : "...")}\nMissing 'then' token`;

        const cmp = new ComparisonNode(this.program.substring("if".length, this.program.indexOf("then")));
        let ret = $.parseXML("<if/>");
        // If + else
        if(this.program.includes("else")){
            let tup = this.findEnd(this.program, "if", "else", "then", true);
            const ifBranch = new ComplexNode(tup.first);
            $(ret).children().append(cmp.toXML());
            $(ret).children().append(ifBranch.toXML());
            tup = this.findEnd(tup.second, "else", "endif", "else", false);
            const elseBranch = new ComplexNode(tup.first);
            $(ret).children().append(elseBranch.toXML());
        }
        // Simple if
        else{
            let tup = this.findEnd(this.program, "if", "endif", "then", true);
            const ifBranch = new ComplexNode(tup.first);
            $(ret).children().append(cmp.toXML());
            $(ret).children().append(ifBranch.toXML());
        }

        return $(ret).children().get(0);
    }
}

class WhileNode extends GeneralNode {
    constructor(program){
        super(program);
    }
    toXML(){
        if(this.program.includes("do") === false)
            throw `Error in line: ${this.program.substring(0, 20) + (this.program.length > 20 ? "" : "...")}\nMissing 'do' token`;

        const cmp = new ComparisonNode(this.program.substring("while".length, this.program.indexOf("do")));

        let tup = this.findEnd(this.program, "while", "endwhile", "do", false);
        const whileBlock = new ComplexNode(tup.first);

        let ret = $.parseXML("<while/>");
        $(ret).children().append(cmp.toXML());
        $(ret).children().append(whileBlock.toXML());
        return $(ret).children().get(0);
    }
}

class ComplexNode extends GeneralNode {
    constructor(program){
        super(program);
    }
    toXML(){
        // Program starts with an if
        if(this.program.indexOf("if") === 0){
            if(this.program.includes("endif") === false)
                throw `Error in line: ${this.program.substring(0, 10) + (this.program.length > 10 ? "" : "...")}\nMissing 'endif' token`

            let tup = this.findEnd(this.program, "if", "endif", "", false);
            tup.first += "endif";
            const ifNode = new IfNode(tup.first);
            if(tup.second.trim().length === 0)
                return ifNode.toXML();

            const restNode = new ComplexNode(tup.second);
            let ret = $.parseXML("<concat/>");
            $(ret).children().append(ifNode.toXML());
            $(ret).children().append(restNode.toXML());
            return $(ret).children().get(0);
        }
        else if(this.program.indexOf("while") === 0){
            if(this.program.includes("endwhile") === false)
                throw `Error in line: ${this.program.substring(0, 10) + (this.program.length > 10 ? "" : "...")}\nMissing 'endwhile' token`;

            let tup = this.findEnd(this.program, "while", "endwhile", "", false);
            tup.first += "endwhile";
            const whileNode = new WhileNode(tup.first);
            if(tup.second.trim().length === 0)
                return whileNode.toXML();

            const restNode = new ComplexNode(tup.second);
            let ret = $.parseXML("<concat/>");
            $(ret).children().append(whileNode.toXML());
            $(ret).children().append(restNode.toXML());
            return $(ret).children().get(0);
        }
        // we start with a variable assignment
        else if(this.program.indexOf("x_") === 0){
            let firstLine = this.program.includes("\n") === true ?
                this.program.substring(0, this.program.indexOf("\n")) : this.program;
            const arithNode = new ArithNode(firstLine);
            let rst = this.program.includes("\n") === true ?
                this.program.substring(this.program.indexOf("\n") + 1) : "";
            if(rst.trim().length === 0)
                return arithNode.toXML();

            const restNode = new ComplexNode(rst);
            let ret = $.parseXML("<concat/>");
            $(ret).children().append(arithNode.toXML());
            $(ret).children().append(restNode.toXML());
            return $(ret).children().get(0);
        }
        else{
            throw `Error in line: ${this.program.substring(0, 10) + (this.program.length > 10 ? "" : "...")}\nA line must start with an assignement, an 'if' or a 'while' statement`;
        }
    }
}

function whileChecks(fieldname){
    try{
        Global.variables = new Set();
        const rawInput = $('#' + fieldname).val();
        const node = new ComplexNode(rawInput);
        Global.programXml = node.toXML();
        const variablesArr = Array.from(Global.variables).sort();
        for(i = 0; i < variablesArr.length; i++)
            if(variablesArr[i] != i)
                throw `Error: variable 'x_${i}' not used`;
    }
    catch(err){
        $('#feedbackdisplay').show();
        if($('#parsingerror p').size() === 0)
            $('#parsingerror').append($("<p/>"))
        $('#parsingerror p').text(err);
        $('#parsingerror p').html($('#parsingerror p').html().replace(/\n/g,'<br/>'));
        // console.log(err);
        return false;
    }
    return true;
}

function getProgram(){
    let ret = $.parseXML("<Program/>");
    let x = $.parseXML("<Exprs/>");
    $(ret).children().append($(x).children().get(0));
    $(ret).children().children().append(Global.programXml);
    x = $.parseXML(`<NumVariables>${Global.variables.size}</NumVariables>`);
    $(ret).children().append($(x).children().get(0));
    x = $.parseXML("<UselessVariables/>");
    $(ret).children().append($(x).children().get(0));
    x = $.parseXML("<UselessVariablesText/>");
    $(ret).children().append($(x).children().get(0));
    xmlString = (new XMLSerializer()).serializeToString(ret);
    // console.log(xmlString);
    return xmlString;
}