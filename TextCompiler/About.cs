using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextCompiler
{
    public partial class About : Form
    {
        public About()
        {
            InitializeComponent();
            if(Settings.language == "Русский")
            {
                Text = "О программе";
                label1.Text = "Версия 0.0.1";
                label2.Text = "Автор: Кузнецова Екатерина Евгеньевна";
                label3.Text = "Языковой процессор";
            }
            else if (Settings.language == "English")
            {
                Text = "About";
                label1.Text = "Version 0.0.1";
                label2.Text = "Author: Kuznetsova Ekaterina Evgenevna";
                label3.Text = "Language processor";
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void About_Load(object sender, EventArgs e)
        {

        }
    }
}
