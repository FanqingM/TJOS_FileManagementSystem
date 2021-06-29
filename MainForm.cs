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

//treeview1��Ŀ¼��������ֻ���ļ�����
//treeview2���ļ���������ļ�¼��FCB
// treeView2����Ҫ��������ϣ��ʵ���ļ�Ԥ�����Լ�˫���������ļ��е�ҳ�棬�Լ�˫���ı��ļ��������ݣ��Ҽ����������������ɾ��ѡ��
// treeview2�����νṹ��������UI�ϵ�
// �����߼��ϵ����Խṹ������catagory����ڻ���Ҫ��������Node�ڵ㣨
// �����½����ļ�or�ļ��ж����ڵ�ǰ·���µĲ����������˵�ǰĿ¼��˼�룬����¼��ǰ���ڵ㣬�����˶��IO
namespace FileManagement
{
    //д�ļ������ǿ���д���ڴ��ͬʱ����һ�������ļ���Ҳд����Щ��Ϣ����Ϊ�ڴ��ڳ���������ͷţ���������ļ������ڴ���
    //�����´λָ�
    public partial class MainForm : Form
    {
        private const int w1 = 17;
        private const int w2 = 39;
        //��ʽ����
        public Category category = new Category();                  //����Ŀ¼
        public Category.Node rootNode = new Category.Node();        //Ŀ¼�ĸ��ڵ�
        public Category.Node currentRoot = new Category.Node();     //��ǰ���ڵ�Ϊroot
        public Disk disk = new Disk(5096, 2);                     //�����ڴ�ռ�
        public string Path;                                         //ָ����ǰ·��

        public MainForm()
        {
            this.Path = "";
            FCB root = new FCB("root", FCB.FOLDER, 0);        //��Ϊһ�����ļ��У�������ļ��ӵ��µ�path����""
            rootNode = new Category.Node(root);
            currentRoot = rootNode;
            category.root = rootNode;
            disk = new Disk(5096, 2);       //�����ڴ�ռ�
            readMyFCBAndCatagory();
            readMyDisk();                   //�Ӵ����ж�Ŀ¼���ļ������Լ�FCB��ϢFAT��Ϣ��λͼ��Ϣ��diskcontent��Ϣ��
            //readMyFCBAndCatagory();
            InitializeComponent();
            //չʾUIһ�������������
            //���ڲ�����Ŀ¼��һ���ļ���Ҫд������Ϣ���д��̵�ʣ��ռ��ʣ����̿�
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
                //MessageBox.Show("��ǰ���ڵ��һ������" + temp.fcb.fileName);
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
            //��һ�����Ӳ�Ϊ�գ�������һ�����ӳ䲻�ظ�
            if (temp.fcb.fileName == name)
            {
                //����˵���ҵ���ͬ��
                currentRoot = temp;
            }
            while (temp != null)
            {
                if (temp.fcb.fileName == name && temp.fcb.type == FCB.FOLDER)
                {
                    //����˵���ҵ���ͬ��
                    currentRoot = temp;
                    break;
                }
                temp = temp.nextBrother;
            }
        }

