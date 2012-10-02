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
// File: DataConverter.cs
// Responsibility:
// Last reviewed:
//
// <remarks>
// </remarks>
// --------------------------------------------------------------------------------------------
using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
//using ECOBJECTSLib;
using System.Diagnostics;

namespace SIL.FieldWorks.DataConverter
{
	[GuidAttribute("95B4C066-744F-4c4a-8DDF-794E8A422272")]
	[ComVisible(true)]
	public interface IDCMapping
	{
		string BeginMarker		{ get; set;	}
		string EndMarker		{ get; set;	}
		string NewBeginMarker	{ get; set;	}
		string NewEndMarker		{ get; set;	}
		string MarkerEncoding	{ get; set;	}
		string DataEncoding		{ get; set;	}

		int Domain { get; set; }
		bool IsInline { get; set; }
	}

	[ProgId("DCMapping")]
	[ClassInterface(ClassInterfaceType.None)]
	[GuidAttribute("10FF9119-B116-4a8c-99A4-950DFAA0138C")]
	[ComVisible(true)]
	public class DCMapping : IDCMapping
	{
		protected string m_sBegin = "";
		protected string m_sEnd = "";
		protected string m_sNewBegin = "";
		protected string m_sNewEnd = "";
		protected string m_sMarkerEncoding = "";
		protected string m_sDataEncoding = "";
		protected int m_eDomain = 0;
		protected bool m_fIsInline = false;

		public string BeginMarker
		{
			get		{ return m_sBegin; }
			set		{ m_sBegin = value; CalcNewBeginMarker(); }
		}
		public string EndMarker
		{
			get		{ return m_sEnd; }
			set		{ m_sEnd = value; CalcNewEndMarker(); }
		}
		public string NewBeginMarker
		{
			get		{ return m_sNewBegin; }
			set		{ m_sNewBegin = value; }
		}
		public string NewEndMarker
		{
			get		{ return m_sNewEnd; }
			set		{ m_sNewEnd = value; }
		}
		public string MarkerEncoding
		{
			get		{ return m_sMarkerEncoding; }
			set		{ m_sMarkerEncoding = value; }
		}
		public string DataEncoding
		{
			get		{ return m_sDataEncoding; }
			set		{ m_sDataEncoding = value; }
		}
		/// <summary>
		/// The underlying data stored in this property is actually an enumerated value, but
		/// </summary>
		public int Domain
		{
			get		{ return m_eDomain; }
			set		{ m_eDomain = value; }
		}
		public bool IsInline
		{
			get		{ return m_fIsInline; }
			set		{ m_fIsInline = value; }
		}

		private string CalcNewMarker(string marker)
		{
			if (marker == null || marker.Length < 1 )
				return "";

			string sTemp = marker;
			if ( sTemp[0] != '\\' )		// has to start with a backslash character
				sTemp = "\\" + marker;

			// pull out '*' as they are not handled correctly in the SO and replace with '+'
			// don't replace the last '*' if it's the last character

			if ( sTemp.IndexOf("*") >= 0 )
			{
				int len = sTemp.Length;
				char lastChar = sTemp[len-1];			// save last character
				sTemp = sTemp.Remove(len-1, 1);			// remove last char
				sTemp = sTemp.Replace('*', '+');		// replace '*' chars with '+' chars
				sTemp += lastChar;						// restore the last character
			}
			return sTemp;
		}

		private void CalcNewBeginMarker()
		{
			m_sNewBegin = CalcNewMarker(m_sBegin);
		}

		private void CalcNewEndMarker()
		{
			m_sNewEnd = CalcNewMarker(m_sEnd);
		}


	}

	[ComVisible(true)]
	public enum DCFileEncoding
	{
		DC_FE_Unknown = 0x01,
		DC_FE_BYTES   = 0x02,
		DC_FE_UTF8	  = 0x04,
		DC_FE_UTF16BE = 0x10,
		DC_FE_UTF16LE = 0x20,
		DC_FE_UTF32BE = 0x40,
		DC_FE_UTF32LE = 0x80,
	}

	[GuidAttribute("4CD19F7D-1BD5-4958-975A-0A5694B226D1")]
	[ComVisible(true)]
	public interface IDCFileInfo
	{
		string InputFileName	{ get; set;	}
		string OutputFileName	{ get; set;	}
		bool HasBOM	{ get; set;	}
		DCFileEncoding FileEncoding { get; set; }
	}

