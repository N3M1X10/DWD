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
        readonly static System.Reflection.Assembly assemblyBlock = System.Reflection.Assembly.GetExecutingAssembly();
        readonly static FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assemblyBlock.Location);
        readonly static string ProjectVersion = fvi.FileVersion;

        // Класс для чтения/редактирования INI-файлов
        public class INIManager // INI Манагер
        {
            //Конструктор, принимающий путь к INI-файлу
            public INIManager(string aPath)
            {
                path = aPath;
            }

            //Конструктор без аргументов (путь к INI-файлу нужно будет задать отдельно)
            public INIManager() : this("") { }

            //Возвращает значение из INI-файла (по указанным секции и ключу) 
            public string GetPrivateString(string aSection, string aKey)
            {
                //Для получения значения
                StringBuilder buffer = new StringBuilder(SIZE);

                //Получить значение в buffer
                GetPrivateString(aSection, aKey, null, buffer, SIZE, path);

                //Вернуть полученное значение
                return buffer.ToString();
            }

            //Пишет значение в INI-файл (по указанным секции и ключу) 
            public void WritePrivateString(string aSection, string aKey, string aValue)
            {
                //Записать значение в INI-файл
                WritePrivateString(aSection, aKey, aValue, path);
            }

            //Возвращает или устанавливает путь к INI файлу
            public string Path { get { return path; } set { path = value; } }

            //Поля класса
            private const int SIZE = 1024; //Максимальный размер (для чтения значения из файла)
            private string path = null; //Для хранения пути к INI-файлу

            //Импорт функции GetPrivateProfileString (для чтения значений) из библиотеки kernel32.dll
            [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileString")]
            private static extern int GetPrivateString(string section, string key, string def, StringBuilder buffer, int size, string path);

            //Импорт функции WritePrivateProfileString (для записи значений) из библиотеки kernel32.dll
            [DllImport("kernel32.dll", EntryPoint = "WritePrivateProfileString")]
            private static extern int WritePrivateString(string section, string key, string str, string path);
        }
        // end ini
        //readonly static string selfpath = Directory.GetCurrentDirectory();

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
        bool WhatsNewIsSelected = false;
        bool AboutIsSelected = true; //стартовая страница
        private void SwitchToWhatsNew_Click(object sender, EventArgs e)
        {
            //ПЕРЕКЛЮЧИТЬСЯ НА "ЧТО НОВОГО"
            WhatsNewIsSelected = true;
            AboutIsSelected = false;
            TabsSwitcher();
        }
        private void SwitchToAbout_Click(object sender, EventArgs e)
        {
            //ПЕРЕКЛЮЧИТЬСЯ НА "О ПРОГРАММЕ"
            WhatsNewIsSelected = !true;
            AboutIsSelected = true;
            TabsSwitcher();
        }

        //Свитчер окошек
        //Сделан патологически банально, потому что я тупой урод.
        //Отстань!?
        void TabsSwitcher()
        {
            if (AboutIsSelected)
            {
                //Элементы о программе ВКЛЮЧИТЬ
                AboutWrap.Visible = true;
                ProgNameWrapper.Visible = true;

                //Элементы что нового ВЫКЛЮЧИТЬ
                WhatsNewFormHeader.Visible = false;
                WhatsNewPanel.Visible = false;

                //Кнопки переключения ПОМЕНЯТЬ СТИЛИ МЕСТАМИ
                SwitchToWhatsNew.ForeColor = Color.White;
                SwitchToWhatsNew.BackColor = ColorTranslator.FromHtml("#232323");

                SwitchToAbout.BackColor = ColorTranslator.FromHtml("#191919");
                SwitchToAbout.ForeColor = Color.Salmon;
            }

            if (WhatsNewIsSelected)
            {
                //Элементы о программе ВЫКЛЮЧИТЬ
                AboutWrap.Visible = false;
                ProgNameWrapper.Visible = false;

                //Элементы что нового ВКЛЮЧИТЬ
                WhatsNewFormHeader.Visible = true;
                WhatsNewPanel.Visible = true;

                //Кнопки переключения ПОМЕНЯТЬ СТИЛИ МЕСТАМИ
                SwitchToWhatsNew.ForeColor = Color.Salmon;
                SwitchToWhatsNew.BackColor = ColorTranslator.FromHtml("#191919");
                
                SwitchToAbout.BackColor = ColorTranslator.FromHtml("#232323");
                SwitchToAbout.ForeColor = Color.White;
            }

        }

        private void DoAutoDisableCheck_CheckedChanged(object sender, EventArgs e)
        {
            ToggleAutoDisable();
        }
        private void RunWhenStartupCheck_CheckedChanged(object sender, EventArgs e)
        {
            ToggleRunWhenStartup();
        }

        //Реестр

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

        void ToggleAutoDisable()
        {
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Disable Windows Defender");
            reg.SetValue("doAutoDisable", doAutoDisableCheck.Checked);
            reg.Flush();
            reg.Close();
        }

    }
}
