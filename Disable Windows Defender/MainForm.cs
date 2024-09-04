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
            Timer timer = new Timer();
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
                    Timer timer2 = new Timer();
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
            RegistryDialUp(true);

            //Цикловая функция
            bool cycleIsWorking = true;
            Cycle();
            async void Cycle()
            {
                int cyctimer;
                await Task.Delay(1000);
                while (cycleIsWorking)
                {
                    // Тело цикла
                    if (WDdisabled)
                    {
                        //цикл бдения за мониторингом
                        DisableWindowsDefender(false);
                        cyctimer = 40000;

                    }
                    else { cyctimer = 1000; }

                    //Задержка цикла бдения
                    await Task.Delay(cyctimer);
                }
            }
        }

        void RegistryCheck()
        {
            ////АВТОЗАПУСК
            RegistryKey reg;
            string regStartupKeyName = "Disable Windows Defender";

            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            string myregkey = (string)reg.GetValue(regStartupKeyName, null);
            string owncurrentpath = Assembly.GetExecutingAssembly().Location;

            //Проверить и при надобности обновить путь ключа автозапуска
            if (myregkey != null) //если значение есть то
            {
                //MessageBox.Show("Ключ нашёл");
                if (myregkey != owncurrentpath) //если путь не совпал то обновить
                {
                    //MessageBox.Show("Путь ключа несовпал. Обновляю на текущий");
                    reg.SetValue(regStartupKeyName, owncurrentpath);
                }
            }

            /////Автозапуск трекера(бдения) мониторинга дефендера
            //чекбокс жмяк
            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
            string doAutoDisableKey = (string)reg.GetValue("doAutoDisable", null);
            string isWinDefEntireDisabled = (string)reg.GetValue("isWindowsDefenderEntireDisabled", null);

            if (isWinDefEntireDisabled != "True" && doAutoDisableKey != null) //если WD не выключен полностью и есть ключик автобдения то
            {
                if (doAutoDisableKey == "True")
                {
                    //автозапуск бдения включен, запускаю бдение
                    WDdisabled = true;
                    //Меняем нажимаемость кнопки и текст на ней
                    disableWinDefenderToolStripMenuItem.Enabled = false;
                    disableWinDefenderToolStripMenuItem.Text = FuncIsWorkingText;
                    //Говорим что трекинг включен
                    reg.SetValue("isWindowsDefenderDisabledByUser", "True");
                }
            }

            if (doAutoDisableKey != "True")
            {
                WDdisabled = false;
                reg.SetValue("isWindowsDefenderDisabledByUser", "False");
                //MessageBox.Show("Нет автозапуска трекера. Отменяю трекинг в коде");
            }

            reg.Flush();
            reg.Close();
        }

        async void RegistryDialUp(bool isCycWorking)
        {
            while (isCycWorking)
            {
                //Смотрим в реестр
                RegistryKey reg;
                reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
                string isWinDefDisabledByUser = (string)reg.GetValue("isWindowsDefenderDisabledByUser");
                string isWinDefEntireDisabled = (string)reg.GetValue("isWindowsDefenderEntireDisabled");

                //принимаем команду к циклу трекера через реестр
                if (isWinDefDisabledByUser == "True")
                {
                    WDdisabled = true;
                    //Нельзя удалять, потому что есть проверка этого параметра на другой форме
                    //reg.DeleteValue("isWindowsDefenderDisabledByUser", false);
                }
                if (isWinDefDisabledByUser == "False")
                {
                    WDdisabled = false;
                    //Нельзя удалять, потому что есть проверка этого параметра на другой форме
                    //reg.DeleteValue("isWindowsDefenderDisabledByUser", false);
                }


                if (isWinDefEntireDisabled == "False") //Если WD не выключен полностью
                {
                    restoreWinDefenderToolStripMenuItem.Enabled = true;

                    if (isWinDefDisabledByUser != "True")
                    {
                        //MessageBox.Show("Так как дефендер не выключен полностью, и трекинг не работает, верну кнопку в начальное состояние");
                        disableWinDefenderToolStripMenuItem.Enabled = true;
                        disableWinDefenderToolStripMenuItem.Text = disabletext;
                    }
                    
                    //Проверить, надо ли включить Трекинг (если выключался Defender)
                    reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
                    string IsThereNeedToLaunchTracking = (string)reg.GetValue("isDefenderDisabledWhileTrackingIsActive");
                    if (IsThereNeedToLaunchTracking == "True")
                    {
                        reg.DeleteValue("isDefenderDisabledWhileTrackingIsActive");
                        reg.SetValue("isWindowsDefenderDisabledByUser", "True");
                        WDdisabled = true;
                        disableWinDefenderToolStripMenuItem.Enabled = false;
                        disableWinDefenderToolStripMenuItem.Text = FuncIsWorkingText;
                    }
                }
                if (isWinDefEntireDisabled == "True")
                {
                    reg.SetValue("isWindowsDefenderDisabledByUser", "False");
                    WDdisabled = false;
                    RegistryCheck();
                    restoreWinDefenderToolStripMenuItem.Enabled = false;
                    disableWinDefenderToolStripMenuItem.Enabled = false;
                    disableWinDefenderToolStripMenuItem.Text = "Windows Defender отключен полностью";
                }
                else
                {
                    if (WDdisabled)
                    {
                        disableWinDefenderToolStripMenuItem.Text = FuncIsWorkingText;
                        disableWinDefenderToolStripMenuItem.Enabled = false;
                    } 
                    else 
                    { 
                        disableWinDefenderToolStripMenuItem.Text = disabletext;
                        disableWinDefenderToolStripMenuItem.Enabled = true;
                    }
                }

                reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\Microsoft\\Windows Defender");
                int isWDED = (int)reg.GetValue("DisableAntiVirus", 0);
                if (isWDED == 1)
                {
                    reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
                    //Записать в своём кусте реестра о состоянии WD
                    reg.SetValue("isWindowsDefenderEntireDisabled", "True");
                } 
                else 
                {
                    reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
                    reg.SetValue("isWindowsDefenderEntireDisabled", "False");
                }


                reg.Flush();
                reg.Close();
                await Task.Delay(1500);
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

        void DisableWindowsDefender(bool byUser)
        {
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
            string isWinDefEntireDisabled = (string)reg.GetValue("isWindowsDefenderEntireDisabled", 0);
            
            if(isWinDefEntireDisabled == "True")
            {
                //MessageBox.Show("Нет надобности. Windows Defender отключен полностью");
            }
            else //если всё в порядке и дефендер включён
            {
                if (byUser == true)
                {
                    WDdisabled = true; //Говорим циклу что надо держать дефендер выключенным
                    reg.SetValue("isWindowsDefenderDisabledByUser", "True");
                    reg.Flush();
                    reg.Close();
                }
                //Меняем нажимаемость кнопки и текст на ней
                disableWinDefenderToolStripMenuItem.Enabled = false;
                disableWinDefenderToolStripMenuItem.Text = FuncIsWorkingText;

                //Отключаем мониторинг в реальном времени
                Cmd($"powershell.exe -command \"Set-MpPreference -DisableRealtimeMonitoring $true\"");

                reg.Flush();
                reg.Close();

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
            DisableWindowsDefender(true);
        }
        private void RestoreWinDefenderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
            string isWinDefEntireDisabled = (string)reg.GetValue("isWindowsDefenderEntireDisabled");

            if (isWinDefEntireDisabled != "True")
            {
                Cmd($"powershell.exe -command \"Set-MpPreference -DisableRealtimeMonitoring $false\"");
                WDdisabled = false;

                reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
                reg.SetValue("isWindowsDefenderDisabledByUser", "False");
            }
            else
            {
                //MessageBox.Show("Нет надобности. Windows Defender отключен полностью");
            }


            reg.Flush();
            reg.Close();

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
