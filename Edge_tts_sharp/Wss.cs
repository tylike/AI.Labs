using Edge_tts_sharp.Model;
using System;
using System.Collections.Generic;
using System.Text;
using WebSocketSharp;

namespace Edge_tts_sharp
{
    public enum SslProtocolsHack
    {
        Tls = 192,
        Tls11 = 768,
        Tls12 = 3072
    }

    public class Wss
    {
        public WebSocket wss { get; set; }
        public event Action<Log> OnLog;
        public event EventHandler<MessageEventArgs> OnMessage;
        public event EventHandler<CloseEventArgs> OnColse;
        public string wssAddress { get; set; }
        public Wss(string url)
        {
            wssAddress = url;
            wss = new WebSocket(wssAddress);
            if (url.Contains("wss://"))
            {
                wss.SslConfiguration.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            }
            wss.OnOpen += (sender, e) => {
                OnLog(new Log { level = level.info, msg = "WebSocket Open" });
            };
            wss.OnMessage += (sender, e)=> OnMessage(sender, e);
            wss.OnClose += (sender, e) =>
            {
                var sslProtocolHack = (System.Security.Authentication.SslProtocols)(SslProtocolsHack.Tls12 | SslProtocolsHack.Tls11 | SslProtocolsHack.Tls);
                //TlsHandshakeFailure
                if (e.Code == 1015 && wss.SslConfiguration.EnabledSslProtocols != sslProtocolHack)
                {
                    OnLog(new Log { level = level.error, msg = "ssl握手失败，正在尝试重新连接." });
                    wss.SslConfiguration.EnabledSslProtocols = sslProtocolHack;
                    wss.Connect();
                }
                else
                {
                    OnColse(sender, e);
                }
            };
        }
        public bool Run()
        {
            wss.Connect();
            if (wss.IsAlive)
            {
                OnLog(new Log { level = level.info, msg = "WebSocket 连接成功." });
            }
            if (wss.IsSecure)
            {
                OnLog(new Log { level = level.info, msg = "WebSocket 是安全的." });
            }
            return wss.IsAlive;
        }
        public void Close()
        {
            wss.Close();
        }
        public void Send(string msg)
        {
            wss.Send(msg);
            OnLog(new Log { level = level.info, msg = $"WebSocket send msg:{msg}" });
        }
        public void SendByte(byte[] msg)
        {
            wss.Send(msg);
            OnLog(new Log { level = level.info, msg = "WebSocket send msg: binary" });
        }
    }
}
