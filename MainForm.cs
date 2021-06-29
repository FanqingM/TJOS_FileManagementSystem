using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.IO;
using System.Xml;
using static FileManagement.Category;
using static FileManagement.Disk;

//treeview1是目录树，里面只有文件名字
//treeview2是文件树，里面的记录是FCB
// treeView2是主要工作区，希望实现文件预览，以及双击进入新文件夹的页面，以及双击文本文件出现内容，右键点击出现重命名和删除选项
// treeview2的树形结构是体现在UI上的
// 对于逻辑上的属性结构定义在catagory里，其内还需要定义他的Node节点（
// 对于新建的文件or文件夹都是在当前路径下的操作，体现了当前目录的思想，即记录当前根节点，避免了多次IO
namespace FileManagement
{
    //写文件，我们考虑写道内存的同时，在一个配置文件中也写入这些信息，因为内存在程序结束后释放，这个配置文件类似于磁盘
    //用于下次恢复
    public partial class MainForm : Form
    {
        private const int w1 = 17;
        private const int w2 = 39;
        //格式控制
        public Category category = new Category();                  //创建目录
        public Category.Node rootNode = new Category.Node();        //目录的根节点
        public Category.Node currentRoot = new Category.Node();     //当前根节点为root
        public Disk disk = new Disk(5096, 2);                     //申请内存空间
        public string Path;                                         //指出当前路径

        public MainForm()
        {
            this.Path = "";
            FCB root = new FCB("root", FCB.FOLDER, 0);        //作为一个根文件夹，在这个文件加底下的path就是""
            rootNode = new Category.Node(root);
            currentRoot = rootNode;
            category.root = rootNode;
            disk = new Disk(5096, 2);       //申请内存空间
            readMyFCBAndCatagory();
            readMyDisk();                   //从磁盘中读目录表，文件内容以及FCB信息FAT信息，位图信息，diskcontent信息等
            //readMyFCBAndCatagory();
            InitializeComponent();
            //展示UI一定放在这个下面
            //对于不考虑目录的一个文件需要写下面信息还有磁盘的剩余空间和剩余磁盘块
            /* FCB newNode = new FCB("test", FCB.TXTFILE, 0);
             category.createFile(newNode, currentRoot);
             newNode.type = FCB.TXTFILE;
             newNode.fileName = "test";
             newNode.indexPointer.start = 0;
             newNode.indexPointer.size = 9;
             disk.MAP[0, 0] = 1;
             disk.MAP[0, 1] = 1;
             disk.MAP[0, 2] = 1;
             disk.MAP[0, 3] = 1;
             disk.MAP[0, 4] = 1;
             disk.FAT[0] = 1;
             disk.FAT[1] = 2;
             disk.FAT[2] = 3;
             disk.FAT[3] = 4;
             disk.FAT[4] = -1;
             disk.DiskContent[0, 0] = "ad";
             disk.DiskContent[0, 1] = "wa";
             disk.DiskContent[0, 2] = "wd";
             disk.DiskContent[0, 3] = "aw";
             disk.DiskContent[0, 4] = "d";
             FCB newNode2 = new FCB("test2", FCB.TXTFILE, 0);
             category.createFile(newNode2, currentRoot);
             newNode2.type = FCB.TXTFILE;
             newNode2.fileName = "test2";
             newNode2.indexPointer.start = 5;
             newNode2.indexPointer.size = 10;
             disk.MAP[0, 5] = 1;
             disk.MAP[0, 6] = 1;
             disk.MAP[0, 7] = 1;
             disk.MAP[0, 8] = 1;
             disk.MAP[0, 9] = 1;
             disk.FAT[5] = 6;
             disk.FAT[6] = 7;
             disk.FAT[7] = 8;
             disk.FAT[8] = 9;
             disk.FAT[9] = -1;
             disk.DiskContent[0, 5] = "ad";
             disk.DiskContent[0, 6] = "wa";
             disk.DiskContent[0, 7] = "wd";
             disk.DiskContent[0, 8] = "aw";
             disk.DiskContent[0, 9] = "df";*/
            currentRoot = rootNode;
            showCurrentRootChild();
        }

