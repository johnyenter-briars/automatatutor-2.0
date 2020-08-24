package com.automatatutor.lib

import com.automatatutor.lib.DownloadHelper.{ZipFile, offerZipDownloadToUser}
import net.liftweb.http.SHtml

import scala.xml.NodeSeq

class UploadHelper {




  def renderCourseUploadLink(zipFileName: String, files: List[(String, String)], linkBody: NodeSeq): NodeSeq = {
    SHtml.link("ignored", () => println("idk"), linkBody)
  }
}
