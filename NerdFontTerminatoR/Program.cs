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
        static void Main()
        {
#if !DEBUG
			Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
#else
			//TestSingle();
			//TestFull();
			TestWork();
#endif
        }

		private static void TestWork()
		{
			string fin   = "/home/benito/Dropbox/Ninokuni español/Fuentes/";
			string fout  = "/home/benito/Dropbox/Ninokuni español/Fuentes/";
			string fname = "font_b9";

			NftrFont fold = new NftrFont(fin + fname + ".NFTR");
			fold.Export(fout + fname + "_.xml", fout + fname + "_.png");

			//NftrFont fnew = new NftrFont(fout + fname + ".xml", fout + fname + ".png");
			//fnew.Write(fout + fname + ".new");
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
