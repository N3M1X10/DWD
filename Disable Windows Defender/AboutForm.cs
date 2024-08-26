using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Reflection;

namespace Disable_Windows_Defender
{
    public partial class AboutForm : Form
    {
        //Получить версию сборки чтобы потом впихнуть куда-нибудь где надо оно
        readonly static Assembly assemblyBlock = Assembly.GetExecutingAssembly();
        readonly static FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assemblyBlock.Location);
        readonly static string ProjectVersion = fvi.FileVersion;

        //имя исполняемого файла данной сборки
        readonly string SelfFileName = Path.GetFileName(Application.ExecutablePath);

        public AboutForm()
        {
            //вывод поверх экрана
            TopMost = true;

            InitializeComponent();
            Opacity = 0;
            Timer timer = new Timer();
            timer.Tick += new EventHandler((sender, e) =>
            {
                if ((Opacity += 0.05d) >= 1) timer.Stop();
            });
            timer.Interval = 1;
            timer.Start();
            Select();
            //Отключение режима поверх всех окон
            TopMost = false;

            //Замена текстов с подставлением переменных при загрузке окна
            VersionLabel.Text = "Версия: " + ProjectVersion;
            SwitchToWhatsNew.Text = "Что нового в " + ProjectVersion;
            WhatsNewFormHeaderText.Text = "Что нового в " + ProjectVersion;
        }
        private async void AboutForm_Load(object sender, EventArgs e)
        {
            await Task.Delay(100);
            RegistryCheck();
        }

        //Функция проверки Ресстра и взаимодействия с ним
        void RegistryCheck()
        {
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
                RunWhenStartupCheck.Checked = true; //синхронизировать галочку с статусом из реестра
            }
            if (myregkey == null)
            {
                //MessageBox.Show("Не нашёл ключ, меня в автозапуске нет");

                RunWhenStartupCheck.Checked = false;
            }

            reg.Flush();
            reg.Close();

            /////Авто отключение дефендера (автобдение при запуске)
            //чекбокс жмяк

            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
            string doAutoDisableKey = (string)reg.GetValue("doAutoDisable");

            if (doAutoDisableKey != null) //если есть ключик автобдения
            {
                if (doAutoDisableKey.Length <= 4)
                {
                    //MessageBox.Show("Я вижу True");
                    doAutoDisableCheck.Checked = true;
                } 
                else 
                { 
                    //MessageBox.Show("Я вижу что-то другое");
                }

            } else { doAutoDisableCheck.Checked = false; }

