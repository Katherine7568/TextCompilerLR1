using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace TextCompiler
{
   
    public partial class Form1 : Form
    {

        const string filter = "Text files(*.txt)|*.txt|All files(*.*)|*.*";
        public int countOpenedFiles = 0;

        ToolStripLabel dateLabel;
        ToolStripLabel infoLabel;
        Timer timer;

        public List<File> files = new List<File>();
        public File file;
        public Form1()
        {
            InitializeComponent();
            openFileDialog1.Filter = filter;
            saveFileDialog1.Filter = filter;
            tabControl1.SelectedIndexChanged += TabControl1_SelectedIndexChanged;
            this.MainMenuStrip = menuStrip1;
            Settings.LoadSettings();
            if (Settings.language != "Русский")
                Settings.UpdateLanguage(this);
            infoLabel = new ToolStripLabel();
            infoLabel.Text = (Settings.language == "Русский") ? "Текущие дата и время: " : "Current time and date: ";
            dateLabel = new ToolStripLabel();
            dateLabel.Text = DateTime.Now.ToString();

            statusStrip1.Items.Add(infoLabel);
            statusStrip1.Items.Add(dateLabel);

            timer = new Timer() { Interval = 1000 };
            timer.Tick += timer_Tick;
            timer.Start();

            KeyDown += Keyboard;
        }
        private void Keyboard(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                try
                {
                    Help help = new Help();
                    help.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (e.Control && e.KeyCode == Keys.N)
            {
                try
                {
                    Create();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (e.Control && e.KeyCode == Keys.O)
            {
                try
                {
                    OpenFile();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (e.Control && e.KeyCode == Keys.S)
            {
                try
                {
                    Save();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else if (e.Control && e.KeyCode == Keys.W)
            {
                try
                {
                    if (tabControl1.TabPages.Count == 0)
                        this.Close();
                    else
                        Exit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            dateLabel.Text = DateTime.Now.ToString();
        }

        private void TabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (countOpenedFiles != 0)
            {
                int index = 0;
                if (tabControl1.TabPages.Count > 1)
                    index = tabControl1.SelectedIndex;
                file = files[index];
            }
        }

        public void ShowEditor(Panel panel, RichTextBox richTextBox)
        {
            panel.Paint += (sender, e) => LineNumberPanel_Paint(sender, e, richTextBox);

            richTextBox.TextChanged += (s, e) => panel.Invalidate();
            richTextBox.VScroll += (s, e) => panel.Invalidate();
            richTextBox.SelectionChanged += (s, e) => panel.Invalidate();
            richTextBox.FontChanged += (s, e) => panel.Invalidate();
        }
        public void InitializeCompiler(File file, string title, string fileText)
        {
            TabPage myTabPage = new TabPage(title);
            tabControl1.TabPages.Add(myTabPage);
            SplitContainer splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                IsSplitterFixed = true,
            };
            int requiredWidth = TextRenderer.MeasureText("100", Settings.font).Width;
            splitContainer.SplitterDistance = requiredWidth;
            splitContainer.SplitterWidth = 1;
            splitContainer.FixedPanel = FixedPanel.Panel1;
            myTabPage.Controls.Add(splitContainer);
            Panel panel = new Panel
            {
                Dock = DockStyle.Left,
                BackColor = SystemColors.Window
            };
            splitContainer.Panel1.Controls.Add(panel);
            RichTextBox richTextBox1 = new RichTextBox
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None
            };
            splitContainer.Panel2.Controls.Add(richTextBox1);
            richTextBox1.Text = fileText;
            ShowEditor(panel, richTextBox1);
            file.textBox = richTextBox1;
            tabControl1.SelectedTab = myTabPage;
            this.file = file;

            Settings.UpdateFont(tabControl1);
        }

        private void LineNumberPanel_Paint(object sender, PaintEventArgs e, RichTextBox richTextBox)
        {
            int firstIndex = richTextBox.GetCharIndexFromPosition(new Point(0, 0));
            int firstLine = richTextBox.GetLineFromCharIndex(firstIndex);

            int lineHeight = TextRenderer.MeasureText("0", richTextBox.Font).Height;
            int y = 0;

            for (int i = firstLine; y < richTextBox.Height; i++)
            {
                y = (i - firstLine) * lineHeight;
                e.Graphics.DrawString((i + 1).ToString(), richTextBox.Font, Brushes.Black, new PointF(5, y));
            }
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        public void OpenFile()
        {
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Title = (Settings.language == "Русский") ? "Открытие" : "Open";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;

            string title = openFileDialog1.SafeFileName;
            string fileName = openFileDialog1.FileName;

            string fileText = System.IO.File.ReadAllText(fileName);
            File file = new File(countOpenedFiles, title, fileName);
            files.Add(file);
            if (countOpenedFiles == 0) tabControl1.TabPages.Clear();
            InitializeCompiler(file, title, fileText);
            countOpenedFiles++;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            file?.textBox.Undo();
        }


        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            file?.textBox.Redo();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Save();
        }

        public void Save()
        {
            if (tabControl1.TabPages.Count > 0)
                System.IO.File.WriteAllText(file.path, file.textBox.Text);
        }

        private void сохранитьКакToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count > 0)
            {
                saveFileDialog1.Title = (Settings.language == "Русский") ? "Сохранить как" : "Save as";
                string fileName = file.fileName;
                saveFileDialog1.FileName = fileName;
                if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                System.IO.File.WriteAllText(saveFileDialog1.FileName, file.textBox.Text);
            }
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Create();
        }
        public void Create()
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Title = (Settings.language == "Русский") ? "Создание" : "Create";
                saveFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";

                saveFileDialog.OverwritePrompt = true;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    System.IO.File.WriteAllText(filePath, "");
                    string fileName = System.IO.Path.GetFileName(saveFileDialog.FileName);
                    string path = saveFileDialog.FileName;
                    File file = new File(countOpenedFiles, fileName, path);
                    files.Add(file);
                    if (countOpenedFiles == 0) tabControl1.TabPages.Clear();
                    InitializeCompiler(file, fileName, null);
                    countOpenedFiles++;
                }
            }
        }

        private void создатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Create();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            file?.textBox.Copy();
        }

        private void копироватьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.Copy();
        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            file?.textBox.Paste();
        }

        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            file?.textBox.Cut();
        }

        private void отменитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.Undo();
        }

        private void повторитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.Redo();
        }

        private void вырезатьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.Cut();
        }

        private void вставитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.Paste();
        }

        private void удалитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (file != null)
                file.textBox.SelectedText = "";
        }

        private void выделитьВсеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            file?.textBox.SelectAll();
        }
        public void Exit()
        {
            string question = (Settings.language == "Русский") ? "Сохранить изменения в файл" : "Save the changes to a file";
            if (System.IO.File.ReadAllText(file.path) != file.textBox.Text)
            {
                DialogResult result = MessageBox.Show($"{question} {file.fileName}?", "Закрытие файла", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    System.IO.File.WriteAllText(file.path, file.textBox.Text);
                }
            }
            countOpenedFiles--;
            files.Remove(file);
            tabControl1.TabPages.Remove(tabControl1.SelectedTab);
        }

        private void выходToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count == 0)
                this.Close();
            else
                Exit();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (var tabPages in tabControl1.TabPages)
            {
                Exit();
            }
        }

        private void настройкиToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void tabControl1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void tabControl1_DragDrop(object sender, DragEventArgs e)
        {
            string[] filesForDrop = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (var fileForDrop in filesForDrop)
            {
                if (Path.GetExtension(fileForDrop) == ".txt")
                {
                    string path = fileForDrop.ToString();
                    string title = Path.GetFileName(path);
                    string fileText = System.IO.File.ReadAllText(path);
                    File file = new File(countOpenedFiles, title, path);
                    files.Add(file);
                    if (countOpenedFiles == 0) tabControl1.TabPages.Clear();
                    InitializeCompiler(file, title, fileText);
                    countOpenedFiles++;
                }
            }
        }

