// -----------------------------------------------------------------------
// <copyright file="Program.cs" company="none">
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
using System.Windows.Forms;

namespace Nftr
{
    static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
			Console.WriteLine("NerdFontTerminatoR ... V 0.1");
			Console.WriteLine("Create, view & edit NFTR fonts from NDS games.");
			Console.WriteLine(" ~~ by pleonex ~~ ");
			Console.WriteLine();

			ConsoleMode(args);
			return;

#if !DEBUG
			Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
#else
			//TestSingle();
			//TestFull();
			//TestWork();
#endif
        }

		private static void ConsoleMode(string[] args)
		{
			if (args.Length != 4 || (args[0] != "-i" && args[0] != "-e")) {
				PrintHelp();
				return;
			}

			NftrFont font;

			// Import mode
			if (args[0] == "-i" && args.Length == 4) {
				Console.WriteLine("Importing from:\n\t{0}\n\t{1}\nto:\n\t{2}", args[1], args[2], args[3]);
				font = new NftrFont(args[1], args[2]);
				font.Write(args[3]);
			}

			// Export mode
			if (args[0] == "-e" && args.Length == 4) {
				Console.WriteLine("Exporting from:\n\t{0}\nto:\n\t{1}\n\t{2}", args[1], args[2], args[3]);
				font = new NftrFont(args[1]);
				font.Export(args[2], args[3]);
			}

			Console.WriteLine();
			Console.WriteLine("Done!");
		}

		private static void PrintHelp()
		{
			Console.WriteLine("USE: NerdFontTerminatoR.exe mode file1 file2 file3");
			Console.WriteLine();
			Console.WriteLine("Modes:");
			Console.WriteLine("\t-e\tExport font to XML + PNG");
			Console.WriteLine("\t\tfile1: NFTR, file2: XML, file3: PNG");
			Console.WriteLine();
			Console.WriteLine("\t-i\tImport (create) font from XML + PNG");
			Console.WriteLine("\t\tfile1: XML,  file2: PNG, file3: NFTR (new)");
		}

		private static void TestWork()
		{
			string fin   = @"C:\Users\Benito\Documents\My Dropbox\Ninokuni español\Fuentes\"; //@"G:\nds\projects\ninokuni\";
			string fout  = @"C:\Users\Benito\Documents\My Dropbox\Ninokuni español\Fuentes\"; //@"G:\nds\projects\ninokuni\";
			string fname = "font_b16";

			//NftrFont fold = new NftrFont(fin + fname + ".NFTR");
			//fold.Export(fout + fname + ".xml", fout + fname + ".png");

			NftrFont fnew = new NftrFont(fout + fname + ".xml", fout + fname + "_VERSION1.png");
			fnew.Write(fout + fname + "_new.NFTR");
		}
		private static void TestSingle()
		{
			string fontPath = "/var/run/media/benito/2038A2E238A2B5E6";
			fontPath += "/nds/projects/NDS/NerdFontTerminatoR/files/";
			fontPath += "Ninokuni [CLEAN]/4096_font_b9.NFTR";

			string outPath = "/home/benito/";
			outPath += "test";

			NftrFont font = new NftrFont(fontPath);
			font.Export(outPath + ".xml", outPath + ".png");

			font = new NftrFont(outPath + ".xml", outPath + ".png");
			font.Write(outPath + ".new");

			Console.WriteLine("{0} written.", 
			                  CompareFiles(fontPath, outPath + ".new") ? "Successfully" : "Unsuccessfully");
		}
		private static void TestFull()
		{
			string fontPath = "/var/run/media/benito/2038A2E238A2B5E6";
			fontPath += "/nds/projects/NDS/NerdFontTerminatoR/files/";

			string outPath = "/home/benito/Proyectos/Fonts";

			foreach (string dir in Directory.GetDirectories(fontPath)) {
				string fontDirOut = Path.Combine(outPath, new DirectoryInfo(dir).Name);
				if (!Directory.Exists(fontDirOut))
					Directory.CreateDirectory(fontDirOut);

				foreach (string f in Directory.GetFiles(dir)) {
					string fontOut = Path.Combine(fontDirOut, Path.GetFileNameWithoutExtension(f));
					Console.WriteLine(fontOut);
					//if (File.Exists(fontOut + ".png"))
					//	continue;

					try {
						NftrFont font = new NftrFont(f);
						if (!font.Check())
							return;
						//font.Export(fontOut + ".xml", fontOut + ".png");

						//MemoryStream ms = new MemoryStream();
						//font.Write(ms);
						//font.Write(fontOut + ".new");
						//if (!CompareFiles(fontOut + ".new", f))
						//	return;
					} catch (Exception ex) { 
						Console.WriteLine("--> ERROR: {0}\n{1}", ex.Message, ex.StackTrace); 
					}
				}
			}

		}

		private static bool CompareFiles(string f1, string f2)
		{
			FileStream fs1 = new FileStream(f1, FileMode.Open);
			FileStream fs2 = new FileStream(f2, FileMode.Open);

			bool result = CompareFiles(fs1, fs2);

			fs1.Close();
			fs2.Close();
			return result;
		}
		private static bool CompareFiles(Stream s1, Stream s2)
		{
			bool result = true;
			if (s1.Length != s2.Length)
				result = false;

			while (s1.Position < s1.Length && result) {
				if (s1.ReadByte() != s2.ReadByte())
					result = false;
			}

			return result;
		}
    }
}
