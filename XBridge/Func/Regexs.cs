using System;
using System.Collections.Generic;
using System.Linq;
using XBridge.Utils;
using HuajiTech.CoolQ.Messaging;
using System.Text;
using HuajiTech.CoolQ;
using HuajiTech.CoolQ.Events;
using System.Text.RegularExpressions;
using XBridge.Config;
using Message = XBridge.Utils.Message;
using System.Threading.Tasks;

namespace XBridge.Func
{
    class Regexs
    {
        /// <summary>
        /// 正则表达式组
        /// </summary>
        public static List<RegexItem> regexs = new List<RegexItem>();
        private static string buildString(GroupMessageReceivedEventArgs e, string input)
        {
            long qq = e.Sender.Number;
            StringBuilder builder = new StringBuilder(input);
            var mb = CurrentPluginContext.Member(qq, e.Source.Number);
            if (Data.wl_exsis(qq))
                builder.Replace("%xboxid%", Data.get_xboxid(qq));
            builder.Replace("%qqid%", qq.ToString());
            builder.Replace("%qqnick%", mb.Nickname ?? mb.DisplayName);
            builder.Replace("%level%", mb.Level ?? "0");
            builder.Replace("%random%", new Random().Next(1, 100).ToString());
            if (Message.GetAt(e).Length != 0)
            {
                try
                {
                    long atqq = Message.GetAt(e)[0];
                    var mmb = CurrentPluginContext.Member(atqq, e.Source.Number);
                    if (Data.wl_exsis(atqq))
                        builder.Replace("%atxboxid%", Data.get_xboxid(atqq));
                    builder.Replace("%atqqid%", atqq.ToString());
                    builder.Replace("%atqqnick%", mmb.Nickname ?? mmb.DisplayName);
                }
                catch(Exception ex) { CurrentPluginContext.Logger.LogError(ex.ToString()); }
            }
            builder.Replace("%datetime%", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            return builder.ToString();
        }
        public static void on_regex(object sender,GroupMessageReceivedEventArgs e)
        {
            long groupid = e.Source.Number;
            if (Setting.setting.Group.main.Contains(groupid) || Setting.setting.Group.chat.Contains(groupid))
            {
                string input = e.Message.Parse().GetPlainText();
                long qq = e.Sender.Number;
                foreach (var i in regexs)
                {
                    Match mat = Regex.Match(input, i.Regex);
                    if (mat.Success)
                    {
                        if (i.permission == 1 && !Data.is_admin(qq))
                            continue;
                        foreach (var it in i.@out)
                        {
                            string o = it.text;
                            for (int io = 1; io < mat.Groups.Count; io++)
                            {
                                o = o.Replace($"${io}", mat.Groups[io].Value);
                            }
                            o = buildString(e, o);
                            bool cou = false;
                            try
                            {
                                cou = on_regex_item(groupid, it, o, qq);
                            }
                            catch (Exception exx) { e.Source.Send($"执行[{i.Regex}]时出错:{exx.Message}"); }
                            if (!cou)
                                break;
                        }

                    }
                }
            }
        }
        private static bool on_regex_item(long group,Out it, string o, long qq)
        {
            switch (it.type)
            {
                case "runcmdall":
                    SendPack.runcmdAll(group,o);
                    return true;
                case "runcmd":
                    var c = o.Split('|');
                    if (Data.is_server(c[0]))
                    {
                        SendPack.runcmd(group,c[0], c[1]);
                        return true;
                    }
                    return false;
                case "group":
                    CurrentPluginContext.Group(group).Send(o);
                    return true;
                case "textall":
                    SendPack.bcText(o);
                    return true;
                case "xb_wl_remove":
                    if (Data.wl_exsis(qq))
                    {
                        Data.wl_remove(qq);
                        return true;
                    }
                    return false;
                case "xb_wl_add":
                    if (!Data.wl_exsis(qq))
                    {
                        Data.wl_add(qq, o);
                        return true;
                    }
                    return false;
                case "xb_cmd":
                    CurrentPluginContext.Group(group).Send(XB_CMD.runcode(o));
                    return true;
                case "http_get":
                    Task.Run(() =>
                    {
                        try
                        {
                            new System.Net.WebClient().DownloadData(o);
                        }
                        catch {  }
                    });
                    return true;
                default:
                    return false;
            }
        }
    }
}
