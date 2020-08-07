package com.automatatutor.lib

import java.io.ByteArrayInputStream

import scala.xml.NodeSeq

import net.liftweb.http.ResponseShortcutException
import net.liftweb.http.SHtml
import net.liftweb.http.StreamingResponse

object DownloadHelper {

  private abstract class FileType(mimeType : String, fileSuffix : String) {
    def getMimeType = mimeType
    def getFileSuffix = fileSuffix
  }
  private case object CsvFile extends FileType("text/csv", ".csv")
  private case object XmlFile extends FileType("text/xml", ".xml")
  private case object XlsxFile extends FileType("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx")
  private case object ZipFile extends  FileType("application/zip", ".zip")

  private def offerDownloadToUser(contents: String, filename: String, filetype: FileType): Unit = {
    def buildDownloadResponse = {
      val contentAsBytes = contents.getBytes()
      val downloadSize = contentAsBytes.length

      def buildStream = {
        new ByteArrayInputStream(contentAsBytes)
      }
      
      def buildHeaders = {
        val filenameWithSuffix = filename + filetype.getFileSuffix
        List(
          "Content-type" -> filetype.getMimeType,
          "Content-length" -> downloadSize.toString,
          "Content-disposition" -> ("attachment; filename=" + filenameWithSuffix)
        )
      }

      val stream = buildStream
      val onEndCallback = () => {}
      val headers = buildHeaders
      val cookies = Nil
      val responseCode = 200
      
      new StreamingResponse(
          stream, onEndCallback, downloadSize, headers, cookies, responseCode
      )
    }

    throw new ResponseShortcutException(buildDownloadResponse)
  }

  private def offerZipDownloadToUser(zipFileName: String, files: List[(String, String)], filetype: FileType): Unit = {
    def buildDownloadResponse = {

      import java.io.ByteArrayOutputStream
      import java.io.IOException
      import java.util.zip.ZipEntry
      import java.util.zip.ZipOutputStream
      val byteArrOutputStream = new ByteArrayOutputStream
      try {
        val zipOutputStream = new ZipOutputStream(byteArrOutputStream)
        try {
          /* File is not on the disk, test.txt indicates
               only the file name to be put into the zip */
          files.foreach(fileTuple => {
            val entry = new ZipEntry(fileTuple._1 + ".zip")
            zipOutputStream.putNextEntry(entry)
            zipOutputStream.write(fileTuple._2.getBytes)
            zipOutputStream.closeEntry()
          })

        } catch {
          case ioe: IOException =>
            ioe.printStackTrace()
        } finally if (zipOutputStream != null) zipOutputStream.close()
      }

      val contentAsBytes = byteArrOutputStream.toByteArray
      val downloadSize = byteArrOutputStream.toByteArray.length

      def buildStream = {
        new ByteArrayInputStream(contentAsBytes)
      }

      def buildHeaders = {
        val filenameWithSuffix = zipFileName + filetype.getFileSuffix
        List(
          "Content-type" -> filetype.getMimeType,
          "Content-length" -> downloadSize.toString,
          "Content-disposition" -> ("attachment; filename=" + filenameWithSuffix)
        )
      }

      val stream = buildStream
      val onEndCallback = () => {}
      val headers = buildHeaders
      val cookies = Nil
      val responseCode = 200

      new StreamingResponse(
        stream, onEndCallback, downloadSize, headers, cookies, responseCode
      )
    }

    throw new ResponseShortcutException(buildDownloadResponse)
  }

  def renderZipDownloadLink(zipFileName: String, files: List[(String, String)], linkBody: NodeSeq): NodeSeq = {
    SHtml.link("ignored", () => offerZipDownloadToUser(zipFileName, files, ZipFile), linkBody)
  }

  def renderCsvDownloadLink(contents : String, filename : String, linkBody : NodeSeq ) : NodeSeq = {
    return SHtml.link("ignored", () => offerDownloadToUser(contents, filename, CsvFile), linkBody)
  }
  
  def renderXmlDownloadLink(contents : NodeSeq, filename : String, linkBody : NodeSeq ) : NodeSeq = {
    return SHtml.link("ignored", () => offerDownloadToUser(contents.toString, filename, XmlFile), linkBody)
  }

  def renderDownloadLink(contents : String, filename : String, linkBody : NodeSeq ) : NodeSeq = {
    return SHtml.link("ignored", () => offerDownloadToUser(contents.toString, filename, XlsxFile), linkBody)
  }
}