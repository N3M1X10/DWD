using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Disable_Windows_Defender
{
    internal static class Program
    {
        public static class RunOnlyOneClass
        {
            [DllImport("user32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            static extern bool SetForegroundWindow(IntPtr hWnd);

            [DllImport("user32.dll", SetLastError = true)]
            internal static extern int ShowWindow(int hwnd, int nCmdShow);

            //[DllImport("user32.dll", SetLastError = true)]
            //internal static extern int GetWindow(int hwnd, int nCmdShow);

            static Mutex _syncObject;
            static readonly string AppPath = Path.GetFileNameWithoutExtension(Application.ExecutablePath);

            /// <summary>
            /// Находит запущенную копию приложения и разворачивает окно
            /// </summary>
            /// <param name="Disable Windows Defender">уникальное значение для каждой программы (можно имя)</param>
            /// <returns>true - если приложение было запущено</returns>
            public static bool ChekRunProgramm(string UniqueValue)
            {
                bool applicationRun = false;
                _syncObject = new Mutex(true, UniqueValue, out applicationRun);
                if (!applicationRun)
                {//восстановить/развернуть окно                              
                    try
                    {
                        Process[] procs = Process.GetProcessesByName(AppPath);
                        foreach (Process proc in procs)
                            if (proc.Id != Process.GetCurrentProcess().Id)
                            {
                                MessageBox.Show(
                                "Кажется программа уже запущена ◑﹏◐ \nПроверьте её в трее :)",
                                "DWD : Программа уже запущена",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation
                                );

                                SetForegroundWindow(proc.MainWindowHandle);

                                break;
                            }
                    }
                    catch { return false; }
                }
                return !applicationRun;
            }
        }
            /// <summary>
            /// Главная точка входа для приложения.
            /// </summary>
            [STAThread]
            static void Main()
            {
                if (RunOnlyOneClass.ChekRunProgramm("Disable Windows Defender")) return;

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm());

            }
        }
    }
