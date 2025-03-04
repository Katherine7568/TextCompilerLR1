using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextCompiler
{
    public partial class LocForm : Form
    {
        public bool update = false;

        public LocForm()
        {
            InitializeComponent();
            comboBox1.Text = Settings.language;
            label2.Text = (Settings.language == "Русский") ? "Язык" : "Language";
            button2.Text = (Settings.language == "Русский") ? "Сохранить" : "Save";

            this.FormClosing += LocForm_FormClosing; // Добавляем обработчик закрытия
        }

        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void LocForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Settings.language != comboBox1.Text)
            {
                Settings.language = comboBox1.Text;
                update = true;
            }

            Settings.SaveSettings();
            this.DialogResult = DialogResult.OK; // Форма закрывается корректно
            this.Close();
        }
    }
}
