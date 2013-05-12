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
			TestSingle();
			//TestFull();
#endif
        }

		private static void TestSingle()
		{
			string fontPath = "/var/run/media/benito/2038A2E238A2B5E6";
			fontPath += "/nds/projects/NDS/NerdFontTerminatoR/files/";
			fontPath += "Animal World - Big Cats/3_textfont.nftr";

			string outPath = "/home/benito/Proyectos/Fonts/";
			outPath += "test";
			NftrFont font = new NftrFont(fontPath);
			font.Export(
				outPath + ".xml",
				outPath + ".png");
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
					if (File.Exists(fontOut + ".png"))
						continue;
					try {
						NftrFont font = new NftrFont(f);
						font.Export(
							fontOut + ".xml",
							fontOut + ".png");
					}
					catch { }
				}
			}

		}
    }
}
