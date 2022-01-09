using PDF_Manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Compression;
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
            // using (StreamReader st = new StreamReader(@"../../../TestData/test001.json", Encoding.GetEncoding("UTF-8")))
            using (StreamReader st = new StreamReader(@"../../../TestData/base64.txt", Encoding.GetEncoding("UTF-8")))
            {
                // テキストファイルをString型で読み込みコンソールに表示
                String str = st.ReadToEnd();

                // base64をデコード
                byte[] a = Convert.FromBase64String(str);
                String stCsvData = Encoding.UTF8.GetString(a);

                // カンマ区切りで分割して配列に格納する
                string[] stArrayData = stCsvData.Split(',');

                // byte 配列に変換する
                byte[] b = new byte[stArrayData.Length];
                for(int i = 0; i < stArrayData.Length; i++){
                    b[i] = Convert.ToByte(stArrayData[i]);
                }

                // gzip解凍
                String line = Unzip(b);

                // データの読み込み
                var p = new PrintInput(line);

                p.createPDF();

                MessageBox.Show("終わりました");
            }
        }

        public string Unzip(byte[] bytes)
        {
            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress))
                {
                    gs.CopyTo(mso);
                    //CopyTo(gs, mso);
                }

                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
        public void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];

            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }

    }
}
