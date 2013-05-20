using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Nftr
{
	public abstract class NitroFile
	{
		private const ushort BomLittleEndiannes = 0xFEFF;	// Byte Order Mask

		private Type[] blockTypes;

		private string magicStamp;
		private ushort version;
		private BlockCollection blocks;

		protected NitroFile(params Type[] blockTypes)
		{
			// Check that all types heredites from NitroBlock
			foreach (Type t in blockTypes) {
				if (!t.IsSubclassOf(typeof(NitroBlock)))
					throw new ArgumentException("Invalid type passed.");
			}

			this.blockTypes = blockTypes;
		}

		protected NitroFile(string fileIn, params Type[] blockTypes)
			: this(blockTypes)
		{
			FileStream fs = new FileStream(fileIn, FileMode.Open, FileAccess.Read, FileShare.Read);

			this.Read(fs, (int)fs.Length);

			fs.Dispose();
			fs.Close();
		}

		protected virtual void Read(Stream strIn, int size)
		{
			long basePosition = strIn.Position;
			BinaryReader br = new BinaryReader(strIn);

			// Nitro header
			this.magicStamp = new string(br.ReadChars(4).Reverse().ToArray());
			if (br.ReadUInt16() != BomLittleEndiannes)	// Byte Order Mark
				throw new InvalidDataException("The data is not little endiannes.");
			this.version = br.ReadUInt16();
			if (br.ReadUInt32() != size)
				throw new FormatException("File size doesn't match.");
			ushort blocksStart = br.ReadUInt16();
			ushort numBlocks = br.ReadUInt16();

			strIn.Position = basePosition + blocksStart;
			this.blocks = new BlockCollection(numBlocks);
			for (int i = 0; i < numBlocks; i++)
			{
				long blockPosition = strIn.Position;

				// First get block parameters
				string blockName = new string(br.ReadChars(4).Reverse().ToArray());
				int blockSize = br.ReadInt32();
				strIn.Position = blockPosition;

				Type blockType = Array.Find<Type>(
					this.blockTypes, b => b.Name.ToLower() == blockName.ToLower());
				if (blockType == null)
				{
					throw new FormatException("Unknown block");
				}
				else
				{
					NitroBlock block = (NitroBlock)Activator.CreateInstance(blockType, this);
					block.Read(strIn);
					this.blocks.Add(block);
				}

				strIn.Position = blockPosition + blockSize;
			}
		}

		public void Write(string fileOut)
		{
			if (File.Exists(fileOut))
				File.Delete(fileOut);

			FileStream fs = new FileStream(fileOut, FileMode.CreateNew,
			                               FileAccess.Write, FileShare.Read);

			this.Write(fs);

			fs.Dispose();
			fs.Close();
		}

		public virtual void Write(Stream strOut)
		{
			const ushort blocksStart = 0x10;

			if (this.Blocks.Count > ushort.MaxValue)
				throw new Exception("Too many blocks.");

			long startPos = strOut.Position;
			BinaryWriter bw = new BinaryWriter(strOut);

			// Write header (need to be updated later)
			bw.Write(this.magicStamp.Reverse().ToArray());
			bw.Write(BomLittleEndiannes);
			bw.Write(this.Version);
			bw.Write(0x00);					// File size, unknown at the moment
			bw.Write(blocksStart);
			bw.Write((ushort)this.Blocks.Count);
			bw.Flush();

			while (strOut.Position < startPos + blocksStart)
				strOut.WriteByte(0x00);

			// Starts writing blocks
			foreach (NitroBlock block in this.blocks) {
				long blockPos = strOut.Position;
				block.Write(strOut);
				strOut.Flush();

				// Checks size
				if (strOut.Length < blockPos + block.Size)
					throw new InvalidDataException("Size does not match.");

				strOut.Position = blockPos + block.Size;
			}

			// Update file size
			uint fileSize = (uint)(strOut.Position - startPos);
			strOut.Position = startPos + 0x08;
			bw.Write(fileSize);
			bw.Flush();
		}

		public string MagicStamp {
			get { return this.magicStamp; }
		}

		public ushort Version {
			get { return this.version; }
		}

		public string VersionS {
			get { return (this.version >> 8).ToString() + "." + (this.version & 0xFF).ToString(); }
		}

		public BlockCollection Blocks {
			get { return blocks; }
		}

		protected abstract bool IsSupported(ushort version);
	}

	public class BlockCollection : List<NitroBlock>
	{
		public BlockCollection(int capacity)
			: base(capacity)
		{
		}

		public NitroBlock this[string name, int index] {
			get {
				return this.FindAll(b => b.Name == name)[index];
			}
		}

		public IEnumerable this[string name] {
			get {
				foreach (NitroBlock b in this.FindAll(b => b.Name == name)) {
					yield return b;
				}

				yield break;
			}
		}

		public T GetByType<T>(int index) where T : NitroBlock
		{
			return (T)this.FindAll(b => b is T)[index];
		}

		public IEnumerable<T> GetByType<T>() where T : NitroBlock
		{
			foreach (NitroBlock b in this.FindAll(b => b is T)) {
				yield return (T)b;
			}

			yield break;

		}
	}
}