private void вызовСправкиToolStripMenuItem_Click(object sender, EventArgs e)
    {
        // Полный HTML-код справки
        string htmlContent = @"<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Справка - Компилятор</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; margin: 20px; }
        h1 { color: #2c3e50; text-align: center; }
        h2 { color: #34495e; margin-top: 20px; }
        p { margin: 10px 0; }
        b { font-weight: bold; }
        i { font-style: italic; }
        ul { margin: 10px 0; padding-left: 20px; }
    </style>
</head>
<body>
    <h1>Справка</h1>
    <font size='4'>
      <h2>Работа с файлами</h2>
      <h3>Создание нового файла</h3>
      <p>На вкладке <b>Файл</b> выберите <i>Создать</i> (Ctrl+N)</p>
      <h3>Открытие файла</h3>
      <p>На вкладке <b>Файл</b> выберите <i>Открыть</i> (Ctrl+O) или перетащите файл в окно редактирования</p>
      <h3>Сохранение файла</h3>
      <p>На вкладке <b>Файл</b> выберите <i>Сохранить</i> (Ctrl+S) или <i>Сохранить как</i>, чтобы сохранить файл под другим именем</p>
      <h3>Закрытие файла</h3>
      <p>На вкладке <b>Файл</b> выберите <i>Выход</i> (Ctrl+W), чтобы закрыть текущий файл</p>
      
      <!-- Остальное содержимое справки -->
      <h2>Работа с текстом (Правка)</h2>
      <p><b>Отменить/Повторить</b> - отменяет последнее действие или повторяет отмененное действие</p>
      <p><b>Вырезать</b> - вырезает выделенный текст и копирует его в буфер обмена</p>
      <p><b>Копировать</b> - копирует выделенный текст в буфер обмена</p>
      <p><b>Вставить</b> - вставляет текст из буфера обмена в окно редактирования</p>
      <p><b>Удалить</b> - удаляет выделенный текст</p>
      <p><b>Выделить все</b> - выделяет весь текст в окне редактирования</p>
      
      <h2>Заключение</h2>
      <p align='justify'><i><b>Приятного использования!</b></i></p>
    </font>
</body>
</html>";

        try
        {
            // Создаем временный файл справки
            string tempFile = Path.Combine(Path.GetTempPath(), "compiler_help.html");

            // Записываем HTML в файл
            System.IO.File.WriteAllText(tempFile, htmlContent);

            // Открываем в браузере по умолчанию
            Process.Start(new ProcessStartInfo
            {
                FileName = tempFile,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось открыть справку:\n{ex.Message}",
                          "Ошибка",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Error);
        }
    }

    private void toolStripButton10_Click(object sender, EventArgs e)
        {
            Help help = new Help();
            help.ShowDialog();
        }

        private void оПрограммеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            About about = new About();
            about.ShowDialog();
        }

        private void локализацияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                LocForm locForm = new LocForm();
                locForm.ShowDialog();
                if (locForm.update == true)
                {

                    Settings.UpdateLanguage(this);
                    infoLabel.Text = (Settings.language == "Русский") ? "Текущие дата и время:" : "Current time and date";
                }
                Settings.UpdateFont(tabControl1);
                Settings.UpdateLineNumberPanelWidth(tabControl1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при изменении настроек: {ex.Message}");
            }

        }

        private void видToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SettingsForm settingsForm = new SettingsForm();
                settingsForm.ShowDialog();
                if (settingsForm.update == true)
                {

                    Settings.UpdateLanguage(this);
                    infoLabel.Text = (Settings.language == "Русский") ? "Текущие дата и время:" : "Current time and date";
                }
                Settings.UpdateFont(tabControl1);
                Settings.UpdateLineNumberPanelWidth(tabControl1);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка при изменении настроек: {ex.Message}");
            }
        }

        private void справкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }


        private void RunFiniteStateMachine(string input)
        {
            try
            {
                // Очистка и настройка DataGridView
                dataGridView1.Invoke((MethodInvoker)delegate
                {
                    dataGridView1.Columns.Clear();
                    dataGridView1.Columns.Add("Message", "Сообщение");
                    dataGridView1.Columns.Add("Position", "Позиция");
                    dataGridView1.Columns.Add("Line", "Строка");
                    dataGridView1.Columns["Position"].Width = (int)(dataGridView1.Width * 0.2);
                    dataGridView1.Columns["Line"].Width = (int)(dataGridView1.Width * 0.1);
                    dataGridView1.Rows.Clear();
                });

                // Запуск синтаксического анализа в отдельном потоке
                var task = System.Threading.Tasks.Task.Run(() =>
                {
                    SyntaxParser parser = new SyntaxParser();
                    return parser.Parse(input);
                });

                // Ожидание завершения с таймаутом
                if (!task.Wait(TimeSpan.FromSeconds(5)))
                {
                    MessageBox.Show("Синтаксический анализ занял слишком много времени", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var errors = task.Result;

                // Отображение результатов
                dataGridView1.Invoke((MethodInvoker)delegate
                {
                    foreach (var error in errors)
                    {
                        int rowIndex = dataGridView1.Rows.Add(
                            error.Message,
                            error.Position + 1, // +1 для человекочитаемого формата
                            error.Line
                        );

                        dataGridView1.Rows[rowIndex].DefaultCellStyle.BackColor = Color.LightCoral;
                        dataGridView1.Rows[rowIndex].DefaultCellStyle.Font = new Font(dataGridView1.Font, FontStyle.Bold);
                    }

                    dataGridView1.AutoResizeColumns();
                    dataGridView1.Refresh();

                    if (errors.Count > 0)
                    {
                        MessageBox.Show($"Обнаружены синтаксические ошибки: {errors.Count}", "Результат анализа",
                                      MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show("Синтаксический анализ завершен без ошибок", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Критическая ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Разблокируем интерфейс
                пускToolStripMenuItem.Enabled = true;
            }
        }




        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            if (file != null && file.textBox != null)
            {
                string input = file.textBox.Text;
                RunFiniteStateMachine(input);
            }
            else
            {
                MessageBox.Show("Откройте файл перед обработкой.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void пускToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (file != null && file.textBox != null)
            {
                string input = file.textBox.Text;
                RunFiniteStateMachine(input);
            }
            else
            {
                MessageBox.Show("Откройте или создайте файл перед запуском анализа.", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void постановкаЗадачиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // HTML-контент как строковая переменная
            string htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <title>Постановка задачи</title>
    <meta charset='utf-8'>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; margin: 20px; }
        h1 { color: #2c3e50; }
        h2 { color: #34495e; margin-top: 20px; }
        code { background: #f4f4f4; padding: 2px 5px; border-radius: 3px; }
        pre { background: #f4f4f4; padding: 10px; border-radius: 5px; overflow-x: auto; }
        .example { margin-left: 20px; font-style: italic; }
    </style>
</head>
<body>
    <h1>Постановка задачи</h1>
    
    <p>Структурой называется составной тип данных, объединяющий набор значений различных типов под одним именем. Для объявления структур в языке Rust используется служебное слово <code>struct</code>.</p>
    
    <h2>Формат записи:</h2>
    <pre>struct имя_структуры {
    имя_поля1: тип1,
    имя_поля2: тип2,
    ...
};</pre>
    
    <h2>Примеры допустимых записей:</h2>
    
    <p>1) Структура с полями базовых типов:</p>
    <div class='example'>
    <pre>struct Student {
    name: String,
    roll: u64,
    dept: String,
};</pre>
    </div>
    
    <p>2) Структура с числовыми полями:</p>
    <div class='example'>
    <pre>struct Point {
    x: i32,
    y: i32,
};</pre>
    </div>
    
    <p>В связи с разработанной автоматной грамматикой G[‹Defs›] синтаксический анализатор будет считать верными следующие записи структур:</p>
    
    <p>1) &ldquo;struct Book { title: String, pages: u32 };&rdquo;</p>
    <p>2) &ldquo;struct User { id: u64, name: String, active: bool };&rdquo;</p>
    <p>3) &ldquo;struct Coord { x: f64, y: f64, z: f64 };&rdquo;</p>
    
    <p>Справка (руководство пользователя) представлена в Приложении А. Информация о программе представлена в Приложении Б.</p>
</body>
</html>";

            try
            {
                // Создаём временный файл
                string tempFile = Path.Combine(Path.GetTempPath(), "task_description.html");
                System.IO.File.WriteAllText(tempFile, htmlContent, Encoding.UTF8);

                // Открываем в браузере по умолчанию
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void грамматикаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // HTML-контент как строковая переменная с правильным экранированием
            string htmlContent = @"<!DOCTYPE html>
<html>
<head>
    <title>Грамматика структур Rust</title>
    <meta charset='utf-8'>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; margin: 20px; }
        h1 { color: #2c3e50; }
        h2 { color: #34495e; margin-top: 20px; }
        code { background: #f4f4f4; padding: 2px 5px; border-radius: 3px; }
        pre { background: #f4f4f4; padding: 10px; border-radius: 5px; overflow-x: auto; }
        .production { margin-left: 20px; font-family: monospace; }
        .symbol { color: #8e44ad; font-weight: bold; }
    </style>
</head>
<body>
    <h1>Разработка грамматики</h1>
    
    <p>Определим грамматику структур на языке Rust G[‹Def›] в нотации Хомского с продукциями P:</p>
    
    <div class='production'>1. <span class='symbol'>‹Def›</span> → ""struct "" <span class='symbol'>‹STRUCT›</span></div>
    <div class='production'>2. <span class='symbol'>‹STRUCT›</span> → ' ' <span class='symbol'>‹Name›</span></div>
    <div class='production'>3. <span class='symbol'>‹Name›</span> → <span class='symbol'>‹Letter›</span> <span class='symbol'>‹NameRem›</span></div>
    <div class='production'>4. <span class='symbol'>‹NameRem›</span> → <span class='symbol'>‹Letter›</span> <span class='symbol'>‹NameRem›</span> | <span class='symbol'>‹Digit›</span> <span class='symbol'>‹NameRem›</span> | '{' <span class='symbol'>‹x›</span></div>
    <div class='production'>5. <span class='symbol'>‹x›</span> → <span class='symbol'>‹Letter›</span> <span class='symbol'>‹XRem›</span></div>
    <div class='production'>6. <span class='symbol'>‹XRem›</span> → <span class='symbol'>‹Letter›</span> <span class='symbol'>‹XRem›</span> | <span class='symbol'>‹Digit›</span> <span class='symbol'>‹XRem›</span> | ':' <span class='symbol'>‹y›</span></div>
    <div class='production'>7. <span class='symbol'>‹y›</span> → 'type' <span class='symbol'>‹Field›</span></div>
    <div class='production'>8. <span class='symbol'>‹Field›</span> → ',' <span class='symbol'>‹x›</span></div>
    <div class='production'>9. <span class='symbol'>‹x›</span> → '}' <span class='symbol'>‹End›</span></div>
    <div class='production'>10. <span class='symbol'>‹End›</span> → "";""</div>
    <div class='production'>11. <span class='symbol'>‹Type›</span> → ""String"" | ""u64"" | ""i32"" | ""f64"" | ""bool"" | <span class='symbol'>‹Name›</span></div>
    <div class='production'>12. <span class='symbol'>‹Letter›</span> → ""a""|""b""|...|""z""|""A""|""B""|...|""Z""</div>
    <div class='production'>13. <span class='symbol'>‹Digit›</span> → ""0""|""1""|...|""9""</div>
    
    <h2>Составляющие грамматики G[‹Def›]:</h2>
    <ul>
        <li>Z = <span class='symbol'>‹Def›</span>;</li>
        <li>V<sub>T</sub> = {a, b, c, ..., z, A, B, C, ..., Z, _, :, =, +, -, ;, ., 0, 1, 2, ..., 9};</li>
        <li>V<sub>N</sub> = {<span class='symbol'>‹Def›</span>, <span class='symbol'>‹STRUCT›</span>, <span class='symbol'>‹Name›</span>, 
            <span class='symbol'>‹NameRem›</span>, <span class='symbol'>‹x›</span>, <span class='symbol'>‹XRem›</span>, 
            <span class='symbol'>‹y›</span>, <span class='symbol'>‹Field›</span>, <span class='symbol'>‹End›</span>}.</li>
    </ul>
</body>
</html>";

            try
            {
                // Создаём временный файл
                string tempFile = Path.Combine(Path.GetTempPath(), "grammar_description.html");
                System.IO.File.WriteAllText(tempFile, htmlContent, Encoding.UTF8);

                // Открываем в браузере по умолчанию
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void классификацияГрамматикиToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <title>Классификация грамматики</title>
    <meta charset='utf-8'>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        h1 { color: #2c3e50; }
    </style>
</head>
<body>
    <h1>Классификация грамматики</h1>
    <p>Грамматика G[‹Def›] является автоматной.</p>
    <p>Правила (1)-(17) - праворекурсивные продукции (A → aB | a | ε).</p>
</body>
</html>";

            try
            {
                string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "grammar_classification.html");
                System.IO.File.WriteAllText(tempFile, htmlContent, System.Text.Encoding.UTF8);
                System.Diagnostics.Process.Start(tempFile);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка: {ex.Message}",
                                                  "Ошибка",
                                                  System.Windows.Forms.MessageBoxButtons.OK,
                                                  System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void ShowHtml(string htmlContent, string fileName)
        {
            try
            {
                string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), fileName);
                System.IO.File.WriteAllText(tempFile, htmlContent, System.Text.Encoding.UTF8);

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка: {ex.Message}",
                               "Ошибка",
                               System.Windows.Forms.MessageBoxButtons.OK,
                               System.Windows.Forms.MessageBoxIcon.Error);
            }
        }


        private void методАнализаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string imagePath1 = Path.Combine(Path.GetTempPath(), "Рисунок1.png");
            Properties.Resources.Рисунок1.Save(imagePath1);
            string imagePath2 = Path.Combine(Path.GetTempPath(), "Рисунок2.png");
            Properties.Resources.Рисунок2.Save(imagePath2);


            string htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Метод анализа</title>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial; margin: 20px; line-height: 1.6; }}
        h1 {{ color: #2c3e50; border-bottom: 1px solid #eee; }}
        .image-container {{ margin: 20px 0; text-align: center; }}
        img {{ max-width: 100%; border: 1px solid #ddd; }}
        .note {{ font-style: italic; color: #666; }}
        pre {{ background: #f5f5f5; padding: 10px; }}
    </style>
</head>
<body>
    <h1>Метод анализа</h1>
    <p>Грамматика G[&lt;Def&gt;] является автоматной.</p>
     <div class='rules'>
        <p>Для построения графа введем в правила 13-14 нетерминал &lt;END&gt;:</p>
        <pre>13. &lt;x&gt; → '}}' &lt;End&gt;
14. &lt;End&gt; → &quot;;&quot;</pre>
    </div>
    <div class='image-container'>
        <img src='file:///{imagePath1.Replace(@"\", "/")}' alt='Диаграмма'>
        <div class='note'>Рисунок 1 – Диаграмма состояний сканера</div>
    </div>
    
    <div class='image-container'>
        <img src='file:///{imagePath2.Replace(@"\", "/")}' alt='Граф'>
        <div class='note'>Рисунок 2 – Граф G[Феб]</div>
    </div>
    <div class='rules'>
        <p><strong>Сплошные стрелки</strong> - синтаксически верный разбор</p>
        <p><strong>Пунктирные стрелки</strong> - состояние ошибки (ERROR)</p>
        <p><strong>Состояние END</strong> - успешное завершение разбора</p>
    </div>
</body>
</html>";

            ShowHtml(htmlContent, "analysis_method.html");
        }




        private void тестовыйПримерToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string imagePath3 = Path.Combine(Path.GetTempPath(), "Рисунок3.png");
            Properties.Resources.Рисунок3.Save(imagePath3);
            string imagePath4 = Path.Combine(Path.GetTempPath(), "Рисунок4.png");
            Properties.Resources.Рисунок4.Save(imagePath4);
            string imagePath5 = Path.Combine(Path.GetTempPath(), "Рисунок5.png");
            Properties.Resources.Рисунок5.Save(imagePath5);

            string htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <title>Тестовые примеры</title>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial; margin: 20px; line-height: 1.6; }}
        h1 {{ color: #2c3e50; border-bottom: 1px solid #eee; padding-bottom: 10px; }}
        .example {{ margin-bottom: 40px; }}
        img {{ max-width: 100%; border: 1px solid #ddd; }}
        .caption {{ font-style: italic; color: #666; margin-top: 8px; }}
    </style>
</head>
<body>
    <h1>Тестовые примеры</h1>
    <p>На рисунках 1-3 представлены тестовые примеры запуска разработанного лексического анализатора объявления и определения структуры на языке Rust.</p>
    
    <div class='example'>
        <img src='file:///{imagePath3.Replace(@"\", "/")}' alt='Пример 1'>
        <div class='caption'>Рисунок 1 – Тестовый пример 1</div>
    </div>
    
    <div class='example'>
        <img src='file:///{imagePath4.Replace(@"\", "/")}' alt='Пример 2'>
        <div class='caption'>Рисунок 2 – Тестовый пример 2</div>
    </div>
    
    <div class='example'>
        <img src='file:///{imagePath5.Replace(@"\", "/")}' alt='Пример 3'>
        <div class='caption'>Рисунок 3 – Тестовый пример 3</div>
    </div>
</body>
</html>";

            ShowHtml(htmlContent, "test_examples.html");
        }

        private void списокЛитературыToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string htmlContent = @"
<!DOCTYPE html>
<html>
<head>
    <title>Список литературы</title>
    <meta charset='utf-8'>
    <style>
        body {
            font-family: Arial, sans-serif;
            margin: 20px;
            line-height: 1.6;
        }
        h1 {
            color: #2c3e50;
            text-align: center;
            margin-bottom: 30px;
        }
        .literature-list {
            max-width: 800px;
            margin: 0 auto;
        }
        .source {
            margin-bottom: 15px;
            text-align: justify;
        }
        .source-number {
            font-weight: bold;
            margin-right: 5px;
        }
        a {
            color: #0066cc;
            text-decoration: none;
        }
        a:hover {
            text-decoration: underline;
        }
    </style>
</head>
<body>
    <div class='literature-list'>
        <h1>СПИСОК ИСПОЛЬЗОВАННЫХ ИСТОЧНИКОВ</h1>
        
        <div class='source'>
            <span class='source-number'>1.</span> Шорников Ю.В. Теория и практика языковых процессоров : учеб. пособие / Ю.В. Шорников. – Новосибирск: Изд-во НГТУ, 2004.
        </div>
        
        <div class='source'>
            <span class='source-number'>2.</span> Gries D. Designing Compilers for Digital Computers. New York, Jhon Wiley, 1971. 493 p.
        </div>
        
        <div class='source'>
            <span class='source-number'>3.</span> Теория формальных языков и компиляторов [Электронный ресурс] / Электрон. дан. 
            <a href='https://dispace.edu.nstu.ru/didesk/course/show/8594' target='_blank'>https://dispace.edu.nstu.ru/didesk/course/show/8594</a>, свободный. Яз.рус. (дата обращения 01.04.2021).
        </div>
    </div>
</body>
</html>";

            try
            {
                // Создаем временный файл
                string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "literature_list.html");
                System.IO.File.WriteAllText(tempFile, htmlContent, System.Text.Encoding.UTF8);

                // Открываем в браузере по умолчанию
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempFile,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show($"Ошибка при открытии списка литературы: {ex.Message}",
                                "Ошибка",
                                System.Windows.Forms.MessageBoxButtons.OK,
                                System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        private void исходныйКодПрограммыToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
    
}
