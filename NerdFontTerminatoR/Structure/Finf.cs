using System;
using System.IO;
using System.Linq;

namespace Nftr.Structure
{
	public enum EncodingMode
	{
		UTF8 = 0,
		UNICODE = 1,
		SJIS = 2,
		CP1252 = 3
	}

	public class Finf : NitroBlock
	{
		public Finf(NitroFile file)
			: base(file)
		{
		}

		#region Properties

		public byte Unknown {
			get;
			set;
		}

		public byte LineGap {
			get;
			set;
		}

		public ushort ErrorCharIndex {
			get;
			set;
		}

		public Cwdh.GlyphWidth DefaultWidth {
			get;
			set;
		}

		public EncodingMode Encoding {
			get;
			set;
		}

		public System.Text.Encoding TextEncoding {
			get {
				switch (this.Encoding) {
				case EncodingMode.UTF8:
					return System.Text.Encoding.Unicode;
				case EncodingMode.UNICODE:
					return System.Text.Encoding.Unicode;
				case EncodingMode.SJIS:
					return System.Text.Encoding.GetEncoding("shift_jis");
				case EncodingMode.CP1252:
					return System.Text.Encoding.GetEncoding(1252);
				default:
					return null;
				}
			}
		}

		public uint OffsetCglp {
			get;
			set;
		}

		public uint OffsetGwdh {
			get;
			set;
		}

		public uint OffsetCmap {
			get;
			set;
		}

		public byte GlyphHeight {
			get;
			set;
		}

		public byte GlyphWidth {
			get;
			set;
		}

		public byte BearingY {
			get;
			set;
		}

		public byte BearingX {
			get;
			set;
		}

		#endregion

		#region implemented abstract members of NitroBlock

		protected override void ReadData(Stream strIn)
		{
			BinaryReader br = new BinaryReader(strIn);

			this.Unknown = br.ReadByte();
			this.LineGap = br.ReadByte();
			this.ErrorCharIndex = br.ReadUInt16();
			this.DefaultWidth = Cwdh.GlyphWidth.FromStream(strIn);
			this.Encoding = (EncodingMode)br.ReadByte();
			this.OffsetCglp = br.ReadUInt32();
			this.OffsetGwdh = br.ReadUInt32();
			this.OffsetCmap = br.ReadUInt32();

			if (this.File.Version == 0x0102) {
				this.GlyphHeight = br.ReadByte();
				this.GlyphWidth = br.ReadByte();
				this.BearingY = br.ReadByte();
				this.BearingX = br.ReadByte();
			}

			br = null;
		}

		protected override void WriteData(Stream strOut)
		{
			BinaryWriter bw = new BinaryWriter(strOut);

			bw.Write(this.Unknown);
			bw.Write(this.LineGap);
			bw.Write(this.ErrorCharIndex);
			this.DefaultWidth.Write(strOut);
			bw.Write((byte)this.Encoding);
			bw.Write(this.OffsetCglp);
			bw.Write(this.OffsetGwdh);
			bw.Write(this.OffsetCmap);

			if (this.File.VersionS == "1.2") {
				bw.Write(this.GlyphHeight);
				bw.Write(this.GlyphWidth);
				bw.Write(this.BearingY);
				bw.Write(this.BearingX);
			}

			bw.Flush();
			bw = null;
		}

		public override bool Check()
		{
			throw new NotImplementedException();
		}

		public override string Name {
			get { return "FINF"; }
		}

		#endregion

		public string GetChar(int charCode)
		{
			byte[] charCodeB = BitConverter.GetBytes((ushort)charCode);
			if (this.Encoding == EncodingMode.SJIS &&
			    charCodeB[1] != 0x00) {
				charCodeB = charCodeB.Reverse().ToArray();
			}

			char ch = this.TextEncoding.GetChars(charCodeB)[0];

			if (Char.IsLetterOrDigit(ch) || Char.IsPunctuation(ch) ||
			    Char.IsSeparator(ch) || Char.IsSymbol(ch))
				return ch.ToString();
			else
				return "";
		}
	}
}

