// -----------------------------------------------------------------------
// <copyright file="Painter.cs" company="none">
// Copyright (C) 2016
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
// <date>1/3/2016</date>
// -----------------------------------------------------------------------
using System;
using System.Linq;
using System.Drawing;
using Nftr.Structure;

namespace Nftr
{
    public class Painter
    {
        private readonly NftrFont font;
        private readonly int baseLineGap;
        private readonly Glyph defaultChar;

        internal Painter(NftrFont font)
        {
            this.font = font;
            this.baseLineGap = font.Blocks.GetByType<Cglp>(0).BoxHeight;
            this.defaultChar = font.ErrorChar;
        }

        public void DrawString(string text, Graphics graphics, int x, int y,
            int? maxWidth = null, int extraGap = 0, int extraSpace = 0)
        {
            int baseX = x;

            int lineGap = baseLineGap + extraGap;
            foreach (char ch in text) {
                if (ch == '\r')
                    continue;

                // If a new line char, go to next line.
                if (ch == '\n') {
                    x = baseX;
                    y += lineGap;
                    continue;
                }

                // Get glyph, if not found, take the deafault char.
                var glyph = font.SearchGlyphByChar(ch) ?? defaultChar;

                // If we are over the max width, go to next line.
                if (maxWidth.HasValue && x + glyph.Width.Advance > maxWidth) {
                    x = baseX;
                    y += lineGap;
                }

                x += glyph.Width.BearingX;
                graphics.DrawImageUnscaled(glyph.ToImage(1, true), x, y);
                x += glyph.Width.Advance - glyph.Width.BearingX;
                x += extraSpace;
            }
        }

        public int GetStringLength(string text, int extraSpace = 0)
        {
            return text.Split('\n').Max(line =>
                line.Sum(ch => (font.SearchGlyphByChar(ch) ?? defaultChar).Width.Advance + extraSpace));
        }
    }
}

