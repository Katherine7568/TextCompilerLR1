using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextCompiler
{
    public class File
    {
        public int index;
        public string fileName;
        public string path;
        public RichTextBox textBox {  get; set; }
        public File(int index, string fileName, string path)
        {
            this.index = index;
            this.fileName = fileName;
            this.path = path;
        }
    }
}



