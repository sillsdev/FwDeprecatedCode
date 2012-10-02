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
// File: Token.cs
// Responsibility:
// Last reviewed:
//
// <remarks>
// </remarks>
// --------------------------------------------------------------------------------------------
using System;
using System.Collections;
using System.Diagnostics;
using System.Resources;
using SilEncConverters;

namespace SIL.FieldWorks.DataConverter
{

	public class NoDataConverterException : ApplicationException
	{
		private uint hrBase = 0x8004FFF1;	// Error, FACILITY_ITF, FFF1 code value

		/// <summary>
		/// Basic constructor.
		/// </summary>
		public NoDataConverterException()
			: base()
		{
			HResult = (int)(hrBase);
		}
		/// <summary>
		/// Constructor with (localized) message string.
		/// </summary>
		/// <param name="sMsg"></param>
		public NoDataConverterException(string sMsg)
			: base(sMsg)
		{
			HResult = (int)(hrBase);
		}
	};

	public class TokenizerStackException: ApplicationException
	{
		private uint hrBase = 0x8004FFF2;	// Error, FACILITY_ITF, FFF1 code value

		/// <summary>
		/// Basic constructor.
		/// </summary>
		public TokenizerStackException()
			: base()
		{
			HResult = (int)(hrBase);
		}
		/// <summary>
		/// Constructor with (localized) message string.
		/// </summary>
		/// <param name="sMsg"></param>
		public TokenizerStackException(string sMsg)
			: base(sMsg)
		{
			HResult = (int)(hrBase);
		}
	};


	/// <summary>
	/// Summary description for Token.
	/// </summary>
	public abstract class Token
	{
		int m_lineNo; // Where token was found (for error reporting).
		int m_column; // Character position within line m_lineNo.
		string m_map; // Mapping to convert data to Unicode.
		Tokenizer m_tokenizer;
		/// -----------------------------------------------------------------------------------
		/// <summary>
		/// Initializes a new instance of the <see cref="Token"/> class.
		/// </summary>
		/// -----------------------------------------------------------------------------------
		public Token(int lineNo, int column)
		{
			m_lineNo = lineNo;
			m_column = column;
		}

		public int LineNo
		{
			get {return m_lineNo;}
		}
		public int Column
		{
			get {return m_column;}
		}
		public string Map
		{
			get {return m_map;}
			set {m_map = value;}
		}
		public Tokenizer Tokenizer
		{
			get {return m_tokenizer;}
			set {m_tokenizer = value;}
		}

		/// <summary>
		/// Raw data in a (data or marker) token.  Mainly used for testing.
		/// </summary>
		/// <returns></returns>
		public abstract string RawOutput();

		public virtual string Modify(string unicodeInput)
		{
			return unicodeInput;
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// Tokenizer's client
		/// </summary>
		/// <param name="fAlreadyUnicode"></param>
		/// <returns></returns>
		/// ------------------------------------------------------------------------------------
		public virtual string Output(bool fAlreadyUnicode)
		{
			string rawOutput = RawOutput();
			string unicodeString = fAlreadyUnicode ? rawOutput : ConvertToUnicode(rawOutput);
			return Modify(unicodeString);
		}

		/// <summary>
		/// </summary>
		/// <param name="tk"></param>
		public virtual void Initialize(Tokenizer tk)
		{
			Tokenizer = tk;
			Action();
			SetMap();
		}

		/// <summary>
		/// The derived classes' Action() methods implement the action appropriate to encountering an object
		/// of their type in regard to the token stack. The default does nothing.
		/// </summary>
		public virtual void Action()
		{
		}

		public virtual void SetMap()
		{
			Map = ((MarkerToken)m_tokenizer.TokenStack.Peek()).Spec.DataMap;
		}

		public string ConvertToUnicode(string input)
		{
			if (Map.Length == 0)
			{
				// The specified map is an empty string,
				// input is already Unicode ... use default mapping ...
				// the default mapping is currently ISO-8859-1 (Latin 1)
				return input;
			}

			IEncConverter iconverter = Tokenizer.Converters[Map];
			if (iconverter == null)
			{
				ResourceManager resources =
					new ResourceManager("SIL.FieldWorks.DataConverter.DataConverterStrings",
					System.Reflection.Assembly.GetExecutingAssembly());

				string msg = resources.GetString("NoDataConverterExceptionText");
				throw new NoDataConverterException(msg);
			}

			iconverter.CodePageInput = DataConverter.ReversibleCodePage;

			string output = iconverter.Convert(input);
			return output;
		}
	}

