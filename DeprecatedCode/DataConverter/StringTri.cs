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
// File: StringTri.cs
// Responsibility:
// Last reviewed:
//
// <remarks>
// </remarks>
// --------------------------------------------------------------------------------------------
using System;

namespace SIL.FieldWorks.DataConverter
{
	/// <summary>
	/// Summary description for StringTri.
	/// </summary>
	public class StringTri
	{
		TriNode m_tnRoot = new TriNode();
		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="StringTri"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public StringTri()
		{
		}

		/// <summary>
		/// Add a new search target to the Tri.
		/// </summary>
		/// <param name="markerSpec">MarkerSpec containing the marker string to match</param>
		public void Add(MarkerSpec markerSpec)
		{
			m_tnRoot.Add(markerSpec.Marker, 0, markerSpec);
		}

		/// <summary>
		/// Stop searching for the specified key.
		/// </summary>
		/// <param name="key"></param>
		public void Remove(string key)
		{
			// We implement this by replacing the target, if any, for this key with null.
			// It would be possible to instead remove one or more nodes from the tree,
			// but for curretly expected applications of this class, something we remove is
			// very likely to get added again, so it's more efficient to keep the node around.
			m_tnRoot.Add(key, 0, null);
		}



		/// <summary>
		/// Look for one of your keys at the specified position or any subsequenct position.
		///
		/// Note that this returns an index, while Match returns the object. The rationale is that
		/// Match doesn't return a position at all, so the most useful return result seems to be the
		/// object. On the other hand, it is a widely used convention that search functions return
		/// the position where the match was found.
		/// </summary>
		/// <param name="line">One line of input (begin of line and end of line are
		/// assumed to occur before and after this string</param>
		/// <param name="startIndex">Position to look for keyword</param>
		/// <param name="nextIndex">Next char position after match (or line.Length if no match)</param>
		/// <param name="target">null if no match, or the object that matches.</param>
		/// <returns>-1 if not found, or char index of first character of match</returns>
		public int Search(string line, int startIndex, out int nextIndex, out MarkerSpec target)
		{
			target = null;
			nextIndex = line.Length;
			for (int ich = startIndex; ich < line.Length; ich++)
			{
				int nextIch;
				target = Match(line, ich, out nextIch);
				if (target != null)
				{
					nextIndex = nextIch;
					return ich;
				}
			}
			return -1;
		}


		/// <summary>
		/// Look for one of your keys at the specified position.
		/// </summary>
		/// <param name="line">One line of input (begin of line and end of line are
		/// assumed to occur before and after this string</param>
		/// <param name="startIndex">Position to look for keyword</param>
		/// <param name="nextIndex">Next char position after match (or startIndex + 1
		/// if no match). Note that this is the character immediately after the
		/// string matched; even if following whitespace is required for a match,
		/// it is not included.</param>
		/// <returns>null if no match, or the object that matches.</returns>
		public MarkerSpec Match(string line, int startIndex, out int nextIndex)
		{
			int ich = startIndex;
			// set to target if we find a viable match, but we go on searching as
			// there may be a longer one.
			MarkerSpec bestMatch = null;
			nextIndex = startIndex + 1; // default for no match.
			TriNode triNode = m_tnRoot;
			while (ich < line.Length)
			{
				triNode = triNode.Match(line[ich]);
				if (triNode == null)
					break;
				ich++;
				if (triNode.TargetMarkerSpec != null)
				{
					// The following is from a conversation with Ted Goller in Dallas on 18-Feb-2003.
					// Standard Format Markers like \c, \p, \v, etc are preceeded by a
					// newline, and followed by either a newline or a space. The exception is
					// a footnote marker, which does not want to have any whitespace before
					// the marker.

					// The following is from Darrel Eppler via email in Dec-2002.
					// Backslash codes (e.g., \p) are placed at the beginning of a line.
					// Bar codes (e.g. |i) appear in the text stream without additional spacing:
					// "This is |iitalic|r text" means "This is italic text."

					// These comments are implemented in the code below.

					// This is a possible match. Does it satisfy before/after conditions?
					MarkerSpec markerSpec = triNode.TargetMarkerSpec;
					// If this is only an intermediate node and has no real target, don't remember it.
					if (markerSpec == null)
						continue;
					// If we require a start-of-line match and weren't looking at the start of the line,
					// not a match.
					if (markerSpec.IsNewlineBeforeRequired() && startIndex != 0)
						continue;
					// If we require white space following, and aren't at the end of the line,
					// the next character must be a space. If not, this match is no good.
					if (markerSpec.IsWhitespaceAfterRequired() && ich < line.Length && line[ich] != ' ')
						continue;
					// OK, this is a match. Remember it, but continue in case we find a longer match.
					bestMatch = markerSpec;
					nextIndex = (markerSpec.IsWhitespaceAfterRequired() && ich < line.Length - 1 ?
						ich + 1 : ich);
				}
			}
			return bestMatch;
		}

	}

	/// <summary>
	/// Used for the implementation of StringTri, public to allow testing.
	/// </summary>
	public class TriNode
	{
		MarkerSpec m_target;

		TriNode[] m_values;

		public TriNode()
		{
		}

		public MarkerSpec TargetMarkerSpec
		{
			get {return m_target;}
			set {m_target = value;}
		}

		/// <summary>
		/// Add the specified key to the tri.
		/// Assumes this is the node reached by traversing key[0]..key[ich-1].
		/// Add nodes as necessary so that key can be matched and will return value.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="ich"></param>
		/// <param name="markerSpec"></param>
		public void Add(string key, int ich, MarkerSpec markerSpec)
		{
			if (ich >= key.Length)
				TargetMarkerSpec = markerSpec;
			else
			{
				TriNode tn = Match(key[ich]);
				if (tn == null)
				{
					tn = new TriNode();
					Add(key[ich], tn);
				}
				tn.Add(key, ich + 1, markerSpec);
			}
		}

		/// <summary>
		/// Associate the specified TriNode with the specified char.
		/// </summary>
		/// <param name="key"></param>
		/// <param name="triNode"></param>
		public void Add(char key, TriNode triNode)
		{
			if (m_values == null)
				m_values = new TriNode[256];
			m_values[Convert.ToInt32(key)] = triNode;
		}

		public TriNode Match(char key)
		{
			if (m_values == null)
				return null;

			// Review (BobbyD): I suspect that the line below that says
			// return new TriNode();
			// should be
			// return null;
			// The code that calls this function can handle a null return,
			// and this change might give an performance improvement.
			// The current code works, I think, only because the code will encounter
			// nulls in the new TriNode on the next call.

			// Also, the 255 below is related to the 256 above,
			// this could be refactored to a constant value.
			if ( Convert.ToInt32(key) > 255 )
				return new TriNode();
			return m_values[Convert.ToInt32(key)];
		}
	}
}
