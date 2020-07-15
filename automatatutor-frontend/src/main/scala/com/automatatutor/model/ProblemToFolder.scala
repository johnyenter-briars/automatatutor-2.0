package com.automatatutor.model

import net.liftweb.mapper._
import net.liftweb.common.Box

class ProblemToFolder extends LongKeyedMapper[ProblemToFolder] with IdPK {
  def getSingleton = ProblemToFolder

  protected object problemID extends MappedLongForeignKey(this, ProblemLink)
  protected object folderID extends MappedLongForeignKey(this, Folder)

  def getProblem : ProblemLink = this.problemID.obj openOrThrowException "Every ProblemToFolder must have a Problem"
  def setProblem ( problem : ProblemLink ) = this.problemID(ProblemLink)

  def getFolder : Folder = this.folderID.obj openOrThrowException "Every ProblemToFolder must have a Folder"
  def setFolder ( folder : Folder ) = this.folderID(folder)
}

object ProblemToFolder extends ProblemToFolder with LongKeyedMetaMapper[ProblemToFolder] {

  def findAllByFolder(folder: Folder) : List[ProblemToFolder] =
    this.findAll(By(ProblemToFolder.folderID, folder))

  def deleteProblemsUnderFolder(folder: Folder): Unit = {

    //TODO 7/15/2020 fix this
//    this.findAllByFolder(folder).foreach(problemToFolder => {
//      //Have to set the course of each problem to None so it goes back to the "autogenerated bucket"
//      ProblemLink.findAll()
//        .filter(prob => prob.getProblemID == problemToFolder.getProblem.getProblemID)
//        .foreach(problem =>{
//            problem.setCourse(None)
//            problem.save
//          })
//      problemToFolder.delete_!
//    })
  }
}
