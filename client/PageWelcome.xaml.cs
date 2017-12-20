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
    /// PageWelcome.xaml 的交互逻辑
    /// </summary>
    public partial class PageWelcome : Page
    {
        public PageWelcome()
        {
            InitializeComponent();
        }

        private void textAvm_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textAvm.Text.Length % 2 != 0)//必须是双数
                return;
            if (listASM == null)
                return;
            byte[] data = null;
            try
            {
                data = ThinNeo.Helper.HexString2Bytes(textAvm.Text);
            }
            catch (Exception err)
            {
                MessageBox.Show("string->hex error:" + err.Message);
                return;
            }
            ThinNeo.Compiler.Op[] ops = null;
            try
            {
                ops = ThinNeo.Compiler.Avm2Asm.Trans(data);
            }
            catch (Exception err)
            {
                MessageBox.Show("avm->asm error:" + err.Message);
                return;
            }
            listASM.Items.Clear();
            listSysCall.Items.Clear();
            foreach (var op in ops)
            {
                try
                {
                    var str = op.ToString();
                    if (op.code == ThinNeo.VM.OpCode.SYSCALL)
                    {
                        listSysCall.Items.Add(op);
                    }
                    listASM.Items.Add(op);
                }
                catch
                {
                    listASM.Items.Add("format error:");

                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {//load avm from file
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.Filter = "*.avm|*.avm";
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    var bytes = System.IO.File.ReadAllBytes(ofd.FileName);
                    this.textAvm.Text = ThinNeo.Helper.Bytes2HexString(bytes);
                }
                catch (Exception err)
                {
                    MessageBox.Show("load file error:" + ofd.FileName);
                    return;
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            textAvm_TextChanged(null, null);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            textPrivate.Text = "";
            try
            {
                var prikey = ThinNeo.Helper.GetPrivateKeyFromWIF(textWIF.Text);
                textPrivate.Text = "Prikey:" + ThinNeo.Helper.Bytes2HexString(prikey) + "\n";
                var pubkey = ThinNeo.Helper.GetPublicKeyFromPrivateKey(prikey);
                textPrivate.Text += "Pubkey:" + ThinNeo.Helper.Bytes2HexString(pubkey) + "\n";
                var pubkeyhash = ThinNeo.Helper.GetPublicKeyHash(pubkey);
                textPrivate.Text += "PubkeyHash:" + ThinNeo.Helper.Bytes2HexString(pubkeyhash) + "\n";
                var address = ThinNeo.Helper.GetAddressFromScriptHash(pubkeyhash);
                textPrivate.Text += "Address:" + address + "\n";
            }
            catch
            {

            }
        }

        private void textAdress_TextChanged(object sender, TextChangedEventArgs e)
        {
            textHash.Text = "";
            try
            {
                var pubkeyhash = ThinNeo.Helper.GetPublicKeyHashFromAddress(textAdress.Text);

                textHash.Text= "Hash:" + ThinNeo.Helper.Bytes2HexString(pubkeyhash) + "\n";
             }
            catch
            {
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Dialog_Get_Contract dialog = new Dialog_Get_Contract();
            if(dialog.ShowDialog()==true)
            {
                this.textAvm.Text = dialog.avmResult;
            }
        }
    }
}