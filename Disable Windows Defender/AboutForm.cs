using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Disable_Windows_Defender
{
    public partial class AboutForm : Form
    {
        //Получить версию сборки чтобы потом впихнуть куда-нибудь где надо оно
        readonly static System.Reflection.Assembly assemblyBlock = System.Reflection.Assembly.GetExecutingAssembly();
        readonly static FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assemblyBlock.Location);
        readonly static string ProjectVersion = fvi.FileVersion;

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

        private void GithubButton_Click(object sender, EventArgs e)
        {
            //Гитхаб автора
            Process.Start("https://github.com/N3M1X10/DWD");
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

        private void discordButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/2jJ3Qn4");
        }
        private void ShutDownButton_MouseMove(object sender, MouseEventArgs e)
        {
            ShutDownButton.ForeColor = Color.White;
        }
        private void ShutDownButton_MouseLeave(object sender, EventArgs e)
        {
            ShutDownButton.ForeColor = ColorTranslator.FromHtml("#ff8888");
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
    }
}
