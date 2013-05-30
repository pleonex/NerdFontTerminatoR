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

			this.Map = new int[0, 2];
			this.Size = 8 + 0xC;
		}

		#region Properties

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

			// Ok... Let's write it... but it won't work always and games don't read it
			// If it is not the last one write it
			int blockIdx = this.File.Blocks.FindIndex(b => b == this);
			if (blockIdx + 1 != this.File.Blocks.Count) {
				this.NextCmap = (uint)strOut.Position + 8; // Skip nitroblock header
				if (this.NextCmap % 4 != 0)
					this.NextCmap += 4 - (this.NextCmap % 4);

				strOut.Position = startPos + 8;
				bw.Write(this.NextCmap);
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

		public void InsertCharCode(int imgIndex, ushort charCode)
		{
			if (charCode < this.FirstChar || charCode > this.LastChar) {
				throw new ArgumentException("Invalid char code");
			}

			// Update size
			if (this.Type == 0 && this.Map.GetLength(0) == 0)
				this.Size += 2;
			else if (this.Type == 1)
				this.Size += 2;
			else if (this.Type == 2) {
				if (this.Map.GetLength(0) == 0)
					this.Size += 2;
				this.Size += 4;
			}
			

			// Resize and do a binary search copy
			int[,] map = new int[this.Map.GetLength(0) + 1, 2];
			for (int i = 0, j = 0; i < map.GetLength(0); i++) {
				if (j < this.Map.GetLength(0) && charCode > this.Map[j, 0]) {
					map[i, 0] = this.Map[j, 0];
					map[i, 1] = this.Map[j, 1];
					j++;
				} else if (charCode != 0xFFFF) {
					map[i, 0] = charCode;
					map[i, 1] = imgIndex;
					charCode = 0xFFFF; // It won't be copied again
				} else {
					throw new Exception("Unknown error");
				}
			}

			this.Map = map;
		}
	}
}

