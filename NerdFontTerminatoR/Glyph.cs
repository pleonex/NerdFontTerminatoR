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

