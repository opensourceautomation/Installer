﻿using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace DBInstall
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1(args[0], args[1]));
        }
    }
}
