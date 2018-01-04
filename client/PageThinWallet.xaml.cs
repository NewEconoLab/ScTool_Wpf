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
using System.Windows.Threading;

namespace client
{
    /// <summary>
    /// PageThinWallet.xaml 的交互逻辑
    /// </summary>
    public partial class PageThinWallet : Page
    {
        public PageThinWallet()
        {
            InitializeComponent();
        }
        DispatcherTimer timer = new DispatcherTimer();
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            updateInfo();
            timer.Tick += (s, ee) =>
              {
                  updateInfo();
              };
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Start();
        }
        async void updateInfo()
        {
            System.Net.WebClient wc = new MyWC();
            var str = MakeRpcUrl(this.texturl_node.Text, "getblockcount");
            var result = await wc.DownloadStringTaskAsync(str);
            var json = MyJson.Parse(result).AsDict()["result"].AsInt();
            labelHeight.Content = json;
        }
        string MakeRpcUrl(string url, string method, params MyJson.IJsonNode[] _params)
        {
            StringBuilder sb = new StringBuilder();
            if (url.Last() != '/')
                url = url + "/";

            sb.Append(url + "?jsonrpc=2.0&id=1&method=" + method + "&params=[");
            for (var i = 0; i < _params.Length; i++)
            {
                _params[i].ConvertToString(sb);
                if (i != _params.Length - 1)
                    sb.Append(",");
            }
            sb.Append("]");
            return sb.ToString();
        }
        ThinNeo.NEP6.NEP6Wallet nep6wallet;
        string password = null;
        Dictionary<string, byte[]> mapprikey = new Dictionary<string, byte[]>();
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "*.json|*.json";
            if (ofd.ShowDialog() == true)
            {

                nep6wallet = new ThinNeo.NEP6.NEP6Wallet(ofd.FileName);
                Dialog_Input_password pass = new Dialog_Input_password();

                if (pass.ShowDialog() == true)
                {
                    foreach (var w in nep6wallet.accounts)
                    {
                        try
                        {
                            var pkey = w.Value.GetPrivate(pass.password);
                            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(pkey);
                            var phash = ThinNeo.Helper.GetPublicKeyHash(pubkey);
                            string hex = ThinNeo.Helper.Bytes2HexString(phash);
                            var add1 = ThinNeo.Helper.GetAddressFromScriptHash(ThinNeo.Helper.HexString2Bytes(w.Key));
                            var add2 = ThinNeo.Helper.GetAddressFromScriptHash(phash);
                            if (hex != w.Key)
                                throw new Exception("密码错");
                            mapprikey[w.Key] = pkey;

                        }
                        catch
                        {
                            MessageBox.Show("密码错误");
                            password = null;
                            break;
                        }
                    }
                }


            }
        }
    }
}
