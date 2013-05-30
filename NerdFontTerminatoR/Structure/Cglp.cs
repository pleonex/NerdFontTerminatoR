using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Linq;

namespace Nftr.Structure
{
	public enum RotationMode : byte
	{
		Rot0 = 0,
		Rot90 = 1,
		Rot180 = 2,
		Rot270 = 3
	}

	public class Cglp : NitroBlock
	{
		private List<Colour[,]> glyphs;

		public Cglp(NitroFile file) : base(file)
		{
		}

		public Cglp(NitroFile file, Colour[][,] glyphs, byte boxWidth, byte boxHeight,
		            byte glyphWidth, byte glyphHeight, byte depth, RotationMode rotation)
			: base(file)
		{
			this.BoxWidth = boxWidth;
			this.BoxHeight = boxHeight;
			this.GlyphWidth = glyphWidth;
			this.GlyphHeight = glyphHeight;
			this.Depth = (byte)depth;
			this.Rotation = (byte)rotation;
			this.glyphs = new List<Colour[,]>(glyphs);

			int boxSize = this.BoxWidth * this.BoxHeight * this.Depth;
			this.Size = 0x08 + 0x08 + (int)Math.Ceiling(boxSize / 8.0) * this.glyphs.Count;
		}

		#region Properties

		public byte BoxWidth {
			get;
			private set;
		}

		public byte BoxHeight {
			get;
			private set;
		}

		public byte GlyphHeight {
			get;
			set;
		}

		public byte GlyphWidth {
			get;
			set;
		}

		public byte Depth {
			get;
			private set;
		}

		public byte Rotation {
			get;
			private set;
		}

		public int NumGlyphs {
			get { return this.glyphs.Count; }
		}

		#endregion

		#region implemented abstract members of NitroBlock

		protected override void ReadData(Stream strIn)
		{
			BinaryReader br = new BinaryReader(strIn);

			this.BoxWidth = br.ReadByte();
			this.BoxHeight = br.ReadByte();
			ushort boxByteSize = br.ReadUInt16();
			this.GlyphHeight = br.ReadByte();
			this.GlyphWidth = br.ReadByte();
			this.Depth = br.ReadByte();
			this.Rotation = br.ReadByte();

			// Read glyph data
			Colour[] palette = this.GetPalette();
			int numGlyphs = (this.Size - 0x08) / boxByteSize;
			this.glyphs = new List<Colour[,]>(numGlyphs);

			for (int i = 0; i < numGlyphs; i++) {
				Colour[,] glyph = new Colour[this.BoxWidth, this.BoxHeight];
				byte[] data = br.ReadBytes(boxByteSize);
				int bitPos = 0;

				for (int h = 0; h < this.BoxHeight; h++) {
					for (int w = 0; w < this.BoxWidth; w++) {
						int colorIndex = this.ReadBits(data, ref bitPos);
						glyph[w, h] = palette[colorIndex];
					}
				}

				this.glyphs.Add(glyph);
			}

			br = null;
		}

		protected override void WriteData(Stream strOut)
		{
			BinaryWriter bw = new BinaryWriter(strOut);

			bw.Write(this.BoxWidth);
			bw.Write(this.BoxHeight);
			int boxSize = this.BoxWidth * this.BoxHeight * this.Depth;
			boxSize = (ushort)Math.Ceiling(boxSize / 8.0);	// Convert to bytes
			bw.Write((ushort)boxSize);
			bw.Write(this.GlyphHeight);
			bw.Write(this.GlyphWidth);
			bw.Write(this.Depth);
			bw.Write(this.Rotation);

			Colour[] palette = this.GetPalette();
			foreach (Colour[,] gl in this.glyphs) {

				byte val = 0;
				int bitPos = 0;

				for (int h = 0; h < this.BoxHeight; h++) {
					for (int w = 0; w < this.BoxWidth; w++) {
						int colIndex = Array.FindIndex(palette, p => p.Equals(gl[w, h]));

						for (int d = this.Depth - 1; d >= 0; d--, bitPos++) {
							// Update pos and write if need
							if (bitPos % 8 == 0 && bitPos != 0) {
								bw.Write(val);
								val = 0;
							}

							int bit = (colIndex >> d) & 1;
							val |= (byte)(bit << (7 - (bitPos % 8)));
						}
					} // End for width
				} // End for height

				// Write the last value
				bw.Write(val);
			} // End for glyph

			bw.Flush();
		}

		public override bool Check()
		{
			throw new NotImplementedException();
		}

		public override string Name {
			get { return "CGLP"; }
		}

		#endregion

		public Colour[,] GetGlyph(int index)
		{
			return this.glyphs[index];
		}

		public Bitmap GetGlyphImage(int index)
		{
			Colour[,] glyph = this.GetGlyph(index);
			Bitmap image = new Bitmap(this.BoxWidth, this.BoxHeight);

			for (int h = 0; h < this.BoxHeight; h++) {
				for (int w = 0; w < this.BoxWidth; w++) {
					image.SetPixel(w, h, glyph[w, h].ToColor());
				}
			}

			return image;
		}

		private Colour[] GetPalette()
		{
			Colour[] palette = new Colour[1 << this.Depth];

			for (int i = 0; i < palette.Length; i++) {
				int colorIndex = (int)(255 * (1 - (float)i / (palette.Length - 1)));
				palette[i] = new Colour(colorIndex, colorIndex, colorIndex);
			}

			return palette;
		}

		private int ReadBits(byte[] data, ref int bitPos)
		{
			int val = 0;

			for (int b = this.Depth - 1; b >= 0; b--, bitPos++) {
				// First get the bit
				int bit = data[bitPos / 8] >> (7 - bitPos % 8);
				bit &= 1;
				// Then set the bit
				val |= bit << b;
			}

			return val;
		}
	}
}

