package com.automatatutor.lib

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

	private def renderTableHeaderWithAttributes(headings: Seq[Text], attributes: List[(String, String)]): Node = {
		val headingsXml : NodeSeq = headings.map(heading => {
			<th>{ heading } </th>
		})

		var classString = ""
		var styleString = ""

		//NOTE: This assumes that the only two attributes you want to set are "class" and "style"
		attributes.foreach(attr =>{
			if (attr._1.equals("class")) classString = attr._2
			else if(attr._1.equals("style")) styleString = attr._2
		})

		<tr class={classString} style={styleString}> { headingsXml } </tr>
	}

	private def renderSingleRowWithAttributes[T] (ele: T, displayFuncs: Seq[T => NodeSeq], attributes: List[(String, String)]): Node = {

		var classString = ""
		var styleString = ""

		//NOTE: This assumes that the only two attributes you want to set are "class" and "style"
		attributes.foreach(attr =>{
			if (attr._1.equals("class")) classString = attr._2
			else if(attr._1.equals("style")) styleString = attr._2
		})

		<tr class={classString} style={styleString}>
			{
				displayFuncs.map(func => {
					<td>{func(ele)}</td>
				})
			}
		</tr>
	}

	private def renderTableBodyWithAttributes[T](data : Seq[T], displayFuncsPlusAttr : Seq[T => NodeSeq], attributes: List[(String, String)]) : NodeSeq = {
		data.map(ele => renderSingleRowWithAttributes(ele, displayFuncsPlusAttr, attributes))
	}

	def renderTableWithHeaderPlusAttributes[T](data : Seq[T], attributes: List[(String, String)], colSpec : (String, (T => NodeSeq))*) : NodeSeq = {
		val headings = colSpec.map(x => Text(x._1))
		val headerRow = renderTableHeaderWithAttributes(headings, attributes)

		val displayFuncs = colSpec.map(x => x._2)
		val dataRows = renderTableBodyWithAttributes(data, displayFuncs, attributes)

		<table> { headerRow ++ dataRows} </table>
	}
	
	def renderTableWithComplexHeader[T] (data : Seq[T], colSpec : (NodeSeq, (T => NodeSeq))*) : NodeSeq = {
	  val headings = colSpec.map(_._1)
	  val headerRow = renderTableHeader(headings)

	  val displayFuncs = colSpec.map(_._2)
	  val dataRows = renderTableBody(data, displayFuncs)

	  return <table> { headerRow ++ dataRows } </table>
	}
}
