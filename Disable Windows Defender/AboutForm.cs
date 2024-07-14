using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Disable_Windows_Defender
{
    public partial class AboutForm : Form
    {
        //Получить версию сборки чтобы потом впихнуть в заголовок окна
        readonly static System.Reflection.Assembly assemblyBlock = System.Reflection.Assembly.GetExecutingAssembly();
        readonly static FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assemblyBlock.Location);
        readonly static string ProjectVersion = fvi.FileVersion;

        public AboutForm()
        {
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

            VersionLabel.Text = "Версия: " + ProjectVersion;

        }

        private void GithubButton_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/N3M1X10/DWD");
        }

        private void ShutDownButton_Click(object sender, EventArgs e)
        {
            CloseProg();
        }
        async void CloseProg()
        {
            Opacity = 1;
            Timer timer = new Timer();
            timer.Tick += new EventHandler((sender, e) =>
            {
                if ((Opacity -= 0.05d) <= 0) timer.Stop();
            });
            timer.Interval = 1;
            timer.Start();
            await Task.Delay(500);
            Application.Exit();
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

    }
}