            reg.Flush();
            reg.Close();
        }

        //
        // Кастом методы перетаскивания окна
        //
        Point LastPoint; // Переменная последней позиции окна
        private void DockPanel_MouseDown(object sender, MouseEventArgs e) // Перемещение окна за док панель
        {
            LastPoint = new Point(e.X, e.Y); //Запись координат курсора в переменную для перемещения окна за док панель
        }
        private void DockTitle_MouseDown(object sender, MouseEventArgs e)
        {
            LastPoint = new Point(e.X, e.Y);
        }
        private void DockPanel_MouseMove(object sender, MouseEventArgs e) // Перемещение окна за док панель
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Left += e.X - LastPoint.X;
                this.Top += e.Y - LastPoint.Y;
            }
        }
        private void DockTitle_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) //Перемещение окна за док панель
            {
                this.Left += e.X - LastPoint.X;
                this.Top += e.Y - LastPoint.Y;
            }
        }
        private void QuitButton_Click(object sender, EventArgs e)
        {
            CloseWin();
        }
        async void CloseWin()
        {
            //Плавное затухание формы при простом закрытии формы
            Opacity = 1;
            Timer timer = new Timer();
            timer.Tick += new EventHandler((sender, e) =>
            {
                if ((Opacity -= 0.05d) <= 0) timer.Stop();
            });
            timer.Interval = 1;
            timer.Start();
            await Task.Delay(500);
            Close();
        }
        //
        // end Win Dragging
        //

        //Кнопки на форме
        private void ShutDownButton_Click(object sender, EventArgs e) { CloseProg(); }
        async void CloseProg()
        {
            //Плавное затухание этой формы
            Opacity = 1;
            Timer timer = new Timer();
            timer.Tick += new EventHandler((sender, e) =>
            {
                if ((Opacity -= 0.05d) <= 0) timer.Stop();
            });
            timer.Interval = 1;
            timer.Start();
            await Task.Delay(500);

            //Закрытие всей программы
            Application.Exit();
        }
        private void ShutDownButton_MouseMove(object sender, MouseEventArgs e)
        {
            ShutDownButton.ForeColor = Color.White;
        }
        private void ShutDownButton_MouseLeave(object sender, EventArgs e)
        {
            ShutDownButton.ForeColor = ColorTranslator.FromHtml("#ff8888");
        }
        
        //внешние ссылки
        private void GithubButton_Click(object sender, EventArgs e)
        {
            //Гитхаб автора
            Process.Start("https://github.com/N3M1X10/DWD");
        }
        private void DiscordButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/2jJ3Qn4");
        }


        //Переключения между вкладками
        private void SwitchToWhatsNew_Click(object sender, EventArgs e)
        {
            //ПЕРЕКЛЮЧИТЬСЯ НА "ЧТО НОВОГО"
            TabsSwitcher(false, false, true);
        }
        private void OtherOptionsButton_Click(object sender, EventArgs e)
        {
            TabsSwitcher(false, true, false);
        }
        private void SwitchToAbout_Click(object sender, EventArgs e)
        {
            //ПЕРЕКЛЮЧИТЬСЯ НА "О ПРОГРАММЕ"
            TabsSwitcher(true, false, false);
        }


        //Свитчер окошек
        //Сделан патологически банально и топорно, потому что я ещё учусь, тошнить на C#
        //Отстань!?
        void TabsSwitcher(bool isAbout, bool isOthers, bool isWhatsNew)
        {
            Color disabledFore = Color.White;
            Color disabledBack = ColorTranslator.FromHtml("#232323");

            Color enabledFore = Color.Salmon;
            Color enabledBack = ColorTranslator.FromHtml("#191919");

            ////ЭЛЕМЕНТЫ
            
            //О программе
            AboutWrap.Visible = false;
            ProgNameWrapper.Visible = false;

            //Что нового
            WhatsNewFormHeader.Visible = false;
            WhatsNewPanel.Visible = false;

            //Опции
            OtherOptionsPanel.Visible = false;

            ////СТИЛИ

            //Что нового
            SwitchToWhatsNew.ForeColor = disabledFore;
            SwitchToWhatsNew.BackColor = disabledBack;

            //О программе
            SwitchToAbout.ForeColor = disabledFore;
            SwitchToAbout.BackColor = disabledBack;

            //Опции
            OtherOptionsButton.ForeColor = disabledFore;
            OtherOptionsButton.BackColor = disabledBack;

            //Триггеры условий
            if (isAbout)
            {
                AboutWrap.Visible = true;
                ProgNameWrapper.Visible = true;

                SwitchToAbout.ForeColor = enabledFore;
                SwitchToAbout.BackColor = enabledBack;
            }
            if (isWhatsNew)
            {
                WhatsNewFormHeader.Visible = true;
                WhatsNewPanel.Visible = true;

                SwitchToWhatsNew.ForeColor = enabledFore;
                SwitchToWhatsNew.BackColor = enabledBack;
            }
            if (isOthers)
            {
                OtherOptionsPanel.Visible = true;

                OtherOptionsButton.ForeColor = enabledFore;
                OtherOptionsButton.BackColor = enabledBack;
            }
        }

        //Опции
        private void DoAutoDisableCheck_CheckedChanged(object sender, EventArgs e)
        {
            ToggleAutoDisable();
        }
        private void RunWhenStartupCheck_CheckedChanged(object sender, EventArgs e)
        {
            ToggleRunWhenStartup();
        }


        ////Реестр

        //Запуск программы с системой
        void ToggleRunWhenStartup()
        {
            string regStartupKey = "Disable Windows Defender";
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");

            if (RunWhenStartupCheck.Checked == true)
            {
                reg.SetValue(regStartupKey, Assembly.GetExecutingAssembly().Location);
                reg.Flush();
                reg.Close();
            }
            if (RunWhenStartupCheck.Checked == false)
            {
                reg.DeleteValue(regStartupKey);
                reg.Flush();
                reg.Close();
            }
        }
        //Автозапуск бдения
        void ToggleAutoDisable()
        {
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
            reg.SetValue("doAutoDisable", doAutoDisableCheck.Checked);
            reg.Flush();
            reg.Close();
        }

        ////
        ////ДРУГИЕ ОПЦИИ
        ////
        private void WFdisableButton_Click(object sender, EventArgs e)
        {
            FireWallToggle(false);
        }
        private void WFenableButton_Click(object sender, EventArgs e)
        {
            FireWallToggle(true);
        }
        void FireWallToggle(bool isEnabled)
        {
            if (isEnabled) {
                Cmd($"NetSh Advfirewall set allprofiles state on");
                MessageBox.Show("Брандмауэр включён");
                WFdisableButton.Enabled = true;
                WFenableButton.Enabled = true;
            }
            if (!isEnabled) {
                Cmd($"NetSh Advfirewall set allprofiles state off");
                MessageBox.Show("Брандмауэр выключен");
                WFdisableButton.Enabled = true;
                WFenableButton.Enabled = true;
            }

            void Cmd(string line)
            {
                WFdisableButton.Enabled = false;
                WFenableButton.Enabled = false;
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {line}",
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            } //Модуль цмд-шника
        }
        //Добавить эту программу в исключения Defender
        private void AddToExtensionsButton_Click(object sender, EventArgs e)
        {
            Cmd($"powershell.exe Add-MpPreference -ExclusionProcess " + "\'" + SelfFileName + "\'");
            MessageBox.Show("Исключение добавлено");
            void Cmd(string line)
            {
                AddToExtensionsButton.Enabled = false;
                RemoveFromExtensionsButton.Enabled = false;
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {line}",
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            AddToExtensionsButton.Enabled = true;
            RemoveFromExtensionsButton.Enabled = true;
            } //Модуль цмд-шника
        }
        private void RemoveFromExtensionsButton_Click(object sender, EventArgs e)
        {
            Cmd($"powershell.exe Remove-MpPreference -ExclusionProcess " + "\'" + SelfFileName + "\'");
            MessageBox.Show("Исключение удалено");
            void Cmd(string line)
            {
                AddToExtensionsButton.Enabled = false;
                RemoveFromExtensionsButton.Enabled = false;
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {line}",
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
                AddToExtensionsButton.Enabled = true;
                RemoveFromExtensionsButton.Enabled = true;
            } //Модуль цмд-шника
        }

        //Полное включение или выключение Defender
        private void DisableEntireDefenderButton_Click(object sender, EventArgs e)
        {
            DisableEntireDefenderButton.Enabled = false;
            RegistryKey reg;
            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows Defender");
            reg.SetValue("DisableAntiSpyware", 1);
            reg.SetValue("DisableAntiVirus", 1);

            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows Defender\\MpEngine");
            reg.SetValue("MpEnablePus", 0);

            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Real-Time Protection");
            reg.SetValue("DisableBehaviorMonitoring", 1);
            reg.SetValue("DisableIOAVProtection", 1);
            reg.SetValue("DisableOnAccessProtection", 1);
            reg.SetValue("DisableRealtimeMonitoring", 1);
            reg.SetValue("DisableRoutinelyTakingAction", 1);
            reg.SetValue("DisableScanOnRealtimeEnable", 1);

            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows Defender\\Reporting");
            reg.SetValue("DisableEnhancedNotifications", 1);

            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows Defender\\SpyNet");
            reg.SetValue("DisableBlockAtFirstSeen", 1);
            reg.SetValue("SpynetReporting", 0);
            reg.SetValue("SubmitSamplesConsent", 2);
            
            reg.Flush();
            reg.Close();

            MessageBox.Show("Полное отключение функций завершено. \nКлючи реестра внесены. \nМесто работ в реестре: \"HKLM\\SOFTWARE\\Policies\\Microsoft\\Windows Defender\\\" ");
            DisableEntireDefenderButton.Enabled = true;
        }
        private void RestoreEntireDefenderButton_Click(object sender, EventArgs e)
        {
            RestoreEntireDefenderButton.Enabled = false;
            RegistryKey reg;
            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows Defender");
                reg.DeleteValue("DisableAntiVirus", false);
                reg.DeleteValue("DisableAntiSpyware", false);
                reg.DeleteSubKey("MpEngine", false);
                reg.DeleteSubKey("Real-Time Protection", false);
                reg.DeleteSubKey("Reporting", false);
                reg.DeleteSubKey("SpyNet", false);
            reg.Flush();
            reg.Close();
            MessageBox.Show("Восстановление функций Windows Defender завершено!");
            RestoreEntireDefenderButton.Enabled = true;
        }

    }
}
