﻿// ----------------------------------------------------------------------
// <copyright file="Font.cs" company="none">
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
//
// </copyright>
// <author>pleoNeX</author>
// <email>benito356@gmail.com</email>
// <date>01/05/2013 2:45:24</date>
// -----------------------------------------------------------------------
namespace Nftr
{
    using System;
    using System.Collections.Generic;
	using System.Drawing;
    using System.IO;
    using System.Linq;
	using System.Text;
	using System.Xml.Linq;
	using Nftr.Structure;
	using GWidth = Nftr.Structure.Cwdh.GlyphWidth;
	
    public class NftrFont : NitroFile
    {
		private const int CharsPerLine = 16;
		private const int BorderWidth = 2;
		private static readonly ushort[] Versions = { 0100, 0101, 0102 };   // 1.0, 1.1 and 1.2
		private static readonly Pen BorderPen = new Pen(Color.Olive, BorderWidth);

        private List<Glyph>  glyphs;
        private byte         depth;
        private ushort       errorChar;
        private RotationMode rotation; 
        private EncodingMode encoding;

        // Size variables
        private byte   lineGap;
        private byte   boxWidth;
        private byte   boxHeight;
        private byte   glyphWidth;
        private byte   glyphHeight;
        private GWidth defaultWidth;


        public NftrFont(string nftrPath)
			: base(nftrPath, typeof(Finf), typeof(Cglp), typeof(Cwdh), typeof(Cmap))
        {
        }

        public NftrFont(string xmlInfo, string glyphs)
			: base(typeof(Finf), typeof(Cglp), typeof(Cwdh), typeof(Cmap))
        {
			this.Import(xmlInfo, glyphs);
        }

        public static uint Header
        {
            get { return 0x4E465452; }
        }

		protected override bool IsSupported(ushort version)
		{
			return Versions.Contains(version);
		}

		protected override void Read(Stream strIn, int size)
		{
			base.Read(strIn, size);
		
			this.errorChar    = this.Blocks.GetByType<Finf>(0).ErrorCharIndex;
			this.encoding     = this.Blocks.GetByType<Finf>(0).Encoding;
			this.lineGap      = this.Blocks.GetByType<Finf>(0).LineGap;
			this.defaultWidth = this.Blocks.GetByType<Finf>(0).DefaultWidth;
			this.glyphHeight  = this.Blocks.GetByType<Cglp>(0).GlyphHeight;
			this.glyphWidth   = this.Blocks.GetByType<Cglp>(0).GlyphWidth;
			this.boxWidth     = this.Blocks.GetByType<Cglp>(0).BoxWidth;
			this.boxHeight    = this.Blocks.GetByType<Cglp>(0).BoxHeight;
			this.depth        = this.Blocks.GetByType<Cglp>(0).Depth;
			this.rotation     = (RotationMode)this.Blocks.GetByType<Cglp>(0).Rotation;

			// Get glyphs info
			this.glyphs = new List<Glyph>();
			for (int i = 0; i < this.Blocks.GetByType<Cglp>(0).NumGlyphs; i++) {
				ushort charCode = this.SearchCharByImage(i);

				Glyph g = new Glyph();
				g.Id       = i;
				g.Image    = this.Blocks.GetByType<Cglp>(0).GetGlyph(i);
				g.Width    = this.Blocks.GetByType<Cwdh>(0).GetWidth(charCode);
				g.CharCode = charCode;

				this.glyphs.Add(g);
			}
		}

		public void PrintInfo()
		{
			Console.WriteLine("Version:       {0}", this.VersionS);
			Console.WriteLine("Error char:    {0}", this.errorChar);
			Console.WriteLine("Encoding:      {0}", this.encoding);
			Console.WriteLine("Line gap:      {0}", this.lineGap);
			Console.WriteLine("Default width: {0}", this.defaultWidth);
			Console.WriteLine("Glyph width:   {0}", this.glyphWidth);
			Console.WriteLine("Glyph height:  {0}", this.glyphHeight);
			Console.WriteLine("Box width:     {0}", this.boxWidth);
			Console.WriteLine("Box height:    {0}", this.boxHeight);
			Console.WriteLine("Depth:         {0}", this.depth);
			Console.WriteLine("Rotation:      {0}", this.rotation);
			Console.WriteLine("Chars read:    {0}", this.glyphs.Count);
		}

