""" -----------------------------------------------------------------------
<copyright file="VectorialFont.py" company="none">
Copyright (C) 2013

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by 
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful, 
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details. 

   You should have received a copy of the GNU General Public License
  along with this program.  If not, see "http://www.gnu.org/licenses/". 
</copyright>
<author>pleoNeX</author>
<email>benito356@gmail.com</email>
<date>06/09/2013</date>
----------------------------------------------------------------------- """
import fontforge

SQUARE_SIDE = 160
FONT_PATH   = "/usr/share/fonts/truetype/arial.ttf"
GLYPHS_PATH = "glyphs.txt"

def drawSquare(pen, pos, side, end):
  x1 = pos[0]
  x2 = pos[0] + side
  y1 = pos[1]
  y2 = pos[1] + side
  
  pen.moveTo( (x1, y1) )
  pen.lineTo( (x1, y2) )
  pen.lineTo( (x2, y2) )
  pen.lineTo( (x2, y1) )
  if end:
    pen.closePath()
  else:
    pen.endPath()

def drawGlyph(glyph, pen, side):
  x = 0
  y = 0
  
  # Reverse the glyph to draw from up to bottom
  glyph = glyph.splitlines()
  glyph.reverse()
  glyph = "\n".join(glyph)
  
  for c in glyph:
    if c == '\n':
      y += side
      x =  0
    else:
      if c == '*':
	drawSquare(pen, (x, y), side, False)
      x += side

if __name__ == '__main__':
  # Open font
  font = fontforge.open(FONT_PATH)

  ## Draw glyphs
  # Open & read glyph file
  f = open(GLYPHS_PATH, 'r')
  data = f.read().split('---\n')
  f.close()
  
  # For each glyph get encoding and data
  for g in data:
    lines = g.splitlines()
    encoding = int(lines[0], 16)
    glyph = "\n".join(lines[1:])

    pen = font[encoding].glyphPen()
    drawGlyph(glyph, pen, SQUARE_SIDE)

  # Save font
  font.generate('output.otf')
  font.close()