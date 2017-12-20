using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
    /// ContractCompiler.xaml 的交互逻辑
    /// </summary>
    public partial class ContractCompiler : Page
    {
        public ContractCompiler()
        {
            InitializeComponent();
        }
        System.Net.WebClient wc = new client.MyWC();

        public void ClearLog()
        {
            Action safelog = () =>
            {
                this.listDebug.Items.Clear();
            };
            this.Dispatcher.Invoke(safelog);
        }
        public void Log(string log)
        {
            Action<string> safelog = (_log) =>
            {
                this.listDebug.Items.Add(_log);
            };
            this.Dispatcher.Invoke(safelog, log);
        }
        public class HighlightCurrentLineBackgroundRenderer : IBackgroundRenderer
        {
            private TextEditor _editor;

            public HighlightCurrentLineBackgroundRenderer(TextEditor editor)
            {
                _editor = editor;
            }

            public KnownLayer Layer
            {
                get { return KnownLayer.Selection; }
            }

            public void Draw(TextView textView, DrawingContext drawingContext)
            {
                if (_editor.Document == null)
                    return;

                textView.EnsureVisualLines();
                var currentLine = _editor.Document.GetLineByOffset(_editor.CaretOffset);
                foreach (var rect in BackgroundGeometryBuilder.GetRectsForSegment(textView, currentLine))
                {
                    drawingContext.DrawRectangle(
                        new SolidColorBrush(Color.FromArgb(0x40, 0, 0, 0xFF)), null,
                        new Rect(rect.Location, new Size(textView.ActualWidth - 32, rect.Height)));
                }
            }
        }
        Result buildResult = null;
        ThinNeo.Debug.Helper.AddrMap debugInfo = null;
        private void Button_Click(object sender, RoutedEventArgs e)
        {//testApi
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
        public static string CalcScriptHashString(byte[] script)
        {
            var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] hash256 = sha256.ComputeHash(script);
            var ripemd160 = new ThinNeo.Cryptography.Cryptography.RIPEMD160Managed();
            var hash = ripemd160.ComputeHash(hash256);
            StringBuilder sb = new StringBuilder();
            sb.Append("0x");
            foreach (var b in hash.Reverse().ToArray())
            {
                sb.Append(b.ToString("x02"));
            }
            return sb.ToString();
        }
        public class Result
        {
            public string script_hash;
            public string srcfile;
            public byte[] avm;
            public string debuginfo;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ICSharpCode.AvalonEdit.TextEditor code = codeEdit;
            codeEdit.TextArea.TextView.BackgroundRenderers.Add(
    new HighlightCurrentLineBackgroundRenderer(code));
            codeEdit.TextArea.Caret.PositionChanged += (s, ee) =>
              {
                  if (this.debugInfo == null)
                      return;
                  var pos = codeEdit.CaretOffset;
                  var line = codeEdit.Document.GetLineByOffset(pos).LineNumber;
                  var addr = this.debugInfo.GetAddrBack(line);
                  if (addr >= 0)
                  {
                      foreach (ThinNeo.Compiler.Op item in this.listASM.Items)
                      {
                          if (item != null && item.addr == addr)
                          {
                              this.listASM.SelectedItem = item;
                              this.listASM.ScrollIntoView(item);
                              break;
                          }
                      }
                  }
              };
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {//save button

            //save file
            var code = codeEdit.Text;
            byte[] bs = System.Text.Encoding.UTF8.GetBytes(code);
            SHA1 sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(bs);
            var hashstr = ThinNeo.Helper.Bytes2HexString(hash);
            var path = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location), "temp");
            if (System.IO.Directory.Exists(path) == false)
                System.IO.Directory.CreateDirectory(path);

            var filename = System.IO.Path.Combine(path, hashstr + ".cs");
            System.IO.File.WriteAllBytes(filename, bs);


            //call api upload file
            var apitext = textAPI.Text;
            var url = apitext + "parse?language=csharp";

            string strback = null;
            try
            {
                byte[] retvar = wc.UploadFile(url, filename);
                strback = System.Text.Encoding.UTF8.GetString(retvar);
            }
            catch(Exception err)
            {
                return;
            }
            ClearLog();
            this.textHash.Text = "";
            this.textHexScript.Text = "";
            var compilereuslt = MyJson.Parse(strback).AsDict();
            if (compilereuslt.ContainsKey("tag"))
            {
                var tag = compilereuslt["tag"].AsInt();
                if (tag == 0)
                {
                    this.Log("build ok.");
                    var rhash = compilereuslt["hash"].AsString();
                    var rhex = compilereuslt["hex"].AsString();
                    var rabi = Uri.UnescapeDataString(compilereuslt["funcsigns"].AsString());
                    this.textHash.Text = rhash;
                    this.textHexScript.Text = rhex;
                    updateASM(rhex);
                }
                else
                {
                    this.Log("build error:" + tag);
                    try
                    {
                        var msg = compilereuslt["msg"].AsString();
                        this.Log(msg);
                    }
                    catch
                    {
                    }
                    try
                    {
                        var error = compilereuslt["errors"].AsList();
                        foreach (MyJson.JsonNode_Object ee in error)
                        {
                            var _tag = ee.ContainsKey("tag") ? ee["tag"].AsString() : "";
                            var _id = ee.ContainsKey("id") ? ee["id"].AsString() : "";
                            var _msg = ee.ContainsKey("msg") ? ee["msg"].AsString() : "";
                            var _line = ee.ContainsKey("line") ? ee["line"].AsInt() : -1;
                            var _col = ee.ContainsKey("col") ? ee["col"].AsInt() : -1;
                            string line = _id + ":" + _tag + " " + _msg + "(" + _line + "," + _col + ")";
                            Log(line);
                        }
                    }
                    catch
                    {

                    }
                }
            }

        }
        void updateASM(string hex)
        {
            listASM.Items.Clear();
            //build asm
            ThinNeo.Compiler.Op[] ops = null;
            try
            {
                var data = ThinNeo.Helper.HexString2Bytes(hex);
                ops = ThinNeo.Compiler.Avm2Asm.Trans(data);
                foreach (var op in ops)
                {
                    var str = "";
                    try
                    {
                        str = op.ToString();
                    }
                    catch
                    {
                        str = "op fail:";
                    }
                    listASM.Items.Add(str);
                }
            }
            catch (Exception err)
            {
            }

        }
        private void listASM_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var op = this.listASM.SelectedItem as ThinNeo.Compiler.Op;
            if (op == null) return;
            var line = this.debugInfo.GetLineBack(op.addr);
            textAsm.Text = "srcline=" + line;
            if (line > 0)
            {
                var ioff = this.codeEdit.Document.Lines[line - 1].Offset;
                var len = this.codeEdit.Document.Lines[line - 1].Length;
                this.codeEdit.CaretOffset = ioff;
                //this.codeEdit.Select(ioff, 0);
                this.codeEdit.ScrollToLine(line - 1);
                codeEdit.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
            }

        }
    }
}
