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
		private const int BorderThickness = 2;
		private static readonly ushort[] Versions = { 0100, 0101, 0102 };   // 1.0, 1.1 and 1.2
		private static readonly Pen BorderPen = new Pen(Color.Olive, BorderThickness);

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
				if (idMap == -1) {
					this.cglp.GetGlyphImage(i).Save("Unvalid char " + i.ToString() + ".png");
					continue;
				}

				Glyph g = new Glyph();
				g.Id       = i;
				g.Image    = this.cglp.GetGlyph(i);
				g.Width    = this.cwdh.GetWidth(charCode, g.Id);
				g.CharCode = charCode;
				g.IdMap    = idMap;
			
				this.glyphs.Add(g);
			}

			this.PrintInfo();
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

			XElement xcmap = new XElement("Maps");						// Remove in basic export mode
			root.Add(xcmap);
			foreach (Cmap cmap in this.cmaps) {
				XElement xmap = new XElement("Map");
				xcmap.Add(xmap);

				xmap.Add(new XElement("Id", cmap.Id));
				xmap.Add(new XElement("FirstChar", cmap.FirstChar.ToString("X")));
				xmap.Add(new XElement("LastChar", cmap.LastChar.ToString("X")));
				xmap.Add(new XElement("Type", cmap.Type));
			}

			XElement xcwdh = new XElement("Widths");					// Remove in basic export mode
			root.Add(xcwdh);
			Cwdh.WidthRegion wr = this.cwdh.FirstRegion;
			while (wr != null) {
				XElement xwidthreg = new XElement("Region");
				xcwdh.Add(xwidthreg);

				xwidthreg.Add(new XElement("Id", wr.Id));
				xwidthreg.Add(new XElement("FirstChar", wr.FirstChar.ToString("X")));
				xwidthreg.Add(new XElement("LastChar", wr.LastChar.ToString("X")));

				wr = wr.NextRegion;
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

		/// <summary>
		/// Export all glyphs in a image using default settings.
		/// </summary>
		/// <param name="imgPath">Image path to draw glyphs.</param>
		public void ExportMap(string imgPath)
		{
			int numChars = this.glyphs.Count;
	
			// Gets the number of columns and rows from the CharsPerLine value.
			int numColumns = (numChars < CharsPerLine) ? numChars : CharsPerLine;
			int numRows    = (int)Math.Ceiling((double)numChars / numColumns);

			this.ExportMap(
				imgPath,
				this.boxWidth, this.boxHeight, numRows, numColumns,
				1, BorderPen);
		}

		/// <summary>
		/// Exports all glyphs in a image.
		/// </summary>
		/// <param name="imgPath">Image path to draw glyphs.</param>
		/// <param name="charWidth">Char width.</param>
		/// <param name="charHeight">Char height.</param>
		/// <param name="numRows">Number of rows.</param>
		/// <param name="numColumns">Number of columns.</param>
		/// <param name="zoom">Zoom.</param>
		/// <param name="borderPen">Pen used to draw the border.</param>
		public void ExportMap(string imgPath, int charWidth, int charHeight, int numRows, int numColumns,
		                      int zoom, Pen borderPen)
		{
			if (zoom != 1)
				throw new NotImplementedException();

			int numChars = this.glyphs.Count;
			int borderThickness = (int)borderPen.Width;

			// Char width + border from one side + border from the other side only at the end
			int width    = numColumns * charWidth  + (numColumns + 1) * borderThickness;
			int height   = numRows    * charHeight + (numRows + 1 )   * borderThickness;

			Bitmap image = new Bitmap(width, height);
			Graphics graphic = Graphics.FromImage(image);

			// Draw chars
			for (int r = 0; r < numRows; r++) {
				for (int c = 0; c < numColumns; c++) {

					int index = r * numColumns + c;	// Index of the glyph
					if (index >= numChars)
						break;

					// Gets coordinates
					int x = c * (charWidth  + borderThickness);
					int y = r * (charHeight + borderThickness);

					// Alignment due to rectangle drawing method.
					int borderAlign = (int)System.Math.Floor(borderThickness / 2.0) + (1 - (borderThickness % 2));

					if (borderThickness > 0) {
						graphic.DrawRectangle(
							borderPen,
						    x + borderAlign,
						    y + borderAlign,
							charWidth  + borderThickness,
							charHeight + borderThickness);
					}

					graphic.DrawImage(this.glyphs[index].ToImage(zoom), x + borderThickness, y + borderThickness);
				}
			}

			graphic.Dispose();

			if (File.Exists(imgPath))
				File.Delete(imgPath);
			image.Save(imgPath, System.Drawing.Imaging.ImageFormat.Png);
		}

		private void Import(string xmlInfo, string glyphs)
		{
			Bitmap image = (Bitmap)Image.FromFile(glyphs);
			XDocument doc = XDocument.Load(xmlInfo);
			XElement root = doc.Element("NFTR");

			this.MagicStamp = "NFTR";
			this.VersionS   = root.Element("Version").Value;

			this.errorChar    = ushort.Parse(root.Element("ErrorChar").Value);
			this.lineGap      = byte.Parse(root.Element("LineGap").Value);
			this.boxWidth     = byte.Parse(root.Element("BoxWidth").Value);
			this.boxHeight    = byte.Parse(root.Element("BoxHeight").Value);
			this.glyphWidth   = byte.Parse(root.Element("GlyphWidth").Value);
			this.glyphHeight  = byte.Parse(root.Element("GlyphHeight").Value);
			this.depth        = byte.Parse(root.Element("Depth").Value);
			this.defaultWidth = GWidth.FromXml(root.Element("DefaultWidth"));
			this.rotation     = (RotationMode)Enum.Parse(typeof(RotationMode), root.Element("Rotation").Value);
			this.encoding     = (EncodingMode)Enum.Parse(typeof(EncodingMode), root.Element("Encoding").Value);

			// Gets Width regions
			XElement xwidths = root.Element("Widths");
			Cwdh.WidthRegion[] widthRegs = new Cwdh.WidthRegion[xwidths.Elements("Region").Count()];

				// ... gets the data from the xml
			foreach (XElement xreg in xwidths.Elements("Region")) {
				int id = int.Parse(xreg.Element("Id").Value);

				widthRegs[id] = new Cwdh.WidthRegion(this.defaultWidth);
				widthRegs[id].Id = id;
				widthRegs[id].FirstChar = Convert.ToUInt16(xreg.Element("FirstChar").Value, 16);
				widthRegs[id].LastChar  = Convert.ToUInt16(xreg.Element("LastChar").Value,  16);
			}

				// ... assign the next region
			for (int i = 0; i < widthRegs.Length; i++) {
				if (i + 1 == widthRegs.Length)
					widthRegs[i].NextRegion = null;
				else
					widthRegs[i].NextRegion = widthRegs[i + 1];
			}

			// Gets Cmap regions
			XElement xcmaps = root.Element("Maps");
			this.cmaps = new Cmap[xcmaps.Elements("Map").Count()];

			foreach (XElement xmap in xcmaps.Elements("Map")) {
				int id = int.Parse(xmap.Element("Id").Value);

				this.cmaps[id] = new Cmap(this);
				this.cmaps[id].Id = id;
				this.cmaps[id].Type      = Convert.ToUInt32(xmap.Element("Type").Value);
				this.cmaps[id].FirstChar = Convert.ToUInt16(xmap.Element("FirstChar").Value, 16);
				this.cmaps[id].LastChar  = Convert.ToUInt16(xmap.Element("LastChar").Value,  16);
			}

			// Gets Glyphs
			XElement xglyphs = root.Element("Glyphs");
			this.glyphs = new List<Glyph>(xglyphs.Elements("Glyph").Count());
			for (int i = 0; i < xglyphs.Elements("Glyph").Count(); i++)
				this.glyphs.Add(new Glyph());

			// Reversing the glyphs, there are more probability to have high char codes first
			// ... so there will be less array resizing in InsertWidth and InsertChar
			foreach (XElement xglyph in xglyphs.Elements("Glyph").Reverse()) {
				int id = int.Parse(xglyph.Element("Id").Value);

				Glyph g = new Glyph();
				g.Id       = id;
				g.Width    = GWidth.FromXml(xglyph.Element("Width"));
				g.Image    = this.CharFromMap(image, id);
				g.IdMap    = int.Parse(xglyph.Element("IdMap").Value);
				g.CharCode = Convert.ToUInt16(xglyph.Element("Code").Value, 16);

				this.glyphs[id] = g;
				if (g.Width.IdRegion >= 0)
					widthRegs[g.Width.IdRegion].InsertWidth(g.CharCode, g.Width, g.Id);
				this.cmaps[g.IdMap].InsertCharCode(g.Id, g.CharCode);
			}

			this.CreateStructure(widthRegs[0]);
		}

		/// <summary>
		/// Gets a glyph from an image using default settings.
		/// </summary>
		/// <returns>The glyph</returns>
		/// <param name="img">Image with all glyphs drawn</param>
		/// <param name="glyphIdx">Glyph index.</param>
		private Colour[,] CharFromMap(Bitmap img, int glyphIdx)
		{
			int numChars = this.glyphs.Capacity;

			// Get the number of columns and rows
			int numColumns = (numChars < CharsPerLine) ? numChars : CharsPerLine;
			int numRows = (int)Math.Ceiling((double)numChars / numColumns);

			return this.CharFromMap(
				img, glyphIdx,
			    this.boxWidth, this.boxHeight, numRows, numColumns,
				1, BorderThickness);
		}

		/// <summary>
		/// Gets a glyph from an image.
		/// </summary>
		/// <returns>The glyph</returns>
		/// <param name="img">Image with all glyphs drawn</param>
		/// <param name="glyphIdx">Glyph index.</param>
		/// <param name="charWidth">Char width.</param>
		/// <param name="charHeight">Char height.</param>
		/// <param name="numRows">Number of rows.</param>
		/// <param name="numColumns">Number of columns.</param>
		/// <param name="zoom">Zoom.</param>
		/// <param name="borderThickness">Border thickness.</param>
		private Colour[,] CharFromMap(Bitmap img, int glyphIdx, int charWidth, int charHeight, int numRows, int numColumns,
		                              int zoom, int borderThickness)
		{
			if (zoom != 1)
				throw new NotImplementedException();

			int width  = numColumns * charWidth  + (numColumns + 1) * borderThickness;
			int height = numRows    * charHeight + (numRows + 1)    * borderThickness;
			
			if (width != img.Width || height != img.Height) {
				throw new ArgumentException("Incorrect image size.");
			}

			Colour[,] glyph = new Colour[charWidth, charHeight];

			int column = glyphIdx % numColumns;
			int row    = glyphIdx / numColumns;

			int startX = column * charWidth  + (column + 1) * borderThickness;
			int startY = row    * charHeight + (row + 1)    * borderThickness;
			for (int x = startX, gx = 0; x < startX + charWidth; x += zoom, gx++) {
				for (int y = startY, gy = 0; y < startY + charHeight; y += zoom, gy++) {
					glyph[gx, gy] = Colour.FromColor(img.GetPixel(x, y));
				}
			}
			return glyph;
		}

		private void CreateStructure(Cwdh.WidthRegion firstReg)
		{
			// CGLP
			// ... gets the glyphs in a array
			Colour[][,] glyphs = new Colour[this.glyphs.Count][,];
			for (int i = 0; i < this.glyphs.Count; i++)
				glyphs[i] = this.glyphs[i].Image;

			this.cglp = new Cglp(this, glyphs, this.boxWidth, this.boxHeight,
			                     this.glyphWidth, this.glyphHeight, this.depth, this.rotation);
			//if (!cglp.Check())
			//	throw new InvalidDataException("Invalid data for CGLP.");
			this.Blocks.Add(cglp);

			// CWDH
			this.cwdh = new Cwdh(this, firstReg);
			//if (!cwdh.Check())
			//	throw new InvalidDataException("Invalid data for CWDH.");
			this.Blocks.Add(cwdh);

			// CMAP
			//for (int i = 0; i < this.cmaps.Length; i++)
			//	if (!this.cmaps[i].Check())
			//		throw new InvalidDataException("Invalid data for CMAP (" + i.ToString() + ")");
			this.Blocks.AddRange(this.cmaps);

			// FINF
			this.finf = new Finf(this);
			this.finf.Unknown        = 0;
			this.finf.LineGap        = this.lineGap;
			this.finf.ErrorCharIndex = this.errorChar;
			this.finf.DefaultWidth   = this.defaultWidth;
			this.finf.Encoding       = this.encoding;
			this.finf.GlyphWidth     = this.glyphWidth;
			this.finf.GlyphHeight    = this.glyphHeight;
			this.finf.BearingX       = this.defaultWidth.BearingX;
			this.finf.BearingY       = this.lineGap;
			this.finf.UpdateOffsets();
			//if (!finf.Check())
			//	throw new InvalidDataException("Invalid data for FINF.");
			this.Blocks.Insert(0, finf);
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

			Console.WriteLine("WARNING: Charcode not found. Index: {0}", index);
			mapId = -1;
			return 0;
		}
    }
}
