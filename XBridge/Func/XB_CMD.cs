using HuajiTech.CoolQ;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using File = System.IO.File;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XBridge.Config;
using XBridge.Utils;

namespace XBridge.Func
{
    class XB_CMD
    {
        public static string runcode(string cmd) {
            var c = cmd.Split(' ');
            try
            {
                #region xb_console解释器
                switch (c[0])
                {
                    case "wl":
                        switch (c[1])
                        {
                            case "add":
                                if (Data.xboxid_exsis(c[2]))
                                    return $"qq {c[2]} already in whitelist.";
                                Data.wl_add(long.Parse(c[2]), cmd.Substring($"wl add {c[2]} ".Length));
                                return "key add to whitelist";
                            case "remove":
                                if (Data.xboxid_exsis(c[2]))
                                {
                                    Data.wl_exsis(Data.id2qq(c[2]));
                                    return $"xboxid {c[2]} remove from whitelist.";
                                }
                                if(long.TryParse(c[2],out var foo))
                                {
                                    if (Data.wl_exsis(long.Parse(c[2])))
                                    {
                                        Data.wl_remove(long.Parse(c[2]));
                                        return $"qq {c[2]} remove from whitelist.";
                                    }
                                }
                                return $"{c[2]} not in whitelist.";
                            case "list":
                                string b = "[whitelist]";
                                foreach (var i in Main.playerdatas)
                                {
                                    b += $"\n{i.Key}:{i.Value.xboxid}";
                                }
                                return b;
                            case "get":
                                if (long.TryParse(c[2], out var fooo))
                                {
                                    if (Data.wl_exsis(long.Parse(c[2])))
                                    {
                                        return "find " + Data.get_xboxid(long.Parse(c[2]));
                                    }
                                }
                                if(Data.xboxid_exsis(cmd.Substring($"wl get ".Length)))
                                {
                                    return "find " + Data.id2qq(cmd.Substring($"wl get ".Length)).ToString();
                                }
                                return "not find";
                            default:
                                return $"command wl donot have overload >>{c[1]}<<";
                        }
                    case "lua":
                        switch (c[1])
                        {
                            case "run":
                                try
                                {
                                    return Main.lua.DoString(cmd.Substring(8)).ToDebugPrintString();
                                }
                                catch (Exception e) { return e.Message; }
                            case "call":
                                try
                                {
                                    return Main.lua.Globals.Get(c[2]).Function.Call().ToDebugPrintString();
                                }
                                catch (Exception ex) { return ex.Message; }
                            default:
                                return $"command lua donot have overload >>{c[1]}<<";
                        }
                    case "ws":
                        switch (c[1])
                        {
                            case "close":
                                if (Data.is_server(c[2]))
                                {
                                    Main.sockets[c[2]].Close();
                                    return $"socket {c[2]} close";
                                }
                                else { return $"not a socket named {c[2]}"; }
                            case "send":
                                if (Data.is_server(c[2]))
                                {
                                    Main.sockets[c[2]].Send(cmd.Substring(8));
                                    return $"pack send to {c[2]}";
                                }
                                else { return $"not a socket named {c[2]}"; }
                            default:
                                return $"command ws donot have overload >>{c[1]}<<";
                        }
                    case "admin":
                        switch (c[1])
                        {
                            case "add":
                                try { long q = long.Parse(c[2]); }
                                catch { return $"unable to convert {c[2]} to number"; }
                                if (Data.is_admin(long.Parse(c[2])))
                                    return $"member {c[2]} already in admins.";
                                else
                                    Main.setting.Admins.Add(long.Parse(c[2]));
                                return $"member {c[2]} add to admins.";
                            case "remove":
                                try { long q = long.Parse(c[2]); }
                                catch { return $"unable to convert {c[2]} to number."; }
                                if (!Data.is_admin(long.Parse(c[2])))
                                    return $"member not in admins.";
                                else
                                    Main.setting.Admins.Remove(long.Parse(c[2]));
                                return $"member {c[2]} remove from admins.";
                            default:
                                return $"command admin donot have overload >>{c[1]}<<";
                        }
                    case "setconfig":
                        switch (c[1])
                        {
                            case "chat":
                                Main.tmp.chatindex = c[2].Replace("{空格}", " ");
                                return "chat 索引已更改为 " + Main.tmp.chatindex;
                            case "autowl":
                                Main.tmp.autowl = bool.Parse(c[2]);
                                return "自助白名单 模式已更新为 " + Main.tmp.autowl.ToString();

                            case "chatenable":
                                Main.tmp.chatenable = bool.Parse(c[2]);
                                return "聊天转发 已更新为 " + Main.tmp.chatenable.ToString();

                            case "joinenable":
                                Main.tmp.joinenable = bool.Parse(c[2]);
                                return "入服通知 已更新为 " + Main.tmp.joinenable.ToString();

                            case "leftenable":
                                Main.tmp.leftenable = bool.Parse(c[2]);
                                return "出服通知 已更新为 " + Main.tmp.leftenable.ToString();

                            case "mobdieenable":
                                Main.tmp.mobdieenable = bool.Parse(c[2]);
                                return "死亡通知 已更新为 " + Main.tmp.mobdieenable.ToString();
                            case "chatsub":
                                Main.tmp.chatsub = int.Parse(c[2]);
                                return "聊天长度截取值 已更新为 " + Main.tmp.chatsub.ToString();
                            default:
                                return $"command setconfig donot have overload >>{c[1]}<<";
                        }
                    case "reload":
                        var path = CurrentPluginContext.Bot.AppDirectory.FullName;
                        var l = File.ReadAllLines(path + ".lang");
                        foreach (var i in l)
                        {
                            if (i.StartsWith("#"))
                                continue;
                            try
                            {
                                var mm = i.Split('=');
                                Lang.set(mm[0], mm[1]);
                            }
                            catch { }
                        }
                        try
                        {
                            LUAAPI.Clear();
                            LUAAPI.LoadFile();
                            Main.regexs = JsonConvert.DeserializeObject<List<RegexItem>>(File.ReadAllText(path + "Regex.json"));
                            Main.setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(path + "setting.json"));
                            return "配置文件重载完成";
                        }
                        catch (Exception x) { return $"配置文件重载失败:{x}"; }
                    default:
                        return $"unknow command {c[0]}";
                }
                #endregion
            }catch(Exception e)
            { return e.ToString(); }
        }
    }
}
