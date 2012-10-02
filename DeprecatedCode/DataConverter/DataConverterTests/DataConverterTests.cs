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
// File: DataConverterTests.cs
// Responsibility:
// Last reviewed:
//
// <remarks>
// </remarks>
// --------------------------------------------------------------------------------------------
using System;
using SIL.FieldWorks.DataConverter;
using SIL.FieldWorks.Common.Utils;
using SilEncConverters;
using NUnit.Framework;
using System.Text;
using System.IO;
using System.Collections;
using System.Reflection;
//using ECOBJECTSLib;

namespace SIL.FieldWorks.DataConverter.DataConverterTests
{
	#region class Info
	public class Info
	{
		public static string TestFileDir
		{
			get
			{
				return DirectoryFinder.FWInstallDirectory +
					@"\..\Src\Common\DataConverter\DataConverterTests\";
			}
		}
	}
	#endregion

	#region class MarkerClass
	public class MarkerClass
	{
		public struct Marker
		{
			public string m_marker;
			public string m_markerMap;
			public string m_dataMap;
			public bool m_isInline;
			public string m_end;

			public Marker(string marker, string markerMap, string dataMap, bool isInline, string end)
			{
				m_marker = marker;
				m_markerMap = markerMap;
				m_dataMap = dataMap;
				m_isInline = isInline;
				m_end = end;
			}
		}

		public Marker[] markers =
		{
			// field markers
			new Marker(@"\id",   "marker", "marker", false, @""),
			new Marker(@"\hr",   "marker", "data", false, @""),
			new Marker(@"\is",   "marker", "data", false, @""),
			new Marker(@"\h",    "marker", "data", false, @""),
			new Marker(@"\ie",   "marker", "data", false, @""),
			new Marker(@"\c",    "marker", "data", false, @""),
			new Marker(@"\io",   "marker", "data", false, @""),
			new Marker(@"\io2",  "marker", "data", false, @""),
			new Marker(@"\s",    "marker", "data", false, @""),
			new Marker(@"\f",    "marker", "data", false, @""),
			new Marker(@"\ip",   "marker", "data", false, @""),
			new Marker(@"\btvt", "marker", "data", false, @""),
			new Marker(@"\v",    "marker", "data", false, @""),
			new Marker(@"\vt",   "marker", "data", false, @""),
			new Marker(@"\p",    "marker", "data", false, @""),

			// start inline markers (implicitly end inline marker)
			new Marker(@"|fv{",  "marker", "inline", true, @"}"),
			new Marker(@"|i",    "marker", "inline", true, @"|r"),
			new Marker(@"|b",    "marker", "inline", true, @"|bb"),

			// alone inline markers
			new Marker(@"|fn", "marker", "dummyInline", true, @""),
		};
	}
	#endregion

	#region MarkerSpecTests
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// MarkerSpec Tests
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class MarkerSpecTests
	{
		[Test]
		public void TestMarkerSpec()
		{
			MarkerSpec fms = MarkerSpec.CreateMarkerSpec(@"\f", "MyMarkerMap", "MyDataMap",
				false, null, null, null);

			Assert.AreEqual(@"\f", fms.Marker.ToString());
			Assert.AreEqual("MyMarkerMap", fms.MarkerMap);
			Assert.AreEqual("MyDataMap", fms.DataMap);
			Assert.IsTrue(fms.IsNewlineBeforeRequired());
			Assert.IsTrue(fms.IsWhitespaceAfterRequired());
		}
	}
	#endregion

