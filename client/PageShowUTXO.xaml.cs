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
        public class UTXO
        {
            public string txid;
            public int n;
            public string asset;
            public UInt64 value;
            public override string ToString()
            {
                return asset + ":" + value;
            }
        }
        List<UTXO> utxos = new List<UTXO>();
        byte[] prikey;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            utxos.Clear();
            comboUtxo.Items.Clear();
            System.Net.WebClient wc = new System.Net.WebClient();
            listUTXO.Items.Clear();
            Dictionary<string, decimal> count = new Dictionary<string, decimal>();

            var str = MakeRpcUrl(this.texturl.Text, "getutxo", new MyJson.JsonNode_ValueString(textaddress.Text));
            var result = await wc.DownloadStringTaskAsync(str);
            var json = MyJson.Parse(result).AsDict()["result"].AsList();
            foreach (var item in json)
            {
                UTXO txio = new UTXO();
                //var use = item.GetDictItem("vinTx").AsString();
                txio.txid = item.GetDictItem("txid").AsString();
                txio.n = item.GetDictItem("n").AsInt();
                txio.asset = item.GetDictItem("asset").AsString();
                var value = decimal.Parse(item.GetDictItem("value").AsString());
                txio.value = (ulong)((decimal)value * 100000000);
                //if (use == "")
                {
                    if (count.ContainsKey(txio.asset) == false)
                    {
                        count[txio.asset] = 0;
                    }
                    count[txio.asset] += value;

                    listUTXO.Items.Add(txio.txid + "[" + txio.n + "] " + value + ":" + txio.asset);
                    comboUtxo.Items.Add(txio);
                }
                utxos.Add(txio);
                //else
                //{
                //    listUTXO.Items.Add("[已花费]" + value + ":" + asset);
                //}
            }
            listMoney.Items.Clear();
            foreach (var m in count)
            {
                listMoney.Items.Add("资产:" + m.Value + "  " + m.Key);
            }
            if (comboUtxo.Items.Count > 0)
                comboUtxo.SelectedIndex = 0;
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

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            System.Net.WebClient wc = new System.Net.WebClient();
            var str = MakeRpcUrl(this.texturl_node.Text, "getblockcount");
            var result = await wc.DownloadStringTaskAsync(str);
            var json = MyJson.Parse(result).AsDict()["result"].AsInt();
            labelHeight.Content = json;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            listInfo.Items.Clear();

            var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(wifText.Text);
            this.prikey = prikey;
            var bytes = ThinNeo.Cryptography.Cryptography.Base58.Decode(wifText.Text);
            listInfo.Items.Add("base58decode=" + ThinNeo.Helper.Bytes2HexString(bytes));
            Console.WriteLine(ThinNeo.Helper.Bytes2HexString(bytes));
            listInfo.Items.Add("prikey=" + ThinNeo.Helper.Bytes2HexString(prikey));
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
            listInfo.Items.Add("pubkey=" + ThinNeo.Helper.Bytes2HexString(pubkey));
            var addrScript = ThinNeo.Helper.GetScriptFromPublicKey(pubkey);
            listInfo.Items.Add("addrscript=" + ThinNeo.Helper.Bytes2HexString(addrScript));
            var addrScriptHash = ThinNeo.Helper.GetScriptHashFromPublicKey(pubkey);
            listInfo.Items.Add("addrscriptHash=" + ThinNeo.Helper.Bytes2HexString(addrScriptHash));
            var addr = ThinNeo.Helper.GetAddressFromScriptHash(addrScriptHash);
            var addrbase58 = ThinNeo.Cryptography.Cryptography.Base58.Decode(addr);
            listInfo.Items.Add("addrDeCode=" + ThinNeo.Helper.Bytes2HexString(addrbase58));

            listInfo.Items.Add("addr=" + addr);
            textaddress.Text = addr;

        }
        ThinNeo.Transaction lastTran;
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            lastTran = new ThinNeo.Transaction();
            lastTran.type = ThinNeo.TransactionType.ContractTransaction;//转账
            lastTran.attributes = new ThinNeo.Attribute[0];
            lastTran.inputs = new ThinNeo.TransactionInput[1];
            var utxo = comboUtxo.SelectedItem as UTXO;
            lastTran.inputs[0] = new ThinNeo.TransactionInput();
            lastTran.inputs[0].hash = ThinNeo.Helper.HexString2Bytes(utxo.txid);//吃掉一个utxo
            lastTran.inputs[0].index = (ushort)utxo.n;
            var valuecount = utxo.value;
            ulong eat = ulong.Parse(textTrans.Text);
            lastTran.outputs = new ThinNeo.TransactionOutput[2];
            lastTran.outputs[0] = new ThinNeo.TransactionOutput();//给对方转账
            lastTran.outputs[0].assetId = ThinNeo.Helper.HexString2Bytes(utxo.asset);
            lastTran.outputs[0].toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(textaddressTo.Text);
            lastTran.outputs[0].value = eat;
            lastTran.outputs[1] = new ThinNeo.TransactionOutput();//给自己找零
            lastTran.outputs[1].assetId = ThinNeo.Helper.HexString2Bytes(utxo.asset);
            lastTran.outputs[1].toAddress = ThinNeo.Helper.GetPublicKeyHashFromAddress(textaddress.Text);
            lastTran.outputs[1].value = valuecount - eat;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            lastTran.SerializeUnsigned(ms);
            textTran.Text = ThinNeo.Helper.Bytes2HexString(ms.ToArray());
            ms.Close();

        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            lastTran.SerializeUnsigned(ms);
            byte[] sign = ThinNeo.Helper.Sign(ms.ToArray(), prikey);
            ms.Close();
            listSign.Items.Clear();
            listSign.Items.Add("sign=" + ThinNeo.Helper.Bytes2HexString(sign));
            var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);

            var addr = ThinNeo.Helper.GetAddressFromPublicKey(pubkey);
            lastTran.AddWitness(sign, ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey), addr);
        }

        //private async void Button_Click_2(object sender, RoutedEventArgs e)
        //{
        //    System.Net.WebClient wc = new System.Net.WebClient();
        //    var str = MakeRpcUrl("getassets");
        //    var result = await wc.DownloadStringTaskAsync(str);
        //    var json = MyJson.Parse(result).AsDict()["result"].AsDict();
        //    listAssets.Items.Clear();
        //    Dictionary<string, string> mapName = new Dictionary<string, string>();
        //    foreach (var item in json)
        //    {
        //        var names = item.Value.AsDict()["name"].AsList();
        //        string outname = "";
        //        foreach (var n in names)
        //        {

        //            if (n.AsDict()["lang"].AsString() == "en")
        //            {
        //                outname = n.AsDict()["name"].AsString();
        //                mapName[item.Key] = outname;
        //            }
        //        }
        //        listAssets.Items.Add(item.Key + ":[" + outname + "]");
        //    }
        //}
    }
}
