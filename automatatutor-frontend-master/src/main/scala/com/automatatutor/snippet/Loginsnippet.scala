package com.automatatutor.snippet

import com.automatatutor.model.User

import scala.io.Source.fromURL
import net.liftweb.http.{S, SHtml, Templates}
import net.liftweb.util.Helpers._
import net.liftweb.http.js.JE.JsRaw
import net.liftweb.http.js.JsCmd
import net.liftweb.http.js.JsCmds
import net.liftweb.http.js.JsCmds.jsExpToJsCmd

import scala.io.BufferedSource
import scala.util.parsing.json._
import scala.xml.{NodeSeq, Text, XML}

class Loginsnippet {

  def ajaxCallString(cmd:String, func: String => JsCmd) : String = {
    val ajaxCall: JsCmd = SHtml.ajaxCall(JsRaw(cmd), func)
    return ajaxCall.toString().substring(6, ajaxCall.toString().length - 2)
  }

  def request() : NodeSeq = {
    var dummy : String = ""
    return SHtml.text("success", dummy = _)
  }

  def preprocessXml(input: String): String = {
    var output = input
    output.filter(!List('\n', '\r').contains(_)).replace("\u0027", "\'")
    output = output.replace('\n', ' ')
    output = output.replace("\"", "'")  // ' 
    output = output.replace('\r', ' ') 
    output = output.replace('\t', ' ')
    val index = output.indexOf("<table")
    val index2 = output.indexOf("</table")
    output = output.slice(index, index2+8)
    return output
  }

  def checktumaccesscode() : NodeSeq = {
    var code = S.param("code") getOrElse("none")
    if (code == "none")
      {
        return <span></span>
      }
    else
      {
        var url = "https://campus.tum.de/tumonline/wbOAuth2.token?grant_type=authorization_code" +
          "&client_id=JK3CI3OQN1URU6FHHXTQVUHEYBSHOF9A5CKDHJUBXYBWJ3GV4WUAQKYIVWU6IX1T" +
          "&client_secret=TVIRlOqVuHgWxwOEKgpVALqBMVdzXtHQUZLOTOPBCXmkaNeDbgeaHrrgrlRJHxlO&code=" + code

        try {
          var resp = fromURL(url)
          var json = JSON.parseFull(resp.mkString).orNull
          if (json == null){
            return <script>alert("Failed parsing the json response.")</script>
          }
          var map = json.asInstanceOf[Map[String, Any]]
          val accesstoken = map.get("access_token").mkString

          url = "https://campus.tum.de/tumonline/pl/rest/loc_dsmetadir.educnnamemailaffil?"+
            "access_token=" + accesstoken
          resp = fromURL(url)
          var str = preprocessXml(resp.mkString)
          var xmlFile = XML.loadString(str)
          val nodes = xmlFile \\ "td"
          val tumID = nodes(0).text
          val givenname = nodes(1).text
          val lastname = nodes(2).text
          val fullname = nodes(3).text
          val affiliation = nodes(4).text
          val mail = nodes(5).text

          url = "https://campus.tum.de/tumonline/pl/rest/loc_dsmetadir.schacpersonaluniquecode?"+
            "access_token=" + accesstoken
          resp = fromURL(url)
          str = preprocessXml(resp.mkString)
          xmlFile = XML.loadString(str)
          val matrnr = (xmlFile \\ "td").head.mkString.split(":").last.split("<").head

          User.signupExternalAccount("TUM", tumID, givenname, lastname, mail, matrnr)
        }
        catch
        {
          case e: java.io.IOException => return <script>alert("Your authorization code has expired.")</script>
          case _: Throwable => return <script>alert("Error while trying to retreive access token.")</script>
        }
        S.redirectTo("/main/index")
      }
  }

  
  def renderloginform(xhtml: NodeSeq): NodeSeq =
  {
    if (User.loggedIn_?) return S.redirectTo("/main/index")
    return User.login
  }
  
  def rendertumlogin(xhtml: NodeSeq): NodeSeq = {
    val s = """https://campus.tum.de/tumonline/wbOAuth2.authorize?
client_id=JK3CI3OQN1URU6FHHXTQVUHEYBSHOF9A5CKDHJUBXYBWJ3GV4WUAQKYIVWU6IX1T&
response_type=code&
redirect_uri=https%3A%2F%2Fautomata.model.in.tum.de%2F&scope=GET@loc.mdir.cnnamemailaffil%20GET@loc.mdir.matrnr&
state=NONCE"""
    return <a href={s}><button type="" class="landingPageButton">TUM Login</button></a>
  }
}
