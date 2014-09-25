using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace 短信收发系统
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        Client client;
        String msg;
        public delegate void DoTask();
        Thread ReceData;
        string timing = "";
        bool run = true;
        int page = 0;

        public MainWindow()
        {
            InitializeComponent();
            Init();
        }

        void Init()
        {
            client = new Client();

            grid.Children.Remove(txtAddress);
            grid.Children.Remove(txtMsg);
            grid.Children.Remove(txtName);
            grid.Children.Remove(txtInput);
            
            grid.Children.Remove(imgSend);
            grid.Children.Remove(imgTiming);
            grid.Children.Remove(imgShowInput);
            grid.Children.Remove(imgAB);
            grid.Children.Remove(imgInput);
            grid.Children.Remove(imgAddress);

            grid.Children.Remove(listMsg);
            grid.Children.Remove(listDig);

            gridHead.Children.Remove(imgCancel);
            gridHead.Children.Remove(imgShowMsg);
            gridHead.Children.Remove(imgClock);
            gridHead.Children.Remove(imgShowMsg);
            gridHead.Children.Remove(imgDel);
            gridHead.Children.Remove(imgDelCancel);
            gridHead.Children.Remove(imgEdit_Del);
            gridHead.Children.Remove(imgEdit_DelCancel);

            ReceData = new Thread(ReceiveData);
            ReceData.Start();

            ReadContacts();
        }

        //读取联系人目录
        void ReadContacts()
        {
            StreamReader sr = new StreamReader(@"Message.txt", Encoding.UTF8);
            listContract.Items.Clear();
            string line;
            string[] flag = new string[100000];
            int num = 0;
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "")
                    continue;
                string when = null;
                string what = null;
                string who = null;
                string content = null;

                char[] sep = { ' '};
                string[] info = line.Split(sep, 5);

                when = info[0] + " " + info[1];
                what = info[2];
                who = info[3];
                content = info[4];

                if (content.Length > 60)
                    content = content.Substring(0, 60) + " ......";

                int i;
                for (i = 0; i < num; i++)
                {
                    if (flag[i] == who)
                        break;
                }
                if (i != num)
                    continue;

                flag[num++] = who;

                listContract.Items.Add(who);
                TextBox TX = new TextBox();
                TX.IsReadOnly = true;
                TX.BorderThickness = new Thickness(0);
                TX.FontSize = 12;
                TX.Foreground = Brushes.Gray;
                TX.Width = 255;
                TX.TextWrapping = TextWrapping.Wrap;
                TX.Text = content;
                listContract.Items.Add(TX);
            }
            sr.Close();
        }

        //再次显示联系人目录
        private void ReshowData(object sender, MouseButtonEventArgs e)
        {
            ReadContacts();
        }

        //新建短信息
        private void imgNew_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!delFlag)
            {
                page = 1;

                imgMsg.Source = new BitmapImage(new Uri(@"image\new\new_msg.png", UriKind.Relative));

                grid.Children.Remove(imgSearch);
                grid.Children.Remove(txtSearch);
                grid.Children.Remove(listContract);

                gridHead.Children.Remove(imgEdit);
                gridHead.Children.Remove(imgEditCancel);
                gridHead.Children.Remove(imgNew);

                grid.Children.Add(imgInput);
                grid.Children.Add(imgAddress);
                grid.Children.Add(txtAddress);
                grid.Children.Add(txtMsg);
                grid.Children.Add(imgSend);
                grid.Children.Add(imgAB);
                grid.Children.Add(listMsg);
                grid.Children.Add(imgTiming);

                gridHead.Children.Add(imgCancel);

                listMsg.Height = 175;
            }
        }

        //取消新建短信息
        private void imgCancel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            page = 0;

            imgMsg.Source = new BitmapImage(new Uri(@"image\main\msg.png", UriKind.Relative));

            grid.Children.Add(imgSearch);
            grid.Children.Add(txtSearch);
            grid.Children.Add(listContract);

            gridHead.Children.Add(imgEditCancel);
            gridHead.Children.Add(imgEdit);
            gridHead.Children.Add(imgNew);

            grid.Children.Remove(imgAddress);
            grid.Children.Remove(txtAddress);
            grid.Children.Remove(txtMsg);
            grid.Children.Remove(imgSend);
            grid.Children.Remove(listMsg);
            grid.Children.Remove(imgTiming);
            grid.Children.Remove(imgAB);
            grid.Children.Remove(imgInput);

            gridHead.Children.Remove(imgCancel);

            listMsg.Height = 403;
        }

        //改变“发送”的颜色
        private void txtMsg_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Text.Length == 0)
                imgSend.Source = new BitmapImage(new Uri(@"image\send.png", UriKind.Relative));
            else
                imgSend.Source = new BitmapImage(new Uri(@"image\send2.png", UriKind.Relative));
        }

        //发送信息
        private void imgSend_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            page = 2;

            string[] num;
            if (txtMsg.IsFocused)
                num = txtAddress.Text.Split(';');
            else
                num = txtName.Text.Split(';');

            Socket socket = client.socketClient;
            string format = "";
            if (timing != "")
                format = "#" + timing;
            format += ":" + client.name;
            for (int i = 0; i < num.Length; i++)
                format += "@" + num[i];
            format += ":" + (txtMsg.IsFocused ? txtMsg.Text : txtInput.Text);
            byte[] outBuf = Encoding.Unicode.GetBytes(format);
            socket.Send(outBuf);

            imgMsg.Source = new BitmapImage(new Uri(@"image\main\ms.png", UriKind.Relative));

            if (txtMsg.IsFocused)
            {

                txtName.Text = txtAddress.Text;

                grid.Children.Remove(imgNew);
                grid.Children.Remove(imgTiming);
                grid.Children.Remove(txtAddress);
                grid.Children.Remove(txtInput);
                grid.Children.Remove(imgAB);
                grid.Children.Remove(imgSend);
                grid.Children.Remove(imgInput);
                grid.Children.Remove(txtMsg);

                gridHead.Children.Remove(imgMsg);
                gridHead.Children.Remove(imgCancel);

                gridHead.Children.Add(imgClock);
                gridHead.Children.Add(imgShowMsg);

                grid.Children.Add(listDig);
                grid.Children.Add(imgShowInput);
                grid.Children.Add(txtInput);
                grid.Children.Add(imgSend);
                grid.Children.Add(txtName);
                imgSend.Margin = new Thickness(239, 373, 4, 10);

                try
                {
                    ReadDialog(txtAddress.Text);
                }
                catch (Exception err) { }
            }

            TextBox timeBox = new TextBox();
            timeBox.Text = DateTime.Now.ToString();
            timeBox.TextAlignment = TextAlignment.Center;
            timeBox.Background = Brushes.Transparent;
            timeBox.Height = 15;
            timeBox.FontSize = 10;
            timeBox.Width = 255;
            timeBox.Foreground = Brushes.Gray;
            timeBox.IsReadOnly = true;
            timeBox.BorderThickness = new Thickness(0);
            listDig.Items.Add(timeBox);

            TextBox tb = new TextBox();
            tb.Text = txtMsg.IsFocused ? txtMsg.Text : txtInput.Text;
            tb.TextAlignment = TextAlignment.Right;
            tb.BorderThickness = new Thickness(0);
            tb.Foreground = Brushes.White;
            tb.Background = Brushes.RoyalBlue;
            tb.TextWrapping = TextWrapping.Wrap;
            tb.IsReadOnly = true;
            tb.Width = 255;

            txtMsg.Text = "";
            txtInput.Text = "";
            listDig.Items.Add(tb);
            listDig.UpdateLayout();
            listDig.ScrollIntoView(listDig.Items[listDig.Items.Count - 1]);

            string msg = DateTime.Now.ToString() + " to " + txtName.Text + " " + tb.Text;
            string path = txtName.Text + ".txt";
            SaveData(msg, path);
        }

        //判断能否执行enter快速发送信息。
        private void Send_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (txtMsg.IsFocused || txtInput.IsFocused)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        //“enter”快捷键：发送信息
        private void Send_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            page = 2;

            string[] num;
            if (txtMsg.IsFocused)
                num = txtAddress.Text.Split(';');
            else
                num = txtName.Text.Split(';');

            Socket socket = client.socketClient;
            string format = "";
            if (timing != "")
                format = "#" + timing;
            format += ":" + client.name;
            for (int i = 0; i < num.Length; i++)
                format += "@" + num[i];
            format += ":" + (txtMsg.IsFocused ? txtMsg.Text : txtInput.Text);
            byte[] outBuf = Encoding.Unicode.GetBytes(format);
            socket.Send(outBuf);

            imgMsg.Source = new BitmapImage(new Uri(@"image\main\ms.png", UriKind.Relative));

            if (txtMsg.IsFocused)
            {

                txtName.Text = txtAddress.Text;

                grid.Children.Remove(imgNew);
                grid.Children.Remove(imgTiming);
                grid.Children.Remove(txtAddress);
                grid.Children.Remove(txtInput);
                grid.Children.Remove(imgAB);
                grid.Children.Remove(imgSend);
                grid.Children.Remove(imgInput);
                grid.Children.Remove(txtMsg);

                gridHead.Children.Remove(imgMsg);
                gridHead.Children.Remove(imgCancel);

                gridHead.Children.Add(imgClock);
                gridHead.Children.Add(imgShowMsg);

                grid.Children.Add(listDig);
                grid.Children.Add(imgShowInput);
                grid.Children.Add(txtInput);
                grid.Children.Add(imgSend);
                grid.Children.Add(txtName);
                imgSend.Margin = new Thickness(239, 373, 4, 10);

            }

            string info = txtMsg.IsFocused ? txtMsg.Text : txtInput.Text;

            txtMsg.Text = "";
            txtInput.Text = "";
            listDig.Items.Add(info);
            listDig.UpdateLayout();
            listDig.ScrollIntoView(listDig.Items[listDig.Items.Count - 1]);

            string msg = DateTime.Now.ToString() + " to " + txtName.Text + " " + info;
            string path = txtName.Text +".txt";
            SaveData(msg, path);

            ReadDialog(txtName.Text);
        }

        //保存消息
        private void SaveData(string msg, string path)
        {
            if (!File.Exists(path))
            {
                FileStream fs = new FileStream(path, FileMode.Create);
                fs.Close();
            }
            StreamReader sr = new StreamReader(@path, Encoding.UTF8);
            string txt1 = sr.ReadToEnd() + msg + "\r\n";
            sr.Close();
            StreamWriter sw = new StreamWriter(@path);
            sw.Write(txt1);
            sw.Close();

            string[] str = msg.Split(' ');
            StreamReader sr2 = new StreamReader(@"Message.txt", Encoding.UTF8);
            string txt2 = msg + "\r\n" + sr2.ReadToEnd();
            sr2.Close();
            StreamWriter sw2 = new StreamWriter(@"Message.txt", false, Encoding.UTF8);
            sw2.Write(txt2);
            sw2.Close();
        }

        //接收消息
        private void ReceiveData()
        {
            Socket socket = client.socketClient;
            Byte[] inBuf;
            int inLen;

            while (run)
            {
                try
                {
                    inBuf = new Byte[8192];
                    inLen = socket.Available;

                    if (inLen == 0)
                        continue;

                    socket.Receive(inBuf, inLen, SocketFlags.None);

                    msg = System.Text.Encoding.Unicode.GetString(inBuf, 0, inLen);

                    char sep = ':';
                    string[] msgs = msg.Split(sep);

                    for (int i = 1; i < msgs.Length; i++)
                    {
                        string info = DateTime.Now.ToString() + " from " + msgs[i].Substring(0, 4) + " " + msgs[i].Substring(9);
                        string path = msgs[i].Substring(0, 4) + ".txt";

                        Thread.Sleep(200);
                        SaveData(info, path);
                        System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new DoTask(ReadContacts));
                        if (page == 2)
                            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new DoTask(ReadDialog));
                    }
                    
        //            System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new DoTask(ShowScreen));
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    break;
                }
            }
        }

        //移动客户端
        private void imgBlank_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        //关闭客户端
        private void imgExit_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            run = false;
            this.Close();
        }

        //设置时间
        private void imgTiming_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            SetTiming setTiming = new SetTiming();
            setTiming.ShowDialog();
            timing = setTiming.timing;
        }

        //显示对话框
        private void ShowMsg(object sender, MouseButtonEventArgs e)
        {
            if (!delFlag)
            {
                page = 2;
                if (listContract.SelectedItem != null)
                {
                    string selectedname = listContract.SelectedItem.ToString();

                    imgShowInput.Source = new BitmapImage(new Uri(@"image\new\input.png", UriKind.Relative));

                    gridHead.Children.Remove(imgMsg);
                    gridHead.Children.Remove(imgNew);
                    gridHead.Children.Remove(imgEdit);
                    gridHead.Children.Remove(imgEditCancel);

                    grid.Children.Remove(listContract);
                    grid.Children.Remove(listDig);
                    grid.Children.Remove(imgSearch);
                    grid.Children.Remove(txtSearch);

                    grid.Children.Add(listDig);
                    grid.Children.Add(imgSend);
                    grid.Children.Add(imgShowInput);

                    gridHead.Children.Add(imgShowMsg);
                    gridHead.Children.Add(imgClock);

                    grid.Children.Add(txtName);
                    grid.Children.Add(txtInput);

                    txtName.Text = selectedname;

                    Thickness myThickness = new Thickness();
                    myThickness.Bottom = 10;
                    myThickness.Left = 239;
                    myThickness.Right = 4;
                    myThickness.Top = 373;
                    imgSend.Margin = myThickness;

                    grid.Children.Remove(imgSend);
                    grid.Children.Add(imgSend);

                    listDig.Items.Clear();
                    listDig.Height = 403;

                    ReadDialog(selectedname);
                    listDig.ScrollIntoView(listDig.Items[listDig.Items.Count - 1]);
                }
            }
        }

        private void ReadDialog()
        {
            if (page == 2)
                ReadDialog(txtName.Text);
        }

        //读取对话文本
        private void ReadDialog(string name)
        {
            string path = name + ".txt";
            StreamReader sr = new StreamReader(@path, Encoding.UTF8);
            string line;
            listDig.Items.Clear();
            while ((line = sr.ReadLine()) != null)
            {
                if (line == "")
                    continue;
                string when = null;
                string what = null;
                string who = null;
                string content = null;

                char[] sep = { ' '};
                string[] info = line.Split(sep, 5);

                when = info[0] + " " + info[1];
                what = info[2];
                who = info[3];
                content = info[4];



                TextBox timeBox = new TextBox();
                timeBox.Text = when;
                timeBox.TextAlignment = TextAlignment.Center;
                timeBox.Background = Brushes.Transparent;
                timeBox.Height = 15;
                timeBox.FontSize = 10;
                timeBox.Width = 255;
                timeBox.Foreground = Brushes.Gray;
                timeBox.IsReadOnly = true;
                timeBox.BorderThickness = new Thickness(0);
                listDig.Items.Add(timeBox);

                if (what == "from")
                {
                    TextBox tb = new TextBox();
                    tb.Text = content;
                    tb.IsEnabled = false;
                    tb.BorderThickness = new Thickness(0);
                    tb.Background = Brushes.LightGray;
                    tb.TextWrapping = TextWrapping.Wrap;
                    tb.IsReadOnly = true;
                    tb.FontSize = 14;
                    tb.Width = 255;

                    listDig.Items.Add(tb);
                }
                else
                {
                    TextBox tb = new TextBox();
                    tb.IsEnabled = false;
                    tb.Text = content;
                    tb.TextAlignment = TextAlignment.Right;
                    tb.Foreground = Brushes.White;
                    tb.Background = Brushes.Blue;
                    tb.Width = 255;
                    tb.IsReadOnly = true;
                    tb.FontSize = 14;
                    tb.BorderThickness = new Thickness(0);
                    tb.TextWrapping = TextWrapping.Wrap;
                    listDig.UpdateLayout();
                    listDig.Items.Add(tb);
                    listDig.ScrollIntoView(tb);
                }
            }
            sr.Close();
        }

        //返回联系人目录
        private void ReturnContact(object sender, MouseButtonEventArgs e)
        {
            page = 0;

            grid.Children.Remove(imgSend);
            grid.Children.Remove(listDig);
            grid.Children.Remove(imgShowInput);
            grid.Children.Remove(txtInput);
            grid.Children.Remove(imgAddress);
            grid.Children.Remove(listMsg);
            grid.Children.Remove(txtName);

            gridHead.Children.Remove(imgShowMsg);
            gridHead.Children.Remove(imgClock);

            gridHead.Children.Add(imgNew);
            gridHead.Children.Add(imgEditCancel);
            gridHead.Children.Add(imgEdit);

            grid.Children.Add(listContract);
            grid.Children.Add(imgSearch);
            grid.Children.Add(txtSearch);

            imgMsg.Source = new BitmapImage(new Uri(@"image\main\msg.png", UriKind.Relative));
            gridHead.Children.Remove(imgMsg);
            gridHead.Children.Add(imgMsg);

            imgSend.Margin = new Thickness(235, 170, 0, 187);
            ReadContacts();
        }

        //判断搜索能否执行
        private void Search_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (txtSearch.IsFocused)
                e.CanExecute = true;
            else
                e.CanExecute = false;
        }

        //执行搜索功能
        private void Search_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            StreamReader sr = new StreamReader(@"res.txt", Encoding.UTF8);
            string line;
            listContract.Items.Clear();
            while ((line = sr.ReadLine()) != null)
            {
                char[] sep = {' '};
                string[] str = line.Split(sep, 2);
                string name = str[0];
                string msg = str[1];
                listContract.Items.Add(name);
                if (msg.Length > 40)
                    msg = msg.Substring(0, 40) + "...";

                TextBox TX = new TextBox();
                TX.IsReadOnly = true;
                TX.BorderThickness = new Thickness(0);
                TX.FontSize = 12;
                TX.Foreground = Brushes.Gray;
                TX.Width = 255;
                TX.TextWrapping = TextWrapping.Wrap;
                TX.Text = msg;
                listContract.Items.Add(TX);
            }
            sr.Close();
        }

        //消除搜索结果
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string key = txtSearch.Text;
            if (txtSearch.Text.Length == 0)
            {
                listContract.Items.Clear();
                ReadContacts();
            }
            else
            {
                StreamReader sr = new StreamReader(@"Message.txt", Encoding.UTF8);
                StreamWriter sw = new StreamWriter(@"res.txt");
                string line;
                int num = 0;
                string[] flag = new string[10000];
                while ((line = sr.ReadLine()) != null)
                {
                    if (line.IndexOf(key) > 0)
                    {
                        string[] info = line.Split(' ');
                        int i;
                        for (i = 0; i < num; i++)
                            if (flag[i] == info[3])
                                break;
                        if (i != num)
                            continue;
                        sw.WriteLine(info[3] + " " + info[4]);
                        flag[num++] = info[3];
                    }
                }
                sw.Close();
                sr.Close();

                sr = new StreamReader(@"res.txt", Encoding.UTF8);
                listContract.Items.Clear();
                while ((line = sr.ReadLine()) != null)
                {
                    char[] sep = { ' ' };
                    string[] str = line.Split(sep, 2);
                    string name = str[0];
                    string msg = str[1];
                    listContract.Items.Add(name);
                    if (msg.Length > 40)
                        msg = msg.Substring(0, 40) + "...";

                    TextBox TX = new TextBox();
                    TX.IsReadOnly = true;
                    TX.BorderThickness = new Thickness(0);
                    TX.FontSize = 12;
                    TX.Foreground = Brushes.Gray;
                    TX.Width = 255;
                    TX.TextWrapping = TextWrapping.Wrap;
                    TX.Text = msg;
                    listContract.Items.Add(TX);
                }
                sr.Close();
            }
        }
        //删除对话
        bool delFlag = false;
        private void Edit_delete(object sender, MouseButtonEventArgs e)
        {
            gridHead.Children.Remove(imgEdit);
            gridHead.Children.Remove(imgNew);

            gridHead.Children.Add(imgEdit_Del);
            gridHead.Children.Add(imgEdit_DelCancel);

            // imgEditCancel.Source = new BitmapImage(new Uri(@"image\new\cancel.png", UriKind.Relative));
            // gridHead.Children.Remove(imgEdit);

            delFlag = true;
        }

        private void Edit_delete_Cancel(object sender, MouseButtonEventArgs e)
        {
            listContract.SelectedIndex = -1;
            gridHead.Children.Remove(imgEdit_Del);
            gridHead.Children.Remove(imgEdit_DelCancel);

            gridHead.Children.Add(imgEdit);
            gridHead.Children.Add(imgNew);
            delFlag = false;
        }

        private void Edit_delete_Confirm(object sender, MouseButtonEventArgs e)
        {
            gridHead.Children.Remove(imgEdit_Del);
            gridHead.Children.Remove(imgEdit_DelCancel);

            gridHead.Children.Add(imgEdit);
            gridHead.Children.Add(imgNew);

            int num = listContract.SelectedItems.Count;

            object[] selected_objs = new object[num];
            listContract.SelectedItems.CopyTo(selected_objs, 0);

            if (delFlag == true && listContract.SelectedItem != null)
            {
                StreamReader sr = new StreamReader(@"Message.txt", Encoding.UTF8);
                string line;
                string msgLine = null;
                while ((line = sr.ReadLine()) != null)
                {
                    char sep = ' ';
                    string[] msg = line.Split(sep);
                    int flag = 0;
                    for (int i = 0; i < num; i++)
                    {
                        if (msg[3] == selected_objs[i].ToString())
                        {
                            flag = 1;

                        }
                    }
                    if (flag == 0)
                    {
                        msgLine += line;
                        msgLine += "\r\n";
                    }
                }
                sr.Close();
                StreamWriter sw = new StreamWriter(@"Message.txt", false, Encoding.UTF8);
                sw.Write(msgLine);
                sw.Close();
                for (int i = 0; i < num; i++)
                {
                    string delFile = selected_objs[i].ToString() + ".txt";
                    if (File.Exists(delFile))
                    {
                        FileInfo fi = new FileInfo(delFile);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                            fi.Attributes = FileAttributes.Normal;

                        File.Delete(delFile);
                    }
                }
                ReadContacts();
            }
            delFlag = false;
        }

        //对话框内删除功能
        private void DelDialog(object sender, MouseButtonEventArgs e)
        {
            if (imgClock.IsLoaded == false)
                return;

            gridHead.Children.Remove(imgClock);
            gridHead.Children.Remove(imgShowMsg);

            gridHead.Children.Add(imgDel);
            gridHead.Children.Add(imgDelCancel);

        }

        private void DelConfirm(object sender, MouseButtonEventArgs e)
        {
            gridHead.Children.Remove(imgDel);
            gridHead.Children.Remove(imgDelCancel);

            gridHead.Children.Add(imgClock);
            gridHead.Children.Add(imgShowMsg);

            int num = listDig.SelectedItems.Count;

            object[] selected_objs = new object[num];
            listDig.SelectedItems.CopyTo(selected_objs, 0);

            for (int m = 0; m < num; m++)
            {
                listDig.Items.Remove(selected_objs[m]);
            }
            string path = txtName.Text + ".txt";
            StreamReader sr = new StreamReader(@path, Encoding.UTF8);
            string line;
            string tempTxt = null;

            while ((line = sr.ReadLine()) != null)
            {
                if (line == "")
                    continue;
                string when = null;
                string what = null;
                string who = null;
                string content = null;

                char[] sep = { ' ' };
                string[] info = line.Split(sep, 5);

                when = info[0] + " " + info[1];
                what = info[2];
                who = info[3];
                content = info[4];
                int flag = 0;
                for (int m = 0; m < num; m++)
                {
                    if (selected_objs[m].ToString().Length == 31)
                    {
                        if (content == "")
                        {
                            flag = 1;
                        }
                        break;
                    }
                    else if (content == selected_objs[m].ToString().Substring(33))
                    {
                        flag = 1;
                        break;
                    }
                }
                if (flag == 0)
                {
                    tempTxt += line;
                    tempTxt += "\r\n";
                }
            }
            sr.Close();
            StreamWriter sw = new StreamWriter(@path, false, Encoding.UTF8);
            sw.Write(tempTxt);
            sw.Close();
            string tempTxt2 = tempTxt;

             tempTxt = null;
            StreamReader sm = new StreamReader(@"Message.txt", Encoding.UTF8);
            while ((line = sm.ReadLine()) != null)
            {
                string temp;
                string[] kin = line.Split(' ');
                temp = kin[4];
                int flag = 0;
                
                for (int m = 0; m < num; m++)
                {
                    string[] info = selected_objs[m].ToString().Split(' ');
                    if (selected_objs[m].ToString().Length == 31)
                    {
                        if (temp == "")
                        {
                            flag = 1;
                        }
                        break;
                    }
                    else if (temp == info[1])
                    {
                        flag = 1;
                        break;
                    }
                }
                if (flag == 0)
                {
                    tempTxt += line;
                    tempTxt += "\r\n";
                }
            }
            sm.Close();
            StreamWriter sq = new StreamWriter(@"Message.txt", false, Encoding.UTF8);
            sq.Write(tempTxt);
            sq.Close();
            if (tempTxt2 == null)
            {
                string delFile = txtName.Text + ".txt";
                if (File.Exists(delFile))
                {
                    FileInfo fi = new FileInfo(delFile);
                    if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                        fi.Attributes = FileAttributes.Normal;

                    File.Delete(delFile);
                }

                page = 0;

                grid.Children.Remove(imgSend);
                grid.Children.Remove(listDig);
                grid.Children.Remove(imgShowInput);
                grid.Children.Remove(txtInput);
                grid.Children.Remove(imgAddress);
                grid.Children.Remove(listMsg);
                grid.Children.Remove(txtName);

                gridHead.Children.Remove(imgShowMsg);
                gridHead.Children.Remove(imgClock);

                gridHead.Children.Add(imgNew);
                gridHead.Children.Add(imgEditCancel);
                gridHead.Children.Add(imgEdit);

                grid.Children.Add(listContract);
                grid.Children.Add(imgSearch);
                grid.Children.Add(txtSearch);

                imgMsg.Source = new BitmapImage(new Uri(@"image\main\msg.png", UriKind.Relative));
                gridHead.Children.Remove(imgMsg);
                gridHead.Children.Add(imgMsg);

                imgSend.Margin = new Thickness(235, 170, 0, 187);
                ReadContacts();
                return;
            }
            ReadDialog(txtName.Text);
        }

        private void DelCancel(object sender, MouseButtonEventArgs e)
        {
            listDig.SelectedIndex = -1;
            gridHead.Children.Remove(imgDel);
            gridHead.Children.Remove(imgDelCancel);

            gridHead.Children.Add(imgClock);
            gridHead.Children.Add(imgShowMsg);
        }
    }
}
