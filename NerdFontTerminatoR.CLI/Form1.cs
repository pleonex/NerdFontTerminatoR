// -----------------------------------------------------------------------
// <copyright file="Form1.cs" company="none">
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Nftr.CLI
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
