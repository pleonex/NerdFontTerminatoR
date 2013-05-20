// ----------------------------------------------------------------------
// <copyright file="Colour.cs" company="none">
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
// <date>01/05/2013 3:14:01</date>
// -----------------------------------------------------------------------
namespace Nftr
{
    public struct Colour
    {
        public Colour(int r, int g, int b) : this()
        {
            this.Red = r;
            this.Green = g;
            this.Blue = b;
        }

        public int Red
        {
            get;
            private set;
        }

        public int Green
        {
            get;
            private set;
        }

        public int Blue
        {
            get;
            private set;
        }

		public override bool Equals(object obj)
		{
			if (obj == null)
				return false;
			if (ReferenceEquals(this, obj))
				return true;
			if (obj.GetType() != typeof(Colour))
				return false;
			Colour other = (Colour)obj;
			return (this.Red == other.Red) && (this.Green == other.Green) &&
				(this.Blue == other.Blue);
		}

		public override int GetHashCode()
		{
			unchecked {
				return this.Red.GetHashCode() ^ this.Green.GetHashCode() ^ 
					this.Blue.GetHashCode();
			}
		}

		public System.Drawing.Color ToColor()
		{
			return System.Drawing.Color.FromArgb(255, this.Red, this.Green, this.Blue);
		}
    }
}