		public void Export(string xmlPath, string imgPath)
		{
			this.ExportInfo(xmlPath);
			this.ExportMap(imgPath);
		}

		public void ExportInfo(string xmlPath)
		{
			XDocument doc = new XDocument();
			doc.Declaration = new XDeclaration("1.0", "utf-8", "yes");

			XElement root = new XElement("NFTR");
			doc.Add(root);

			// Export general info
			root.Add(new XElement("Version", this.VersionS));
			root.Add(new XElement("LineGap", this.lineGap));
			root.Add(new XElement("BoxWidth", this.boxWidth));
			root.Add(new XElement("BoxHeight", this.boxHeight));
			root.Add(new XElement("GlyphWidth", this.glyphWidth));
			root.Add(new XElement("GlyphHeight", this.glyphHeight));
			root.Add(this.defaultWidth.Export("DefaultWidth"));
			root.Add(new XElement("ErrorChar", this.errorChar));
			root.Add(new XElement("Depth", this.depth));
			root.Add(new XElement("Rotation", this.rotation));
			root.Add(new XElement("Encoding", this.encoding));

			// Export glyph info
			XElement cmapRoot = new XElement("CharacterMaps");
			root.Add(cmapRoot);

			foreach (Cmap cmap in this.Blocks.GetByType<Cmap>()) {
				XElement xcmap = new XElement("Cmap");
				cmapRoot.Add(xcmap);

				xcmap.Add(new XElement("FirstChar", cmap.FirstChar.ToString("X")));
				xcmap.Add(new XComment(" (" + this.GetChar(cmap.FirstChar) + ") "));
				xcmap.Add(new XElement("LastChar", cmap.LastChar.ToString("X")));
				xcmap.Add(new XComment(" (" + this.GetChar(cmap.LastChar) + ") "));
				xcmap.Add(new XElement("Type", cmap.Type));

				XElement map = new XElement("Map");
				xcmap.Add(map);
				switch (cmap.Type) {
				case 0:
					map.Add(new XElement("FirstImage", cmap.Map[0, 1]));
					StringBuilder mapComment = new StringBuilder();
					for (int i = 0; i < cmap.Map.GetLength(0); i++) {
						if (i != 0 && i % 5 == 0)
							mapComment.AppendLine();
						mapComment.AppendFormat(" {0}:{1:X} ({2})  ",
						                  cmap.Map[i, 1], cmap.Map[i, 0], this.GetChar(cmap.Map[i, 0]));
					}
					map.Add(new XComment(mapComment.ToString()));
					break;

				case 1:
					for (int i = 0; i < cmap.Map.GetLength(0); i++) {
						XElement ximg = new XElement("Image", cmap.Map[i, 1]);
						map.Add(new XComment(string.Format(
							" {0:X} ({1}) ",
						    cmap.Map[i, 0],
							this.GetChar(cmap.Map[i, 0]))));
						map.Add(ximg);
					}
					break;

				case 2:
					for (int i = 0; i < cmap.Map.GetLength(0); i++) {
						XElement xchar = new XElement("Entry");
						xchar.Add(new XElement("Image", cmap.Map[i, 1]));
						xchar.Add(new XElement("Char", cmap.Map[i, 0].ToString("X")));
						xchar.Add(new XComment(" (" + this.GetChar(cmap.Map[i, 0]) + ") "));
					}
					break;
				}
			}

			// Export widths
			XElement widthsRoot = new XElement("CharacterWidths");
			root.Add(widthsRoot);

			foreach (Glyph g in this.glyphs) {
				XElement xwidth = g.Width.Export("CWidth");
				xwidth.SetAttributeValue("Id", g.Id);
				xwidth.Add(new XComment(string.Format(
					" {0:X} ({1}) ", 
					g.CharCode,
					this.GetChar(g.CharCode))));
				widthsRoot.Add(xwidth);
			}

			if (File.Exists(xmlPath))
				File.Delete(xmlPath);
			doc.Save(xmlPath);
		}

