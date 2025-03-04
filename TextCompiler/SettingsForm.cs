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
    public partial class SettingsForm : Form
    {
        public TabControl editTabControl;
        public bool update = false;
        public SettingsForm()
        {
            InitializeComponent();
            numericUpDown1.Value = (decimal)Settings.font.Size;
            label1.Text = (Settings.language == "Русский") ? "Размер шрифта" : "Font Size";
            button1.Text = (Settings.language == "Русский") ? "Сохранить" : "Save";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            float fontSize = (float)numericUpDown1.Value;
            Font font = new Font(Settings.font.FontFamily, fontSize);
            Settings.font = font;
            Settings.SaveSettings();
            this.Close();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
