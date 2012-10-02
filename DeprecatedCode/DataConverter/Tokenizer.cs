// --------------------------------------------------------------------------------------------
#region // Copyright (c) 2003, SIL International. All Rights Reserved.
// <copyright from='2003' to='2003' company='SIL International'>
//		Copyright (c) 2003, SIL International. All Rights Reserved.
//
//		Distributable under the terms of either the Common Public License or the
//		GNU Lesser General Public License, as specified in the LICENSING.txt file.
// </copyright>
#endregion
//
// File: Tokenizer.cs
// Responsibility:
// Last reviewed:
//
// <remarks>
// </remarks>
// --------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Diagnostics;
using SIL.FieldWorks.DataConverter;
using SilEncConverters;

namespace SIL.FieldWorks.DataConverter
{
	/// <summary>
	/// Summary description for Tokenizer.
	/// </summary>
	public class Tokenizer
	{
		StringTri m_tri;
		string m_line;
		Stack m_stack; // of tokens.
		StreamReader m_input;
		int m_lineNo;
		int m_column;
		bool m_startOfFile;
		bool m_fGetNextLine;

		EncConverters m_converters;

		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="Tokenizer"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public Tokenizer()
		{
			m_stack = new Stack();
			m_tri = new StringTri();
		}

		public Stack TokenStack
		{
			get {return m_stack;}
		}
		public StringTri Tri
		{
			get {return m_tri;}
		}
		public EncConverters Converters
		{
			get
			{
				if (m_converters == null)
				{
					// loads converters from on disk XML encoding repository file
					m_converters = new EncConverters();
				}
				return m_converters;
			}
		}

		public Token Next()
		{
			Token returnToken;

			if (m_startOfFile)
			{
				m_startOfFile = false;
				// Since all field tokens need a token on the stack (they do a pop) before they push themselves
				// on the stack, this is needed to put something on the stack for the first field token.
				returnToken = new StartOfFileToken();
				returnToken.Initialize(this);

				return returnToken;
			}

			if (m_column == m_line.Length && m_fGetNextLine)
			{
				m_fGetNextLine = false;

				m_line = m_input.ReadLine();
				if (m_line == null)
				{
					returnToken = new EndOfFileToken(m_lineNo, m_column);
					returnToken.Initialize(this);
					return returnToken;
				}

				m_lineNo++;
				m_column = 0;
			}

			if (m_column == m_line.Length)
			{
				// Finish processing this line by returning a NewlineToken
				m_fGetNextLine = true;
				returnToken = new NewlineToken(m_lineNo, m_column);
				returnToken.Initialize(this);
				return returnToken;
			}

			// m_column is the column in the line to start searching
			int nextMarkerStart; // column in line where the next marker starts
			int nextColumn; // column in line after end of the next marker
			MarkerSpec target;

			nextMarkerStart = m_tri.Search(m_line, m_column, out nextColumn, out target);
			if (nextMarkerStart == -1)
			{
				// no more markers on m_line after m_column
				nextMarkerStart = m_line.Length;
			}

			if (nextMarkerStart > m_column)
			{
				// there was a non-zero distance between the end of the last found marker
				// and the start of the current marker, therefore there is data on the line,
				// and needs to be returned in a data token

				// Review (BobbyD): Possible enhancement: The leading whitespace after a field marker
				// could be returned in a separate WhitespaceToken, separate from the DataToken.
				// This would allow the whitespace to be converted using the MarkerMap from the field marker
				// (which is what the Waxhaw TE team would like to see), rather than using the DataMap.
				// In addition, the whitespace could be normalized to one space if needed.
				string data = m_line.Substring(m_column, nextMarkerStart-m_column);
				returnToken = new DataToken(m_lineNo, m_column, data);
				m_column = nextMarkerStart;
			}
			else
			{
				// since there was no data to return, return some type of marker token
				returnToken = ((MarkerSpec)target).MakeToken(m_lineNo, m_column);
				m_column = nextColumn;
			}

			returnToken.Initialize(this);
			return returnToken;
		}

		public StreamReader Input
		{
			get {return m_input;}
			set
			{
				// Is called only once per stream.
				m_input = value;
				m_startOfFile = true;
				m_lineNo = 0;
				m_column = 0;
				m_line = "";
				m_fGetNextLine = true;
			}
		}
	}
}
