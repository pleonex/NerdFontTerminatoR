using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace Nftr.Structure
{
	public class Cmap : NitroBlock
	{
		private static int IdMap = 0;

		public Cmap(NitroFile file) : base(file)
		{
			this.Id = IdMap++;
		}

		public Cmap(NitroFile file, XElement node)
			: base(file)
		{
			this.Id = IdMap++;
			this.Import(node);
		}

		#region Properties

		public int Id {
			get;
			private set;
		}

		public ushort FirstChar {
			get;
			set;
		}

		public ushort LastChar {
			get;
			set;
		}

		public uint Type {
			get;
			set;
		}

		public uint NextCmap {
			get;
			set;
		}

		/// <summary>
		/// Gets or sets the map.
		/// The first column contains the char codes and in the second
		/// the glyph index.
		/// </summary>
		/// <value>The map.</value>
		public int[,] Map {
			get;
			set;
		}

		#endregion

		public static void ResetCount()
		{
			IdMap = 0;
		}

		#region implemented abstract members of NitroBlock

		protected override void ReadData(Stream strIn)
		{
			BinaryReader br = new BinaryReader(strIn);

			this.FirstChar = br.ReadUInt16();
			this.LastChar = br.ReadUInt16();
			this.Type = br.ReadUInt32();
			this.NextCmap = br.ReadUInt32();

			// Read map
			int numEntries = this.LastChar - this.FirstChar + 1;
			this.Map = new int[numEntries, 2];

			switch (this.Type) {
			case 0:
				ushort firstGlyph = br.ReadUInt16();
				for (int i = 0; i < numEntries; i++) {
					this.Map[i, 0] = this.FirstChar + i;
					this.Map[i, 1] = firstGlyph + i;
				}
				break;

			case 1:
				List<Tuple<int, int>> map = new List<Tuple<int, int>>();
				for (int i = 0; i < numEntries; i++) {
					ushort gidx = br.ReadUInt16();
					if (gidx != 0xFFFF)
						map.Add(Tuple.Create<int, int>(this.FirstChar + i, gidx));
				}

				this.Map = new int[map.Count, 2];
				for (int i = 0; i < this.Map.GetLength(0); i++) {
					this.Map[i, 0] = map[i].Item1;
					this.Map[i, 1] = map[i].Item2;
				}

				break;

			case 2:
				numEntries = br.ReadUInt16();
				this.Map = new int[numEntries, 2];
				for (int i = 0; i < numEntries; i++) {
					this.Map[i, 0] = br.ReadUInt16();
					this.Map[i, 1] = br.ReadUInt16();
				}
				break;
			}

			br = null;
		}

		protected override void WriteData(Stream strOut)
		{
			BinaryWriter bw = new BinaryWriter(strOut);
			long startPos = strOut.Position;

			bw.Write(this.FirstChar);
			bw.Write(this.LastChar);
			bw.Write(this.Type);
			bw.Write(0x00);		// Not actually used

			switch (this.Type) {
			case 0:
				bw.Write((ushort)this.Map[0, 1]);
				break;

			case 1:
				for (int i = 0; i < this.Map.GetLength(0); i++) {
					bw.Write((ushort)this.Map[i, 1]);
				}
				break;

			case 2:
				bw.Write((ushort)this.Map.GetLength(0));;
				for (int i = 0; i < this.Map.GetLength(0); i++) {
					bw.Write((ushort)this.Map[i, 0]);
					bw.Write((ushort)this.Map[i, 1]);
				}
				break;
			}

			// Ok... Let's write it... but it won't work always and
			//  games don't need it
			if (this.NextCmap > 0) {
				uint startNextPos = (uint)strOut.Position + 8; // Skip nitroblock header
				if (startNextPos % 4 != 0)
					startNextPos += 4 - (startNextPos % 4);

				strOut.Position = startPos + 8;
				bw.Write(startNextPos);
			}

			bw.Flush();
		}

		public override bool Check()
		{
			throw new NotImplementedException();
		}

		public override string Name {
			get { return "CMAP"; }
		}

		#endregion

		private void Import(XElement node)
		{
			this.FirstChar = Convert.ToUInt16(node.Element("FirstChar").Value);
			this.LastChar  = Convert.ToUInt16(node.Element("LastChar").Value);
			this.Type      = Convert.ToUInt32(node.Element("Type").Value);

			int numEntries = this.LastChar - this.FirstChar + 1;
			this.Map = new int[numEntries, 2];
			XElement xmap = node.Element("Map");

			switch (this.Type) {
			case 0:
				int firstGlyph = Convert.ToInt32(xmap.Element("FirstImage").Value);
				for (int i = 0; i < numEntries; i++) {
					this.Map[i, 0] = this.FirstChar + i;
					this.Map[i, 1] = firstGlyph + i;
				}
				break;

			case 1:
				int idx = 0;
				foreach (XElement el in xmap.Elements("Image")) {
					this.Map[idx, 0] = this.FirstChar + idx;
					this.Map[idx, 1] = Convert.ToInt32(el.Value);
					idx++;
				}
				break;

			case 2:
				List<XElement> entries = new List<XElement>(xmap.Elements("Entry"));
				this.Map = new int[entries.Count, 2];
				for (int i = 0; i < entries.Count; i++) {
					this.Map[i, 0] = Convert.ToInt32(entries[i].Element("Char").Value);
					this.Map[i, 1] = Convert.ToInt32(entries[i].Element("Image").Value);
				}
				break;

			default:
				throw new FormatException("Invalid type for Cmap");
			}
		}

		public XElement Export()
		{
			Finf finf = this.File.Blocks.GetByType<Finf>(0);
			XElement xcmap = new XElement("Cmap");

			xcmap.Add(new XElement("FirstChar", this.FirstChar.ToString("X")));
			xcmap.Add(new XComment(" (" + finf.GetChar(this.FirstChar) + ") "));
			xcmap.Add(new XElement("LastChar", this.LastChar.ToString("X")));
			xcmap.Add(new XComment(" (" + finf.GetChar(this.LastChar) + ") "));
			xcmap.Add(new XElement("Type", this.Type));
	
			XElement map = new XElement("Map");
			xcmap.Add(map);
			switch (this.Type) {
				case 0:
				map.Add(new XElement("FirstImage", this.Map[0, 1]));
				StringBuilder mapComment = new StringBuilder();
				for (int i = 0; i < this.Map.GetLength(0); i++) {
					if (i != 0 && i % 5 == 0)
						mapComment.AppendLine();
					mapComment.AppendFormat(" {0}:{1:X} ({2})  ",
					                        this.Map[i, 1], this.Map[i, 0], finf.GetChar(this.Map[i, 0]));
				}
				map.Add(new XComment(mapComment.ToString()));
				break;

				case 1:
				for (int i = 0; i < this.Map.GetLength(0); i++) {
					XElement ximg = new XElement("Image", this.Map[i, 1]);
					map.Add(new XComment(string.Format(
						" {0:X} ({1}) ",
						this.Map[i, 0],
						finf.GetChar(this.Map[i, 0]))));
					map.Add(ximg);
				}
				break;

				case 2:
				for (int i = 0; i < this.Map.GetLength(0); i++) {
					XElement xchar = new XElement("Entry");
					xchar.Add(new XElement("Image", this.Map[i, 1]));
					xchar.Add(new XElement("Char", this.Map[i, 0].ToString("X")));
					xchar.Add(new XComment(" (" + finf.GetChar(this.Map[i, 0]) + ") "));
				}
				break;
			}

			return xcmap;
		}

		public bool Contains(int imgIndex)
		{
			for (int i = 0; i < this.Map.GetLength(0); i++) {
				if (this.Map[i, 1] == imgIndex)
					return true;
			}

			return false;
		}

		public bool Contains(ushort charCode)
		{
			if (charCode < this.FirstChar || charCode > this.LastChar)
				return false;

			for (int i = 0; i < this.Map.GetLength(0); i++) {
				if (this.Map[i, 0] == charCode)
					return true;
			}

			return false;
		}

		public int GetCharCode(int imgIndex)
		{
			for (int i = 0; i < this.Map.GetLength(0); i++) {
				if (this.Map[i, 1] == imgIndex)
					return this.Map[i, 0];
			}

			return -1;
		}
	}
}