	[ProgId("DCFileInfo")]
	[ClassInterface(ClassInterfaceType.None)]
	[GuidAttribute("AD024A49-FD84-4e58-96EB-F7B7B7AC167A")]
	[ComVisible(true)]
	public class DCFileInfo : IDCFileInfo
	{
		protected string m_sInputFileName = "";
		protected string m_sOutputFileName = "";
		protected bool m_fHasBOM = false;
		protected DCFileEncoding m_eFileEncoding;

		public string InputFileName
		{
			get		{ return m_sInputFileName; }
			set		{ m_sInputFileName = value; }
		}
		public string OutputFileName
		{
			get		{ return m_sOutputFileName; }
			set		{ m_sOutputFileName = value; }
		}
		public bool HasBOM
		{
			get		{ return m_fHasBOM; }
			set		{ m_fHasBOM = value; }
		}
		public DCFileEncoding FileEncoding
		{
			get		{ return m_eFileEncoding; }
			set		{ m_eFileEncoding = value; }
		}
	}

	[GuidAttribute("6C739D32-FAED-4771-99D7-C6AE6D4BE0A8")]
	[ComVisible(true)]
	public interface IDataConverter
	{
//		void Convert(IECMapping[] mappings, IECProjectFileInfo[] fileInfos);
		void ConvertNew(IDCMapping[] mappings, IDCFileInfo[] fileInfos);	// new with DataConverter objs

		void SetDefaultMarkerMap(string defaultMarkerMap);
		void SetDefaultDataMap(string defaultDataMap);

	}

	[GuidAttribute("BDF0DEF4-F18F-42f2-BFC8-F5A7F366F009")]
	[ClassInterface(ClassInterfaceType.None)]
	[ProgId("SIL.FieldWorks.DataConverter")]
	[ComVisible(true)]
	public class DataConverter : IDataConverter
	{
		/// <summary>
		/// Control how much output we send to the application's listeners (e.g. visual studio
		/// output window)
		/// </summary>
		/// <remarks>NOTE: to set the trace level, create a config file like the following and
		/// set it there. Values are: Off=0, Error=1, Warning=2, Info=3, Verbose=4.
		/// </remarks>
		protected TraceSwitch m_traceSwitch = new TraceSwitch("DataConverter", "");
		// <configuration>
		//    <system.diagnostics>
		//       <switches>
		//          <add name="DataConverter" value="4" />
		//       </switches>
		//    </system.diagnostics>
		// </configuration>

		Tokenizer m_tokenizer;
		string m_defaultMarkerMap;
		string m_defaultDataMap;

		public DataConverter()
		{
			m_defaultMarkerMap = "";
			m_defaultDataMap = "";
		}

		public void SetDefaultMarkerMap(string defaultMarkerMap)
		{
			DefaultMarkerMap = defaultMarkerMap;
		}

		public void SetDefaultDataMap(string defaultDataMap)
		{
			DefaultDataMap = defaultDataMap;
		}

		public string DefaultMarkerMap
		{
			get {return m_defaultMarkerMap;}
			set {m_defaultMarkerMap = value;}
		}

		public string DefaultDataMap
		{
			get {return m_defaultDataMap;}
			set {m_defaultDataMap = value;}
		}

		static public int ReversibleCodePage
		{
			// 28591 is the code page for ISO-8859-1 (Latin 1)
			// This code page is used since the codepoint values
			// between bytes and Unicode characters are one-to-one
			// which means that this code page is reversible,
			// which is needed so Encoding Converters can convert
			// the legacy data in C# strings back to bytes.

			// One-to-one above means conversion in the form of
			// 0xNN   => U+00NN (byte    to unicode) or
			// U+00NN => 0xNN   (unicode to byte)
			// where N is a hex digit.
			get {return 28591;}
		}

		static public Encoding ReversibleEncoding
		{
			get {return Encoding.GetEncoding(ReversibleCodePage);}
		}

