﻿using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Disable_Windows_Defender
{
    public partial class MainForm : Form
    {
        bool WDdisabled = false;
        private string disabletext = "Отключить Мониторинг";
        private string FuncIsWorkingText = "Бдение за дефендером активно . . .";

        public MainForm()
        {
            InitializeComponent();
            TrayIcon.Visible = true;

            //плавное появление

            Opacity = 0;
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
            timer.Tick += new EventHandler((sender, e) =>
            {
                if ((Opacity += 0.05d) >= 1) timer.Stop();
            });
            timer.Interval = 1;
            timer.Start();

            bool RevealingIsReady = false;
            RevealingCyc();
            async void RevealingCyc()
            {
                while (!RevealingIsReady)
                {
                    if (Opacity >= 1)
                    {
                    Disappearing();
                    }
                    //задержка
                    await Task.Delay(1400);
                }
            }
                void Disappearing() {
                    bool DisappearingIsReady = false;
                    if (!DisappearingIsReady)
                    {
                        Opacity = 1;
                    System.Windows.Forms.Timer timer2 = new System.Windows.Forms.Timer();
                        timer2.Tick += new EventHandler((sender, e) =>
                        {
                            if ((Opacity -= 0.05d) <= 0) timer2.Stop(); HideFromAltTab(Handle);
                        });
                        timer2.Interval = 1;
                        timer2.Start();
                    }
                }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

            bool cycleIsWorking = true;
            Cycle();
            async void Cycle()
            {
                while (cycleIsWorking)
                {
                    // Тело цикла

                    if (WDdisabled == true)
                    {
                        //цикл бдения за мониторингом

                        disableWinDefenderToolStripMenuItem.Enabled = false;
                        Cmd($"powershell.exe -command \"Set-MpPreference -DisableRealtimeMonitoring $true\"");
                        WDdisabled = true;
                        void Cmd(string line)
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = "cmd",
                                Arguments = $"/c {line}",
                                WindowStyle = ProcessWindowStyle.Hidden
                            }).WaitForExit();
                        } //Модуль цмд-шника
                    }

                    //Задержка цикла бдения
                    await Task.Delay(40000);
                }
            }
        }

        //
        // Скрывалка из Альт Таба
        //
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr window, int index, int value);

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr window, int index);

        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        public static void HideFromAltTab(IntPtr Handle)
        {
            SetWindowLong(Handle, GWL_EXSTYLE, GetWindowLong(Handle,
                GWL_EXSTYLE) | WS_EX_TOOLWINDOW);
        }
        //
        // конец скрывалки
        //

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void disableWinDefenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            disableWinDefenderToolStripMenuItem.Enabled = false;
            disableWinDefenderToolStripMenuItem.Text = FuncIsWorkingText;
            Cmd($"powershell.exe -command \"Set-MpPreference -DisableRealtimeMonitoring $true\"");
            WDdisabled = true;
            void Cmd(string line)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {line}",
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            } //Модуль цмд-шника
        }


        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            AboutForm about;
            about = new AboutForm();

            bool able = false;
            foreach (Form f in Application.OpenForms)
                if (f.Name == "AboutForm")
                    able = true;
            if (!able)
            {
                about = new AboutForm();
                about.Show();
            }
            else
            {
                MessageBox.Show(
                    "Кажется это окно уже где-то есть ◑﹏◐",
                    "DWD : Окно уже открыто",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation
                    );
            }
        }

        private void restoreWinDefenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Cmd($"powershell.exe -command \"Set-MpPreference -DisableRealtimeMonitoring $false\"");
            WDdisabled = false;
            void Cmd(string line)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {line}",
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            } //Модуль цмд-шника
            disableWinDefenderToolStripMenuItem.Enabled = true;
            disableWinDefenderToolStripMenuItem.Text = disabletext;
        }

        void AboutButton_Click(object sender, EventArgs e)
        {
            AboutForm about;
            about = new AboutForm();

            bool able = false;
            foreach (Form f in Application.OpenForms)
                if (f.Name == "AboutForm")
                    able = true;
            if (!able)
            {
                about = new AboutForm();
                about.Show();
            }
            else
            {
                MessageBox.Show(
                    "Кажется это окно уже где-то есть ◑﹏◐",
                    "DWD : Окно уже открыто",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation
                    );
            }
        }
    }
}