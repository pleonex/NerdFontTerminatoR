// -----------------------------------------------------------------------
// <copyright file="Finf.cs" company="none">
// Copyright (C) 2013
//
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by 
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//
//   This program is distributed in the hope that it will be useful, 
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details. 
//
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see "http://www.gnu.org/licenses/". 
// </copyright>
// <author>pleoNeX</author>
// <email>benito356@gmail.com</email>
// <date>30/05/2013</date>
// -----------------------------------------------------------------------

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
			if (file.VersionS != "1.2")
				this.Size = 0x1C;
			else
				this.Size = 0x20;
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

		public uint OffsetCwdh {
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
			this.DefaultWidth = Cwdh.GlyphWidth.FromStream(strIn, -1);
			this.Encoding = (EncodingMode)br.ReadByte();
			this.OffsetCglp = br.ReadUInt32();
			this.OffsetCwdh = br.ReadUInt32();
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
			bw.Write(this.OffsetCwdh);
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

		public void UpdateOffsets()
		{
			uint offsetFinf = NitroFile.BlocksStart;
			this.OffsetCglp = (uint)(offsetFinf + this.Size) + 8;
			this.OffsetCwdh = (uint)(this.OffsetCglp + this.File.Blocks.GetByType<Cglp>(0).Size);
			this.OffsetCmap = (uint)(this.OffsetCwdh + this.File.Blocks.GetByType<Cwdh>(0).Size);

			if (this.OffsetCwdh % 4 != 0)
				this.OffsetCwdh += 4 - (this.OffsetCwdh % 4);
			if (this.OffsetCmap % 4 != 0)
				this.OffsetCmap += 4 - (this.OffsetCmap % 4);
		}
	}
}