	public class NewlineToken : Token
	{
		public NewlineToken(int lineNo, int column) : base (lineNo, column) {}

		public override string RawOutput()
		{
			return "\n";
		}

		// We are depending on StreamWriter to generate the appropriate newline characters for the output file.
		public override string Output(bool fAlreadyUnicode)
		{
			return "";
		}
	}
	public class DataToken : Token
	{
		protected string m_data; // The data

		public DataToken(int lineNo, int column, string data) : base (lineNo, column)
		{
			m_data = data;
		}

		public override string RawOutput()
		{
			return m_data;
		}

		public override string Modify(string unicodeInput)
		{
			// escape backslashes with another backslash
			string modified = unicodeInput.Replace(@"\", @"\\");
			return modified;
		}
	}

	public class MarkerToken : Token
	{
		protected MarkerSpec m_markerSpec;
		public MarkerToken(int lineNo, int column, MarkerSpec markerSpec) : base (lineNo, column)
		{
			m_markerSpec = markerSpec;
		}

		public override void SetMap()
		{
			Map = Spec.MarkerMap;
		}

		public override string RawOutput()
		{
			if (m_markerSpec.IsWhitespaceAfterRequired() &&
				!m_markerSpec.ReplaceMarker.EndsWith(" ") &&
				m_markerSpec.ReplaceMarker.Length > 0)
			{
				return m_markerSpec.ReplaceMarker + " ";
			}
			return m_markerSpec.ReplaceMarker;
		}

		public MarkerSpec Spec
		{
			get {return m_markerSpec;}
		}

		protected void RemoveEndMarkerFromTri(Tokenizer tk)
		{
			if (tk.TokenStack.Peek() is StartInlineToken)
			{
				StartInlineToken sik = (StartInlineToken)(tk.TokenStack.Peek());
				StartInlineMarkerSpec sims = (StartInlineMarkerSpec)(sik.Spec);
				tk.Tri.Remove(sims.End);
			}
		}
	}

	public class StartOfFileToken : MarkerToken
	{
		public StartOfFileToken() : base (0, 0,	new FieldMarkerSpec("", "", "", ""))
		{
		}

		public override void Action()
		{
			Tokenizer.TokenStack.Push(this);
		}
	}

	public class EndOfFileToken : MarkerToken
	{
		public EndOfFileToken(int lineNo, int column) : base (lineNo, column, new FieldMarkerSpec("", "", "", ""))
		{
		}
	}

	public class FieldToken : MarkerToken
	{
		public FieldToken(int lineNo, int column, MarkerSpec markerSpec) : base (lineNo, column, markerSpec)
		{
		}
		/// <summary>
		/// Remove any inline markers from the stack (and report to user?).
		/// Remove the top non-inline marker and replace it with your own.
		/// </summary>
		/// <param name="tk"></param>
		public override void Action()
		{
			RemoveEndMarkerFromTri(Tokenizer);

			// Remove all StartInlineTokens from the TokenStack.
			// This is an exception to the rule that all StartInlineTokens
			// must have an explicit EndInlineToken.
			while (Tokenizer.TokenStack.Peek() is StartInlineToken)
			{
				// Review: report or log unmatched token?
				Tokenizer.TokenStack.Pop();
			}

			if (!(Tokenizer.TokenStack.Peek() is FieldToken) && !(Tokenizer.TokenStack.Peek() is StartOfFileToken))
			{
				ResourceManager resources =
					new ResourceManager("SIL.FieldWorks.DataConverter.DataConverterStrings",
					System.Reflection.Assembly.GetExecutingAssembly());

				string msg = resources.GetString("TokenizerExceptionText");
				throw new TokenizerStackException(msg);
			}

			// The following is from Darrel Eppler via email in Dec-2002.
			// Each backslash code marks the end of the previous one.

			// In this program, FieldToken is equivalent to a backslash code that Darrel mentions.
			// Since a FieldToken marks the end of the last FieldToken, we pop the last
			// FieldToken off of the token stack and push the new FieldToken on the token stack.
			Tokenizer.TokenStack.Pop();
			Tokenizer.TokenStack.Push(this);
		}
	}

