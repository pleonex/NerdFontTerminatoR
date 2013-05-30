// -----------------------------------------------------------------------
// <copyright file="Glyph.cs" company="none">
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
// </copyright>
// <author>pleoNeX</author>
// <email>benito356@gmail.com</email>
// <date>30/05/2013</date>
// -----------------------------------------------------------------------

using System;
using System.Drawing;
using GWidth = Nftr.Structure.Cwdh.GlyphWidth;

namespace Nftr
{
	public struct Glyph
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

		public int IdMap {
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

