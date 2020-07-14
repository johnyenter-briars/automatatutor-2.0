package com.automatatutor.model

import net.liftweb.mapper._
import net.liftweb.common.Box

class ProblemToFolder extends LongKeyedMapper[ProblemToFolder] with IdPK {
  def getSingleton = ProblemToFolder

  protected object problemID extends MappedLongForeignKey(this, Problem)
  protected object folderID extends MappedLongForeignKey(this, Folder)

  def getProblem : Problem = this.problemID.obj openOrThrowException "Every ProblemToFolder must have a Problem"
  def setProblem ( problem : Problem ) = this.problemID(problem)

  def getFolder : Folder = this.folderID.obj openOrThrowException "Every ProblemToFolder must have a Folder"
  def setFolder ( folder : Folder ) = this.folderID(folder)
}

object ProblemToFolder extends ProblemToFolder with LongKeyedMetaMapper[ProblemToFolder] {

  def findAllByFolder(folder: Folder) : List[ProblemToFolder] =
    this.findAll(By(ProblemToFolder.folderID, folder))

  def deleteProblemsUnderFolder(folder: Folder): Unit = {
    this.findAllByFolder(folder).foreach(problemToFolder => {
      //Have to set the course of each problem to None so it goes back to the "autogenerated bucket"
      Problem.findAll()
        .filter(prob => prob.getProblemID == problemToFolder.getProblem.getProblemID)
        .foreach(problem =>{
            problem.setCourse(None)
            problem.save
          })
      problemToFolder.delete_!
    })
  }
}
