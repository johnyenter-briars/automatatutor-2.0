# Hands-On: How to add a new type of problem

## Step 1: Design the Problem

Model your new problem type. This includes:

* problem name, short (used in code, **\<PN\>** for future reference)
* problem name, long (for html, **\<PN_long\>** for future reference)
* fields needed for instructions 
* fields needed for answer

## Step 2: Create the Model

Create a new file called `<PN>Problem.scala` in the model folder. Here we define the problem model with its function for copying and XML import / export. (Helpful chapter for [XML in Scala](https://books.google.de/books?id=MFjNhTjeQKkC&pg=PA513&lpg=PA516))

Here we use a common code pattern with a class and an object with the same name. This enables "static" methods.

```scala
package com.automatatutor.model
import ...

class <PN>Problem extends LongKeyedMapper[<PN>Problem] with IdPK with SpecificProblem[<PN>Problem] {
  def getSingleton = <PN>Problem

  object problemId extends MappedLongForeignKey(this, Problem)
  
  //instruction fields
  object instructionField1 extends MappedText(this) //or maybe MappedInt(this) 
  def getInstructionField1 = this.instructionField1.is
  def setInstructionField1(s: String) = this.instructionField1(s)

  override def copy(): <PN>Problem = {
    val retVal = new <PN>Problem
    retVal.problemId(this.problemId.get)
    retVal.instructionField1(this.instructionField1.get)
    return retVal
  }

  override def toXML(): Node = {
    return <PNProblem>
             <InstructionField1>{ this.getInstructionField1 }</InstructionField1>
           </PNProblem>
  }

  override def setGeneralProblem(newProblem: Problem) = this.problemId(newProblem)

}

object <PN>Problem extends <PN>Problem with SpecificProblemSingleton with LongKeyedMetaMapper[<PN>Problem] {
  def findByGeneralProblem(generalProblem: Problem): <PN>Problem =
    find(By(<PN>Problem.problemId, generalProblem)) openOrThrowException ("Must only be called if we are sure that generalProblem is a <PN>Problem")

  def deleteByGeneralProblem(generalProblem: Problem): Boolean =
    this.bulkDelete_!!(By(<PN>Problem.problemId, generalProblem))

  override def fromXML(generalProblem: Problem, xml: Node): Boolean = {
    val retVal = new <PN>Problem
    retVal.problemId(generalProblem)
    retVal.instructionField1((xml \ "InstructionField1").text)		//or maybe .text.toInt
    retVal.save()
    return true
  }
}
```

## Step 3: Solution Attempt

In the file `SolutionAttempt.scala` add the code to save a users solution attempts. They are needed so that a user can save his work and doesn't need to start over all the time. Furthermore they enable a detailed evaluation by examining the database.

```scala
class <PN>SolutionAttempt extends LongKeyedMapper[<PN>SolutionAttempt] with IdPK {
	def getSingleton = <PN>SolutionAttempt

	object solutionAttemptId extends MappedLongForeignKey(this, SolutionAttempt)
	object attemptAnswerField1 extends MappedText(this) //or maybe MappedInt(this)
}

object <PN>SolutionAttempt extends <PN>SolutionAttempt with LongKeyedMetaMapper[<PN>SolutionAttempt] {
	def getByGeneralAttempt ( generalAttempt : SolutionAttempt ) : <PN>SolutionAttempt = {
		return this.find(By(<PN>SolutionAttempt.solutionAttemptId, generalAttempt)) openOrThrowException "Must only be called if we are sure that the general attempt also has a <PN> attempt"
	}
}
```

## Step 4: HTML Files

In the webapp folder add a new folder `<PN>-problem` with 3 HTML files:

* `create.html`
* `edit.html`
* `solve.html`

Keep it clean, informative and structure your pages like the HTML files for the other problem types (e.g. WordsInGrammar). The `feedbackdisplay` displays feedback to the user after an AJAX call.

The main idea is to write all static parts of the file down and bind the rest dynamically. To do so, mark those places with special HTML tags:

* `<lift:A.f></lift:A.f>` (active): calls the function `f` in class `A` returning a XML NodeSeq
* `<group:name> </group:name>` (passive): marks location for manual replacement using `net.liftweb.util.Helpers`.



## Step 5: Problem Snippet

In the snippet folder add a new file `<PN>Snippet.scala`. Here we implement the corresponding ProblemSnippet that needs to offer the 4 methods of the ProblemSnippet trait.

```scala
package com.automatatutor.snippet
import ...

object <PN>Snippet extends ProblemSnippet {

  override def renderCreate( createUnspecificProb : (String, String) => Problem,
      returnFunc : () => Nothing ) : NodeSeq = {

    var shortDescription : String = "default short description"
    var longDescription : String = "default long description"
    var instruction1 : String = "deault value"  			//or maybe other types like int

    def create() = {
      val unspecificProblem = createUnspecificProb(shortDescription, longDescription)
      
      val specificProblem : <PN>Problem = <PN>Problem.create
      specificProblem.setGeneralProblem(unspecificProblem).setInstructionField1(instruction1)
      specificProblem.save
      
      returnFunc()
    }
    
    //create HTML
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _)
    val longDescriptionField = SHtml.textarea(longDescription, longDescription = _, "cols" -> "80", "rows" -> "5")
    val instruction1Field = SHtml.text(instruction1, instruction1 = _)
    val submitButton = SHtml.submit("Create", create)
    
    //put HTML into template
    val template : NodeSeq = Templates(List("<PN>-problem", "create")) openOr Text("Could not find template /<PN>-problem/create")
    Helpers.bind("createform", template,
        "instruction1" -> instruction1Field,
        "shortdescription" -> shortDescriptionField,
        "longdescription" -> longDescriptionField,
        "submit" -> submitButton)
  }

  override def renderEdit: Box[(Problem, () => Nothing) => NodeSeq] = Full(renderEditFunc)
  private def renderEditFunc(problem: Problem, returnFunc: () => Nothing): NodeSeq = {

    val specificProblem = <PN>Problem.findByGeneralProblem(problem)
    var shortDescription: String = problem.getShortDescription
    var longDescription: String = problem.getLongDescription
    var instruction1: String = specificProblem.getInstructionField1

    def edit(formValues: String): JsCmd = {
      val formValuesXml = XML.loadString(formValues)
      
      //reconstruct values from xml
      val instruction1 = (formValuesXml \ "instruction1").head.text
      
      //check input values (here: Is given exercise is feaseable?)
      val exerciseFeedback = GraderConnection.getProblemsForExercise(instruction1)

      if (exerciseFeedback.isEmpty) { //edit
        problem.setShortDescription(shortDescription).setLongDescription(shortDescription).save()
        specificProblem.instructionField1(instruction1).save()
        returnFunc()
      } else { //display feedback
        return JsCmds.JsShowId("submitbutton") & JsCmds.JsShowId("feedbackdisplay") & JsCmds.SetHtml("parsingerror", Text(parsingErrors.mkString("<br/>")))
      }
    }

    //create HTML
    val instruction1Field = SHtml.text(instruction1, instruction1 = instruction1, "id" -> "instruction1field")
    val shortDescriptionField = SHtml.text(shortDescription, shortDescription = _, "id" -> "shortdescriptionfield")
    val longDescriptionField = SHtml.textarea(longDescription, longDescription = _, "cols" -> "80", "rows" -> "5", "id" -> "longdescriptionfield")
    
    //if inputs need to be checked: Javascript calls to collect inputs as XML
    	//! only collect inputs needed for check!
    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val instruction1ValXmlJs: String = "<instruction1>' + document.getElementById('instruction1field').value + '</instruction1>"
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("'<createattempt>" + instruction1ValXmlJs"</createattempt>'"), edit(_))
    val submit: JsCmd = hideSubmitButton & ajaxCall

    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ submit }>Submit</button>

	//put HTML into template
    val template: NodeSeq = Templates(List("<PN>-problem", "edit")) openOr Text("Could not find template /<PN>-problem/edit")
    Helpers.bind("editform", template,
      "instruction1" -> instruction1Field,
      "shortdescription" -> shortDescriptionField,
      "longdescription" -> longDescriptionField,
      "submit" -> submitButton)
  }

  override def renderSolve(generalProblem: Problem, maxGrade: Long, lastAttempt: Box[SolutionAttempt],
                           recordSolutionAttempt: (Int, Date) => SolutionAttempt, returnFunc: () => Unit, remainingAttempts: () => Int,
                           bestGrade: () => Int): NodeSeq = {
    val specificProblem = <PN>Problem.findByGeneralProblem(generalProblem)

    def grade(answer: String): JsCmd = {

      if (remainingAttempts() <= 0) {
        return JsShowId("feedbackdisplay") & SetHtml("feedbackdisplay", Text("You do not have any attempts left for this problem. Your final grade is " + bestGrade().toString + "/" + maxGrade.toString + "."))
      }
      
      val attemptTime = Calendar.getInstance.getTime()
      val gradeAndFeedback = GraderConnection.get<PN>Feedback(...)

      var numericalGrade = gradeAndFeedback._1
      val generalAttempt = recordSolutionAttempt(numericalGrade, attemptTime)
      val validAttempt = true; //TODO: check if attempts was valid... e.g. parseable

      // Only save the specific attempt if the attempt was valid and we saved the general attempt a
      if (generalAttempt != null && validAttempt) {
      	<PN>SolutionAttempt.create.solutionAttemptId(generalAttempt).attemptAnswerField1(answer).save
      }

      val setNumericalGrade: JsCmd = SetHtml("grade", Text(numericalGrade.toString + "/" + maxGrade.toString))
      val setFeedback: JsCmd = SetHtml("feedback", gradeAndFeedback._2)
      val showFeedback: JsCmd = JsShowId("feedbackdisplay")

      return setNumericalGrade & setFeedback & showFeedback & JsCmds.JsShowId("submitbutton")
    }
	
	//reconstruct last attempt
	val lastAttemptAnswerField1 = lastAttempt.map({generalAttempt => 
		<PN>SolutionAttempt.getByGeneralAttempt(generalAttempt).attemptAnswerField1.is
	}) openOr ""
	
	//build html
	val someTextForProblemStatement = Text(specificProblem.getInstruction1Field)
    val answerField1 = SHtml.text(lastAttemptAnswerField1, value => {}, "id" -> "answerfield1")
    val hideSubmitButton: JsCmd = JsHideId("submitbutton")
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw("document.getElementById('answerfield1').value"), grade(_))
    val submitButton: NodeSeq = <button type='button' id='submitbutton' onclick={ hideSubmitButton & ajaxCall }>Submit</button>
    val returnLink: NodeSeq = SHtml.link("/courses/show", returnFunc, Text("Return to Course"))

	//put HTML into template
    val template: NodeSeq = Templates(List("<PN>-problem", "solve")) openOr Text("Could not find template /<PN>-problem/solve")
    Helpers.bind("solveform", template,
      "instructionText" -> someTextForProblemStatement,
      "answerfield1" -> answerField1,
      "submitbutton" -> submitButton,
      "returnlink" -> returnLink)
  }

  override def onDelete(generalProblem: Problem): Unit = {
	//can usually stay empty 
  }
}
```

This is a lot of code. Therefore, some remarks...

Each of the 3 render methods consists of 3 parts: 
* HTML/JS creation
* internal function (submits the form and evaluates, e.g. create/edit/solve)
* Binding HTML to Template

##### HTML/JS Creation

We use the functions provided by [SHTML](https://www.liftweb.net/api/32/api/net/liftweb/http/SHtml.html) to create most of the HTML. It helps to create most of the input fields.

In the solve case you are given the last solution attempt. We need to fill all the forms with the old values. Use `lastAttempt.map({generalAttempt => ...} openOr <defaultValue>` as lastAttempt is a boxed value.

Javascript is only needed if the input values need to be checked (see Internal Function).

##### Internal Function

The intern function can be done in two ways, depending on whether the input values need to be checked or not. 
* NO CHECK NEEDED: This is only the case for create and edit if all possible inputs always form a valid exercise. Then we don't need to handle feedback. The function has no parameters and we use SHTML to set local variables when the form is submitted. *(The create method in the example above is done this way).*
* CHECK NEEDED: The internal function has a parameter for the current form values. When the button is clicked a Javascript call collects all input values (e.g. as XML string or in a different encoding) and calls the internal function. The function then connects to the Backend (GraderConnection) and displays the feedback. *(The edit and solve method in the example above is done this way).*

## Step 6: Enable New Problem Type

In `model/problem.scala` add your new problem type. To do this add `val <PN>TypeName = "<PN_long>"` to the class. Now extend the mapping `knownProblemSnippets` with `<PN>TypeName -> <PN>Snippet` and also extend the mapping `knownProblemSingletons` with `<PN>TypeName -> <PN>Problem`.

In `bootstrap/liftweb/Boot.scala` add `<PN>SolutionAttempt` and `<PN>Problem` to the list of objects that are supported by the database.

Now restart the frontend to add your new problem type to the database.

## Step 7: Evaluation by Grader

All the communication with the backend is done in `lib/SOAPConnection.scala`. Here you need to implement all methods your snippet needs:
* grading method: to compute the feedback for inputs in *rendersolve* (e.g. `getWordsInGrammarFeedback()`)
* problem check: to check if the created/edited problem is feasible (e. g. `getGrammarParsingErrors()`)

Try to handle things like in the other problem types. Also make sure to encode in your feedback method whether the attempt was valid. (E.g. An attempt isn't valid if a submitted grammar has a syntax error.) 

## Step 8: Testing

Make sure to test all functionality for your new problem type:
* creation
* editing
* solving
* deletion
* sharing
* old solution reuse *(Create a course with a problem of your new problem type. Try to solve it, return to the course page and open the problem again. You should see your old attempt.)*
* export
* import

### Other things

There are often some problems with encoding when sending input values. Try to use XML as often as possible. Scala has native [XML](https://books.google.de/books?id=MFjNhTjeQKkC&pg=PA513&lpg=PA516) support. Also, it can be helpful to first remove all unwanted characters from inputs via Javascript or Scala.

After implementing automatic problem generation for you new problem type in the backend, make sure to enable it in `Generator.rendergeneration()` by adding it to the `typeOptions` array.