        public void showCurrentRootChild()
        {
            if (currentRoot.firstChild != null)
            {
                Node temp = currentRoot.firstChild;
                //MessageBox.Show("当前根节点第一个孩子" + temp.fcb.fileName);
                while (temp != null)
                {
                    TreeNode tn = new TreeNode();
                    tn.Name = temp.fcb.fileName;
                    tn.Text = temp.fcb.fileName;
                    if (temp.fcb.type == FCB.FOLDER)
                    {
                        tn.Tag = 1;
                        tn.ImageIndex = 1;
                        tn.SelectedImageIndex = 1;
                        for (int i = temp.fcb.fileName.Length; i < w1; i++)
                        {
                            tn.Text += " ";
                        }
                        tn.Text += "folder";
                        for (int i = w1 + 6; i < w2; i++)
                        {
                            tn.Text += " ";
                        }
                       // tn.Text += temp.fcb.indexPointer.size.ToString();
                    }
                    else
                    {
                        tn.Tag = 0;
                        tn.ImageIndex = 0;
                        tn.SelectedImageIndex = 0;
                        for (int i = temp.fcb.fileName.Length; i < w1; i++)
                        {
                            tn.Text += " ";
                        }
                        tn.Text += "txt";
                        for (int i = w1 + 3; i < w2; i++)
                        {
                            tn.Text += " ";
                        }
                        tn.Text += temp.fcb.indexPointer.size.ToString();
                    }
                    temp = temp.nextBrother;
                    treeView2.Nodes.Add(tn);
                }
            }
            else
            {
                return;
            }
        }

        public void calculateFolderSize(Node node)
        {
            if(node.firstChild == null)
            {
                node.fcb.indexPointer.size = 0;
            }
            else
            {
                Node temp = node.firstChild;
                node.fcb.indexPointer.size = 0;
                while (temp!=null)
                {
                    node.fcb.indexPointer.size += temp.fcb.indexPointer.size;
                    temp = temp.nextBrother;
                }
            }
        }

        public void updateCurrentRoot(string name)
        {
            Node temp = currentRoot.firstChild;
            //第一个孩子不为空，看看第一个孩子充不重复
            if (temp.fcb.fileName == name)
            {
                //这里说明找到相同的
                currentRoot = temp;
            }
            while (temp != null)
            {
                if (temp.fcb.fileName == name && temp.fcb.type == FCB.FOLDER)
                {
                    //这里说明找到相同的
                    currentRoot = temp;
                    break;
                }
                temp = temp.nextBrother;
            }
        }

        public void newUI()
        {
            //根据当前的根节点重新更新主界面的UI
            //此操作与逻辑无关
            //应当用于双击文件夹后，更新path和currentroot的逻辑操作后，进行的UI操作
        }
        public void createFolder()
        {
            string str = Interaction.InputBox("请输入文件的名称", "字符串", "", 100, 100);
            TreeNode tn = new TreeNode();
            tn.Tag = 1;
            tn.ImageIndex = 1;
            tn.SelectedImageIndex = 1;
            if (str != "")
            {

                // 先判断当前目录下有无同名同类型
                // 在主界面中直接nodes.add就行了
                // 目录树更新可以先clear，在重新渲染
                tn.Name = str;
                tn.Text = str;
                for(int i=str.Length;i<w1;i++)
                {
                    tn.Text += " ";
                }
                tn.Text += "folder";
                for(int i=w1+6;i<w2;i++)
                {
                    tn.Text += " ";
                }
                //tn.Text += "0";
                FCB newNode = new FCB(str, FCB.FOLDER, 0);
                if (category.isSameFile(newNode, currentRoot))
                {
                    MessageBox.Show("有重复的");
                }
                else
                {
                    category.createFile(newNode, currentRoot);
                    treeView2.Nodes.Add(tn);                     //UI上主界面的改变
                }
            }
            else
            {
                MessageBox.Show("文件名不能为空");
            }
        }
        public void createFile()
        {
            string str = Interaction.InputBox("请输入文件的名称", "字符串", "", 100, 100);
            TreeNode tn = new TreeNode();
            tn.Tag = 0;
            tn.ImageIndex = 0;
            tn.SelectedImageIndex = 0;
            if (str != "")
            {
                // 先判断当前目录下有无同名同类型
                // 在主界面中直接nodes.add就行了
                // 目录树更新可以先clear，在重新渲染
                tn.Text = str;
                tn.Name = str;
                for (int i = str.Length; i < w1; i++)
                {
                    tn.Text += " ";
                }
                tn.Text += "txt";
                for (int i = w1 + 3; i < w2; i++)
                {
                    tn.Text += " ";
                }
                tn.Text += "0";
                FCB newNode = new FCB(str, FCB.TXTFILE, 0);
                if (category.isSameFile(newNode, currentRoot))
                {
                    MessageBox.Show("有重复的");
                }
                else
                {
                    category.createFile(newNode, currentRoot);
                    treeView2.Nodes.Add(tn);                     //UI上主界面的改变
                }

                //下面是逻辑上的改变
                //给currentNode加孩子，需要判断他有没有孩子，没有就是firstChild，否则就是孩子们的nextBrother
            }
            else
            {
                MessageBox.Show("文件名不能为空");
            }
        }

