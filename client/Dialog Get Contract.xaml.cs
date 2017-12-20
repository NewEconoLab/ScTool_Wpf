using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace client
{
    /// <summary>
    /// Dialog_Get_Contract.xaml 的交互逻辑
    /// </summary>
    public partial class Dialog_Get_Contract : Window
    {
        public Dialog_Get_Contract()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

        }

        System.Net.WebClient wc = new MyWC();
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var apitext = textAPI.Text;
            try
            {
                var str = wc.DownloadString(apitext + "help");
                MessageBox.Show("api ok:" + str);
            }
            catch (Exception err)
            {
                MessageBox.Show("api fail:" + err.Message, "", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public string avmResult;
        public string srcResult;
        public string abiResult;
        public string mapResult;
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //checkhash
            var hashtext = textHash.Text.ToLower(); ;
            if (hashtext.IndexOf("0x") == -1)
                hashtext = "0x" + hashtext;

            coder.Text = "";
            textAVM.Text = "";
            textMap.Text = "";
            textABI.Text = "";
            var apitext = textAPI.Text;
            var str = wc.DownloadString(apitext + "get?hash=" + hashtext);
            var json = MyJson.Parse(str).AsDict();
            if (json.ContainsKey("cs"))
            {
                srcResult = json["cs"].AsString();
                coder.Text = Uri.UnescapeDataString(srcResult);
            }
            if(json.ContainsKey("avm"))
            {
                avmResult = json["avm"].AsString();
                textAVM.Text = avmResult;
            }
            if (json.ContainsKey("abi"))
            {
                abiResult = json["abi"].AsString();
                var _str = Uri.UnescapeDataString(abiResult);
                StringBuilder sb = new StringBuilder();
                MyJson.Parse(_str).ConvertToStringWithFormat(sb,0);
                textABI.Text = sb.ToString();
            }
            if (json.ContainsKey("map"))
            {
                mapResult = json["map"].AsString();
                var _str = Uri.UnescapeDataString(mapResult);
                StringBuilder sb = new StringBuilder();
                MyJson.Parse(_str).ConvertToStringWithFormat(sb, 0);
                textMap.Text = sb.ToString();
            }
        }
    }
}
