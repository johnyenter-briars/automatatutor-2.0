package com.automatatutor.lib

import java.io.{ByteArrayOutputStream, InputStream}
import java.text.SimpleDateFormat
import java.util.Calendar
import java.util.zip.{ZipEntry, ZipFile}

import net.liftweb.common.{Box, Empty, EmptyBox, Full}
import net.liftweb.http.SHtml.{ajaxForm, ajaxSubmit, fileUpload}
import net.liftweb.http.{FileParamHolder, InMemFileParamHolder, OnDiskFileParamHolder, S, SHtml}
import net.liftweb.util._
import Helpers._
import com.automatatutor.model.{Course, Exercise, Folder, Problem, User}
import java.util.zip.ZipInputStream

import scala.xml.{NodeSeq, XML}

object UploadTargetEnum extends Enumeration {
  type UploadTargetEnum = Value
  val ProblemPool, Folder, Course = Value
}

import UploadTargetEnum._

class UploadTarget( target: UploadTargetEnum, targetObj: Object) {
  def getTarget: UploadTargetEnum = target
  def getFolder: Folder = targetObj.asInstanceOf[Folder]
  def getCourse: Course = targetObj.asInstanceOf[Course]
}

class UploadHelper(target: UploadTarget) {

  def addProblem(fileText: String): Unit = {
    try {
      val xml = scala.xml.XML.loadString(fileText)
      println((xml \ "_").head)
      (xml \ "_").foreach((problemXml) => {
        val problem = Problem.fromXML(problemXml)

        if(problem == Empty) {
          S.error("Could not import problem. XMl not formatted properly.")
        }else{
          if(target.getTarget == UploadTargetEnum.Folder){
            val openedProblem = problem.openOrThrowException("Problem should not be empty")
            if(!target.getFolder.hasIdenticalProblem(openedProblem)){
              val exercise = new Exercise
              exercise.setFolder(target.getFolder).setProblem(openedProblem).save()
            }
          }
        }
      })


    }
    catch {
      case e : Exception => S.error("Could not import problem. XMl not formatted properly.")
    }
  }

  def loadProblemsFromZip(inputStream: InputStream): Unit = {
    val zipInputStream = new ZipInputStream(inputStream)
    Stream.continually(zipInputStream.getNextEntry).takeWhile(_ != null).foreach{ file: ZipEntry =>
      //If there's an xml file that has a size over 999999999 bytes idk what to tell you,
      //That means you have a giagantic xml file that you're asking to be imported. You should just import those problems
      //via the single file import in the problem pool
      val byteOutputStream = new ByteArrayOutputStream(999999999)
      val buffer = new Array[Byte](1024)
      Stream.continually(zipInputStream.read(buffer)).takeWhile(_ != -1).foreach(byteOutputStream.write(buffer, 0, _))
      addProblem(byteOutputStream.toString)
    }
  }

  def loadProblemFromXml(inputStream: InputStream): Unit = {
    val problemText = scala.io.Source.fromInputStream(inputStream).mkString
    addProblem(problemText)
  }

  def loadFolderToCourseFromXml(inputStream: InputStream, folderName: String): Unit = {
    val problemText = scala.io.Source.fromInputStream(inputStream).mkString
    val user = User.currentUser openOrThrowException "Lift only allows logged in users on here"

    val dateFormat = new SimpleDateFormat("EEE, MMM d, K:mm ''yy")
    val now: Calendar = Calendar.getInstance()
    val oneWeekFromNow: Calendar = Calendar.getInstance()
    oneWeekFromNow.add(Calendar.WEEK_OF_YEAR, 1)

    if(target.getTarget == UploadTargetEnum.Course){
      val folder = new Folder
      folder.setCourse(target.getCourse)
        .setCreator(user)
        .setStartDate(now.getTime)
        .setEndDate(oneWeekFromNow.getTime)
        .setLongDescription(folderName)
        .setVisible(false)
        .save()

      def importProblems(importing: String) = {
        var imported = 0
        var failed = 0

        val xml = XML.loadString(importing)
        (xml \ "_").foreach(
          (problemXML) => {
            val problem = Problem.fromXML(problemXML)
            if (problem != Empty) {
              imported += 1
              val exercise = new Exercise
              exercise.setProblem(problem.get).setFolder(folder).save()
            }
            else {
              failed += 1
            }
          })
        //give feedback
        if (imported > 0) {
          S.notice("Successfully imported " + imported.toString + " problems")
        }
        if (failed > 0) {
          S.error("Could not import " + failed.toString + " problems")
        }
        S.redirectTo("/main/course/index")
      }

      importProblems(problemText)
    }
  }

  def loadProblemsToCourse(holder: InMemFileParamHolder): Unit ={
    val inputStream = holder.fileStream
    if(holder.fileName.contains(".xml")) loadFolderToCourseFromXml(inputStream, holder.fileName.replace(".xml", ""))
//    else if (holder.fileName.contains(".zip")) loadProblemsFromZip(inputStream)
    else S.error("Could not import problems. Incorrect file format.")
  }

  def fileUploadToCourseForm(form: NodeSeq): NodeSeq = {
    var fileHolder: Box[FileParamHolder] = Empty

    def handleFile(): Unit = {
      fileHolder match {
        case Full(holder) => loadProblemsToCourse(holder.asInstanceOf[InMemFileParamHolder])
        case box: EmptyBox => S.error("Error reading file. Did you remember to select a file before submitting?")
      }
    }


    val bindForm =
      "type=file" #> fileUpload((fph) => {
        fileHolder = Full(fph)
      }) &
        "type=submit" #> ajaxSubmit("Submit", handleFile)

    ajaxForm(
      bindForm(form)
    )
  }

  def loadProblems(holder: InMemFileParamHolder): Unit ={
    val inputStream = holder.fileStream
    if(holder.fileName.contains(".xml")) loadProblemFromXml(inputStream)
    else if (holder.fileName.contains(".zip")) loadProblemsFromZip(inputStream)
    else S.error("Could not import problems. Incorrect file format.")
  }

  def fileUploadForm(form: NodeSeq): NodeSeq = {
    var fileHolder: Box[FileParamHolder] = Empty

    def handleFile(): Unit = {
      fileHolder match {
        case Full(holder) => loadProblems(holder.asInstanceOf[InMemFileParamHolder])
        case box: EmptyBox => S.error("Error reading file. Did you remember to select a file before submitting?")
      }
    }

    val bindForm =
      "type=file" #> fileUpload((fph) => {
        fileHolder = Full(fph)
      }) &
        "type=submit" #> ajaxSubmit("Submit", handleFile)

    ajaxForm(
      bindForm(form)
    )
  }
}
