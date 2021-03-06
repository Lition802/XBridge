using System;
using WebSocketSharp;
using XBridge.Utils;

namespace XBridge.Websocket
{
    public class Websocket
    {
        private WebSocket webSocket;
        private string name;
        private string k;
        private string iv;
        public Websocket(string url,string n,string password)
        {
            webSocket = new WebSocket(url);
            this.name = n;
            k = MD5.MD5Encrypt(password).Substring(0, 16);
            iv = MD5.MD5Encrypt(password).Substring(16);
        }
        public bool Start()
        {
            if (!webSocket.IsAlive)
            {
                webSocket.Connect();
                return true;
            }
            return false;
        }
        public bool IfAlive()
        {
            return webSocket.IsAlive;
        }
        public bool Close()
        {
            if (webSocket.IsAlive)
            {
                webSocket.Close();
                return true;
            }
            return false;
        }
        public string getK { get { return this.k; } }
        public string getiv { get { return this.iv; } }
        public bool Send(object message)
        {
            if (webSocket.IsAlive)
            {
                webSocket.Send(message.ToString());
                return true;
            }
            return false;
        }
        /// <summary>
        /// 添加方法处理事件
        /// </summary>
        /// <param name="type">模式</param>
        /// <param name="func">调用函数</param>
        public void AddFunction(string type,Action<string,string> func)
        {
            switch (type)
            {
                case "onMessage":
                    webSocket.OnMessage += (sender, message) =>
                    {
                        try
                        {
                            func.Invoke(name, message.Data);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.ToString());
                        }
                    };
                    break;
                case "onOpen":
                    webSocket.OnOpen += (sender, ex) =>
                    {
                        try
                        {
                            func.Invoke(name,null);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    };
                    break;
                case "onClose":
                    webSocket.OnClose += (sender, message) =>
                    {
                        try
                        {
                            func.Invoke(name, message.Reason);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    };
                    break;
                case "onError":
                    webSocket.OnError += (sender, message) =>
                    {
                        try
                        {
                            func.Invoke(name, message.Message);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    };
                    break;
            }

        }
    }
}