		public void ExportMap(string imgPath)
		{
			int numChars = this.glyphs.Count;

			// Get the image size
			int numColumns = (numChars < CharsPerLine) ? numChars : CharsPerLine;
			int numRows = (int)Math.Ceiling((double)numChars / numColumns);

			int charWidth = this.boxWidth + BorderWidth;
			int charHeight = this.boxHeight + BorderWidth;

			this.ExportMap(imgPath, charWidth, charHeight, numRows, numColumns, 1);
		}

		public void ExportMap(string imgPath, int charWidth, int charHeight, 
		                      int numRows, int numColumns, int zoom)
		{
			int numChars = this.glyphs.Count;
			int width = numColumns * charWidth + BorderWidth;
			int height = numRows * charHeight + BorderWidth;

			Bitmap image = new Bitmap(width, height);
			Graphics graphic = Graphics.FromImage(image);

			// Draw chars
			for (int i = 0; i < numRows; i++)
			{
				for (int j = 0; j < numColumns; j++)
				{
					int index = i * numColumns + j;
					if (index >= numChars)
						break;

					int x = j * charWidth + BorderWidth;
					int y = i * charHeight + BorderWidth;

					int align = BorderWidth - (BorderWidth / 2);
					graphic.DrawRectangle(BorderPen, x - align, y - align, charWidth, charHeight);
					graphic.DrawImage(this.glyphs[index].ToImage(zoom), x, y);
				}
			}

			graphic.Dispose();
			graphic = null;

			if (File.Exists(imgPath))
				File.Delete(imgPath);
			image.Save(imgPath);
		}

		private void Import(string xmlInfo, string glyphs)
		{
			XDocument doc = XDocument.Load(xmlInfo);
			XElement root = doc.Element("NFTR");

			this.VersionS = root.Element("Version").Value;

			this.lineGap = byte.Parse(root.Element("LineGap").Value);
			this.errorChar = ushort.Parse(root.Element("ErrorChar").Value);
			this.defaultWidth = GWidth.FromXml(root.Element("DefaultWidth"));
			this.boxWidth = byte.Parse(root.Element("BoxWidth").Value);
			this.boxHeight = byte.Parse(root.Element("BoxHeight").Value);
			this.glyphWidth = byte.Parse(root.Element("GlyphWidth").Value);
			this.glyphHeight = byte.Parse(root.Element("GlyphHeight").Value);
			this.rotation = (RotationMode)Enum.Parse(typeof(RotationMode), root.Element("Rotation").Value);
			this.depth = byte.Parse(root.Element("Depth").Value);

			// Get Glyphs

			throw new NotImplementedException();
		}

		private void ImportMap()
		{
			throw new NotImplementedException();
//			int numChars = font.plgc.tiles.Length;
//
//			// Get the image size
//			int numColumns = (numChars < CHARS_PER_LINE) ? numChars : CHARS_PER_LINE;
//			int numRows = (int)Math.Ceiling((double)numChars / numColumns);
//
//			int charWidth = font.plgc.tile_width + BORDER_WIDTH;
//			int charHeight = font.plgc.tile_height + BORDER_WIDTH;
//
//			int width = numColumns * charWidth + BORDER_WIDTH;
//			int height = numRows * charHeight + BORDER_WIDTH;
//
//			if (width != image.Width || height != image.Height)
//			{
//				System.Windows.Forms.MessageBox.Show("Incorrect size.");
//				return;
//			}
//
//			// Draw chars
//			for (int i = 0; i < numRows; i++)
//			{
//				for (int j = 0; j < numColumns; j++)
//				{
//					int index = i * numColumns + j;
//					if (index >= numChars)
//						break;
//
//					int x = j * charWidth + BORDER_WIDTH;
//					int y = i * charHeight + BORDER_WIDTH;
//
//					Bitmap charImg = image.Clone(new Rectangle(x, y, charWidth - BORDER_WIDTH, charHeight - BORDER_WIDTH),
//					                             System.Drawing.Imaging.PixelFormat.Format32bppArgb);
//
//					font.plgc.tiles[index] = SetChar(charImg, font.plgc.depth, palette);
//				}
//			}
		}

