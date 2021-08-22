using System.Collections.Generic;
using HuajiTech.CoolQ;
using HuajiTech.CoolQ.Events;
using XBridge.Config;
using XBridge.Func;
using MoonSharp.Interpreter;
using System.Threading;
using XBridge.Utils;

namespace XBridge
{
    /// <summary>
    /// 包含应用的逻辑。
    /// </summary>
    internal class Main : Plugin
    {
        #region 配置文件和临时文件
        public static TMP tmp = new TMP();
        /// <summary>
        /// 正则表达式组
        /// </summary>
        public static List<RegexItem> regexs = new List<RegexItem>();
        /// <summary>
        /// 玩家数据
        /// </summary>
        public static Dictionary<long, PlayerData> playerdatas = new Dictionary<long, PlayerData>();
        /// <summary>
        /// 基本设置
        /// </summary>
        public static Setting setting = new Setting();
        /// <summary>
        /// 执行命令临时id
        /// </summary>
        public static Dictionary<string, string> runcmdid = new Dictionary<string, string>();
        /// <summary>
        /// 连接池
        /// </summary>
        public static Dictionary<string, Websocket.Websocket> sockets = new Dictionary<string, Websocket.Websocket>();
        /// <summary>
        /// 生物文件
        /// </summary>
        public static Dictionary<string, string> mobs = new Dictionary<string, string>();
        /// <summary>
        /// 死亡id
        /// </summary>
        public static Dictionary<string, string> die_id = new Dictionary<string, string>();
        /// <summary>
        /// lua虚拟机
        /// </summary>
        public static Script lua = new Script();
        #endregion
        /// <summary>
        /// 使用指定的事件源初始化一个 <see cref="Main"/> 类的新实例。
        /// </summary>
        public Main(INotifyGroupMessageReceived notifyGroupMessageReceived, IGroupEventSource groupEventSource)
        {
            INIT.init();
            foreach (var i in setting.Servers) {
                var ws = new Websocket.Websocket(i.Url,i.name,i.password);
                ws.AddFunction("onMessage", WSReceive.on_ws_pack);
                ws.Start();
                sockets.Add(i.name, ws);
                runcmdid.Add(i.name, string.Empty);
            }
            new Thread(() =>
            {
                while (true) {
                    Thread.Sleep(5000);
                    try
                    {
                        foreach (var i in sockets)
                        {
                            if (!i.Value.IfAlive())
                                i.Value.Start();
                        }
                    }
                    catch { }
                }
            }).Start();
            notifyGroupMessageReceived.GroupMessageReceived += Group_CMD.xbridge_cmd;
            groupEventSource.MemberLeft += MemberLeft.Member_Left;
            if (setting.enable.xb_lua)
            {
                LUAAPI.init();
                LUAAPI.LoadFile();
                notifyGroupMessageReceived.GroupMessageReceived += GroupLua.on_message;
            }
            if (setting.enable.xb_regex)
            {
                notifyGroupMessageReceived.GroupMessageReceived += Regexs.on_regex;
            }
            if (setting.enable.xb_native)
            {
                notifyGroupMessageReceived.GroupMessageReceived += GroupMain.on_main;
                notifyGroupMessageReceived.GroupMessageReceived += GroupChat.on_chat;
            }
            
        }
    }
}