using System;
using System.IO;
using System.Linq;

namespace Nftr
{
	public abstract class NitroBlock
	{
		private NitroFile file;

		protected NitroBlock(NitroFile file)
		{
			this.file = file;
		}

		public abstract string Name
		{
			get;
		}

		public int Size {
			get;
			protected set;
		}

		protected NitroFile File {
			get { return this.file; }
		}

		public void Read(Stream strIn)
		{
			BinaryReader br = new BinaryReader(strIn);

			if (new string(br.ReadChars(4).Reverse().ToArray()) != this.Name)
				throw new FormatException("Block name does not match");

			this.Size = br.ReadInt32();
			this.ReadData(strIn);

			br = null;
		}

		protected abstract void ReadData(Stream strIn);

		public void Write(Stream strOut)
		{
			BinaryWriter bw = new BinaryWriter(strOut);
			long startPos = strOut.Position;

			bw.Write(this.Name.Reverse().ToArray());
			bw.Write(this.Size);

			this.WriteData(strOut);

			strOut.Position = startPos + this.Size;
			if (strOut.Position > strOut.Length)
				strOut.Position = strOut.Length;

			while (strOut.Position % 0x04 != 0)
				strOut.WriteByte(0x00);
		}

		protected abstract void WriteData(Stream strOut);

		/// <summary>
		/// Checks the block content.
		/// </summary>
		public abstract bool Check();
	}
}

