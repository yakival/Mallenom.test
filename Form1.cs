using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;


namespace Mallenom.test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            // АВТОРИЗУЕМСЯ И НАЧАЛЬНАЯ ИНИЦИАЛИЗАЦИЯ ТАБЛИЦЫ
            WebAPI.Autorize("guest", "");
            GetData();
        }

        private void GetData()
        {
            Cursor.Current = Cursors.WaitCursor;

            // ФОРМИРУЕМ ПЕРИОД
            DateTime dt1 = DateTime.UtcNow;
            DateTime dt2 = DateTime.UtcNow; dt1 = dt1.AddDays(-7);

            // ЧИТАЕМ ДАННЫЕ
            WebAPI.GetFromPeriod(dt1, dt2);
            Grid.DataSource = WebAPI.dsGrid;

            Cursor.Current = Cursors.Default;
        }

        private void Grid_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;

            Photo.Image = Bitmap.FromStream(WebAPI.GetPhoto(WebAPI.dsGrid[e.RowIndex].id));

            Cursor.Current = Cursors.Default;
        }

        private void RefreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetData();
        }
    }
}
