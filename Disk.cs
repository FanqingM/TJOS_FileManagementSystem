using System.Windows.Forms;
//这里做的是英文的文本文件，以及没有特殊符号，即默认了所有的输入都是单字节编码的
//同时我们仿照linux的文件案例，当创建一个文件但不给他内容时，该文件的大小为0，他仅仅占用了catagory里面的一个Node，而在磁盘不占有任何空间

//这个类中涉及到对空闲磁盘块的管理，以及文件存储空间管理
//文件存储空间管理采用FAT，这里就是一个磁盘类有一张FAT表，由于其各个表项在物理上连续存储，所以其本质上在规定磁盘块个数后就是一个数组
//其中磁盘空闲空间管理采用位图
//所以我们同样用一个数组来代表位图，或者规定字长，用二位数组来
namespace FileManagement
{
    public class Disk
    {
        public int WORD = 16;   //这里设一个字的字长是16位

        //大小都是以字节为单位的
        public int size;              //磁盘总空间大小
        public int blockSize;         //磁盘块大小
        public int blockNum;          //总的磁盘块数量
        public int remainSize;        //剩余空间
        public int remainBolckNum;
        public int row;
        public int[,] MAP;                   //位图二维数组
        public int[] FAT;                    //FAT数组
        public string[,] DiskContent;        //存储内容的磁盘数组，与位图对应的 
        public Disk(int size, int blockSize)
        {
            this.size = size;
            this.blockSize = blockSize;
            this.blockNum = size / blockSize;
            this.row = size / WORD + (size % WORD != 0 ? 1 : 0);

            //上面的信息都不用写
            this.FAT = new int[this.blockNum];
            this.MAP = new int[row, WORD];
            this.DiskContent = new string[row, WORD];
            this.remainSize = this.size;
            this.remainBolckNum = this.blockNum;
            for (int i = 0; i < this.blockNum; i++)
            {
                FAT[i] = -1;
            }
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < WORD; j++)
                {
                    MAP[i, j] = 0;
                    DiskContent[i, j] = "";
                }
            }
        }

        public int getBlockNum(int size)
        {
            return size / blockSize + (size % blockSize != 0 ? 1 : 0);
        }

        public int getBlockid(int i, int j)
        {
            //根据位图中的坐标找到对应的磁盘块号
            return i * WORD + j;
        }

        public int getMapX(int blockID)
        {
            return blockID / WORD;
        }

        public int getMapY(int blockID)
        {
            return blockID % WORD;
        }

        public void deleteFileContent(FCB file)
        {
            //要删除file文件占有的空间
            int start = file.indexPointer.start;
           // MessageBox.Show(file.fileName + "  " + start.ToString());
            while (FAT[start] != -1)
            {
                MAP[getMapX(start), getMapY(start)] = 0;
                DiskContent[getMapX(start), getMapY(start)] = "";
                int temp = start;
                start = FAT[start];
                FAT[temp] = -1;
            }
            file.indexPointer.start = -1;
        }

        //下面是对于第一次修改文件以及后续修改文件的操作
        //他们的差异在于，一个原来未在磁盘分配空间，是第一次分配，另一个是已经分配了空间，修改之后需要重新分配

        public void giveDiskSpace(FCB file, string fileContent)      //FCB里面是没有内容信息的
        {
            int cnt = 0;
            int cnt2 = 0;
            int temp = 0;
            int num = getBlockNum(file.indexPointer.size);
            //MessageBox.Show("cnt:" + cnt.ToString() + "num:" + num.ToString());
            if (num > this.remainBolckNum)
            {
                MessageBox.Show("磁盘空间不足");
            }
            //需要考虑文件最后长度不够，subString会报异常的
            else
            {
                //先找可以放置的地方
                for (int i = 0; i < row; i++)
                {
                    if (cnt >= num)
                    {
                        break;
                    }
                    for (int j = 0; j < WORD; j++)
                    {
                        if (cnt >= num)
                        {
                            break;
                        }
                        if (MAP[i, j] == 0 && cnt == 0)
                        {
                            file.indexPointer.start = getBlockid(i, j);
                            // MessageBox.Show("文件起始位置"+file.indexPointer.start.ToString());
                            MAP[i, j] = 1;
                            if (num == 1)
                            {
                                if (file.indexPointer.size % blockSize != 0)
                                {
                                    DiskContent[i, j] = fileContent.Substring(0, fileContent.Length);
                                    //  MessageBox.Show(DiskContent[i, j] + i.ToString() + "," + j.ToString() + "," + temp.ToString());
                                }
                                else if (file.indexPointer.size % blockSize == 0)
                                {
                                    DiskContent[i, j] = fileContent.Substring(0, blockSize);
                                    // MessageBox.Show(DiskContent[i, j] + i.ToString() + "," + j.ToString() + "," + temp.ToString());
                                }
                                remainBolckNum--;
                                remainSize -= 2;
                                temp = getBlockid(i, j);
                                cnt++;
                            }
                            else
                            {
                                DiskContent[i, j] = fileContent.Substring(0, blockSize);
                                //  MessageBox.Show(DiskContent[i, j] + i.ToString() + "," + j.ToString() + "," + temp.ToString());
                                remainBolckNum--;
                                remainSize -= 2;
                                temp = getBlockid(i, j);
                                cnt++;
                            }
                        }
                        else if (MAP[i, j] == 0 && cnt != 0)
                        {
                            //这里要判断截取字符串函数是否超过界限

                            if (cnt == num - 1)
                            {
                                if (file.indexPointer.size % blockSize != 0)
                                {
                                    MAP[i, j] = 1;
                                    DiskContent[i, j] = fileContent.Substring(cnt * blockSize, fileContent.Length - cnt * blockSize);
                                    // MessageBox.Show(DiskContent[i, j] + i.ToString() + "," + j.ToString()+"," +temp.ToString());
                                    remainBolckNum--;
                                    remainSize -= 2;
                                    FAT[temp] = getBlockid(i, j);
                                    temp = getBlockid(i, j);
                                    cnt++;
                                    cnt2++;
                                }
                                else
                                {
                                    MAP[i, j] = 1;
                                    DiskContent[i, j] = fileContent.Substring(cnt * blockSize, blockSize);
                                    // MessageBox.Show(DiskContent[i, j] + i.ToString() + "," + j.ToString() + "," + temp.ToString());
                                    remainBolckNum--;
                                    remainSize -= 2;
                                    FAT[temp] = getBlockid(i, j);
                                    temp = getBlockid(i, j);
                                    cnt++;
                                    cnt2++;
                                }
                            }
                            else
                            {
                                MAP[i, j] = 1;
                                DiskContent[i, j] = fileContent.Substring(cnt * blockSize, blockSize);
                                // MessageBox.Show(DiskContent[i, j] + i.ToString() + "," + j.ToString() + "," + temp.ToString());
                                remainBolckNum--;
                                remainSize -= 2;
                                FAT[temp] = getBlockid(i, j);
                                temp = getBlockid(i, j);
                                cnt++;
                                cnt2++;
                            }
                        }
                    }
                }
                FAT[temp] = -1;
                //结束
            }
        }
        public string getFileContent(FCB fcb)
        {
            //老是少读了最后一个块
            //MessageBox.Show("执行了getFileContent");
            string res = "";
            if (fcb.indexPointer.start == -1)
            {
                if (fcb.indexPointer.size == 0)
                {
                    return "";
                }
                else
                {
                    //也可能是文件过小
                    int start = fcb.indexPointer.start;
                    res = DiskContent[getMapX(start), getMapY(start)];
                    return res;
                }
            }
            else
            {
                int start = fcb.indexPointer.start;
                //不知道因为什么他总是不读最后一个
                // MessageBox.Show(start.ToString()+","+FAT[0].ToString() + "," + FAT[1].ToString() + "," + FAT[2].ToString() + "," + FAT[3].ToString());
                while (start != -1)
                {
                    string temp = DiskContent[getMapX(start), getMapY(start)];
                    //MessageBox.Show(temp + getMapX(start).ToString()+","+getMapY(start).ToString()+","+start.ToString());
                    res += temp;
                    start = FAT[start];
                }
                /*string temp2 = DiskContent[getMapX(0), getMapY(2)];
                MessageBox.Show(temp2);*/
                //最后一块没有读
                /* string temp2 = DiskContent[getMapX(start), getMapY(start)];
                 MessageBox.Show("最后一块"+temp2);
                 res += temp2;*/

                return res;
            }
        }
        public void fileContentUpdate(FCB oldFile, FCB newFile, string content)
        {
            deleteFileContent(oldFile);
            giveDiskSpace(newFile, content);
        }
    }

}
