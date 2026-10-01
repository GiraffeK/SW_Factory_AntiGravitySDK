using System;
using System.Windows.Forms;
using ScreenRecorder.Core;

namespace ScreenRecorder.Core
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}