        public void newUI()
        {
            //���ݵ�ǰ�ĸ��ڵ����¸����������UI
            //�˲������߼��޹�
            //Ӧ������˫���ļ��к󣬸���path��currentroot���߼������󣬽��е�UI����
        }
        public void createFolder()
        {
            string str = Interaction.InputBox("�������ļ�������", "�ַ���", "", 100, 100);
            TreeNode tn = new TreeNode();
            tn.Tag = 1;
            tn.ImageIndex = 1;
            tn.SelectedImageIndex = 1;
            if (str != "")
            {

                // ���жϵ�ǰĿ¼������ͬ��ͬ����
                // ����������ֱ��nodes.add������
                // Ŀ¼�����¿�����clear����������Ⱦ
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
                    MessageBox.Show("���ظ���");
                }
                else
                {
                    category.createFile(newNode, currentRoot);
                    treeView2.Nodes.Add(tn);                     //UI��������ĸı�
                }
            }
            else
            {
                MessageBox.Show("�ļ�������Ϊ��");
            }
        }
        public void createFile()
        {
            string str = Interaction.InputBox("�������ļ�������", "�ַ���", "", 100, 100);
            TreeNode tn = new TreeNode();
            tn.Tag = 0;
            tn.ImageIndex = 0;
            tn.SelectedImageIndex = 0;
            if (str != "")
            {
                // ���жϵ�ǰĿ¼������ͬ��ͬ����
                // ����������ֱ��nodes.add������
                // Ŀ¼�����¿�����clear����������Ⱦ
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
                    MessageBox.Show("���ظ���");
                }
                else
                {
                    category.createFile(newNode, currentRoot);
                    treeView2.Nodes.Add(tn);                     //UI��������ĸı�
                }

                //�������߼��ϵĸı�
                //��currentNode�Ӻ��ӣ���Ҫ�ж�����û�к��ӣ�û�о���firstChild��������Ǻ����ǵ�nextBrother
            }
            else
            {
                MessageBox.Show("�ļ�������Ϊ��");
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
        // treeview1��Ŀ¼ҳ��
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
        }
        private void treeView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
        }
        //�½��ļ��а�ť
        //�½��ļ��к��ļ�ҲӦ��ע��һ�½ڵ��tag
        private void button2_Click(object sender, EventArgs e)
        {
            //test
            /*Button bt = (Button)sender;  //��ǰ����İ�ť
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


        //�½��ļ���ť
        private void button3_Click(object sender, EventArgs e)
        {
            //treeView2.Nodes.Add("folder");
            createFile();
        }

        // treeview2��������
        private void treeView2_MouseDown(object sender, TreeViewEventArgs e)
        {

        }
        //��ʽ������
        private void button1_Click_1(object sender, EventArgs e)
        {
            //UI��
            DialogResult res = MessageBox.Show("ȷ�����и�ʽ��������", "��ʾ", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
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
            //���Ҳ���Ĭ�����ڵ�ǰ·���²���
            string str = Interaction.InputBox("������Ҫ�����ļ�������", "�ļ�����", "", 100, 100);
            //�����ǲ��Ҳ���
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
                MessageBox.Show("��ǰĿ¼û��" + str + "ͬ���ļ�");
            }
            else
            {
                MessageBox.Show("��ǰĿ¼��" + str + "ͬ���ļ���" + cnt.ToString() + "��");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string str = Interaction.InputBox("����ڵ�ǰ·������������ת��·��", "·����ת", "", 100, 100);
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
                MessageBox.Show("����·���Ƿ�");
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
                    MessageBox.Show("���������·���а������ļ��л��߰�����ǰ�����ڵ��ļ�");
                }
            }
        }

        //���ָ���ǰ��ť���лָ�����
        private void button6_Click(object sender, EventArgs e)
        {
            FCB newNode = new FCB("test", FCB.TXTFILE, 0);
            category.createFile(newNode, currentRoot);
            showCurrentRootChild();
        }

        //�ǵ�ǰĿ¼
        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void treeView2_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //�Ҽ�ѡ��ɾ������������
                //MessageBox.Show("�Ҽ�");
                // DialogResult res = MessageBox.Show("ȷ��ɾ�����ļ���","��ʾ", MessageBoxButtons.YesNoCancel,MessageBoxIcon.Warning

                //ɾ��
                TreeNode tn = treeView2.SelectedNode;
                //MessageBox.Show(tn.Name);
                DialogResult res = MessageBox.Show("ȷ��ɾ�����ļ���", "��ʾ", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (res == DialogResult.Yes)
                {
                    if (tn.Tag.ToString() == "0")
                    {
                        string name = tn.Name;
                        //MessageBox.Show(name);
                        //����ֱ���ҵ�ǰ��Ŀ¼���£�������ͬ���ı��ļ�����Ŀ¼����ɾ�������ڸ���UI
                        //ɾ����һ�����ӽڵ��Ŀ¼Ϊ�յ�������������
                        //ͬʱ��Ŀ¼��Ҫ�Ĵ���
                        Node fileNode = category.searchNode(name, 0, currentRoot);
                        MessageBox.Show("��ǰѡ��"+fileNode.fcb.fileName+"�������Ŀ�꣬�����������Ŀ����ѡ�У����Ҽ�����");
                        if(fileNode.fcb.indexPointer.start!=-1)
                        {
                            disk.deleteFileContent(fileNode.fcb);
                        }
                        category.deleteFile(name, 0, currentRoot);
                        treeView2.Nodes.Clear();
                        if (currentRoot.firstChild != null)
                        {
                           // calculateFolderSize(currentRoot);
                            //���¼��������ļ��еĴ�С
                            showCurrentRootChild();
                        }
                    }
                    else
                    {
                        //������һ���߼���ֻ����ɾ���ļ����漰���ݹ�ɾ��
                        category.deleteFolder(Name, 1, currentRoot);
                        treeView2.Nodes.Clear();
                        showCurrentRootChild();
                    }
                }
            }
        }

        private void treeView2_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            //�����������ļ��л��ߴ��ı��ļ�
            //string str = Interaction.InputBox("�������ļ�������", "�½��ļ�", "", 100, 100);
            TreeNode tn = treeView2.SelectedNode;
            //MessageBox.Show(tn.Tag.ToString());
            //�����������жϵ�����Ч��
            if (tn.Tag.ToString() == "0")
            {
                //�㿪�ı��ļ�������һ��������Ȼ�������ı��ļ������ݣ���С�ȣ���������µ�ǰ���ڵ��Լ���ǰ·��
                //MessageBox.Show(this.Name);
                //���this��mainform
                TextForm textForm = new TextForm(tn.Name, this);
                textForm.Show();
            }
            else
            {
                //�򿪵�ǰ�ļ��в����ڸ����µĸ��ڵ㣬�����µ��������UI�������µ�path
                string name = tn.Name;
                Path += "\\" + tn.Name;
                textBox1.Text = Path;
                treeView2.Nodes.Clear();
                //���µ�ǰ���ڵ�
                updateCurrentRoot(tn.Name);
                //չʾ��ǰ���ڵ��µ�����
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
                MessageBox.Show("�����ٺ����ˣ�������");
            }
            else
            {
                Path = Path.Replace("\\" + currentRoot.fcb.fileName, "");
                calculateFolderSize(currentRoot);
                currentRoot = currentRoot.parent;
                //MessageBox.Show("��ǰ���ڵ�λ" + currentRoot.fcb.fileName);
                textBox1.Text = Path;
                treeView2.Nodes.Clear();
                showCurrentRootChild();
            }

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }


        //�����Ƕ�д�����ļ��Ĳ���

        //����FCB�����ϴ�����Ŀ¼���Ľڵ��У�����������д����ʱ��������Ŀ¼��һ��д��
        //�������Ҫ�ľ���˳�����⣬���ǲ��ôӸ��ڵ㿪ʼ����д��Ȼ���ڵ�ǰ���ڵ����create
        public void writeMyFCBAndCatagory()
        {
            //�������ԣ�ֻдroot���µĺ���
            //�������ǲ���BFS��д
            //������һ������
            if (category.root == null)
            {
                //MessageBox.Show("��ǰĿ¼���ڵ�Ϊ��");
                ;
            }
            else if (category.root.firstChild == null)
            {
                // MessageBox.Show("��ǰĿ¼�ǿյ�");
                ;
            }
            else
            {
                //ֻд��root�ڵ��µģ�������
                //����д�Լ�Ŀ¼
                Queue<Node> q = new Queue<Node>();
                Node temp;
                Node mid = category.root.firstChild;
                string path = ".\\MyFCBAndCatagory.txt";
                using (StreamWriter sw = File.CreateText(path))
                {
                    //д��һ��
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
                    //д�ڶ���
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
                    sw.WriteLine("@");              //��������
                    sw.Close();
                }


            }

        }

        public void readMyFCBAndCatagory()
        {
            //������������Ŀ¼��д��
            string path = ".\\MyFCBAndCatagory.txt";
            if (File.Exists(path))
            {
                //�߶��߳�ʼ��
                StreamReader reader = new StreamReader(path);
                //MessageBox.Show("ִ���˶�Ŀ¼����");
                string temp = reader.ReadLine();
               // MessageBox.Show(temp);
                while (temp != "@")
                {
                    if (temp == "$")
                    {
                        //˵����һ���Ǹ��ڵ�
                        temp = reader.ReadLine();
                        if (temp != "root")
                        {
                           // MessageBox.Show("����root");
                            updateCurrentRoot(temp);
                        }
                        //�������root���ڵ㣬�����ڹ��캯����ʱ���Ѿ�Ĭ���ˣ�����Ҫ������
                    }
                    else
                    {
                        //��ʱtempֵ��filename
                        string filename = temp;
                        temp = reader.ReadLine();
                        int type = int.Parse(temp);
                        temp = reader.ReadLine();
                        int start = int.Parse(temp);
                        temp = reader.ReadLine();
                        int size = int.Parse(temp);
                        //���϶�����һ���ڵ����Ϣ
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
            //��������remain
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
            //�رմ��ڣ�Ҫ��������Ϣȫ��д�������ļ�
            writeMyDisk();
            writeMyFCBAndCatagory();
        }
        //���ҵĴ����ļ�����ʼ��
        public void readMyDisk()
        {
            string path = ".\\MyDisk.txt";
            if (File.Exists(path))
            {
                StreamReader reader = new StreamReader(path);
                //MessageBox.Show("ִ���˶����̺���");
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
