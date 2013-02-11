using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Hal.CookieGetterSharp;


namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        delegate void SetTextCallback(string restring);
        Socket sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
        public static byte[] rebite = new byte[1024];
        byte[] rebyte = new byte[1024];
        Cookie us;
        CookieCollection cc;
        ICookieGetter cookie = null;
        SocketAsyncEventArgs e = new SocketAsyncEventArgs();


        private void button1_Click(object sender, EventArgs e)
        {
           
            getplayerstatus(textBox1.Text);
            commentreceive(sender);
        }

        public void commentreceive(object sender)
        {
            string addr = null;
            int port = 0;
            string thread = null;
            byte[] sendbyte = null;
            try
            {
                XDocument doc = XDocument.Load("status.xml");
                XElement element = doc.Element("getplayerstatus");
                element = element.Element("ms");
                addr = element.Element("addr").Value;
                port = int.Parse(element.Element("port").Value);
                thread = element.Element("thread").Value;



                string send = "<thread thread=\"" + thread + "\"  res_from=\"-50\" version=\"20061206\"/>\0";


                sendbyte = Encoding.UTF8.GetBytes(send);
                sock.Connect(addr, port);



                sock.Send(sendbyte);

                object state = new object();



                e.SetBuffer(rebyte, 0, 1023);

                sock.ReceiveAsync(e);
            
            
            Task.Factory.StartNew(() =>
                {
                   
                    while (true)
                    {
                        sock.Receive(rebyte);
                        string restring = Encoding.UTF8.GetString(rebyte);
                        

                        if (this.InvokeRequired)
                        {
                            SetTextCallback d = new SetTextCallback(text);
                            this.Invoke(d, new object[] {restring});
                        }
                        else
                        {
                            textBox2.Text = restring;
                            
                        }

                    }                
                });
            }
            catch (Exception)
            {
                label1.Text = "ステータスの取得に失敗しました。放送が終了している可能性があります。";
            }
        }

        public void text(string restring)
        {
            textBox2.Text = restring;
            
             
        }

        public void getplayerstatus(string liveid)
        {
            


            WebClientEx wc = new WebClientEx();

            wc.CookieContainer = new CookieContainer();
            try
            {
                wc.CookieContainer.Add(us);
           

            wc.DownloadFile(new Uri("http://live.nicovideo.jp/api/getplayerstatus?v=" + liveid), "status.xml");
            }
            catch (Exception)
            {
                MessageBox.Show("クッキー取得先を選択してください");
                
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.Items.AddRange(CookieGetter.CreateInstances(true));
            label1.Text = null;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == 0)
            {
                cookie = CookieGetter.CreateInstance(BrowserType.IEComponent);
            }
            if (comboBox1.SelectedIndex == 1)
            {
                cookie = CookieGetter.CreateInstance(BrowserType.IESafemode);
            }
            if (comboBox1.SelectedIndex == 2)
            {
                cookie = CookieGetter.CreateInstance(BrowserType.Firefox);
            }
            if (comboBox1.SelectedIndex == 3)
            {
                cookie = CookieGetter.CreateInstance(BrowserType.GoogleChrome);
            }
            if (comboBox1.SelectedIndex == 4)
            {
                cookie = CookieGetter.CreateInstance(BrowserType.Safari);
            }
            

            try
            {
                cc = cookie.GetCookieCollection(new Uri("http://nicovideo.jp"));
                us = cc["user_session"];
                if (us != null)
                {

                    label1.Text = "クッキー取得成功" + us;
                }
                else
                {
                    label1.Text = "クッキー取得失敗";
                }
            }
            catch(Exception ex)
            {
                label1.Text = "クッキー取得失敗";

            }


        }

    }
    class WebClientEx : System.Net.WebClient
    {
        private CookieContainer cookieContainer;

        public CookieContainer CookieContainer
        {
            get
            {
                return cookieContainer;
            }
            set
            {
                cookieContainer = value;
            }
        }

        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest webRequest = base.GetWebRequest(uri);

            if (webRequest is HttpWebRequest)
            {
                HttpWebRequest httpWebRequest = (HttpWebRequest)webRequest;
                httpWebRequest.CookieContainer = this.cookieContainer;
            }

            return webRequest;
        }
      


    }
}
