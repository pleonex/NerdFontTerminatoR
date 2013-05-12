using System;
using System.IO;

namespace Nftr.Structure
{
	public class Cmap : NitroBlock
	{
		public Cmap(NitroFile file)
			: base(file)
		{
		}

		#region Properties

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

		#region implemented abstract members of NitroBlock

		protected override void ReadData(Stream strIn)
		{
			BinaryReader br = new BinaryReader(strIn);

			this.FirstChar = br.ReadUInt16();
			this.LastChar = br.ReadUInt16();
			this.Type = br.ReadUInt32();
			this.NextCmap = br.ReadUInt32();

			// Read map
			int numEntries = this.LastChar - this.FirstChar;
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
				for (int i = 0; i < numEntries; i++) {
					this.Map[i, 0] = this.FirstChar + i;
					this.Map[i, 1] = br.ReadUInt16();
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
	}
}

