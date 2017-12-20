using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

namespace client
{
    /// <summary>
    /// ShowUTXO.xaml 的交互逻辑
    /// </summary>
    public partial class ShowUTXO : Page
    {
        public ShowUTXO()
        {
            InitializeComponent();
        }
        System.Net.WebClient wc = new System.Net.WebClient();
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            listUTXO.Items.Clear();
            Dictionary<string, decimal> count = new Dictionary<string, decimal>();

            var str = MakeRpcUrl("getutxo", new MyJson.JsonNode_ValueString(textaddress.Text));
            var result = await wc.DownloadStringTaskAsync(str);
            var json = MyJson.Parse(result).AsDict()["result"].AsDict();
            foreach (var item in json)
            {
                var use = item.Value.GetDictItem("use").AsString();
                var asset = item.Value.GetDictItem("asset").AsString();
                var value = item.Value.GetDictItem("value").AsString();
                if (use == "")
                {
                    if (count.ContainsKey(asset) == false)
                    {
                        count[asset] = 0;
                    }
                    count[asset] += decimal.Parse(value);

                    listUTXO.Items.Add(value + ":" + asset);
                }
                else
                {
                    listUTXO.Items.Add("[已花费]" + value + ":" + asset);
                }
            }
            listMoney.Items.Clear();
            foreach (var m in count)
            {
                listMoney.Items.Add("资产:" + m.Value + "  " + m.Key);
            }
        }
        string MakeRpcUrl(string method, params MyJson.IJsonNode[] _params)
        {
            StringBuilder sb = new StringBuilder();
            if (this.texturl.Text.Last() != '/')
                this.texturl.Text = this.texturl.Text + "/";

            sb.Append(this.texturl.Text + "?jsonrpc=2.0&id=1&method=" + method + "&params=[");
            for (var i = 0; i < _params.Length; i++)
            {
                _params[i].ConvertToString(sb);
                if (i != _params.Length - 1)
                    sb.Append(",");
            }
            sb.Append("]");
            return sb.ToString();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var str = MakeRpcUrl("getstate");
            var result = await wc.DownloadStringTaskAsync(str);
            var json = MyJson.Parse(result).AsDict()["result"].AsInt();
            labelHeight.Content = json;
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var str = MakeRpcUrl("getassets");
            var result = await wc.DownloadStringTaskAsync(str);
            var json = MyJson.Parse(result).AsDict()["result"].AsDict();
            listAssets.Items.Clear();
            Dictionary<string, string> mapName = new Dictionary<string, string>();
            foreach (var item in json)
            {
                var names = item.Value.AsDict()["name"].AsList();
                string outname = "";
                foreach (var n in names)
                {

                    if (n.AsDict()["lang"].AsString() == "en")
                    {
                        outname = n.AsDict()["name"].AsString();
                        mapName[item.Key] = outname;
                    }
                }
                listAssets.Items.Add(item.Key + ":[" + outname + "]");
            }
        }
    }
}