	public class InlineToken : MarkerToken
	{
		public InlineToken(int lineNo, int column, MarkerSpec markerSpec) : base (lineNo, column, markerSpec)
		{
		}

		/// <summary>
		/// Change output text so ScriptureObject can read it.
		/// For an InlineToken, this means to add a space after a marker if the marker is replaced.
		/// </summary>
		/// <param name="unicodeInput"></param>
		/// <returns></returns>
		public override string Modify(string unicodeInput)
		{
			string output;

			output = unicodeInput;

			return output;
		}

		protected void AddEndMarkerToTri(Tokenizer tk, StartInlineMarkerSpec sims)
		{
			tk.Tri.Add(sims.EndSpec);
		}
	}

	public class StartInlineToken : InlineToken
	{
		public StartInlineToken(int lineNo, int column, MarkerSpec markerSpec) : base (lineNo, column, markerSpec)
		{
		}
		public override void Action()
		{
			RemoveEndMarkerFromTri(Tokenizer);
			Tokenizer.TokenStack.Push(this);
			StartInlineMarkerSpec sims = (StartInlineMarkerSpec)(Spec);
			AddEndMarkerToTri(Tokenizer, sims);
		}
	}

	public class EndInlineToken : InlineToken
	{
		public EndInlineToken(int lineNo, int column, MarkerSpec markerSpec) : base (lineNo, column, markerSpec)
		{
		}
		public override void Action()
		{
			// The following is from Darrel Eppler via email in Dec-2002.
			// Bar codes do not have explicit ending markers, either; although a
			// backslash code will end the formatting initiated by a bar
			// code. Normally text formatted with a |b (bold) or |i (italic) is
			// followed by a |r (return to regular).

			// The following is from a conversation with Ted Goller in Dallas on 03-Feb-2003.
			// If he encounters inline markers such as
			//   some |b text |i that |r I typed
			// The |i causes the |b to stop, the |r returns to regular, so that the
			// end of the |b is implicit. This nesting of inline markers could get
			// confusing, so he strongly suggests having the translator add a |r at
			// the end of the text marked by the |b, giving some input like the
			// following.
			//   some |b text |r |i that |r I typed
			// You could nest the effects of the inline markers, but that leads to
			// confusion. The key is to go back to the translator and ask them what
			// they intended to do. If they wanted they styles to nest, then define a
			// new markers with both styles set and marked up the data with that new
			// marker.

			// In this program, StartInlineToken is equivalent to a |b or |i that Darrel mentions.
			// An EndInlineToken is equivalent to a |r. Currently, this program does not behave
			// like Darrel describes, multiple |r EndInlineTokens are needed to close the
			// corresponding StartInlineToken, since the code below only does one pop.
			// See the comment for Action() in the class FieldToken for an exception to this.
			// Ted Goller thinks that this behavior is correct, it makes the data less
			// ambiguous if the translator has to explicitly terminate StartInlineTokens.

			// Enhance: report error if Peek() isn't the right start token.
			RemoveEndMarkerFromTri(Tokenizer);
			Tokenizer.TokenStack.Pop();
			// Maybe start looking for a different one if Peek() is another InlineMarker
			if (Tokenizer.TokenStack.Peek() is StartInlineToken)
			{
				StartInlineToken sik = (StartInlineToken)(Tokenizer.TokenStack.Peek());
				StartInlineMarkerSpec sims = (StartInlineMarkerSpec)(sik.Spec);
				AddEndMarkerToTri(Tokenizer, sims);
			}
		}
	}
	public class AloneInlineToken : InlineToken
	{
		public AloneInlineToken(int lineNo, int column, MarkerSpec markerSpec) : base (lineNo, column, markerSpec)
		{
		}
	}
}
