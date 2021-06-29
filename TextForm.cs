using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileManagement
{
    public partial class TextForm : Form
    {
        public static bool ischanged = false;       //状态位 -> 用户打开记事本是否进行编辑
        public MainForm mainForm;
        public string filename;
        public TextForm(string Name, MainForm parent)
        {
            mainForm = parent;
            filename = Name;

            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //这是刚加载出来，需要从磁盘读文件
            this.Text = filename + ".txt";
            FCB nowFCB = mainForm.category.searchNode(filename, FCB.TXTFILE, mainForm.currentRoot).fcb;
            string content = mainForm.disk.getFileContent(nowFCB);
            //MessageBox.Show(content);
            if (content != "")
            {
                textBox1.AppendText(content);    //读取保存的文本文件信息
            }
            ischanged = false;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            //触发这个事件说明内容出现改变
            ischanged = true;                        
        }

        private void TextForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (ischanged)
            {
                DialogResult res = MessageBox.Show("是否需要保存该文件？", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (res == DialogResult.Yes)
                {
                    //取得该文件
                    FCB nowFCB = mainForm.category.searchNode(filename, FCB.TXTFILE, mainForm.currentRoot).fcb;
                    FCB oldFCB = new FCB(nowFCB.fileName, nowFCB.type, nowFCB.indexPointer.size, nowFCB.indexPointer.start);
                    int oldSize = nowFCB.indexPointer.size;
                    int oldStart = nowFCB.indexPointer.start;
                   // MessageBox.Show("此文件起始磁盘块" + oldFCB.indexPointer.start.ToString());
                    //取得先前的
                    string content = textBox1.Text.Trim();
                   // MessageBox.Show(content);
                    if (content != "")
                    { 
                        MessageBox.Show("保存成功！"); 
                    }
                    nowFCB.indexPointer.size = textBox1.Text.Trim().Length;
                   // MessageBox.Show(nowFCB.indexPointer.size.ToString());
                    if(oldStart == -1)
                    {
                        //说明是第一次写
                       // MessageBox.Show("执行了giveDiskSpace函数");
                        mainForm.disk.giveDiskSpace(nowFCB, content);
                    }
                    else
                    {
                        //不是第一次写，里面已经又内容了
                        //所以我们先清楚其原来在磁盘中的痕迹，在重新分配空间
                        mainForm.disk.deleteFileContent(oldFCB);
                        mainForm.disk.giveDiskSpace(nowFCB, content);
                    }
                    //MessageBox.Show("当前文件是" + nowFCB.fileName);
                }
                mainForm.clear();
                mainForm.calculateFolderSize(mainForm.currentRoot);
                mainForm.showCurrentRootChild();
            }

        }
    }
}
