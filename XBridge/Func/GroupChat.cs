using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HuajiTech.CoolQ;
using HuajiTech;
using HuajiTech.CoolQ.Events;
using HuajiTech.CoolQ.Messaging;
using XBridge.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XBridge.Config;
using Message = XBridge.Utils.Message;

namespace XBridge.Func
{
    class GroupChat
    {
        public static void on_chat(object sender, GroupMessageReceivedEventArgs e)
        {
            if (Main.setting.Group.chat.Contains(e.Source.Number))
            {
                var plain = e.Message.Parse().GetPlainText();
                if(Main.tmp.chatindex == "*")
                {
                    if(Message.GetMsg(e) != string.Empty)
                        SendPack.bcText(Lang.get("GROUP_MEMBER_CHAT",name2xboxid(e.Sender.DisplayName,e.Sender.Number), Message.GetMsg(e)));
                }
                else
                {
                    if(plain.Substring(0,Main.tmp.chatindex.Length) == Main.tmp.chatindex)
                    {
                        if (plain.Length > Main.tmp.chatindex.Length+1)//plain.Substring(Main.tmp.chatindex.Length)
                            SendPack.bcText(Lang.get("GROUP_MEMBER_CHAT", name2xboxid(e.Sender.DisplayName, e.Sender.Number), plain.Substring(Main.tmp.chatindex.Length)));
                    }
                }
            }
        }
        private static string name2xboxid(string name,long q)
        {
            if (Data.wl_exsis(q))
                return Data.get_xboxid(q);
            return name;
        }

    }
}
