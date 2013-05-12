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
