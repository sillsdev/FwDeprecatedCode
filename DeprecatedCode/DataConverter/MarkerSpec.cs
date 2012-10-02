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
// File: MarkerSpec.cs
// Responsibility:
// Last reviewed:
//
// <remarks>
// </remarks>
// --------------------------------------------------------------------------------------------
using System;
//using ECOBJECTSLib;

namespace SIL.FieldWorks.DataConverter
{
	/// <summary>
	/// MarkerSpec provides information about something we're looking for in the source text.
	/// </summary>
	public abstract class MarkerSpec
	{
		string m_marker; // Marker we're looking for to identify (the start of) the target.
		string m_markerMap; // Mapping to convert marker to Unicode.
		string m_dataMap; // Mapping to convert following data to Unicode.
		string m_replaceMarker; // replace marker with this string on output.

		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="MarkerSpec"/> class.
		/// If replaceMaker is null or empty, use marker (no replacement done).
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public MarkerSpec(string marker, string markerMap, string dataMap, string replaceMarker)
		{
			m_marker = marker;
			m_markerMap = markerMap;
			m_dataMap = dataMap;
			m_replaceMarker = replaceMarker;
		}

		public abstract bool IsInline();

		// if true, marker is recognized only at start of line.
		public abstract bool IsNewlineBeforeRequired();

		// if true, marker is recognized only before space or end of line.
		public abstract bool IsWhitespaceAfterRequired();

		public string Marker
		{
			get {return m_marker;}
			set {m_marker = value;}
		}
		public string MarkerMap
		{
			get {return m_markerMap;}
			set {m_markerMap = value;}
		}
		public string DataMap
		{
			get {return m_dataMap;}
			set {m_dataMap = value;}
		}
		public string ReplaceMarker
		{
			get {return m_replaceMarker;}
			set {m_replaceMarker = value;}
		}

		/// <summary>
		/// Make a new token from a MarkerSpec.
		/// </summary>
		/// <param name="line"></param>
		/// <param name="column"></param>
		/// <returns></returns>
		public virtual Token MakeToken(int line, int column)
		{
			return new MarkerToken(line, column, this);
		}

#if false
		/// <summary>
		/// A Factory method to create the correct type of MarkerSpec object.
		/// </summary>
		/// <param name="mapping"></param>
		/// <returns></returns>
		public static MarkerSpec CreateMarkerSpec(IECMapping mapping)
		{
			MarkerSpec ms;

			ms = CreateMarkerSpec(mapping.BeginMarker, mapping.MarkerEncoding, mapping.DataEncoding,
				mapping.IsInline != 0, mapping.NewBeginMarker, mapping.EndMarker, mapping.NewEndMarker);

			return ms;
		}
#endif
		/// <summary>
		/// A Factory method to create the correct type of MarkerSpec object.
		/// </summary>
		/// <param name="mapping"></param>
		/// <returns></returns>
		public static MarkerSpec CreateMarkerSpec(IDCMapping mapping)
		{
			MarkerSpec ms;

			ms = CreateMarkerSpec(mapping.BeginMarker, mapping.MarkerEncoding, mapping.DataEncoding,
				mapping.IsInline, mapping.NewBeginMarker, mapping.EndMarker, mapping.NewEndMarker);

			return ms;
		}

		/// <summary>
		/// A Factory method to create the correct type of MarkerSpec object.
		/// </summary>
		/// <param name="marker"></param>
		/// <param name="markerMap"></param>
		/// <param name="dataMap"></param>
		/// <param name="isInline"></param>
		/// <param name="replaceMarker"></param>
		/// <param name="end"></param>
		/// <param name="endReplaceMarker"></param>
		/// <returns></returns>
		public static MarkerSpec CreateMarkerSpec(string marker, string markerMap, string dataMap,
			bool isInline, string replaceMarker, string end, string endReplaceMarker)
		{
			MarkerSpec ms;

			if (isInline)
			{
				// check to see if there is an end marker specified
				if (end == null || end.Length == 0)
				{
					// create an AloneInlineMarkerSpec
					ms = new AloneInlineMarkerSpec(marker, markerMap, replaceMarker);
				}
				else
				{
					// create a StartInlineMarkerSpec with a EndInlineMarkerSpec
					EndInlineMarkerSpec endMs;

					endMs = new EndInlineMarkerSpec(end, markerMap, endReplaceMarker);
					ms = new StartInlineMarkerSpec(marker, markerMap, dataMap, replaceMarker, endMs);
				}
			}
			else
			{
				// create an FieldMarkerSpec
				ms = new FieldMarkerSpec(marker, markerMap, dataMap, replaceMarker);
			}
			return ms;
		}
	}

	/// <summary>
	/// Subclass for main markers that start new fields w/o end markers.
	/// </summary>
	public class FieldMarkerSpec : MarkerSpec
	{
		public FieldMarkerSpec(string marker, string markerMap, string dataMap, string replaceMarker) :
			base(marker, markerMap, dataMap, replaceMarker)
		{
		}
		public override bool IsInline() {return false;}
		public override bool IsNewlineBeforeRequired() {return true;}
		public override bool IsWhitespaceAfterRequired() {return true;}

		public override Token MakeToken(int line, int column)
		{
			return new FieldToken(line, column, this);
		}
	}

	/// <summary>
	/// Inline markers don't have to use backslashes, and require a corresponding end string
	/// to indicate where they end, within the same field. (Some inline markers don't
	/// have an end marker...just leave it blank.)
	/// </summary>
	public class InlineMarkerSpec : MarkerSpec
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="marker"></param>
		/// <param name="markerMap"></param>
		/// <param name="dataMap"></param>
		/// <param name="replaceMarker"></param>
		/// <param name="end"></param>
		/// <param name="endReplaceMarker"></param>
		public InlineMarkerSpec(string marker, string markerMap, string dataMap, string replaceMarker):
			base(marker, markerMap, dataMap, replaceMarker)
		{
		}

		public override bool IsInline() {return true;}
		public override bool IsNewlineBeforeRequired() {return false;}
		public override bool IsWhitespaceAfterRequired() {return false;}
	}

	public class AloneInlineMarkerSpec : InlineMarkerSpec
	{
		public AloneInlineMarkerSpec(string marker, string markerMap, string replaceMarker) :
			base (marker, markerMap, "", replaceMarker)
		{
		}

		public override Token MakeToken(int line, int column)
		{
			return new AloneInlineToken(line, column, this);
		}
	}


	public class StartInlineMarkerSpec : InlineMarkerSpec
	{
		EndInlineMarkerSpec m_end;

		public StartInlineMarkerSpec(string marker, string markerMap, string dataMap,
			string replaceMarker, EndInlineMarkerSpec end) :
			base (marker, markerMap, dataMap, replaceMarker)
		{
			m_end = end;
		}

		public override Token MakeToken(int line, int column)
		{
			return new StartInlineToken(line, column, this);
		}

		public EndInlineMarkerSpec EndSpec
		{
			get {return m_end;}
		}

		public string End
		{
			get {return m_end.Marker;}
		}

		public string EndReplaceMarker
		{
			get {return m_end.ReplaceMarker;}
		}
	}

	public class EndInlineMarkerSpec : InlineMarkerSpec
	{
		public EndInlineMarkerSpec(string marker, string markerMap, string replaceMarker) :
			base (marker, markerMap, "", replaceMarker)
		{
		}

		public override Token MakeToken(int line, int column)
		{
			return new EndInlineToken(line, column, this);
		}
	}
}