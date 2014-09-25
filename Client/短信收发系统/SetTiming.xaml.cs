using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Shapes;

namespace 短信收发系统
{
    /// <summary>
    /// SetTiming.xaml 的交互逻辑
    /// </summary>
    public partial class SetTiming : Window
    {
        public string timing = "";
        public delegate void DoTask();
        Thread time;

        public SetTiming()
        {
            InitializeComponent();

            time = new Thread(Time);
            time.Start();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            timing = txtHour.Text + txtMin.Text + txtSec.Text;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            timing = "";
            this.Close();
        }

           private void Time()
        {
            while (true)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new DoTask(Update));
                Thread.Sleep(1000);
            }
        }

        private void Update()
        {
            string time = DateTime.Now.ToString();
            char[] sep = { ':', ' ' };
            string[] str = time.Split(sep);
            labTime.Content = str[1] + ":" + str[2] + ":" + str[3];        
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            time.Abort();
        }

    }
}
