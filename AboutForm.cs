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
        //путь к сценарию
        string mycurrentpath = Assembly.GetExecutingAssembly().Location;

        public AboutForm()
        {
            //вывод поверх экрана
            //TopMost = true;

            InitializeComponent();
            Opacity = 0;
            //Timer timer = new Timer();
            //timer.Tick += new EventHandler((sender, e) =>
            //{
            //    if ((Opacity += 0.05d) >= 1) timer.Stop();
            //});
            //timer.Interval = 1;
            //timer.Start();
            //Select();
            ////Отключение режима поверх всех окон
            //TopMost = false;

            //Замена текстов с подставлением переменных при загрузке окна
            VersionLabel.Text = "Версия: " + ProjectVersion;
            SwitchToWhatsNew.Text = "Что нового в " + ProjectVersion;
            WhatsNewFormHeaderText.Text = "Что нового в " + ProjectVersion;
            RegistryCheck();
        }
        private async void AboutForm_Load(object sender, EventArgs e)
        {
            await Task.Delay(100);
            RegistryDialUp(true);
        }

        //Функция проверки Ресстра и взаимодействия с ним
        void RegistryCheck()
        {
            string regStartupKey = "Disable Windows Defender";
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run");
            string myregkey = (string)reg.GetValue(regStartupKey);

            if (myregkey != null) //если значение есть
            {
                //MessageBox.Show("Ключ нашёл");

                if (myregkey != mycurrentpath) //если не совпадает путь то обновить на текущий
                {
                    //MessageBox.Show("Ключ несовпал. Обновляю на текущий");
                    reg.SetValue(regStartupKey, mycurrentpath);
                    Cmd($"SCHTASKS /Delete /TN \"Disable Windows Defender\" /F");
                    Cmd($"SCHTASKS /Create /TN \"Disable Windows Defender\" /TR \"{mycurrentpath}\" /SC ONSTART /RL HIGHEST /F");

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
                RunWhenStartupCheck.Checked = true; //синхронизировать галочку с статусом из реестра
            }
            if (myregkey == null)
            {
                //MessageBox.Show("Не нашёл ключ, меня в автозапуске нет");
                RunWhenStartupCheck.Checked = false;
            }

            /////Авто отключение дефендера (автобдение при запуске)
            //синхронизация чекбокса с реестром

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
        
        async void RegistryDialUp(bool isCycWorking)
        {
            string DisableStartText = DisableEntireDefenderButton.Text;
            string RestoreStartText = RestoreEntireDefenderButton.Text;

            while (isCycWorking)
            {
                RegistryKey reg;
                reg = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Policies\\Microsoft\\Windows Defender");
                
                
                //Смотрим в реестр, включен ли антивирус
                int isWDED = (int)reg.GetValue("DisableAntiVirus", 0);
                if (isWDED == 1)
                {
                    DisableEntireDefenderButton.Enabled = false;
                    DisableEntireDefenderButton.Text = "(Defender Отключен) " + DisableStartText;

                    reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
                    //Записать в своём кусте реестра о состоянии WD
                    reg.SetValue("isWindowsDefenderEntireDisabled", "True");

                    RestoreEntireDefenderButton.Enabled = true;
                    RestoreEntireDefenderButton.Text = RestoreStartText;
                }
                if (isWDED != 1)
                {
                    DisableEntireDefenderButton.Enabled = true;
                    DisableEntireDefenderButton.Text = DisableStartText;

                    reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
                    //Записать в своём кусте реестра о состоянии WD
                    reg.SetValue("isWindowsDefenderEntireDisabled", "False");

                    RestoreEntireDefenderButton.Enabled = false;
                    RestoreEntireDefenderButton.Text = "(Defender Активен) " + RestoreStartText;
                }


                //Смотрим включен ли UAC
                reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");

                if ((int)reg.GetValue("EnableLUA") == 0)
                {
                    DisableUACbutton.Enabled = false;
                    EnableUACbutton.Enabled = true;
                }
                if ((int)reg.GetValue("EnableLUA") == 1)
                {
                    DisableUACbutton.Enabled = true;
                    EnableUACbutton.Enabled = false;
                }

                reg.Flush();
                reg.Close();
                await Task.Delay(1500);
            }
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
            ////Плавное затухание формы при простом закрытии формы
            //Opacity = 1;
            //Timer timer = new Timer();
            //timer.Tick += new EventHandler((sender, e) =>
            //{
            //    if ((Opacity -= 0.05d) <= 0) timer.Stop();
            //});
            //timer.Interval = 1;
            //timer.Start();
            //await Task.Delay(500);

            bool waitVal = false;
            while (waitVal == false)
            {
                Opacity = 1; //Прозрачность окна
                Timer timerMinimForm = new Timer(); //Создание таймера
                timerMinimForm.Tick += new EventHandler((sender2, e2) =>
                {
                    if ((Opacity -= 0.08) <= 0)
                        timerMinimForm.Stop();
                });
                timerMinimForm.Interval = 1;
                timerMinimForm.Start();
                await Task.Delay(500);
                if (Opacity <= 0)
                {
                    waitVal = true;
                    timerMinimForm.Dispose();
                    Opacity = 0;
                }
            }

            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Hide();
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
                if ((Opacity -= 0.08d) <= 0) timer.Stop();
            });
            timer.Interval = 1;
            timer.Start();
            await Task.Delay(500);

            //Закрытие всей программы
            Process.GetCurrentProcess().Kill();
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
        private void BoostyButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://boosty.to/nemix");
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
            WhatsNewTab.Visible = false;

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
                WhatsNewTab.Visible = true;

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
                Cmd($"SCHTASKS /Create /TN \"Disable Windows Defender\" /TR \"{mycurrentpath}\" /SC ONSTART /RL HIGHEST /F");
            }
            if (RunWhenStartupCheck.Checked == false)
            {
                reg.DeleteValue(regStartupKey);
                Cmd($"SCHTASKS /Delete /TN \"Disable Windows Defender\" /F");
            }
            void Cmd(string line)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {line}",
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            } //Модуль цмд-шника
            reg.Flush();
            reg.Close();
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
                Cmd($"NetSh Advfirewall set allprofiles state on & exit /b");
                MessageBox.Show("Брандмауэр включён!", "DWD", MessageBoxButtons.OK, MessageBoxIcon.Information);
                WFdisableButton.Enabled = true;
                WFenableButton.Enabled = true;
            }
            if (!isEnabled) {
                Cmd($"NetSh Advfirewall set allprofiles state off & exit /b");
                MessageBox.Show("Брандмауэр выключен!", "DWD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
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
        private void AddToExclusionsButton_Click(object sender, EventArgs e)
        {
            string StartText = AddToExclusionsButton.Text;
            AddToExclusionsButton.Enabled = false;
            RemoveFromExclusionsButton.Enabled = false;
            AddToExclusionsButton.Text = "Пожалуйста, подождите . . .";

            Cmd($"powershell.exe Add-MpPreference -ExclusionProcess " + "\'" + SelfFileName + "\' & exit /b");

            MessageBox.Show("Исключение добавлено", "DWD", MessageBoxButtons.OK, MessageBoxIcon.Information);
            AddToExclusionsButton.Text = StartText;
            AddToExclusionsButton.Enabled = true;
            RemoveFromExclusionsButton.Enabled = true;

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
        private void RemoveFromExclusionsButton_Click(object sender, EventArgs e)
        {
            AddToExclusionsButton.Enabled = false;
            RemoveFromExclusionsButton.Enabled = false;
            string StartText = RemoveFromExclusionsButton.Text;
            RemoveFromExclusionsButton.Text = "Пожалуйста, подождите . . .";
            Cmd($"powershell.exe Remove-MpPreference -ExclusionProcess " + "\'" + SelfFileName + "\' & exit /b");
            MessageBox.Show("Исключение удалено", "DWD", MessageBoxButtons.OK, MessageBoxIcon.Information);
            AddToExclusionsButton.Enabled = true;
            RemoveFromExclusionsButton.Enabled = true;
            RemoveFromExclusionsButton.Text = StartText;
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

        //Полное включение или выключение Defender
        private void DisableEntireDefenderButton_Click(object sender, EventArgs e)
        {
            DisableEntireDefenderButton.Enabled = false;
            if (MessageBox.Show(
                "Вы уверены что хотите отключить Windows Defender? \nЕго работа будет прекращена сразу. \n(Но, может потребоваться перезагрузка системы)",
                "DWD : Подтверждение действия",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation
                ) 
                == DialogResult.Yes )
            {
                //Отключение функций в планировщике задач
                Cmd($"schtasks /Change /TN \"Microsoft\\Windows\\ExploitGuard\\ExploitGuard MDM policy Refresh\" /Disable & " +
                    "schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Cache Maintenance\" /Disable &" +
                    "schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Cleanup\" /Disable &" +
                    "schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Scheduled Scan\" /Disable &" +
                    "schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Verification\" /Disable &" +
                    "exit /b");

                void Cmd(string line)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd",
                        Arguments = $"/c {line}",
                        WindowStyle = ProcessWindowStyle.Hidden
                    }).WaitForExit();
                } //Модуль цмд-шника

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

                //Отключение SmartScreen
                if (SmartScreenCheck.Checked)
                {
                    reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\System");
                    reg.SetValue("EnableSmartScreen", 0);
                }

                //Проверяем был ли включен трекинг(бдение) за мониторингом когда отключался весь Defender
                //И если он был включен, то основа поняла что надо запустить трекинг вновь, по восстановлении функций Defender
                reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
                if ((string)reg.GetValue("isWindowsDefenderDisabledByUser") == "True")
                {
                    //Если автозапуск трекера был включен
                    if ((string)reg.GetValue("doAutoDisable") == "True") 
                    {
                        //То сказать основе, что надо будет включить бдение, когда WD будет не будет выключен полностью
                        reg.SetValue("isDefenderDisabledWhileTrackingIsActive", "True");
                    }
                }

                //Говорим основе что бдить пока незачем
                reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
                reg.SetValue("isWindowsDefenderDisabledByUser", "False");
                //Записать в своём кусте реестра о состоянии WD
                reg.SetValue("isWindowsDefenderEntireDisabled", "True");


                reg.Flush();
                reg.Close();

                MessageBox.Show("Полное отключение функций Windows Defender завершено!", "DWD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                DisableEntireDefenderButton.Enabled = true;
            }
        }
        private void RestoreEntireDefenderButton_Click(object sender, EventArgs e)
        {
            RestoreEntireDefenderButton.Enabled = false;

            //Включение функций в планировщике задач
            Cmd($"schtasks /Change /TN \"Microsoft\\Windows\\ExploitGuard\\ExploitGuard MDM policy Refresh\" /Enable & " +
                "schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Cache Maintenance\" /Enable &" +
                "schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Cleanup\" /Enable &" +
                "schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Scheduled Scan\" /Enable &" +
                "schtasks /Change /TN \"Microsoft\\Windows\\Windows Defender\\Windows Defender Verification\" /Enable &" +
                "exit /b");

            void Cmd(string line)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "cmd",
                    Arguments = $"/c {line}",
                    WindowStyle = ProcessWindowStyle.Hidden
                }).WaitForExit();
            } //Модуль цмд-шника

            //Удаление ключей-предписаний для Windows Defender из реестра
            //ака Вернуть Defender в рабочее состояние
            RegistryKey reg;
            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows Defender");
            reg.DeleteValue("DisableAntiVirus", false);
            reg.DeleteValue("DisableAntiSpyware", false);
            reg.DeleteSubKey("MpEngine", false);
            reg.DeleteSubKey("Real-Time Protection", false);
            reg.DeleteSubKey("Reporting", false);
            reg.DeleteSubKey("SpyNet", false);

            //SmartScreen
            if (SmartScreenCheck.Checked)
            {
                reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Policies\\Microsoft\\Windows\\System");
                reg.DeleteValue("EnableSmartScreen", false);
            }

            //Записать в своём кусте реестра о состоянии WD
            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
            reg.SetValue("isWindowsDefenderEntireDisabled", "False");

            reg.Flush();
            reg.Close();
            MessageBox.Show("Восстановление функций Windows Defender завершено!", "DWD", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        //end block

        //Функции с DNS-адаптером
        private void OOflushDNSbutton_Click(object sender, EventArgs e)
        {
            string StartText = OOflushDNSbutton.Text;
            OOflushDNSbutton.Enabled = false;
            OOflushDNSbutton.Text = "Пожалуйста, подождите . . .";

            Cmd($"ipconfig /flushdns & exit /b");

            MessageBox.Show("Кэш DNS Очищен", "DWD", MessageBoxButtons.OK, MessageBoxIcon.Information);

            OOflushDNSbutton.Text = StartText;
            OOflushDNSbutton.Enabled = true;

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
        private void RenewIp4vbutton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(
                "Вы уверены что хотите перенастроить IPv4 адаптер?" +
                "\nЕго работа будет прервана!" +
                "\n\nПрограммы потеряют доступ к интернету, " +
                "что может привести к сбоям. " +
                "\nНо подключение будет восстановлено." +
                "\n\nПродолжить?",
                "DWD : Подтверждение действия",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation
                )
                == DialogResult.Yes)
            {
                string StartText = RenewIp4vbutton.Text;
                RenewIp4vbutton.Enabled = false;
                RenewIp4vbutton.Text = "Пожалуйста, подождите . . .";

                Cmd($"ipconfig /release & ipconfig /Renew & exit /b");

                MessageBox.Show("Перенастройка IPv4-адаптера завершена!\nАдресс IPv4 обновлён.", "DWD", MessageBoxButtons.OK, MessageBoxIcon.Information);

                RenewIp4vbutton.Text = StartText;
                RenewIp4vbutton.Enabled = true;
            }

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

        //Открыть Брандмауэр
        private void OOopenWFbutton_Click(object sender, EventArgs e)
        {
            OOopenWFbutton.Enabled = false;
            Cmd($"start WF.msc & exit /b");
            OOopenWFbutton.Enabled = true;

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

        private void DisableUACbutton_Click(object sender, EventArgs e)
        {
            RegistryKey reg;
            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");

            if ((int)reg.GetValue("EnableLUA") != 0)
            {
                DisableUACbutton.Enabled = false;
                reg.SetValue("EnableLUA", 0);
                MessageBox.Show("Контроль Учётных Записей отключён. \nПерезагрузите компьютер для вступления в силу.", 
                    "DWD", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DisableUACbutton.Enabled = true;
            }
            else
            {
                MessageBox.Show("Не требуется. UAC Уже отключён", "DWD", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            reg.Flush();
            reg.Close();
        }

        private void EnableUACbutton_Click(object sender, EventArgs e)
        {
            RegistryKey reg;
            reg = Registry.LocalMachine.CreateSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System");

            if ((int)reg.GetValue("EnableLUA") != 1)
            {
                DisableUACbutton.Enabled = false;
                reg.SetValue("EnableLUA", 1);
                MessageBox.Show("Контроль Учётных Записей включен. \nПерезагрузите компьютер для вступления в силу.",
                    "DWD", MessageBoxButtons.OK, MessageBoxIcon.Information);
                DisableUACbutton.Enabled = true;
            }
            else
            {
                MessageBox.Show("Не требуется. UAC Уже включен", "DWD" , MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }

            reg.Flush();
            reg.Close();
        }

        //Запрет закрытия формы через панель задач. Перенаправляет дело в другой метод который сделает как надо.
        private void AboutForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            CloseWin();
        }
        //Скрыть, появляющуюся из-за обработчика события закрытия формы,
        //кнопку закрытия формы, которая трижды тут никому уже не нужна.
        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }
    }
}
