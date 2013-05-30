// -----------------------------------------------------------------------
// <copyright file="Analyzer.cs" company="none">
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Nftr
{
    public static class Analyzer
    {

        public static bool CheckFontHeader(string folderPath)
        {
            bool result = true;

            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            foreach (string fontPath in files)
            {
                FileStream fs = new FileStream(fontPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader br = new BinaryReader(fs);

                if (br.ReadUInt32() != NftrFont.Header || !FontVersionSupported(br.ReadUInt32()) ||
                    br.ReadUInt32() != fs.Length || br.ReadUInt16() != 0x10)
                {
                    result = false;
                }

                fs.Close();
                fs.Dispose();
                fs = null;
                br = null;
            }

            return result;
        }

        public static bool FontVersionSupported(uint version)
        {
            uint[] versions = new uint[] { 0x0100FEFF, 0x0101FEFF, 0x0102FEFF };

            foreach (uint v in versions)
            {
                if (version == v)
                    return true;
            }

            return false;
        }

        public static string[] GetFontsByVersion(string folderPath, ushort version)
        {
            List<string> fonts = new List<string>();

            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            foreach (string fontPath in files)
            {
                FileStream fs = new FileStream(fontPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                BinaryReader br = new BinaryReader(fs);
                br.BaseStream.Position += 6;

                if (br.ReadUInt16() == version)
                    fonts.Add(fontPath);

                fs.Close();
                fs.Dispose();
                fs = null;
                br = null;
            }

            return fonts.ToArray();
        }

        public static bool HasExtendedInfo(string fontPath)
        {
            FileStream fs = new FileStream(fontPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            BinaryReader br = new BinaryReader(fs);
            br.BaseStream.Position += 0x14;

            bool result = false;
            if (br.ReadUInt32() == 0x20)
                result = true;

            fs.Close();
            fs.Dispose();
            fs = null;
            br = null;

            return result;
        }

        public static bool AreExtendedInfo(string[] fontList)
        {
            foreach (string f in fontList)
            {
                if (!HasExtendedInfo(f))
                    return false;
            }

            return true;
        }

        public static bool AnyExtendedInfo(string[] fontList)
        {
            foreach (string f in fontList)
            {
                if (HasExtendedInfo(f))
                    return true;
            }

            return false;
        }
    }
}