		private void CreateStructure()
		{
			Cglp cglp = new Cglp(
				this,
				null, // TODO
				this.boxWidth,
				this.boxHeight,
				this.glyphWidth,
				this.glyphHeight,
				this.rotation,
				this.depth);
			if (!cglp.Check())
				throw new InvalidDataException("Invalid data for CGLP.");
			this.Blocks.Add(cglp);

			// CWDH
			Cwdh cwdh = null;
			if (!cwdh.Check())
				throw new InvalidDataException("Invalid data for CWDH.");
			this.Blocks.Add(cwdh);

			// CMAP
			Cmap cmap = null;
			if (!cmap.Check())
				throw new InvalidDataException("Invalid data for CMAP.");
			this.Blocks.Add(cmap);

			// FINF
			Finf finf = null;
			if (!finf.Check())
				throw new InvalidDataException("Invalid data for FINF.");
			this.Blocks.Insert(0, finf);

			throw new NotImplementedException();
		}

		/// <summary>
		/// Its checks if the approximations done in the exporting are rights.
		/// </summary>
		public bool Check()
		{
			bool result = true;

			Finf finf = this.Blocks.GetByType<Finf>(0);
			Cglp cglp = this.Blocks.GetByType<Cglp>(0);

			if (finf.Unknown != 0)
				result = false;

//			if (this.VersionS == "1.2") {
//				if (finf.BearingX != finf.DefaultWidth.BearingX)
//					result = false;
//
//				if (finf.BearingY != cglp.GlyphHeight)
//					result = false;
//
//				if (finf.GlyphWidth != cglp.GlyphWidth)
//					result = false;
//
//				if (finf.GlyphHeight != finf.LineGap)
//					result = false;
//			}

			return result;
		}

		private string GetChar(int charCode)
		{
			byte[] charCodeB = BitConverter.GetBytes((ushort)charCode);
			if (this.Blocks.GetByType<Finf>(0).Encoding == EncodingMode.SJIS &&
				charCodeB[1] != 0x00) {
				charCodeB = charCodeB.Reverse().ToArray();
			}

			char ch = this.Blocks.GetByType<Finf>(0).TextEncoding.GetChars(charCodeB)[0];

			if (Char.IsLetterOrDigit(ch) || Char.IsPunctuation(ch) ||
			    Char.IsSeparator(ch) || Char.IsSymbol(ch))
				return ch.ToString();
			else
				return "";
		}

		private ushort SearchCharByImage(int index)
		{
			foreach (Cmap mapBlock in this.Blocks.GetByType<Cmap>()) 
			{
				int charCode = mapBlock.GetCharCode(index);
				if (charCode != -1)
					return (ushort)charCode;
			}

			return 0;
		}

        private struct Glyph
        {
            public int Id {
				get;
				set;
			}

            public Colour[,] Image {
				get;
				set;
			}

            public GWidth Width {
				get;
				set;
			}

			public ushort CharCode {
				get;
				set;
			}

			public Bitmap ToImage(int zoom)
			{
				Bitmap bmp = new Bitmap(this.Image.GetLength(0) * zoom + 1,
				                        this.Image.GetLength(1) * zoom + 1);

				for (int w = 0; w < this.Image.GetLength(0); w++) {
					for (int h = 0; h < this.Image.GetLength(1); h++) {
						for (int hzoom = 0; hzoom < zoom; hzoom++) {
							for (int wzoom = 0; wzoom < zoom; wzoom++) {
								bmp.SetPixel(
									w * zoom + wzoom,
									h * zoom + hzoom,
									this.Image[w, h].ToColor());
							}
						}
					}
				}

				return bmp;
			}
        }
    }
}
