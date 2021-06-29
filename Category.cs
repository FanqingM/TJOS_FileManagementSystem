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

namespace FileManagement
{
    public class Category
    {
        // FCB里存储了文件的名字以及type以及索引指针
        public class Node
        {
            //树形目录结构使用多叉树，采用左长子，右兄弟，为了方便实现返回上一级目录还需要添加父亲指针
            //除此之外，包含改进的FCB，即文件名和索引指针
            //目录节点
            //节点存储的数据
            public FCB fcb = new FCB();
            public Node firstChild = null;      //左孩子
            public Node nextBrother = null;     //右兄弟
            public Node parent = null;          //父结点

            public Node() { }
            public Node(FCB file)
            {
                fcb.fileName = file.fileName;
                fcb.type = file.type;
                fcb.indexPointer = file.indexPointer;

                firstChild = null;
                nextBrother = null;
                parent = null;
            }
            public Node(string name, int type)
            {
                fcb.fileName = name;
                fcb.type = type;
                firstChild = null;
                nextBrother = null;
                parent = null;
            }
        }

        // 对于目录而言只需要初始化root节点就行，其余的是后期插入
        public Node root;    //目录的根节点
        public Category()
        {
            root = null;
        }

        // 根据FCB初始化根节点
        public Category(FCB rootFile)
        {
            root = new Node(rootFile);
        }

        //下面的currentNode是当前根节点
        //在当前目录下有相同的，返回true，否则返回false
        public bool isSameFile(FCB file, Node currentNode)
        {
            if(currentNode == null)
            {
                return false;
            }
            else
            {
                if(currentNode.firstChild == null)
                {
                    return false;
                }
                else
                {
                    Node temp = currentNode.firstChild;
                    //第一个孩子不为空，看看第一个孩子充不重复
                    if (temp.fcb.fileName == file.fileName && temp.fcb.type == file.type)
                    {
                        //这里说明找到相同的
                        return true;
                    }
                    while (temp != null)
                    {
                        if(temp.fcb.fileName == file.fileName && temp.fcb.type == file.type)
                        {
                            //这里说明找到相同的
                            return true;
                        }
                        temp = temp.nextBrother;
                    }
                    return false;
                }
            }
        }

        public Node searchNode(string filename,Node currentNode)
        {

            Node temp = currentNode.firstChild;
            if (temp == null)
            {
                return null;
            }
            else
            {
                if (temp.fcb.fileName == filename)
                {
                    return temp;
                }
                else
                {
                    while (!(temp.fcb.fileName == filename))
                    {
                        temp = temp.nextBrother;
                        if(temp == null)
                        {
                            break;
                        }
                    }
                    return temp;
                }
            }
        }

        public Node searchNode(string filename,int type,Node currentNode)
        {

            Node temp = currentNode.firstChild;
            if(temp == null)
            {
                return null;
            }
            else
            {
                if (temp.fcb.fileName == filename && temp.fcb.type == type)
                {
                    return temp;
                }
                else
                {
                    while (!(temp.fcb.fileName == filename && temp.fcb.type == type))
                    {
                        temp = temp.nextBrother;
                    }
                    return temp;
                }
            }
        }
        public void createFile(FCB file, Node currentNode)
        {
           // MessageBox.Show("执行了这个函数");
            Node tn = new Node(file);
            tn.fcb.fileName = file.fileName;
            tn.fcb.type = file.type;
            tn.fcb.indexPointer = file.indexPointer;
            tn.nextBrother = null;
            tn.firstChild = null;
            tn.parent = currentNode;
            // currentNode是当前根节点，file应该加到他的子目录底下
            if (currentNode == null)
            {
                //MessageBox.Show("添加文件的当前父亲节点" +currentNode.fcb.fileName+"位空");
                ;
            }
            else
            {
                    // 检测当前目录下是否又同名同类型文件
                    if (currentNode.firstChild == null)
                    {
                        //说明新加的这个文件是他的第一个孩子
                        currentNode.firstChild = tn;
                        tn.nextBrother = null;
                        tn.firstChild = null;
                        tn.parent = currentNode;
                       // MessageBox.Show("在这个节点底下" + currentNode.fcb.fileName + "添加了一个节点" + currentNode.firstChild.fcb.fileName);
                       // MessageBox.Show("当前根节点" + currentNode.fcb.fileName);
                    }
                    else
                    {
                        if(isSameFile(file,currentNode))
                        {
                            MessageBox.Show("有重复的");
                        }
                        else
                        {
                            Node temp = currentNode.firstChild;
                            while (temp.nextBrother != null)        //顺序找到该文件夹下最后一个文件存储的位置
                                temp = temp.nextBrother;
                            temp.nextBrother = tn;
                            tn.nextBrother = null;
                            tn.firstChild = null;
                            tn.parent = currentNode;
                          //  MessageBox.Show("在这个节点右边" + temp.fcb.fileName + "添加了一个节点" + temp.nextBrother.fcb.fileName);
                            //MessageBox.Show("当前根节点" + currentNode.fcb.fileName);
                        }
                    }
            }
        }
        public void deleteAll(Node currentNode)
        {
            //格式化时候
            this.root.firstChild = null;
            this.root.nextBrother = null;
            this.root.parent = null;
        }
        public void deleteCurrentCategory(Node currentNode)
        {
            //删除当前目录下的所有文件
            Node temp = currentNode.firstChild;
            if(temp!=null)
            {
                Node temp2 = temp;
                temp = temp.nextBrother;
                temp2 = null;
            }
            currentNode.firstChild = null;
        }

        //删除比较麻烦
        public void deleteFile(string filename, int filetype, Node currentNode)
        {
            //删除根节点currenrtNode底下的文件file
            if (currentNode.firstChild.fcb.fileName == filename && currentNode.firstChild.fcb.type == filetype)
            {
                //该节点是当前根节点的第一个孩子
                Node temp = currentNode.firstChild;
                if(temp.nextBrother == null)
                {
                    currentNode.firstChild = null;
                }
                else
                {
                    currentNode.firstChild = currentNode.firstChild.nextBrother;
                    currentNode.firstChild.parent = currentNode;
                }
                temp = null;
            }
            else
            {
                //不是第一个孩子，找他之前的那个
                Node temp = currentNode.firstChild;
                while (!(temp.nextBrother.fcb.fileName == filename && temp.nextBrother.fcb.type == filetype))
                {
                    temp = temp.nextBrother;
                }
                Node temp2 = temp.nextBrother;
                temp.nextBrother = temp.nextBrother.nextBrother;
                temp2 = null;
            }
        }

        public void deleteFolder(string filename, int filetype, Node currentNode)
        {
            //删除根节点currenrtNode底下的文件file
            if (currentNode.firstChild.fcb.fileName == filename && currentNode.firstChild.fcb.type == filetype)
            {
                //该节点是当前根节点的第一个孩子
                Node temp = currentNode.firstChild;
                currentNode.firstChild = currentNode.firstChild.nextBrother;
                currentNode.firstChild.parent = currentNode;
                //需要递归删除temp节点以及temp节点所有的孩子
            }
            else
            {
                //不是第一个孩子，找他之前的那个
                Node temp = currentNode.firstChild;
                while (!(temp.nextBrother.fcb.fileName == filename && temp.nextBrother.fcb.type == filetype))
                {
                    temp = temp.nextBrother;
                }
                Node temp2 = temp.nextBrother;
                temp.nextBrother = temp.nextBrother.nextBrother;
                //需要递归删除temp节点以及temp节点所有的孩子
            }
        }

    }
}