		// TODO: remove method if not needed
		public void Test(int skip, IDCMapping mapping, IDCFileInfo fileInfo)
		{
			if (m_traceSwitch.TraceVerbose)
			{
				Debug.WriteLine("DataConverter Test");
				Debug.WriteLine("\tDCMapping");
				Debug.WriteLine("\t\tBeginMarker =" + mapping.BeginMarker + ".");
				Debug.WriteLine("\t\tDomain      =" + mapping.Domain      + ".");
				Debug.WriteLine("\t\tEndMarker   =" + mapping.EndMarker   + ".");
				Debug.WriteLine("\tIDCFileInfo");
				Debug.WriteLine("\t\tFileEncoding       =" + fileInfo.FileEncoding       + ".");
//				Debug.WriteLine("\t\tFileEncodingSource =" + fileInfo.FileEncodingSource + ".");
				Debug.WriteLine("\t\tInputFileName      =" + fileInfo.InputFileName      + ".");
				Debug.WriteLine("\t\tOutputFileName     =" + fileInfo.OutputFileName     + ".");
			}
		}

		public void ConvertNew(IDCMapping[] mappings, IDCFileInfo[] fileInfos)
		{
//			Test(1, mappings[0], fileInfos[0]);

			m_tokenizer = new Tokenizer();

			foreach (IDCMapping mapping in mappings)
			{
				if (mapping.MarkerEncoding.Length <= 0)
					mapping.MarkerEncoding = DefaultMarkerMap;
				if (mapping.DataEncoding.Length <= 0)
					mapping.DataEncoding = DefaultDataMap;
				MarkerSpec ms = MarkerSpec.CreateMarkerSpec(mapping);
				m_tokenizer.Tri.Add(ms);
			}

			Token token;
			string output;

			FileStream stream;
			StreamReader streamReader;
			Stream outputStream;
			bool fBOM;
			StreamWriter outputWriter = null;

			// Do for each input file in fileInfo
			System.Text.Encoding encoding;
			foreach (IDCFileInfo fileInfo in fileInfos)
			{
				stream = new FileStream(fileInfo.InputFileName,
					FileMode.Open, FileAccess.Read);
				bool fAlreadyUnicode = true;
				switch (fileInfo.FileEncoding)
				{
					case DCFileEncoding.DC_FE_BYTES:
					case DCFileEncoding.DC_FE_Unknown:
						encoding = ReversibleEncoding;
						fAlreadyUnicode = false;
						break;
					case DCFileEncoding.DC_FE_UTF16BE:
						encoding = System.Text.Encoding.BigEndianUnicode;
						break;
					case DCFileEncoding.DC_FE_UTF16LE:
						encoding = System.Text.Encoding.Unicode;
						break;
					case DCFileEncoding.DC_FE_UTF8:
						encoding = System.Text.Encoding.UTF8;
						break;
					default:
						Debug.Fail("Requested input file encoding not implemented.");
						encoding = ReversibleEncoding;
						fAlreadyUnicode = false;
						break;
				}
				streamReader = new StreamReader(stream, encoding);
				m_tokenizer.Input = streamReader;

				outputStream = new FileStream(fileInfo.OutputFileName, FileMode.Create,
					FileAccess.Write);

				fBOM = fileInfo.HasBOM;
				if (fBOM)
				{
					// Use StreamWriter if BOM needed.
					outputWriter = new StreamWriter(outputStream, System.Text.Encoding.UTF8);
				}

				do
				{
					// Enhance (BobbyD): It seems that all the StreamWriters output a BOM,
					// even if we don't want one. One solution is below, that is, write the data
					// using Streams instead of StreamWriters. Except this is sort of messy,
					// a cleaner solution may be to subclass Encoding.UTF8, override the method
					// GetPreamble() to return a zero length byte array, instead of the UTF8 BOM,
					// and then the nice and clean StreamWriter can be used. More information on
					// this is under the help topic of Encoding.GetPreamble Method.
					token = m_tokenizer.Next();
					output = token.Output(fAlreadyUnicode);
					byte[] bytes = System.Text.Encoding.UTF8.GetBytes(output);
					if (token is NewlineToken)
					{
						if (fBOM)
							outputWriter.WriteLine(output);
						else
						{
							outputStream.Write(bytes, 0, bytes.Length);
							outputStream.WriteByte((byte)'\r');
							outputStream.WriteByte((byte)'\n');
						}
					}
					else
					{
						if (fBOM)
							outputWriter.Write(output);
						else
							outputStream.Write(bytes, 0, bytes.Length);
					}
				} while (!(token is EndOfFileToken));

				m_tokenizer.Input.Close();			// close the input stream

				if (fBOM)
					outputWriter.Close();
				else
					outputStream.Close();
			}
		}

//		public void Convert(IECMapping[] mappings, IECProjectFileInfo[] fileInfos)
//		{
//			Test(1, mappings[0], fileInfos[0]);
//
//			m_tokenizer = new Tokenizer();
//
//			foreach (IECMapping mapping in mappings)
//			{
//				if (mapping.MarkerEncoding.Length <= 0)
//					mapping.MarkerEncoding = DefaultMarkerMap;
//				if (mapping.DataEncoding.Length <= 0)
//					mapping.DataEncoding = DefaultDataMap;
//				MarkerSpec ms = MarkerSpec.CreateMarkerSpec(mapping);
//				m_tokenizer.Tri.Add(ms);
//			}
//
//			Token token;
//			string output;
//
//			FileStream stream;
//			StreamReader streamReader;
//			Stream outputStream;
//			bool fBOM;
//			StreamWriter outputWriter = null;
//
//			// Do for each input file in fileInfo
//			System.Text.Encoding encoding;
//			foreach (IECProjectFileInfo fileInfo in fileInfos)
//			{
//				stream = new FileStream(fileInfo.InputFileName,
//					FileMode.Open, FileAccess.Read);
//				bool fAlreadyUnicode = true;
//				switch (fileInfo.FileEncoding)
//				{
//					case EC_FileEncoding.FE_BYTES:
//					case EC_FileEncoding.FE_Unknown:
//						encoding = ReversibleEncoding;
//						fAlreadyUnicode = false;
//						break;
//					case EC_FileEncoding.FE_UTF16BE:
//						encoding = System.Text.Encoding.BigEndianUnicode;
//						break;
//					case EC_FileEncoding.FE_UTF16LE:
//						encoding = System.Text.Encoding.Unicode;
//						break;
//					case EC_FileEncoding.FE_UTF8:
//						encoding = System.Text.Encoding.UTF8;
//						break;
//					default:
//						Debug.Fail("Requested input file encoding not implemented.");
//						encoding = ReversibleEncoding;
//						fAlreadyUnicode = false;
//						break;
//				}
//				streamReader = new StreamReader(stream, encoding);
//				m_tokenizer.Input = streamReader;
//
//				outputStream = new FileStream(fileInfo.OutputFileName, FileMode.Create,
//					FileAccess.Write);
//
//				fBOM = (1 == fileInfo.HasBOM);
//				if (fBOM)
//				{
//					// Use StreamWriter if BOM needed.
//					outputWriter = new StreamWriter(outputStream, System.Text.Encoding.UTF8);
//				}
//
//				do
//				{
//					// Enhance (BobbyD): It seems that all the StreamWriters output a BOM,
//					// even if we don't want one. One solution is below, that is, write the data
//					// using Streams instead of StreamWriters. Except this is sort of messy,
//					// a cleaner solution may be to subclass Encoding.UTF8, override the method
//					// GetPreamble() to return a zero length byte array, instead of the UTF8 BOM,
//					// and then the nice and clean StreamWriter can be used. More information on
//					// this is under the help topic of Encoding.GetPreamble Method.
//					token = m_tokenizer.Next();
//					output = token.Output(fAlreadyUnicode);
//					byte[] bytes = System.Text.Encoding.UTF8.GetBytes(output);
//					if (token is NewlineToken)
//					{
//						if (fBOM)
//							outputWriter.WriteLine(output);
//						else
//						{
//							outputStream.Write(bytes, 0, bytes.Length);
//							outputStream.WriteByte((byte)'\r');
//							outputStream.WriteByte((byte)'\n');
//						}
//					}
//					else
//					{
//						if (fBOM)
//							outputWriter.Write(output);
//						else
//							outputStream.Write(bytes, 0, bytes.Length);
//					}
//				} while (!(token is EndOfFileToken));
//
//				m_tokenizer.Input.Close();			// close the input stream
//
//				if (fBOM)
//					outputWriter.Close();
//				else
//					outputStream.Close();
//			}
//		}
	}
}
