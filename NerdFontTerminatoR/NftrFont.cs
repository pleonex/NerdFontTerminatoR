// ----------------------------------------------------------------------
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

		// Blocks
		private Finf finf;
		private Cglp cglp;
		private Cwdh cwdh;
		private Cmap[] cmaps;

		// Font data
        private List<Glyph>  glyphs;
        private byte         depth;
        private ushort       errorChar;
        private RotationMode rotation; 
        private EncodingMode encoding;

        // ... Size variables
        private byte   lineGap;
        private byte   boxWidth;
        private byte   boxHeight;
        private byte   glyphWidth;
        private byte   glyphHeight;
        private GWidth defaultWidth;


        public NftrFont(string nftrPath)
			: base(nftrPath, typeof(Finf), typeof(Cglp), typeof(Cwdh), typeof(Cmap))
        {
			Cmap.ResetCount();
        }

        public NftrFont(string xmlInfo, string glyphs)
			: base(typeof(Finf), typeof(Cglp), typeof(Cwdh), typeof(Cmap))
        {
			this.Import(xmlInfo, glyphs);
			Cmap.ResetCount();
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
		
			this.finf  = this.Blocks.GetByType<Finf>(0);
			this.cglp  = this.Blocks.GetByType<Cglp>(0);
			this.cwdh  = this.Blocks.GetByType<Cwdh>(0);
			this.cmaps = this.Blocks.GetByType<Cmap>().ToArray();

			this.errorChar    = this.finf.ErrorCharIndex;
			this.encoding     = this.finf.Encoding;
			this.lineGap      = this.finf.LineGap;
			this.defaultWidth = this.finf.DefaultWidth;
			this.glyphHeight  = this.cglp.GlyphHeight;
			this.glyphWidth   = this.cglp.GlyphWidth;
			this.boxWidth     = this.cglp.BoxWidth;
			this.boxHeight    = this.cglp.BoxHeight;
			this.depth        = this.cglp.Depth;
			this.rotation     = (RotationMode)this.cglp.Rotation;

			// Get glyphs info
			this.glyphs = new List<Glyph>();
			for (int i = 0; i < this.cglp.NumGlyphs; i++) {
				int idMap;
				ushort charCode = this.SearchCharByImage(i, out idMap);

				Glyph g = new Glyph();
				g.Id       = i;
				g.Image    = this.cglp.GetGlyph(i);
				g.Width    = this.cwdh.GetWidth(charCode);
				g.CharCode = charCode;
				g.IdMap    = idMap;
			
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
			doc.Add(new XComment(" Generated with NerdFontTerminatoR V0.1 ~~ by pleoNeX "));

			XElement root = new XElement("NFTR");
			doc.Add(root);

			// Export general info
			root.Add(new XElement("Version", this.VersionS));
			root.Add(new XElement("LineGap", this.lineGap));			// Remove in basic export mode
			root.Add(new XElement("BoxWidth", this.boxWidth));			// Remove in basic export mode
			root.Add(new XElement("BoxHeight", this.boxHeight));		// Remove in basic export mode
			root.Add(new XElement("GlyphWidth", this.glyphWidth));		// Remove in basic export mode
			root.Add(new XElement("GlyphHeight", this.glyphHeight));	// Remove in basic export mode
			root.Add(this.defaultWidth.Export("DefaultWidth"));			// Remove in basic export mode
			root.Add(new XElement("ErrorChar", this.errorChar));		
			root.Add(new XElement("Depth", this.depth));				// Remove in basic export mode
			root.Add(new XElement("Rotation", this.rotation));
			root.Add(new XElement("Encoding", this.encoding));

			XElement xcmap = new XElement("Maps");
			root.Add(xcmap);

			foreach (Cmap cmap in this.cmaps) {
				XElement xmap = new XElement("Map");
				xcmap.Add(xmap);

				xmap.Add(new XElement("Id", cmap.Id));
				xmap.Add(new XElement("FirstChar", cmap.FirstChar.ToString("X")));
				xmap.Add(new XElement("LastChar", cmap.LastChar.ToString("X")));
				xmap.Add(new XElement("Type", cmap.Type));
			}

			XElement glyphs = new XElement("Glyphs");
			root.Add(glyphs);
			foreach (Glyph g in this.glyphs) {
				XElement xg = new XElement("Glyph");
				glyphs.Add(xg);

				xg.Add(new XComment(string.Format(" ({0}) ", this.finf.GetChar(g.CharCode))));
				xg.Add(new XElement("Id", g.Id));
				xg.Add(g.Width.Export("Width"));						// Remove in basic export mode
				xg.Add(new XElement("Code", g.CharCode.ToString("X")));	// Remove in basic export mode
				xg.Add(new XElement("IdMap", g.IdMap));					// Remove in basic export mode
			}

			if (File.Exists(xmlPath)) {
				File.Delete(xmlPath);
			}

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
			if (zoom != 1)
				throw new NotImplementedException();

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

			this.lineGap      = byte.Parse(root.Element("LineGap").Value);
			this.errorChar    = ushort.Parse(root.Element("ErrorChar").Value);
			this.defaultWidth = GWidth.FromXml(root.Element("DefaultWidth"));
			this.boxWidth     = byte.Parse(root.Element("BoxWidth").Value);
			this.boxHeight    = byte.Parse(root.Element("BoxHeight").Value);
			this.glyphWidth   = byte.Parse(root.Element("GlyphWidth").Value);
			this.glyphHeight  = byte.Parse(root.Element("GlyphHeight").Value);
			this.rotation     = (RotationMode)Enum.Parse(typeof(RotationMode), root.Element("Rotation").Value);
			this.depth        = byte.Parse(root.Element("Depth").Value);

			// TODO: Get Glyphs


			this.CreateStructure(doc);
			throw new NotImplementedException();
		}

		private Colour[,] CharFromMap(Bitmap img, int glyphIdx)
		{
			int numChars = this.glyphs.Count;

			// Get the image size
			int numColumns = (numChars < CharsPerLine) ? numChars : CharsPerLine;
			int numRows = (int)Math.Ceiling((double)numChars / numColumns);

			int charWidth = this.boxWidth + BorderWidth;
			int charHeight = this.boxHeight + BorderWidth;

			return this.CharFromMap(img, glyphIdx, charWidth, charHeight, numRows, numColumns, 1);
		}

		private Colour[,] CharFromMap(Bitmap img, int glyphIdx, int charWidth, int charHeight,
		                              int numRows, int numColumns, int zoom)
		{
			if (zoom != 1)
				throw new NotImplementedException();

			int width = numColumns * charWidth + BorderWidth;
			int height = numRows * charHeight + BorderWidth;
			
			if (width != img.Width || height != img.Height)
			{
				throw new FormatException("Incorrect size.");
			}

			Colour[,] glyph = new Colour[this.boxWidth, this.boxHeight];
			int startX = (glyphIdx % numRows) * charWidth + BorderWidth;
			int startY = (glyphIdx / numRows) * charHeight + BorderWidth;
			for (int x = startX, gx = 0; x < startX + charWidth; x += zoom, gx++) {
				for (int y = startY, gy = 0; y < startY + charHeight; y += zoom, gy++) {
					glyph[gx, gy] = Colour.FromColor(img.GetPixel(x, y));
				}
			}
			return glyph;
		}

		private void CreateStructure(XDocument xml)
		{
			// CGLP
			Colour[][,] glyphs = new Colour[this.glyphs.Count][,];
			for (int i = 0; i < this.glyphs.Count; i++)
				glyphs[i] = this.glyphs[i].Image;

			this.cglp = new Cglp(this, glyphs, this.boxWidth, this.boxHeight,
			                     this.glyphWidth, this.glyphHeight, this.rotation, this.depth);
			if (!cglp.Check())
				throw new InvalidDataException("Invalid data for CGLP.");
			this.Blocks.Add(cglp);

			// CWDH
			this.cwdh = null;
			if (!cwdh.Check())
				throw new InvalidDataException("Invalid data for CWDH.");
			this.Blocks.Add(cwdh);

			// CMAP
			foreach (XElement node in xml.Element("NFTR").Elements("Cmap")) {
				Cmap cmap = new Cmap(this, node);
				if (!cmap.Check())
					throw new InvalidDataException("Invalid data for CMAP.");
				this.Blocks.Add(cmap);
			}
			this.cmaps = this.Blocks.GetByType<Cmap>().ToArray();

			// FINF
			this.finf = null;
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
			//Cglp cglp = this.Blocks.GetByType<Cglp>(0);

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

		private ushort SearchCharByImage(int index)
		{
			int fool;
			return this.SearchCharByImage(index, out fool);
		}

		private ushort SearchCharByImage(int index, out int mapId)
		{
			foreach (Cmap mapBlock in this.cmaps) 
			{
				int charCode = mapBlock.GetCharCode(index);
				if (charCode != -1) {
					mapId = mapBlock.Id;
					return (ushort)charCode;
				}
			}

			mapId = -1;
			return 0;
		}
    }
}
