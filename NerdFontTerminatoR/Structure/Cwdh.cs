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
			WidthRegion.ResetCount();
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

		public override bool Check()
		{
			throw new NotImplementedException();
		}

		public override string Name {
			get { return "CWDH"; }
		}

		#endregion

		public WidthRegion FirstRegion {
			get { return this.firstRegion; }
		}

		public GlyphWidth GetWidth(ushort charCode)
		{
			return this.firstRegion.GetWidth(charCode);
		}

		public struct GlyphWidth
		{
			public int IdRegion {
				get;
				private set;
			}

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

			public static GlyphWidth FromStream(Stream strIn, int idRegion)
			{
				GlyphWidth gw = new GlyphWidth();
				gw.IdRegion = idRegion;
				gw.BearingX = (byte)strIn.ReadByte();
				gw.Width    = (byte)strIn.ReadByte();
				gw.Advance  = (byte)strIn.ReadByte();
				return gw;
			}

			public static GlyphWidth FromXml(XElement node)
			{
				GlyphWidth gw = new GlyphWidth();
				gw.IdRegion = Convert.ToInt32(node.Element("IdRegion").Value);
				gw.BearingX = Convert.ToByte(node.Element("BearingX").Value);
				gw.Width    = Convert.ToByte(node.Element("Width").Value);
				gw.Advance  = Convert.ToByte(node.Element("Advance").Value);
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
				el.Add(new XElement("IdRegion", this.IdRegion));
				el.Add(new XElement("BearingX", this.BearingX));
				el.Add(new XElement("Width", this.Width));
				el.Add(new XElement("Advance", this.Advance));
				return el;
			}

			public override string ToString()
			{
				return string.Format("[Id={0}, BearingX={1}, Width={2}, Advance={3}]",
				                     this.IdRegion, this.BearingX, this.Width, this.Advance);
			}
		}

		public class WidthRegion
		{
			private static int IdWidth = 0;

			private GlyphWidth defaultWidth;

			public WidthRegion(GlyphWidth defaultWidth)
			{
				this.Id = IdWidth++;
				this.defaultWidth = defaultWidth;
			}

			public int Id {
				get;
				set;
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

			public static void ResetCount()
			{
				IdWidth = 0;
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
					wr.Widths[i] = GlyphWidth.FromStream(strIn, wr.Id);
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

