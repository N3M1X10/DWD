using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.IO;
using Microsoft.Win32;
using System.Reflection;

namespace Disable_Windows_Defender
{
    public partial class MainForm : Form
    {
        //говорим циклу, надо ли бдеть за дефендером
        bool WDdisabled = false;

        /////имена для контролов для подставки
        private readonly string disabletext = "Отключить Мониторинг";
        private readonly string FuncIsWorkingText = "Бдение за дефендером активно . . .";
        //Получить версию сборки чтобы потом впихнуть куда-нибудь где надо оно
        readonly static Assembly assemblyBlock = Assembly.GetExecutingAssembly();
        readonly static FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assemblyBlock.Location);
        //Переменная для подставки в контролы
        readonly static string ProjectVersion = fvi.FileVersion;

        public MainForm()
        {
            InitializeComponent();
            cornerversionlabel.Text = ProjectVersion;
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

            RegistryCheck();

            //Цикловая функция
            bool cycleIsWorking = true;
            Cycle();
            async void Cycle()
            {
                await Task.Delay(1000);
                while (cycleIsWorking)
                {
                    // Тело цикла
                    if (WDdisabled)
                    {
                        //цикл бдения за мониторингом
                        disableWinDefenderToolStripMenuItem.Enabled = false;
                        Cmd($"powershell.exe -command \"Set-MpPreference -DisableRealtimeMonitoring $true\"");
                    }

                    //Задержка цикла бдения
                    await Task.Delay(40000);
                }
                //Функция с цмд
                void Cmd(string line)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = $"/c {line}",
                        WindowStyle = ProcessWindowStyle.Hidden
                    }).Close();
                } //Модуль цмд-шника
            }
        }

        void RegistryCheck()
        {
            ////АВТОЗАПУСК
            string regStartupKey = "Disable Windows Defender";
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            string myregkey = (string)reg.GetValue(regStartupKey);
            string mycurrentpath = Assembly.GetExecutingAssembly().Location;

            if (myregkey != null) //если значение есть
            {
                //MessageBox.Show("Ключ нашёл");

                if (myregkey != mycurrentpath) //если не совпадает путь то обновить на текущий
                {
                    //MessageBox.Show("Ключ несовпал. Обновляю на текущий");
                    reg.SetValue(regStartupKey, mycurrentpath);
                    reg.Flush();
                    reg.Close();
                }
            }

            /////Авто отключение дефендера (автобдение при запуске)
            //чекбокс жмяк
            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
            string doAutoDisableKey = (string)reg.GetValue("doAutoDisable");

            if (doAutoDisableKey != null) //если есть ключик автобдения
            {
                if (doAutoDisableKey == "True")
                {
                    WDdisabled = true;
                    //Меняем нажимаемость кнопки и текст на ней
                    disableWinDefenderToolStripMenuItem.Enabled = false;
                    disableWinDefenderToolStripMenuItem.Text = FuncIsWorkingText;
                }
                else
                {
                    WDdisabled = false;
                }

            }
            else { WDdisabled = false; }

            reg.Flush();
            reg.Close();
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

        void DisableWindowsDefender()
        {
            //Меняем нажимаемость кнопки и текст на ней
            disableWinDefenderToolStripMenuItem.Enabled = false;
            disableWinDefenderToolStripMenuItem.Text = FuncIsWorkingText;

            //Отключаем мониторинг в реальном времени
            Cmd($"powershell.exe -command \"Set-MpPreference -DisableRealtimeMonitoring $true\"");
            WDdisabled = true; //Говорим циклу что надо держать дефендер выключенным

            void Cmd(string line)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {line}",
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            } //Модуль цмд-шника
        } //Включить бдение за дефендером
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
                WindowAlreadyExist();
            }
        }
        private void QuitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }
        private void DisableWinDefenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisableWindowsDefender();
        }
        private void RestoreWinDefenderToolStripMenuItem_Click(object sender, EventArgs e)
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
                WindowAlreadyExist();
            }
        }
        
        //Окно уже где-то есть
        bool WAEfuncisable = true;
        void WindowAlreadyExist()
        {
            //Запрет на вызов кучи мсгбоксов

            if (WAEfuncisable)
            {
                //запрещаем вызывать часть функции с мсгбоксом
                WAEfuncisable = false;
                TopMost = true;

                //мсгбокс вызван
                var msgboxexist = MessageBox.Show(
                "Кажется это окно уже где-то есть ◑﹏◐",
                "DWD : Окно уже открыто",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation

              );


                //мсгбокс смотрит нажата ли кнопка ОК
                msgboxexist = DialogResult.OK;
                
                //проверка нажатия кнопки ОК на мсгбоксе
                if (msgboxexist.ToString() == "OK")
                {
                    //Вновь разрешаем вызов мсгбокса после его закрытия
                    TopMost = false;
                    WAEfuncisable = true;
                    //Вызов окна Эбаут
                    
                }

            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
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
                WindowAlreadyExist();
            }
        }
    }
}
