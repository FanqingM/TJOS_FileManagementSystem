using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//改进一下FCB，通过索引节点的方式
namespace FileManagement
{
    public class FCB
    {
        public class Index
        {
            public int start;             //文件在内存中初始存放的位置,然后根据FAT表索引到文件内容
            public int size;              //文件大小,文件夹不显示大小
            public Index()
            {
                this.size = 0;
                this.start = -1;
            }
            public Index(int size)
            {
                this.size = size;
                this.start = -1;
            }
            public Index(int size, int start)
            {
                this.size = size;
                this.start = start;
            }
        }
        public const int TXTFILE = 0;               //文本文件标识
        public const int FOLDER = 1;                //文件夹标识
        public string fileName;                     //文件名
        public int type;                            //文件类型 => TXTDILE/FOLDER    
        public Index indexPointer = null;           //索引指针


        public FCB()
        {

        }

        public FCB(string name, int type, int size)
        {
            this.indexPointer = new Index(size);
            this.fileName = name;
            this.type = type;
        }
        public FCB(string name, int type, int size, int start)
        {
            this.indexPointer = new Index(size,start);
            this.fileName = name;
            this.type = type;
        }
    }
}
