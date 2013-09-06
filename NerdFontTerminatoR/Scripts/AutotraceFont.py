""" -----------------------------------------------------------------------
<copyright file="AutotraceFont.py" company="none">
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
import re
import os

FONT_PATH   = '/usr/share/fonts/truetype/arial.ttf'
GLYPHS_PATH = '/home/benito/'

# Open font
font = fontforge.open(FONT_PATH)

# For each file, match with pattern uni*.png
for filename in os.listdir(GLYPHS_PATH):
  m = re.search(r'(?<=uni)(?P<encoding>\d+)(?=\.png)', filename)
  if m:
    encoding = int(m.group('encoding'), 16)
    font[encoding].clear()
    font[encoding].importOutlines(filename)	# Import image
    font[encoding].autoTrace()			# Convert to vectorial

# Save font
font.generate('output.otf')
font.close()