	#region Token tests
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Token tests
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class TestToken
	{
		// Our current NANT based build system insists on running tests immediately after
		// building the corresponding DLL/EXE.  This is a problem for DataConverter because it
		// uses the ECObjects DLL, but the ECObjects DLL cannot be built until after the
		// DataConverter DLL has been built and the corresponding type library exported.  (The
		// type library and interop dll for ECObjects can be built before DataConverter without
		// building the main DLL itself -- one advantage of the MIDL/C++ approach to life.)
		// Thus, we need to check whether the ECObjects DLL exists before running some of the
		// DataConverter tests.


		[TestFixtureSetUp]
		public void RunBeforeAllTests()
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestDataTokenModify
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestDataTokenModify()
		{
			Token token = new DataToken(0, 0, @"See file C:\so.txt");
			token.Map = "";
			Assert.AreEqual(@"See file C:\\so.txt", token.Output(true));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestFieldTokenModify
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestFieldTokenModify()
		{
			DCMapping mapping = new DCMapping();

			// common settings for both test cases
			mapping.MarkerEncoding = "";
			mapping.DataEncoding = "";
			mapping.IsInline = false;
			mapping.EndMarker = "";

			// Rename a marker.
			mapping.BeginMarker = @"|v";
			MarkerSpec fmsRenamed = MarkerSpec.CreateMarkerSpec(mapping);
			MarkerToken ftRenamed = (MarkerToken)((MarkerSpec)fmsRenamed).MakeToken(0, 0);
			ftRenamed.Map = "";
			Assert.AreEqual(@"\|v ", ftRenamed.Output(true));

			// Don't rename a marker.
			mapping.BeginMarker = @"\v";
			MarkerSpec fmsNoChange = MarkerSpec.CreateMarkerSpec(mapping);
			MarkerToken ftNoChange = (MarkerToken)((MarkerSpec)fmsNoChange).MakeToken(0, 0);
			ftNoChange.Map = "";
			Assert.AreEqual(@"\v ", ftNoChange.Output(true));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestInlineTokenModify
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestInlineTokenModify()
		{
			DCMapping mapping = new DCMapping();

			// common settings for both test cases
			mapping.MarkerEncoding = "";
			mapping.DataEncoding = "";
			mapping.IsInline = true;
			mapping.EndMarker = "";

			// Rename a marker and append a space to marker.
			mapping.BeginMarker = @"|fn";
			MarkerSpec imsRenamed = MarkerSpec.CreateMarkerSpec(mapping);
			MarkerToken itRenamed = (MarkerToken)((MarkerSpec)imsRenamed).MakeToken(0, 0);
			itRenamed.Map = "";
			Assert.AreEqual(@"\|fn", itRenamed.Output(true));	// TE-1856 space is no longer inserted after inline markers

			// Don't rename a marker.
			mapping.BeginMarker = @"\fn";
			MarkerSpec imsNoChange = MarkerSpec.CreateMarkerSpec(mapping);
			MarkerToken itNoChange = (MarkerToken)((MarkerSpec)imsNoChange).MakeToken(0, 0);
			itNoChange.Map = "";
			Assert.AreEqual(@"\fn", itNoChange.Output(true));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestTokenConvert
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestTokenConvert()
		{
			// loads converters from on disk XML encoding repository file
			EncConverters converters = new EncConverters();

			// location of TECkit map files
			string mapDir = Info.TestFileDir;

			// writes three converters to XML encoding repository file on disk.
			converters.Add("ISO-8859-1<>UNICODE", mapDir + @"iso-8859-1.map",
				ConvType.Legacy_to_from_Unicode, "ISO-8859-1", "UNICODE", ProcessTypeFlags.UnicodeEncodingConversion);
			converters.Add("ASCII<>MIXED CASE UNICODE", mapDir + @"mixedcase.map",
				ConvType.Legacy_to_from_Unicode, "ISO-8859-1", "UNICODE", ProcessTypeFlags.UnicodeEncodingConversion);
			converters.Add("ASCII>UPPER CASE UNICODE", mapDir + @"uppercase.map",
				ConvType.Legacy_to_Unicode, "ISO-8859-1", "UNICODE", ProcessTypeFlags.UnicodeEncodingConversion);

			Token token = new DataToken(0, 0, @"Hello, World!");

			Tokenizer tokenizer = new Tokenizer();

			// Setting token.Tokenizer and token.Map simulates Initialize()
			// so a full test environment does not need to be created.
			token.Tokenizer = tokenizer;

			string rawString;

			// an empty string for the map name indicates a default Unicode conversion should be used
			token.Map = "";
			rawString = token.RawOutput();
			Assert.AreEqual(@"Hello, World!", token.ConvertToUnicode(rawString));

			token.Map = "ISO-8859-1<>UNICODE";
			rawString = token.RawOutput();
			Assert.AreEqual(@"Hello, World!", token.ConvertToUnicode(rawString));

			token.Map = "ASCII<>MIXED CASE UNICODE";
			rawString = token.RawOutput();
			Assert.AreEqual(@"hELLO,~~~wORLD!", token.ConvertToUnicode(rawString));

			token.Map = "ASCII>UPPER CASE UNICODE";
			rawString = token.RawOutput();
			Assert.AreEqual(@"HELLO,~~WORLD!", token.ConvertToUnicode(rawString));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestDontConvertUnicodeToken
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestDontConvertUnicodeToken()
		{
			Token token = new DataToken(0, 0, @"Hello, World!");

			Tokenizer tokenizer = new Tokenizer();

			// Setting token.Tokenizer and token.Map simulates Initialize()
			// so a full test environment does not need to be created.
			token.Tokenizer = tokenizer;

			// Set a non-existent converter so we can check to make sure it does not get called
			token.Map = "Garbanzo";
			Assert.AreEqual(@"Hello, World!", token.Output(true));
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestInlineTokenModify
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestNewBeginAndEndMarkerCode()
		{
			DCMapping mapping = new DCMapping();

			// Rename a marker and append a space to marker.
			mapping.BeginMarker = @"This* is A Test**";
			mapping.EndMarker = @"\\NoChange*";

			Assert.AreEqual(@"\This+ is A Test+*", mapping.NewBeginMarker);
			Assert.AreEqual(mapping.EndMarker, mapping.NewEndMarker);
		}

	}
	#endregion

	#region class TestStringTri
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// Tri-node, field marker, and in-line marker tests
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class TestStringTri
	{
		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestTriNode
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestTriNode()
		{
			TriNode tnAbc = new TriNode();
			TriNode tnAbd = new TriNode();
			MarkerSpec fmsAbc = MarkerSpec.CreateMarkerSpec("abc", "dummy1", "dummy2",
				false, null, null, null);
			tnAbc.TargetMarkerSpec = fmsAbc;
			MarkerSpec fmsAbd = MarkerSpec.CreateMarkerSpec("acd", "dummy1", "dummy2",
				false, null, null, null);
			tnAbd.TargetMarkerSpec = fmsAbd;

			TriNode tnAb = new TriNode();
			TriNode tnA = new TriNode();
			TriNode tnRoot = new TriNode();
			tnRoot.Add(fmsAbc.Marker[0], tnA);
			tnA.Add(fmsAbc.Marker[1], tnAb);
			tnAb.Add(fmsAbc.Marker[2], tnAbc);
			tnAb.Add(fmsAbd.Marker[2], tnAbd);

			Assert.IsNull(tnRoot.TargetMarkerSpec);
			Assert.AreEqual(fmsAbc, tnAbc.TargetMarkerSpec);
			Assert.AreEqual(fmsAbd, tnAbd.TargetMarkerSpec);
			Assert.AreEqual(tnA, tnRoot.Match(fmsAbc.Marker[0]));
			Assert.AreEqual(tnAb, tnA.Match(fmsAbc.Marker[1]));
			Assert.AreEqual(tnAbc, tnAb.Match(fmsAbc.Marker[2]));
			Assert.AreEqual(tnAbd, tnAb.Match(fmsAbd.Marker[2]));

			string s1 = "Ax";
			Assert.IsNull(tnRoot.Match(s1[0]));
			Assert.IsNull(tnRoot.Match(s1[1]));
			Assert.IsNull(tnAb.Match(s1[0]));
			Assert.IsNull(tnAb.Match(s1[1]));
			Assert.IsNull(tnAbc.Match(s1[0]));
			Assert.IsNull(tnAbc.Match(s1[1]));

			tnRoot = new TriNode(); // start over using char[] Add.
			tnRoot.Add(fmsAbc.Marker, 0, fmsAbc);
			tnRoot.Add(fmsAbd.Marker, 0, fmsAbd);
			Assert.AreEqual(fmsAbc, tnRoot.Match(fmsAbc.Marker[0]).
				Match(fmsAbc.Marker[1]).Match(fmsAbc.Marker[2]).TargetMarkerSpec);
			Assert.AreEqual(fmsAbd, tnRoot.Match(fmsAbd.Marker[0]).
				Match(fmsAbd.Marker[1]).Match(fmsAbd.Marker[2]).TargetMarkerSpec);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestFieldMarkerMatch
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestFieldMarkerMatch()
		{
			StringTri stringTri = new StringTri();
			int nextIndex;
			bool isInline = false;

			MarkerSpec fmsAbc = MarkerSpec.CreateMarkerSpec("abc", "dummy1", "dummy2",
				isInline, null, null, null);
			MarkerSpec fmsAbd = MarkerSpec.CreateMarkerSpec("abd", "dummy1", "dummy2",
				isInline, null, null, null);
			MarkerSpec fmsAb = MarkerSpec.CreateMarkerSpec("ab", "dummy1", "dummy2",
				isInline, null, null, null);

			stringTri.Add(fmsAbc);
			stringTri.Add(fmsAbd);
			stringTri.Add(fmsAb);

			// Should find at start.
			Assert.AreEqual(fmsAbc, stringTri.Match("abc this is the day",	0, out nextIndex));
			Assert.AreEqual(4, nextIndex);

			// Should only absorb one space.
			Assert.AreEqual(fmsAbc, stringTri.Match("abc  this is the day", 0, out nextIndex));
			Assert.AreEqual(4, nextIndex);

			// Should match on closing newline, but no space to absorb.
			Assert.AreEqual(fmsAbc, stringTri.Match("abc", 0, out nextIndex));
			Assert.AreEqual(3, nextIndex);

			// Should fail with no white space following.
			Assert.IsNull(stringTri.Match("abcXYZ", 0, out nextIndex));
			Assert.AreEqual(1, nextIndex);

			// Should be able to find abd also
			Assert.AreEqual(fmsAbd, stringTri.Match("abd this is the day", 0, out nextIndex));
			Assert.AreEqual(4, nextIndex);

			// Should be able to find ab also
			Assert.AreEqual(fmsAb, stringTri.Match("ab this is the day", 0, out nextIndex));
			Assert.AreEqual(3, nextIndex);

			// Should not find except at start of line.
			Assert.IsNull(stringTri.Match("To abc this is the day", 3, out nextIndex));
			Assert.AreEqual(4, nextIndex);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestInlineMarkerMatch
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestInlineMarkerMatch()
		{
			StringTri stringTri = new StringTri();
			int nextIndex;
			bool isInline = true;

			MarkerSpec aimsAbc = MarkerSpec.CreateMarkerSpec("abc", "dummyMarker", "dummyData",
				isInline, null, null, null);
			MarkerSpec aimsAb = MarkerSpec.CreateMarkerSpec("ab", "dummyMarker", "dummyData",
				isInline, null, null, null);

			stringTri.Add(aimsAbc);
			stringTri.Add(aimsAb);

			// See if we can find an inline marker.
			Assert.AreEqual(aimsAbc, stringTri.Match("To abc this is the day",
				3, out nextIndex));
			Assert.AreEqual(6, nextIndex);

			// If we make ab inline we can find it.
			Assert.AreEqual(aimsAb, stringTri.Match("To ab this is the day",
				3, out nextIndex));
			Assert.AreEqual(5, nextIndex);

			// abc and ab now both match, but we want to find the longer one.
			Assert.AreEqual(aimsAbc, stringTri.Match("To abc this is the day",
				3, out nextIndex));
			Assert.AreEqual(6, nextIndex);

			// Minimal case, verifies we can match inline marker at start and end of line.
			Assert.AreEqual(aimsAb, stringTri.Match("ab", 0, out nextIndex));
			Assert.AreEqual(2, nextIndex);

			// Make sure we can match without a following space.
			Assert.AreEqual(aimsAb, stringTri.Match("To abXYZ", 3, out nextIndex));
			Assert.AreEqual(5, nextIndex);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestFieldMarkerSearch
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestFieldMarkerSearch()
		{
			StringTri stringTri = new StringTri();
			int nextIndex;
			MarkerSpec target;
			bool isInline = false;

			MarkerSpec fmsAbc = MarkerSpec.CreateMarkerSpec("abc", "dummy1", "dummy2",
				isInline, null, null, null);
			MarkerSpec fmsAbd = MarkerSpec.CreateMarkerSpec("abd", "dummy1", "dummy2",
				isInline, null, null, null);
			MarkerSpec fmsAb = MarkerSpec.CreateMarkerSpec("ab", "dummy1", "dummy2",
				isInline, null, null, null);

			stringTri.Add(fmsAbc);
			stringTri.Add(fmsAbd);
			stringTri.Add(fmsAb);

			// Should find at start.
			Assert.AreEqual(0, stringTri.Search("abc this is the day",
				0, out nextIndex, out target));
			Assert.AreEqual(fmsAbc, target);
			Assert.AreEqual(4, nextIndex);

			// Should only absorb one space.
			Assert.AreEqual(0, stringTri.Search("abc  this is the day",
				0, out nextIndex, out target));
			Assert.AreEqual(fmsAbc, target);
			Assert.AreEqual(4, nextIndex);

			// Should match on closing newline, but no space to absorb.
			Assert.AreEqual(0, stringTri.Search("abc",
				0, out nextIndex, out target));
			Assert.AreEqual(fmsAbc, target);
			Assert.AreEqual(3, nextIndex);

			// Should fail with no white space following.
			Assert.AreEqual(-1, stringTri.Search("abcXYZ",
				0, out nextIndex, out target));
			Assert.IsNull(target);
			Assert.AreEqual(6, nextIndex);

			// Should be able to find abd also
			Assert.AreEqual(0, stringTri.Search("abd this is the day",
				0, out nextIndex, out target));
			Assert.AreEqual(fmsAbd, target);
			Assert.AreEqual(4, nextIndex);

			// Should be able to find ab also
			Assert.AreEqual(0, stringTri.Search("ab this is the day",
				0, out nextIndex, out target));
			Assert.AreEqual(3, nextIndex);

			// Should not find except at start of line.
			Assert.AreEqual(-1, stringTri.Search("To abc this",
				0, out nextIndex, out target));
			Assert.IsNull(target);
			Assert.AreEqual(11, nextIndex);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestInlineMarkerSearch
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestInlineMarkerSearch()
		{
			StringTri stringTri = new StringTri();
			int nextIndex;
			MarkerSpec target;
			bool isInline = true;

			MarkerSpec aimsAbc = MarkerSpec.CreateMarkerSpec("abc", "dummyMarker", "dummyData",
				isInline, null, null, null);
			MarkerSpec aimsAb = MarkerSpec.CreateMarkerSpec("ab", "dummyMarker", "dummyData",
				isInline, null, null, null);

			stringTri.Add(aimsAbc);
			stringTri.Add(aimsAb);

			// See if we can find an inline marker.
			Assert.AreEqual(3, stringTri.Search("To abc this is the day",
				0, out nextIndex, out target));
			Assert.AreEqual(aimsAbc, target);
			Assert.AreEqual(6, nextIndex);

			// abc and ab now both match, but we want to find the longer one.
			Assert.AreEqual(3, stringTri.Search("To abc this is the day",
				0, out nextIndex, out target));
			Assert.AreEqual(aimsAbc, target);
			Assert.AreEqual(6, nextIndex);

			// Minimal case, verifies we can match inline marker at start and end of line.
			Assert.AreEqual(0, stringTri.Search("ab",
				0, out nextIndex, out target));
			Assert.AreEqual(aimsAb, target);
			Assert.AreEqual(2, nextIndex);

			// Make sure we can match without a following space.
			Assert.AreEqual(3, stringTri.Search("To abXYZ",
				0, out nextIndex, out target));
			Assert.AreEqual(aimsAb, target);
			Assert.AreEqual(5, nextIndex);

			// Fail if we start search after target
			Assert.AreEqual(-1, stringTri.Search("To abXYZ",
				4, out nextIndex, out target));
			Assert.IsNull(target);
			Assert.AreEqual(8, nextIndex);

			// Find second occurrence.
			Assert.AreEqual(8, stringTri.Search("To abXYZabXYZ",
				4, out nextIndex, out target));
			Assert.AreEqual(aimsAb, target);
			Assert.AreEqual(10, nextIndex);

			// Search on an empty line.
			Assert.AreEqual(-1, stringTri.Search("",
				0, out nextIndex, out target));
			Assert.IsNull(target);
			Assert.AreEqual(0, nextIndex);

			// We don't currently test cases where requiresNewlineBefore is not equal to
			// requiresWhitespaceAfter, because we don't currently use it.
			// This might be need for a footnote position marker, such as |fn,
			// that does not need to start at the beginning of a line,
			// but does need to have a space after it.
		}
	}
	#endregion

	#region class TestTokenizer

	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// TestTokenizer
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class TestTokenizer
	{
		Tokenizer m_tokenizer;

		struct ParseResult
		{
			public Type type;
			public string result;
			public string map;

			public ParseResult(Type t, string r, string m)
			{
				type = t;
				result = r;
				map = m;
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		///
		/// </summary>
		/// <param name="testSource"></param>
		/// <param name="results"></param>
		/// ------------------------------------------------------------------------------------
		private void RunTokenizer(string testSource, ParseResult[] results)
		{
			byte [] bytes = new byte[testSource.Length];
			for (int i = 0; i < testSource.Length; i++)
				bytes[i] = (byte)(testSource[i]);
			MemoryStream stream = new MemoryStream(bytes);
			StreamReader streamReader = new StreamReader(stream, DataConverter.ReversibleEncoding);
			Token token;

			m_tokenizer.Input = streamReader;

			int iresult = 0;
			do
			{
				token = m_tokenizer.Next();
				ParseResult result = results[iresult];
				iresult++;
				string message = "test token: " + iresult + ", line " + token.LineNo + ", column " + token.Column +
					"\n expected type: " + result.type + ", expected result: ^" + result.result + "^" +
					"\n actual type: " + token.GetType() + ", actual result: ^" + token.RawOutput() + "^";
				string detailMessage;

				detailMessage = "type:\n" + message;
				Assert.AreEqual(result.type, token.GetType(), detailMessage);

				// use token's RawOutput so no Unicode conversions or character escapes are done.
				detailMessage = "text:\n" + message;
				Assert.AreEqual(result.result, token.RawOutput(), detailMessage);

				detailMessage = "map:\n" + message;
				Assert.AreEqual(result.map, token.Map, detailMessage);
			} while (!(token is EndOfFileToken));

			Assert.AreEqual(iresult, results.Length);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// SetUpTokenizer
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[SetUp]
		public void SetUpTokenizer()
		{
			MarkerClass markerClass = new MarkerClass();

			m_tokenizer = new Tokenizer();
			MarkerSpec ms;
			for (int i = 0; i < markerClass.markers.Length; i++)
			{
				MarkerClass.Marker marker = markerClass.markers[i];

				// This test does not use an ECMapping object because it modifies replaceMarker
				// and endReplaceMarker automatically, and since we are just testing the tokenizer,
				// we don't want to have to account for markers being replaced. The replace marker functionality
				// is tested elsewhere.
				ms = MarkerSpec.CreateMarkerSpec(marker.m_marker, marker.m_markerMap,
					marker.m_dataMap, marker.m_isInline, marker.m_marker, marker.m_end, marker.m_end);

				m_tokenizer.Tri.Add(ms);
			}
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestMainText
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestMainText()
		{
			string testSource =
				"A long project...\n" +
				"\\id Book of Ephesians\n" +
				"\\h Ephesians\n" +
				"\n" +
				"\\is   Introduction:\n" +
				"\\ip\n" +
				"This letter was written by Paul during his two-year\n" +
				"imprisonment in Rome (about A.D. 60).\n" +
				"\\ip This letter probably\n" +
				"was sent not just to the church at Ephesus but to all the\n" +
				"\\io     \n" +
				"Outline of contents:\n" +
				"\\io2 Greetings (1:1,2)\n" +
				"\\hr\n" +
				"\\ie\n" +
				"\n" +
				"\n" +
				"\n" +
				"\\c 1\n" +
				"\\p\n" +
				"\\v 1 Paul, an apostle of Jesus Christ by the will of God,\n" +
				"to the saints which are at Ephesus,|fn and to the faithful|fn in Christ Jesus:\n" +
				"\\f 1. Some |bearly|bb manuscripts do not have |iin Ephesus|r.\n" +
				"\\f 1. Or |ibelievers |bwho are\n" +
				"\\p\n" +
				"\\v 2 Grace be to you, and peace, from God our Father,\n" +
				"and from the Lord Jesus Christ.\n" +
				"\n" +
				"\\s                 Spiritual Blessings in Christ\n" +
				"\\p\n" +
				"\\v 3\n" +
				"\\vt Lemade Hulasokwe yala us o esw mety lan ne ti tasike ma\n" +
				"besanare lan, ode kyala kabal desike ma enma kimrunu-kimrahas.\n" +
				"\\btvt |fv{Lemade} *God made this big fast rain and wind on the sea so\n" +
				"that the waves were big, and it caused that ship so [it was]\n" +
				"about to/almost break apart. See file C:\\so.txt\n" +
				"\\v 4\n" +
				"\\vt Kabal a kebunare ramtaut ma kiseseman sir. Lemade it o it\n" +
				"yabuk wasi hulasow, ode ral lan o nam rasosan ti kabal desy\n" +
				"ma rotuk ei tasike ma wait kabalke mran.\n" +
				"\\btvt The ship's sailors were very afraid and terrified. |fv{Lemade}\n" +
				"each one called his *god, and they got and threw stuff and\n" +
				"things they stored in that boat into the sea in order to lighten\n" +
				"their ship.\n";

			ParseResult[] results =
			{
				// end of line marker on last line
				new ParseResult(typeof(StartOfFileToken), @"", ""),
				new ParseResult(typeof(DataToken), @"A long project...", ""),
				new ParseResult(typeof(NewlineToken), "\n", ""),
				new ParseResult(typeof(FieldToken), @"\id ", "marker"),
				new ParseResult(typeof(DataToken), @"Book of Ephesians", "marker"),
				new ParseResult(typeof(NewlineToken), "\n", "marker"),
				new ParseResult(typeof(FieldToken), @"\h ", "marker"),
				new ParseResult(typeof(DataToken), @"Ephesians", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\is ", "marker"),
				new ParseResult(typeof(DataToken), @"  Introduction:", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\ip ", "marker"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"This letter was written by Paul during his two-year", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"imprisonment in Rome (about A.D. 60).", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\ip ", "marker"),
				new ParseResult(typeof(DataToken), @"This letter probably", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"was sent not just to the church at Ephesus but to all the", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\io ", "marker"),
				new ParseResult(typeof(DataToken), @"    ", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"Outline of contents:", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\io2 ", "marker"),
				new ParseResult(typeof(DataToken), @"Greetings (1:1,2)", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\hr ", "marker"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\ie ", "marker"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\c ", "marker"),
				new ParseResult(typeof(DataToken), @"1", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\p ", "marker"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\v ", "marker"),
				new ParseResult(typeof(DataToken), @"1 Paul, an apostle of Jesus Christ by the will of God,", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"to the saints which are at Ephesus,", "data"),
				new ParseResult(typeof(AloneInlineToken), @"|fn", "marker"),
				new ParseResult(typeof(DataToken), @" and to the faithful", "data"),
				new ParseResult(typeof(AloneInlineToken), @"|fn", "marker"),
				new ParseResult(typeof(DataToken), @" in Christ Jesus:", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\f ", "marker"),
				new ParseResult(typeof(DataToken), @"1. Some ", "data"),
				new ParseResult(typeof(StartInlineToken), @"|b", "marker"),
				new ParseResult(typeof(DataToken), @"early", "inline"),
				new ParseResult(typeof(EndInlineToken), @"|bb", "marker"),
				new ParseResult(typeof(DataToken), @" manuscripts do not have ", "data"),
				new ParseResult(typeof(StartInlineToken), @"|i", "marker"),
				new ParseResult(typeof(DataToken), @"in Ephesus", "inline"),
				new ParseResult(typeof(EndInlineToken), @"|r", "marker"),
				new ParseResult(typeof(DataToken), @".", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\f ", "marker"),
				new ParseResult(typeof(DataToken), @"1. Or ", "data"),
				new ParseResult(typeof(StartInlineToken), @"|i", "marker"),
				new ParseResult(typeof(DataToken), @"believers ", "inline"),
				new ParseResult(typeof(StartInlineToken), @"|b", "marker"),
				new ParseResult(typeof(DataToken), @"who are", "inline"),
				new ParseResult(typeof(NewlineToken), "\n", "inline"),
				new ParseResult(typeof(FieldToken), @"\p ", "marker"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\v ", "marker"),
				new ParseResult(typeof(DataToken), @"2 Grace be to you, and peace, from God our Father,", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"and from the Lord Jesus Christ.", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\s ", "marker"),
				new ParseResult(typeof(DataToken), @"                Spiritual Blessings in Christ", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\p ", "marker"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\v ", "marker"),
				new ParseResult(typeof(DataToken), @"3", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\vt ", "marker"),
				new ParseResult(typeof(DataToken), @"Lemade Hulasokwe yala us o esw mety lan ne ti tasike ma", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"besanare lan, ode kyala kabal desike ma enma kimrunu-kimrahas.", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\btvt ", "marker"),
				new ParseResult(typeof(StartInlineToken), @"|fv{", "marker"),
				new ParseResult(typeof(DataToken), @"Lemade", "inline"),
				new ParseResult(typeof(EndInlineToken), @"}", "marker"),
				new ParseResult(typeof(DataToken), @" *God made this big fast rain and wind on the sea so", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"that the waves were big, and it caused that ship so [it was]", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"about to/almost break apart. See file C:\so.txt", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\v ", "marker"),
				new ParseResult(typeof(DataToken), @"4", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\vt ", "marker"),
				new ParseResult(typeof(DataToken), @"Kabal a kebunare ramtaut ma kiseseman sir. Lemade it o it", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"yabuk wasi hulasow, ode ral lan o nam rasosan ti kabal desy", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"ma rotuk ei tasike ma wait kabalke mran.", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(FieldToken), @"\btvt ", "marker"),
				new ParseResult(typeof(DataToken), @"The ship's sailors were very afraid and terrified. ", "data"),
				new ParseResult(typeof(StartInlineToken), @"|fv{", "marker"),
				new ParseResult(typeof(DataToken), @"Lemade", "inline"),
				new ParseResult(typeof(EndInlineToken), @"}", "marker"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"each one called his *god, and they got and threw stuff and", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"things they stored in that boat into the sea in order to lighten", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(DataToken), @"their ship.", "data"),
				new ParseResult(typeof(NewlineToken), "\n", "data"),
				new ParseResult(typeof(EndOfFileToken), "", ""),
			};

			RunTokenizer(testSource, results);
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// TestEOFCornerCase
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestEOFCornerCase()
		{
			string testSource =
				"\\id Book of Ephesians";

			ParseResult[] results =
			{
				// no end of line marker on last line (example "foo<EOF>")
				new ParseResult(typeof(StartOfFileToken), @"", ""),
				new ParseResult(typeof(FieldToken), @"\id ", "marker"),
				new ParseResult(typeof(DataToken), @"Book of Ephesians", "marker"),

				// this newline does not exist in the data, but the tokenizer
				// generates one anyway because StreamReader.ReadLine does not
				// distinguish between files with the last line that ends with a newline
				// and files with the last line that ends without a newline
				new ParseResult(typeof(NewlineToken), "\n", "marker"),
				new ParseResult(typeof(EndOfFileToken), "", ""),
			};

			RunTokenizer(testSource, results);
		}
	}
	#endregion

	#region class DataConverterTests
	/// ----------------------------------------------------------------------------------------
	/// <summary>
	/// DataConverter Tests
	/// </summary>
	/// ----------------------------------------------------------------------------------------
	[TestFixture]
	public class DataConverterTests
	{
		// See the comments at the beginning of the TestToken class.

		[TestFixtureSetUp]
		public void RunBeforeAllTests()
		{
		}

		/// ------------------------------------------------------------------------------------
		/// <summary>
		/// This test has no asserts at this time ... it will never fail.
		///
		/// As is, this test, produces an output file that can be visually examined to see if
		/// DataConverter works.
		///
		/// Keep this test as an example of how to call DataConverter.Convert().
		///
		/// Unit and acceptance tests for "users" of DataConverter will test its full functionality.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestDataConverter()
		{
			MarkerClass markerClass = new MarkerClass();

			DataConverter dataConverter = new DataConverter();

			DCMapping mapping;
			DCMapping[] mappings = new DCMapping[markerClass.markers.Length];

			for (int i = 0; i < markerClass.markers.Length; i++)
			{
				MarkerClass.Marker marker = markerClass.markers[i];

				mapping = new DCMapping();
				mapping.BeginMarker = marker.m_marker;

				// the sample input file is ASCII text, so using ISO-8859-1 (Latin 1)
				// to convert to Unicode is a good choice. By specifiying an empty string
				// as the encoding name, the default Unicode conversion will be used,
				// which is ISO-8859-1 (Latin 1).
				mapping.MarkerEncoding = "";
				mapping.DataEncoding = "";

				mapping.IsInline = marker.m_isInline;
				mapping.EndMarker = marker.m_end;

				mappings[i] = mapping;
			}

			// location of sample sfm files
			string SFMFilePath = Info.TestFileDir;

//			IECProjectFileInfo fileInfo;
//			IECProjectFileInfo[] fileInfos = new IECProjectFileInfo[1];
			DCFileInfo fileInfo;
			IDCFileInfo[] fileInfos = new IDCFileInfo[1];

//			fileInfo = new ECProjectFileInfo();
			fileInfo = new DCFileInfo();
			fileInfo.InputFileName = SFMFilePath + @"input.sfm";
			fileInfo.OutputFileName = SFMFilePath + @"output.sfm";

			// fileInfo.HasBOM is initialized to 0 ... which works for this test.
			fileInfo.FileEncoding = DCFileEncoding.DC_FE_BYTES;

			fileInfos[0] = fileInfo;

			// convert the input files
			dataConverter.ConvertNew(mappings, fileInfos);
		}

		///  ------------------------------------------------------------------------------------
		/// <summary>
		/// This test checks for an invalid Encoding Repository string and checks the return
		/// value.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestInvalidEncodingName()
		{
			MarkerClass markerClass = new MarkerClass();

			DataConverter dataConverter = new DataConverter();

			DCMapping mapping;
			DCMapping[] mappings = new DCMapping[markerClass.markers.Length];

			for (int i = 0; i < markerClass.markers.Length; i++)
			{
				MarkerClass.Marker marker = markerClass.markers[i];

				mapping = new DCMapping();
				mapping.BeginMarker = marker.m_marker;

				// use an undefined Encoding Repository name
				mapping.MarkerEncoding = "__NonExistantName__I_HOPE";
				mapping.DataEncoding = "";

				mapping.IsInline = marker.m_isInline;
				mapping.EndMarker = marker.m_end;

				mappings[i] = mapping;
			}

			// location of sample sfm files
			string SFMFilePath = Info.TestFileDir;

			DCFileInfo fileInfo;
			IDCFileInfo[] fileInfos = new IDCFileInfo[1];

			fileInfo = new DCFileInfo();
			fileInfo.InputFileName = SFMFilePath + @"input.sfm";
			fileInfo.OutputFileName = SFMFilePath + @"output.sfm";

			// fileInfo.HasBOM is initialized to 0 ... which works for this test.
			fileInfo.FileEncoding = DCFileEncoding.DC_FE_BYTES;

			fileInfos[0] = fileInfo;

			// convert the input files
			try
			{
				dataConverter.ConvertNew(mappings, fileInfos);
				Assert.Fail("ConvertNew didn't fail with invalid encoding name.");
			}
			catch(NoDataConverterException)
			{
				// no problem here, this is the expected exception
			}
			catch(TokenizerStackException e)
			{
				Assert.Fail("Unexpected Exception: " + e.Message);
			}
			catch(Exception e)
			{
				Assert.Fail("Unexpected Exception: " + e.Message);
			}
		}

		///  ------------------------------------------------------------------------------------
		/// <summary>
		/// This test checks for an invalid input file name.
		/// </summary>
		/// ------------------------------------------------------------------------------------
		[Test]
		public void TestInvalidFileName()
		{
			MarkerClass markerClass = new MarkerClass();

			DataConverter dataConverter = new DataConverter();

			DCMapping mapping;
			DCMapping[] mappings = new DCMapping[markerClass.markers.Length];

			for (int i = 0; i < markerClass.markers.Length; i++)
			{
				MarkerClass.Marker marker = markerClass.markers[i];

				mapping = new DCMapping();
				mapping.BeginMarker = marker.m_marker;

				// use an undefined Encoding Repository name
				mapping.MarkerEncoding = "";
				mapping.DataEncoding = "";

				mapping.IsInline = marker.m_isInline;
				mapping.EndMarker = marker.m_end;

				mappings[i] = mapping;
			}

			// location of sample sfm files
			string SFMFilePath = Info.TestFileDir;

			DCFileInfo fileInfo;
			IDCFileInfo[] fileInfos = new IDCFileInfo[1];

			fileInfo = new DCFileInfo();
			fileInfo.InputFileName = SFMFilePath + @"inputxxxxx.sfm";
			fileInfo.OutputFileName = SFMFilePath + @"output.sfm";

			// fileInfo.HasBOM is initialized to 0 ... which works for this test.
			fileInfo.FileEncoding = DCFileEncoding.DC_FE_BYTES;

			fileInfos[0] = fileInfo;

			// convert the input files
			try
			{
				dataConverter.ConvertNew(mappings, fileInfos);
				Assert.Fail("ConvertNew didn't fail with non-existant input file.");
			}
			catch(System.IO.FileNotFoundException)
			{
			}
			catch(Exception e)
			{
				Assert.Fail("Unexpected Exception: " + e.Message);
			}
		}

	}
	#endregion
}
