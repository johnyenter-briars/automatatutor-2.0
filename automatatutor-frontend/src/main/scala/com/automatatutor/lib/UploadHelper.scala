package com.automatatutor.lib

import java.io.{ByteArrayOutputStream, InputStream}
import java.util.zip.{ZipEntry, ZipFile}

import net.liftweb.common.{Box, Empty, EmptyBox, Full}
import net.liftweb.http.SHtml.{ajaxForm, ajaxSubmit, fileUpload}
import net.liftweb.http.{FileParamHolder, InMemFileParamHolder, OnDiskFileParamHolder, S, SHtml}
import net.liftweb.util._
import Helpers._
import com.automatatutor.model.Problem
import java.util.zip.ZipInputStream

import scala.xml.{NodeSeq, XML}

class UploadHelper {

  def addProblem(problemText: String): Unit = {
    try {
      val xml = scala.xml.XML.loadString(problemText)

      val problem = Problem.fromXML((xml \ "_").head)

      if(problem == Empty)
        S.error("Could not import problem XMl not formatted properly.")
    }
    catch {
      case _ : Exception => S.error("Could not import problem. XMl not formatted properly.")
    }
  }

  def loadProblemsFromZip(inputStream: InputStream): Unit = {
    val zis = new ZipInputStream(inputStream)
    Stream.continually(zis.getNextEntry).takeWhile(_ != null).foreach{ file: ZipEntry =>
      //If theres a file that has a size over 90000 bytes idk what to tell you
      val byteOutputStream = new ByteArrayOutputStream(90000)
      val buffer = new Array[Byte](1024)
      Stream.continually(zis.read(buffer)).takeWhile(_ != -1).foreach(byteOutputStream.write(buffer, 0, _))
      addProblem(byteOutputStream.toString)
    }
  }

  def loadProblemFromXml(inputStream: InputStream): Unit = {
    val problemText = scala.io.Source.fromInputStream(inputStream).mkString
    addProblem(problemText)
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
