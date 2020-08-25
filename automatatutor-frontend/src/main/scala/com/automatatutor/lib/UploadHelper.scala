package com.automatatutor.lib

import java.io.InputStream

import com.automatatutor.lib.DownloadHelper.{ZipFile, offerZipDownloadToUser}
import net.liftweb.common.{Box, Empty, EmptyBox, Full}
import net.liftweb.http.SHtml.{ajaxForm, ajaxSubmit, fileUpload}
import net.liftweb.http.{FileParamHolder, InMemFileParamHolder, OnDiskFileParamHolder, S, SHtml}
import net.liftweb.util._
import net.liftweb.http.js.JsCmds.Alert
import Helpers._
import com.automatatutor.model.Problem

import scala.xml.{NodeSeq, XML}

class UploadHelper {

  def loadProblemsFromZip(inputStream: InputStream)={
//    var imported = 0
//    var failed = 0
//
//    val xml = XML.loadString(importing)
//    (xml \ "_").foreach(
//      (problemXML) => {
//        val problem = Problem.fromXML(problemXML)
//        if (problem != Empty) {
//          imported += 1
//        }
//        else {
//          failed += 1
//        }
//      })
//    //give feedback
//    if (imported > 0) {
//      S.notice("Successfully imported " + imported.toString + " problems")
//    }
//    if (failed > 0) {
//      S.error("Could not import " + failed.toString + " problems")
//    }
//    S.redirectTo("/main/index")
  }

  def loadProblemFromXml(inputStream: InputStream): Unit = {
    val problemText = scala.io.Source.fromInputStream(inputStream).mkString
    try {
      val xml = scala.xml.XML.loadString(problemText)

      val problem = Problem.fromXML((xml \ "_").head)

      if(problem != Empty){
        S.notice("Successfully imported problem.")
      }
      else{
        S.error("Could not import problem. XMl not formatted properly.")
      }
    }
    catch {
      case _ : Exception => S.error("Could not import problem. XMl not formatted properly.")
    }
  }

  def loadProblems(holder: InMemFileParamHolder): Unit ={
    val inputStream = holder.fileStream
    if(holder.fileName.contains(".xml")) loadProblemFromXml(inputStream)
    else if (holder.fileName.contains(".zip")) loadProblemsFromZip(inputStream)
    S.error("Could not import problems. Incorrect file format.")
//    Alert("Oh, you upload file " + holder.fileName + ", yes?")
  }

  def fileUploadForm(form: NodeSeq): NodeSeq = {
    var fileHolder: Box[FileParamHolder] = Empty

    def handleFile() = {
      // Do something with the file.
//      var file: FileParamHolder = fileHolder openOrThrowException "test"
//
//
      val holder: FileParamHolder = fileHolder openOrThrowException "File holder not opened correctly!"
      val inmemHolder: InMemFileParamHolder = holder.asInstanceOf[InMemFileParamHolder]
      loadProblems(inmemHolder)
      //      println(holder)
//
//      fileHolder.map { holder =>
//        val inmemHolder: InMemFileParamHolder = holder.asInstanceOf[InMemFileParamHolder]
//        loadProblems(inmemHolder)
//
//        //        holder match {
////          case holder: InMemFileParamHolder => println("inmem")
////          case holder: OnDiskFileParamHolder => println("on dis??")
////          case _ =>
////        }
//
//
//
//      }

//      Alert("Well *that* upload failed...")

//        fileHolder match {
//          case Full(value) => Alert("Oh, you upload file " + value.fileName + ", yes?")
//          case box: EmptyBox => Alert("Well *that* upload failed...")
//        }
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

  def printRed(thing: Any) = {
    println("\u001b[0m\u001b[31m" + thing + "\u001b[0m")
  }

  def printGreen(thing: Any) = {
    println("\u001b[0m\u001b[32m" + thing + "\u001b[0m")
  }

  def printYellow(thing: Any) = {
    println("\u001b[0m\u001b[33m" + thing + "\u001b[0m")
  }

  def printBox[T](thing: Box[T]) = {
    import net.liftweb.common.{Full, Failure, Empty}
    thing match {
      case Full(_) =>
        printGreen(thing)
      case Failure(_, _, _) =>
        printYellow(thing)
      case Empty =>
        printRed(thing)
    }
  }


  def renderCourseUploadLink(zipFileName: String, files: List[(String, String)], linkBody: NodeSeq): NodeSeq = {
    SHtml.link("ignored", () => println("idk"), linkBody)
  }
}
