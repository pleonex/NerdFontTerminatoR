using System;
using System.IO;
using System.Xml.Linq;

namespace Nftr.Structure
{
	public class Cwdh : NitroBlock
	{
		private WidthRegion firstRegion;

		public Cwdh(NitroFile file)
			: base(file)
		{
		}

		#region implemented abstract members of NitroBlock

		protected override void ReadData(Stream strIn)
		{
			this.firstRegion = WidthRegion.FromStream(
				strIn,
				this.File.Blocks.GetByType<Finf>(0).DefaultWidth);
		}

		protected override void WriteData(Stream strOut)
		{
			this.firstRegion.Write(strOut);
		}

		public override string Name {
			get { return "CWDH"; }
		}

		#endregion

		public GlyphWidth GetWidth(ushort charCode)
		{
			return this.firstRegion.GetWidth(charCode);
		}

		public struct GlyphWidth
		{
			public byte BearingX
			{
				get;
				private set;
			}
			
			public byte Width
			{
				get;
				private set;
			}
			
			public byte Advance
			{
				get;
				private set;
			}

			public static GlyphWidth FromStream(Stream strIn)
			{
				GlyphWidth gw = new GlyphWidth();
				gw.BearingX = (byte)strIn.ReadByte();
				gw.Width = (byte)strIn.ReadByte();
				gw.Advance = (byte)strIn.ReadByte();
				return gw;
			}

			public void Write(Stream strOut)
			{
				strOut.WriteByte(this.BearingX);
				strOut.WriteByte(this.Width);
				strOut.WriteByte(this.Advance);
				strOut.Flush();
			}

			public XElement Export(string name)
			{
				XElement el = new XElement(name);
				el.Add(new XElement("BearingX", this.BearingX));
				el.Add(new XElement("Width", this.Width));
				el.Add(new XElement("Advance", this.Advance));
				return el;
			}

			public override string ToString()
			{
				return string.Format("[BearingX={0}, Width={1}, Advance={2}]",
				                     BearingX, Width, Advance);
			}
		}

		private class WidthRegion
		{
			private GlyphWidth defaultWidth;

			public WidthRegion(GlyphWidth defaultWidth)
			{
				this.defaultWidth = defaultWidth;
			}

			public ushort FirstChar {
				get;
				set;
			}

			public ushort LastChar {
				get;
				set;
			}

			public WidthRegion NextRegion {
				get;
				set;
			}

			public GlyphWidth[] Widths {
				get;
				set;
			}

			public static WidthRegion FromStream(Stream strIn, GlyphWidth defaultWidth)
			{
				BinaryReader br = new BinaryReader(strIn);
				long startPosition = strIn.Position;

				WidthRegion wr = new WidthRegion(defaultWidth);
				wr.FirstChar = br.ReadUInt16();
				wr.LastChar = br.ReadUInt16();
				uint nextRegion = br.ReadUInt32();

				// Read widths
				int numWidths = wr.LastChar - wr.FirstChar + 1;
				wr.Widths = new GlyphWidth[numWidths];

				for (int i = 0; i < numWidths; i++) {
					wr.Widths[i] = GlyphWidth.FromStream(strIn);
				}

				br = null;

				// Get other regions
				if (nextRegion != 0) {
					strIn.Position = startPosition + nextRegion;
					wr.NextRegion = WidthRegion.FromStream(strIn, defaultWidth);
				} else {
					wr.NextRegion = null;
				}

				return wr;
			}

			public void Write(Stream strOut)
			{
				BinaryWriter bw = new BinaryWriter(strOut);
				bw.Write(this.FirstChar);
				bw.Write(this.LastChar);
				if (this.NextRegion == null)
					bw.Write(0);
				else {
					uint size = 8 + 3 * (uint)this.Widths.Length;
					bw.Write(size);
				}

				foreach (GlyphWidth w in this.Widths) {
					w.Write(strOut);
				}

				bw.Flush();
			}

			public GlyphWidth GetWidth(ushort charCode)
			{
				if (charCode >= this.FirstChar && charCode < this.LastChar) {
					return this.Widths[charCode - this.FirstChar];
				} else {
					if (this.NextRegion != null)
						return this.NextRegion.GetWidth(charCode);
					else
						return this.defaultWidth;
				}
			}
		}
	}
}

