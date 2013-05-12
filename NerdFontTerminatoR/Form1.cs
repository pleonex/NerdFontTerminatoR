using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nftr
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnCmd_Click(object sender, EventArgs e)
        {
            this.Parser(this.txtInput.Text);
        }

        private void Parser(string cmd)
        {
            string[] args = cmd.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            switch (args[0])
            {
                case "CheckFonts": 
                    if (args.Length == 2)
                        this.PrintLog(args[0], Analyzer.CheckFontHeader(args[1]));
                    break;

                case "GetByVersion":
                    if (args.Length == 3)
                        this.PrintLog(args[0], Analyzer.GetFontsByVersion(args[1],
                            Convert.ToUInt16(args[2], 16)));
                    break;

                case "TestAlwaysVersionExtended":
                    if (args.Length == 3)
                    {
                        string[] f = Analyzer.GetFontsByVersion(args[1], Convert.ToUInt16(args[2], 16));
                        this.PrintLog(args[0], Analyzer.AreExtendedInfo(f));
                    }
                    break;

                case "TestCanVersionExtended":
                    if (args.Length == 3)
                    {
                        string[] f = Analyzer.GetFontsByVersion(args[1], Convert.ToUInt16(args[2], 16));
                        this.PrintLog(args[0], Analyzer.AnyExtendedInfo(f));
                    }
                    break;

				case "TestMono":
					this.PrintLog("Encodings:");
					this.PrintLog();

					foreach (EncodingInfo enc in Encoding.GetEncodings())
					{
						this.PrintLog(enc.Name + " ");
						this.PrintLog(enc.DisplayName + " ");
						this.PrintLog(enc.CodePage.ToString());
						this.PrintLog();
					}
					break;

                default:
                    this.PrintLog("Unknown command");
                    this.PrintLog();
                    break;
            }
        }

        private void PrintLog()
        {
            this.txtOutput.AppendText("\r\n");
        }

        private void PrintLog(string msg)
        {
            this.txtOutput.AppendText(msg);
        }

        private void PrintLog(string msg, bool result)
        {
            this.PrintLog(string.Format("{0} -> {1}", msg, result));
            this.PrintLog();
        }

        private void PrintLog(string msg, string[] list)
        {
            this.PrintLog(msg);
            this.PrintLog();

            foreach (string s in list)
            {
                this.PrintLog(string.Format("\t{0}", s));
                this.PrintLog();
            }
        }
    }
}
