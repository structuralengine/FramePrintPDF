using PDF_Manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDF_Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 読み込みたいテキストを開く
            //using (StreamReader st = new StreamReader(@"C:\Users\sasaco\Documents\PDF_generate\PDF_Test\TestData\test001.json", Encoding.GetEncoding("UTF-8")))
            using (StreamReader st = new StreamReader(@"../../../TestData/test001.json", Encoding.GetEncoding("UTF-8")))
            {
                // テキストファイルをString型で読み込みコンソールに表示
                String line = st.ReadToEnd();

                // データの読み込み
                var p = new PrintInput(line);

                p.CreatePDF();

                //var o = new main(line);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
