/*
 * SingleInstanceProgram.cs
 * in project Hebrew_English_Flashcard_Tachistoscope
 * This file replaces Visual Studio's original file named Program.cs
 * that was created by Visual Studio 2019, new project, Windows Form Application.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Hebrew_English_Flashcard_Tachistoscope
{
    /// <summary>
    /// Class with program entry point.
	/// Designed to only allow a single instance of this windows app.
	/// as per http://stackoverflow.com/questions/51898/activating-the-main-form-of-a-single-instance-application
	/// add using System.Diagnostics;
	/// add using System.Runtime.InteropServices;
    /// </summary>
    internal sealed class Program
    {
        // Sets the window to be foreground
        [DllImport("User32")]
        private static extern int SetForegroundWindow(IntPtr hwnd);

        // Activate or minimize a window
        [DllImportAttribute("User32.DLL")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        private const int SW_RESTORE = 9;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                // If another instance is already running, activate it and exit
                Process currentProc = Process.GetCurrentProcess();
                foreach (Process proc in Process.GetProcessesByName(currentProc.ProcessName))
                {
                    if (proc.Id != currentProc.Id)
                    {
                        //ShowWindow(proc.MainWindowHandle, SW_RESTORE);
                        //SetForegroundWindow(proc.MainWindowHandle);
                        return;   // Exit application
                    }
                }

                // Continue with the normal contents of Main(): Nah:
                // -- Application.EnableVisualStyles();
                // -- Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new Form1());
            }
            catch (Exception)
            {
            }
        }        
    }
}
