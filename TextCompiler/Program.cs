using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextCompiler
{
    public static class Settings
    {
        public static Font font = new Font("Microsoft Sans Serif", 8);
        public static string language = "Русский";
        public static Dictionary<string, string> translations = new Dictionary<string, string>
        {
            { "Compiler", "Компилятор" },
            { "File", "Файл" },
            { "Edit", "Правка" },
            { "Text", "Текст" },
            { "Run", "Пуск" },
            { "Help", "Справка" },
            { "New", "Создать" },
            { "Open", "Открыть" },
            { "Save", "Сохранить" },
            { "Save As", "Сохранить как" },
            { "Undo", "Отменить" },
            { "Redo", "Повторить" },
            { "Cut", "Вырезать" },
            { "Copy", "Копировать" },
            { "Paste", "Вставить" },
            { "Delete", "Удалить" },
            { "Select All", "Выделить все" },
            { "Calling Help", "Вызов справки" },
            { "Exit", "Выход" },
            { "Settings", "Настройки" },
            {"About",  "О программе"},
            {"Grammar", "Грамматика"},
            {"Grammar Classification", "Классификация грамматики" },
            { "Task definition", "Постановка задачи"},
            { "Analysis method", "Метод анализа"},
            { "Error diagnostics", "Диагностика и нейтрализация ошибок"},
            { "Test example", "Тестовый пример" },
            { "References", "Список литературы" },
            { "Source code", "Исходный код программы" },
            { "List of errors", "Список ошибок" },
            { "Output", "Вывод" },
            {"Line", "Строка" },
            {"Error", "Ошибка" },
            {"Localization", "Локализация" },
            {"View", "Вид" }
        };
        public static void LoadSettings()
        {
            float fontSize = Properties.Settings.Default.FontSize;
            language = Properties.Settings.Default.Language;
            if (fontSize >= 8 && fontSize <= 72)
            {
                font = new Font("Microsoft Sans Serif", fontSize);
            }
        }
        public static void SaveSettings()
        {
            Properties.Settings.Default.FontSize = font.Size;
            Properties.Settings.Default.Language = language;
            Properties.Settings.Default.Save();
        }
        private static void UpdateFontRecursive(Control parent, Font newFont)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is RichTextBox rtb)
                {
                    rtb.Font = newFont;
                }
                if (ctrl.HasChildren)
                {
                    UpdateFontRecursive(ctrl, newFont);
                }
            }
        }
        private static void UpdateMenuStrip(MenuStrip menuStrip)
        {
            foreach (ToolStripMenuItem item in menuStrip.Items)
            {
                UpdateMenuItem(item);
            }
        }

        private static void UpdateMenuItem(ToolStripMenuItem item)
        {
            if (translations.ContainsKey(item.Text) || translations.ContainsValue(item.Text))
            {
                item.Text = (language == "Русский") ? translations[item.Text] : translations.FirstOrDefault(x => x.Value == item.Text).Key;
            }
            foreach (ToolStripItem subItem in item.DropDownItems)
            {
                if (subItem is ToolStripMenuItem subMenuItem)
                {
                    UpdateMenuItem(subMenuItem);
                }
            }
        }
        private static void UpdateToolStrip(ToolStrip toolStrip)
        {
            foreach (ToolStripItem item in toolStrip.Items)
            {
                if (item is ToolStripButton toolStripButton)
                {
                    if (translations.ContainsKey(toolStripButton.ToolTipText) || translations.ContainsValue(toolStripButton.ToolTipText))
                    {
                        toolStripButton.ToolTipText = (language == "Русский") ? translations[toolStripButton.ToolTipText] : translations.FirstOrDefault(x => x.Value == toolStripButton.ToolTipText).Key;
                    }
                }
            }
        }
        public static void UpdateDataGridView(DataGridView dataGridView)
        {
            foreach(DataGridViewColumn item in dataGridView.Columns)
            {
                if (translations.ContainsKey(item.HeaderText) || translations.ContainsValue(item.HeaderText))
                {
                    item.HeaderText = (language == "Русский") ? translations[item.HeaderText] : translations.FirstOrDefault(x => x.Value == item.HeaderText).Key;
                }
            }
        }
        public static void UpdateLanguageRecursive(Control ctrl)
        {
            
            foreach (Control childCtrl in ctrl.Controls)
            {
                if (translations.ContainsKey(childCtrl.Text) || translations.ContainsValue(childCtrl.Text))
                {
                    childCtrl.Text = (language == "Русский") ? translations[childCtrl.Text] : translations.FirstOrDefault(x => x.Value == childCtrl.Text).Key;
                }
                if (childCtrl is ToolStrip toolStrip)
                    UpdateToolStrip(toolStrip);
                if(childCtrl is DataGridView dataGridView)
                    UpdateDataGridView(dataGridView);
                if (childCtrl.HasChildren)
                    UpdateLanguageRecursive(childCtrl);
            }
        }
        public static void UpdateLanguage(Form form)
        {
            if (translations.ContainsKey(form.Text) || translations.ContainsValue(form.Text))
            {
                form.Text = (language == "Русский") ? translations[form.Text] : translations.FirstOrDefault(x => x.Value == form.Text).Key;
            }
            UpdateLanguageRecursive(form);
            if (form.MainMenuStrip != null)
            {
                UpdateMenuStrip(form.MainMenuStrip);
            }
        }
        public static void UpdateLineNumberPanelWidth(TabControl tabControl)
        {
            foreach (TabPage tab in tabControl.TabPages)
            {
                SplitContainer sc = tab.Controls.OfType<SplitContainer>().FirstOrDefault();
                if (sc != null)
                {
                    int requiredWidth = TextRenderer.MeasureText("100", font).Width + 10;
                    sc.SplitterDistance = requiredWidth;
                }
            }
        }

        public static void UpdateFont(TabControl tabControl)
        {
            foreach (TabPage page in tabControl.TabPages)
            {
                UpdateFontRecursive(page, font);
            }
        }
    }
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
