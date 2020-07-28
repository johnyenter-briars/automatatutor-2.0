package com.automatatutor.lib

import net.liftweb.http.SHtml
import net.liftweb.util.Helpers.TheStrBindParam

import scala.xml.{Elem, Node, NodeSeq, Null, Text, TopScope}

object TableHelper {
	private def renderTableHeader( headings : Seq[NodeSeq] ) : Node = {
	  val headingsXml : NodeSeq = headings.map(heading => <th> { heading } </th>)
	  return <tr> { headingsXml } </tr>
	}
	 
	private def renderSingleRow[T] ( datum : T , displayFuncs : Seq[T => NodeSeq]) : Node = {
	  return <tr> { displayFuncs.map(func => <td> { func(datum) } </td>) } </tr>
	}

	private def renderTableBody[T] ( data : Seq[T], displayFuncs : Seq[T => NodeSeq]) : NodeSeq = {
	  return data.map(renderSingleRow(_, displayFuncs))
	}
	
	private def renderTable[T] (data : Seq[T], displayFuncs : (T => NodeSeq)*) : NodeSeq = {
	  val dataRows = renderTableBody(data, displayFuncs)

	  return <table> { dataRows } </table>
	}
	
	def renderTableWithHeader[T] (data : Seq[T], colSpec : (String, (T => NodeSeq))*) : NodeSeq = {
	  val headings = colSpec.map(x => Text(x._1))
	  val headerRow = renderTableHeader(headings)

	  val displayFuncs = colSpec.map(_._2)
	  val dataRows = renderTableBody(data, displayFuncs)

	  return <table> { headerRow ++ dataRows } </table>
	}

	private def renderTableHeaderWithAttributes(tableID: String, headings: Seq[Text], attributes: List[(String, (Int, String) => String)]): Node = {
		//In order to get a custom classString or styleString you need to pass in a function that generates the string dynamically
		//You can then use code similar to below to generate the string calling:
		/*
			val classFunc = attributes.filter(_._1.equals("onClick")).head._2
			val classString = classFunc([parameters])
		 */
		val classString = ""
		val styleString = ""

		val headingsXml: NodeSeq = headings.zipWithIndex.map(heading_index => {
			val onClickFunc = attributes.filter(_._1.equals("onClick")).head._2
			val onClickVal = if(heading_index._1.toString.isEmpty) "" else onClickFunc(heading_index._2, tableID)
			<th style="cursor: pointer;" onClick={onClickVal}>{ heading_index._1 } </th>
		})

		<tr class={classString} style={styleString}> { headingsXml } </tr>
	}

	def renderTableWithHeaderPlusAttributes[T](tableID: String, data : Seq[T], attributes: List[(String, (Int, String) => String)], colSpec : (String, (T => NodeSeq))*) : NodeSeq = {
		val headings = colSpec.map(x => Text(x._1))
		val headerRow = renderTableHeaderWithAttributes(tableID, headings, attributes)

		val displayFuncs = colSpec.map(x => x._2)
		val dataRows = renderTableBody(data, displayFuncs)

		<table id={tableID}> { headerRow ++ dataRows} </table>
	}
	
	def renderTableWithComplexHeader[T] (data : Seq[T], colSpec : (NodeSeq, (T => NodeSeq))*) : NodeSeq = {
	  val headings = colSpec.map(_._1)
	  val headerRow = renderTableHeader(headings)

	  val displayFuncs = colSpec.map(_._2)
	  val dataRows = renderTableBody(data, displayFuncs)

	  return <table> { headerRow ++ dataRows } </table>
	}
}