        public void deleteFile()
        {
            ;
        }

        public void deleteFolder()
        {
            ;
        }
        private void button1_Click(object sender, EventArgs e)
        {

        }
        // treeview1是目录页面
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }
        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }
        //新建文件夹按钮
        //新建文件夹和文件也应该注意一下节点的tag
        private void button2_Click(object sender, EventArgs e)
        {
            //test
            /*Button bt = (Button)sender;  //当前点击的按钮
            treeView1.Nodes.Add("dadwa");
            treeView2.Nodes.Add(bt.Text);*/
            createFolder();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }


        //新建文件按钮
        private void button3_Click(object sender, EventArgs e)
        {
            //treeView2.Nodes.Add("folder");
            createFile();
        }

        // treeview2是主界面
        private void treeView2_MouseDown(object sender, TreeViewEventArgs e)
        {

        }
        //格式化窗口
        private void button1_Click_1(object sender, EventArgs e)
        {
            //UI上
            DialogResult res = MessageBox.Show("确定进行格式化操作？", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (res == DialogResult.Yes)
            {
                for (int i = 0; i < disk.blockNum; i++)
                {
                    disk.FAT[i] = -1;
                }
                for (int i = 0; i < disk.row; i++)
                {
                    for (int j = 0; j < disk.WORD; j++)
                    {
                        disk.MAP[i, j] = 0;
                        disk.DiskContent[i, j] = "";
                    }
                }
                currentRoot = rootNode;
                category.deleteAll(currentRoot);
                treeView2.Nodes.Clear();
                showCurrentRootChild();
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            //查找操作默认是在当前路径下查找
            string str = Interaction.InputBox("请输入要查找文件的名称", "文件检索", "", 100, 100);
            //下面是查找操作
            int cnt = 0;
            Node temp = currentRoot.firstChild;
            while(temp!=null)
            {
                if(temp.fcb.fileName == str)
                {
                    cnt++;
                }
                temp = temp.nextBrother;
            }
            if(cnt == 0)
            {
                MessageBox.Show("当前目录没有" + str + "同名文件");
            }
            else
            {
                MessageBox.Show("当前目录与" + str + "同名文件有" + cnt.ToString() + "个");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string str = Interaction.InputBox("请基于当前路径给出您想跳转的路径", "路径跳转", "", 100, 100);
            bool flag = true;
            bool flag2 = true;
            for(int i=0;i<str.Length;i++)
            {
                if(!((str[i]<='Z'&&str[i]>='A')||(str[i]<='z'&&str[i]>='a')||(str[i] == '\\')||(str[i]>='0'&&str[i]<='9')))
                {
                    //MessageBox.Show(str[i].ToString());
                    flag = false;
                    break;
                }
            }
            if(!flag)
            {
                MessageBox.Show("输入路径非法");
            }
            else
            {
                string[] temp;
                temp = str.Split('\\');
                Node temp3 = new Node();
                for(int i=0;i<temp.Length;i++)
                {
                    Node temp2 = category.searchNode(temp[i], currentRoot);
                    if(temp2!=null&&temp2.fcb.type == FCB.FOLDER)
                    {
                        temp3 = temp2;
                    }
                    else
                    {
                        flag2 = false;
                        break;
                    }
                }
                if(flag2)
                {
                    currentRoot = temp3;
                    for(int i=0;i<temp.Length;i++)
                    {
                        Path += "\\";
                        Path += temp[i];
                    }
                    textBox1.Text = Path;
                    treeView2.Nodes.Clear();
                    showCurrentRootChild();
                }
                else
                {
                    MessageBox.Show("您所输入的路径中包含非文件夹或者包含当前不存在的文件");
                }
            }
        }

        //按恢复先前按钮进行恢复操作
        private void button6_Click(object sender, EventArgs e)
        {
            FCB newNode = new FCB("test", FCB.TXTFILE, 0);
            category.createFile(newNode, currentRoot);
            showCurrentRootChild();
        }

        //是当前目录
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void treeView2_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //右键选择删除或者重命名
                //MessageBox.Show("右键");
                // DialogResult res = MessageBox.Show("确定删除该文件？","提示", MessageBoxButtons.YesNoCancel,MessageBoxIcon.Warning

                //删除
                TreeNode tn = treeView2.SelectedNode;
                //MessageBox.Show(tn.Name);
                DialogResult res = MessageBox.Show("确定删除该文件？", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (res == DialogResult.Yes)
                {
                    if (tn.Tag.ToString() == "0")
                    {
                        string name = tn.Name;
                        //MessageBox.Show(name);
                        //我们直接找当前根目录底下，名字相同的文本文件，在目录树里删除他，在更新UI
                        //删除第一个孩子节点后目录为空的这个情况出错了
                        //同时改目录后还要改磁盘
                        Node fileNode = category.searchNode(name, 0, currentRoot);
                        MessageBox.Show("当前选中"+fileNode.fcb.fileName+"如果不是目标，请先左键单击目标结点选中，再右键单击");
                        if(fileNode.fcb.indexPointer.start!=-1)
                        {
                            disk.deleteFileContent(fileNode.fcb);
                        }
                        category.deleteFile(name, 0, currentRoot);
                        treeView2.Nodes.Clear();
                        if (currentRoot.firstChild != null)
                        {
                           // calculateFolderSize(currentRoot);
                            //重新计算所在文件夹的大小
                            showCurrentRootChild();
                        }
                    }
                    else
                    {
                        //和上面一样逻辑，只不过删除文件夹涉及到递归删除
                        category.deleteFolder(Name, 1, currentRoot);
                        treeView2.Nodes.Clear();
                        showCurrentRootChild();
                    }
                }
            }
        }

        private void treeView2_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //点击后进入新文件夹或者打开文本文件
            //string str = Interaction.InputBox("请输入文件的名称", "新建文件", "", 100, 100);
            TreeNode tn = treeView2.SelectedNode;
            //MessageBox.Show(tn.Tag.ToString());
            //根据其类型判断点击后的效果
            if (tn.Tag.ToString() == "0")
            {
                //点开文本文件仅仅是一个弹窗。然后会更改文本文件的内容，大小等，并不会更新当前根节点以及当前路径
                //MessageBox.Show(this.Name);
                //这个this是mainform
                TextForm textForm = new TextForm(tn.Name, this);
                textForm.Show();
            }
            else
            {
                //打开当前文件夹并且在更新新的根节点，更新新的主界面的UI，更新新的path
                string name = tn.Name;
                Path += "\\" + tn.Name;
                textBox1.Text = Path;
                treeView2.Nodes.Clear();
                //更新当前根节点
                updateCurrentRoot(tn.Name);
                //展示当前跟节点下的内容
                showCurrentRootChild();
                //MessageBox.Show(currentRoot.fcb.fileName);
            }
        }

        public void clear()
        {
            treeView2.Nodes.Clear();
        }

        private void treeView2_AfterSelect(object sender, TreeViewEventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (currentRoot == rootNode)
            {
                MessageBox.Show("不能再后退了，到底了");
            }
            else
            {
                Path = Path.Replace("\\" + currentRoot.fcb.fileName, "");
                calculateFolderSize(currentRoot);
                currentRoot = currentRoot.parent;
                //MessageBox.Show("当前根节点位" + currentRoot.fcb.fileName);
                textBox1.Text = Path;
                treeView2.Nodes.Clear();
                showCurrentRootChild();
            }

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }


        //下面是读写配置文件的操作

        //由于FCB本质上存在于目录树的节点中，所以我们在写他的时候连带着目录树一起写了
        //这里很重要的就是顺序问题，我们采用从根节点开始，逐级写，然后在当前根节点底下create
        public void writeMyFCBAndCatagory()
        {
            //先做测试，只写root底下的孩子
            //这里我们采用BFS来写
            //我们用一个队列
            if (category.root == null)
            {
                //MessageBox.Show("当前目录根节点为空");
                ;
            }
            else if (category.root.firstChild == null)
            {
                // MessageBox.Show("当前目录是空的");
                ;
            }
            else
            {
                //只写了root节点下的，做测试
                //这是写以及目录
                Queue<Node> q = new Queue<Node>();
                Node temp;
                Node mid = category.root.firstChild;
                string path = ".\\MyFCBAndCatagory.txt";
                using (StreamWriter sw = File.CreateText(path))
                {
                    //写第一级
                    //sw.WriteLine("$");
                   // sw.WriteLine(temp.parent.fcb.fileName);
                    //sw.WriteLine(temp.parent.fcb.type.ToString());
                    //sw.WriteLine(temp.parent.fcb.)
                    /*int cnt = 0;
                    while (temp != null)
                    {
                        sw.WriteLine(temp.fcb.fileName);
                        sw.WriteLine(temp.fcb.type.ToString());
                        sw.WriteLine(temp.fcb.indexPointer.start.ToString());
                        sw.WriteLine(temp.fcb.indexPointer.size.ToString());
                        if (temp.fcb.type == FCB.FOLDER && temp.firstChild != null)
                        {
                            q.Enqueue(temp);
                        }
                        temp = temp.nextBrother;
                    }*/
                    q.Enqueue(category.root);

                    while (q.Count() != 0)
                    {
                        temp = q.Dequeue();
                        temp = temp.firstChild;
                        sw.WriteLine("$");
                        sw.WriteLine(temp.parent.fcb.fileName);
                        while (temp != null)
                        {
                            if (temp.fcb.type == FCB.FOLDER && temp.firstChild != null)
                            {
                                q.Enqueue(temp);
                            }
                            sw.WriteLine(temp.fcb.fileName);
                            sw.WriteLine(temp.fcb.type.ToString());
                            sw.WriteLine(temp.fcb.indexPointer.start.ToString());
                            sw.WriteLine(temp.fcb.indexPointer.size.ToString());
                            temp = temp.nextBrother;
                        }
                    }
                    //写第二级
                    /*while(mid!=null)
                    {
                        if(mid.fcb.type == FCB.FOLDER && mid.firstChild!=null)
                        {
                             Node temp2 = mid.firstChild;
                             sw.WriteLine("$");
                             sw.WriteLine(temp2.parent.fcb.fileName);
                             //sw.WriteLine(temp.parent.fcb.type.ToString());
                             //sw.WriteLine(temp.parent.fcb.)
                             while (temp2 != null)
                             {
                                 sw.WriteLine(temp2.fcb.fileName);
                                 sw.WriteLine(temp2.fcb.type.ToString());
                                 sw.WriteLine(temp2.fcb.indexPointer.start.ToString());
                                 sw.WriteLine(temp2.fcb.indexPointer.size.ToString());
                                 temp2 = temp2.nextBrother;
                             }
                            MessageBox.Show(mid.fcb.fileName);
                        }
                        mid = mid.nextBrother;
                    }*/
                    sw.WriteLine("@");              //结束符号
                    sw.Close();
                }


            }

        }

        public void readMyFCBAndCatagory()
        {
            //这个是针对整个目录树写的
            string path = ".\\MyFCBAndCatagory.txt";
            if (File.Exists(path))
            {
                //边读边初始化
                StreamReader reader = new StreamReader(path);
                //MessageBox.Show("执行了读目录函数");
                string temp = reader.ReadLine();
               // MessageBox.Show(temp);
                while (temp != "@")
                {
                    if (temp == "$")
                    {
                        //说明下一行是根节点
                        temp = reader.ReadLine();
                        if (temp != "root")
                        {
                           // MessageBox.Show("不是root");
                            updateCurrentRoot(temp);
                        }
                        //否则就是root根节点，我们在构造函数的时候已经默认了，不需要做处理
                    }
                    else
                    {
                        //此时temp值是filename
                        string filename = temp;
                        temp = reader.ReadLine();
                        int type = int.Parse(temp);
                        temp = reader.ReadLine();
                        int start = int.Parse(temp);
                        temp = reader.ReadLine();
                        int size = int.Parse(temp);
                        //以上读完了一个节点的信息
                        FCB newNode = new FCB(filename, type, size);
                        newNode.type = type;
                        newNode.fileName = filename;
                        newNode.indexPointer.start = start;
                        newNode.indexPointer.size = size;
                        category.createFile(newNode, currentRoot);
                       // MessageBox.Show(newNode.fileName + "," + newNode.type.ToString() + "," + newNode.indexPointer.start.ToString() + "," + newNode.indexPointer.size.ToString());
                        /* FCB newNode = new FCB("test", FCB.TXTFILE, 0);
                             category.createFile(newNode, currentRoot);
                             newNode.type = FCB.TXTFILE;
                             newNode.fileName = "test";
                             newNode.indexPointer.start = 0;
                             newNode.indexPointer.size = 9;*/
                    }
                    temp = reader.ReadLine();
                }
                reader.Close();

            }
            File.Delete(path);
        }
        public void writeMyDisk()
        {
            //还差两个remain
            string path = ".\\MyDisk.txt";
            using (StreamWriter sw = File.CreateText(path))
            {
                for (int i = 0; i < disk.blockNum; i++)
                {
                    sw.WriteLine(disk.FAT[i].ToString());

                }
                for (int i = 0; i < disk.row; i++)
                {
                    for (int j = 0; j < disk.WORD; j++)
                    {
                        sw.WriteLine(disk.MAP[i, j].ToString());
                    }
                }
                for (int i = 0; i < disk.row; i++)
                {
                    for (int j = 0; j < disk.WORD; j++)
                    {
                        sw.WriteLine(disk.DiskContent[i, j]);
                    }
                }
                sw.Close();
            }

        }
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //关闭窗口，要将已有信息全部写入配置文件
            writeMyDisk();
            writeMyFCBAndCatagory();
        }
        //读我的磁盘文件并初始化
        public void readMyDisk()
        {
            string path = ".\\MyDisk.txt";
            if (File.Exists(path))
            {
                StreamReader reader = new StreamReader(path);
                //MessageBox.Show("执行了读磁盘函数");
                for (int i = 0; i < disk.blockNum; i++)
                {
                    string temp;
                    temp = reader.ReadLine();
                    disk.FAT[i] = int.Parse(temp);
                    // MessageBox.Show(disk.FAT[i].ToString());
                }
                for (int i = 0; i < disk.row; i++)
                {
                    for (int j = 0; j < disk.WORD; j++)
                    {
                        string temp;
                        temp = reader.ReadLine();
                        disk.MAP[i, j] = int.Parse(temp);
                        // MessageBox.Show(disk.MAP[i,j].ToString());
                    }
                }
                for (int i = 0; i < disk.row; i++)
                {
                    for (int j = 0; j < disk.WORD; j++)
                    {
                        string temp;
                        temp = reader.ReadLine();
                        disk.DiskContent[i, j] = temp;
                    }
                }
                reader.Close();
            }
            File.Delete(path);
        }
    }
}
