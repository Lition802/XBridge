﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HuajiTech.CoolQ;
using HuajiTech;
using HuajiTech.CoolQ.Events;
using HuajiTech.CoolQ.Messaging;
using XBridge.Utils;
using XBridge.Config;
using Message = XBridge.Utils.Message;

namespace XBridge.Func
{
    class GroupMain
    {
        public static void on_main(object sender, GroupMessageReceivedEventArgs e)
        {
            if (Main.setting.Group.main.Contains(e.Source.Number))
            {
                var plain = e.Message.Parse().GetPlainText();
                var pl = plain.Split(' ');
                switch (pl[0])
                {
                    case "/bind":
                        if(plain.Length > 7)
                        {
                            if (Data.wl_exsis(e.Sender.Number))
                            {
                                e.Reply(Lang.get("MEMBER_ALREADY_IN_WHITELIST",Data.get_xboxid(e.Sender.Number)));
                                return;
                            }
                            if (Data.xboxid_exsis(plain.Substring(6)))
                            {
                                e.Reply(Lang.get("XBOXID_ALREADY_BIND"));
                                return;
                            }
                            Data.wl_add(e.Sender.Number, plain.Substring(6));
                            e.Reply(Lang.get("MEMBER_BIND_SUCCESS",plain.Substring(6)));
                            if (Main.tmp.autowl)
                            {
                                SendPack.runcmdAll($"whitelist add \"{Data.get_xboxid(e.Sender.Number)}\"");
                                e.Reply(Lang.get("XBOXID_ADD_TO_SERVER_SUCCESS"));
                            }
                        }
                        break;
                    case "/unbind":
                        if (Data.wl_exsis(e.Sender.Number))
                        {
                            Data.wl_remove(e.Sender.Number);
                            e.Reply(Lang.get("MEMBER_UNBIND"));
                        }
                        else
                        {
                            e.Reply(Lang.get("MEMBER_NOT_BIND"));
                        };
                        break;
                    case "wl+":
                        if (Message.GetAt(e).Length != 0)
                        {
                            //long q = Message.GetAt(e)[0];
                            if (!Data.is_admin(e.Sender.Number))
                            {
                                e.Reply(Lang.get("MEMBER_NOT_ADMIN"));
                                return;
                            }
                            foreach (long q in Message.GetAt(e))
                            {
                                if (Data.wl_exsis(q))
                                {
                                    e.Reply(Lang.get("ADD_WL_TO_SERVER", q.ToString(), Data.get_xboxid(q)));
                                    SendPack.runcmdAll($"whitelist add \"{Data.get_xboxid(q)}\"");
                                }
                                else
                                {
                                    e.Reply(Lang.get("MEMBER_NOT_BIND_WHEN_EWMOVE", q.ToString()));
                                }
                            }
                        }
                        break;
                    case "wl-":
                        var m2 = e.Message.Parse();
                        if (m2.Contains(CurrentPluginContext.CurrentUser.Mention()))
                        {
                            //long q = Message.GetAt(e)[0];
                            if (!Data.is_admin(e.Sender.Number))
                            {
                                e.Reply(Lang.get("MEMBER_NOT_ADMIN"));
                                return;
                            }
                            foreach(long q in Message.GetAt(e))
                            {
                                if (Data.wl_exsis(q))
                                {
                                    e.Reply(Lang.get("REMOVE_WL_TO_SERVER", q.ToString(), Data.get_xboxid(q)));
                                    SendPack.runcmdAll($"whitelist remove \"{Data.get_xboxid(q)}\"");
                                    Data.wl_remove(q);
                                }
                                else
                                {
                                    e.Reply(Lang.get("MEMBER_NOT_BIND_WHEN_EWMOVE", q.ToString()));
                                }
                            }
                        }
                        break;
                    case "/cmd":
                        if (!Data.is_admin(e.Sender.Number))
                        {
                            e.Reply(Lang.get("MEMBER_NOT_ADMIN"));
                            return;
                        }
                        if (Main.setting.Servers.Count == 1)
                        {                            
                            if(pl.Length < 2)
                            {
                                e.Reply(Lang.get("COMMAND_OVERLOAD_NOTFIND"));
                                return;
                            }
                            e.Reply(Lang.get("COMMAND_SENDTO_ALL_SERVER",plain.Substring(5)));
                            SendPack.runcmdAll(plain.Substring(5));
                        }
                        else
                        {
                            if (pl.Length < 3)
                            {
                                e.Reply(Lang.get("COMMAND_OVERLOAD_NOTFIND"));
                                return;
                            }
                            if (Data.is_server(pl[1]))
                            {
                                e.Reply(Lang.get("COMMAND_SENDTO_SERVER", plain.Substring($"/cmd {pl[1]} ".Length),pl[1]));
                                SendPack.runcmd(pl[1], plain.Substring($"/cmd {pl[1]} ".Length));
                            }
                        }
                        break;
                    case "查服":
                        SendPack.runcmdAll("list");
                        break;
                    case "白名单列表":
                        string b = "[白名单列表]";
                        foreach(var i in Main.playerdatas)
                        {
                            b += $"\n{i.Key}:{i.Value.xboxid}";
                        }
                        e.Source.Send(b);
                        break;
                }
            }
        }
    }
}