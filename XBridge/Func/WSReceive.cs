using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HuajiTech.CoolQ;
using XBridge.WSPack;
using XBridge.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using XBridge.Config;
using MoonSharp.Interpreter;

namespace XBridge.Func
{
    class WSReceive
    {
        public static void on_ws_pack(string ser, string pack) {
            var jp = JObject.Parse(pack);
            if (jp["type"] != null) {
                switch (jp["type"].ToString()) {
                    case "pack":
                        try
                        {
                            on_pack(ser, pack);
                            if (Main.setting.enable.xb_lua)
                                lua_on_pack(ser, pack);
                        }
                        catch (Exception e) { send(0, $"与服务器[{ser}]的收信文本转换失败!({e.Message})"); }
                        break;
                    case "encrypted":
                        try {
                            string unecrypt = AES.AesDecrypt(jp["params"]["raw"].ToString(), Main.sockets[ser].getK, Main.sockets[ser].getiv);
                            on_pack(ser, unecrypt);
                            if (Main.setting.enable.xb_lua)
                                lua_on_pack(ser, unecrypt);
                        }
                        catch (Exception e) { send(0, $"与服务器[{ser}]的收信文本转换失败!({e.Message})"); }
                        break;
                }
            }
        }
        private static void send(int mode, object o) {
            switch (mode) {
                case 0:
                    foreach (long id in Main.setting.Group.main)
                        CurrentPluginContext.Group(id).Send(o.ToString());
                    break;
                case 1:
                    foreach (long id in Main.setting.Group.chat)
                        CurrentPluginContext.Group(id).Send(o.ToString());
                    break;
            }
        }
        private static void add_time(int mode, string id,int data) {
            if (Data.id2qq(id) != 0) {
                long q = Data.id2qq(id);
                switch (mode) {
                    case 0:
                        Main.playerdatas[q].count.join += data;
                        Data.SAVE();
                        break;
                    case 1:
                        Main.playerdatas[q].count.death += data;
                        Data.SAVE();
                        break;
                    case 2:
                        Main.playerdatas[q].count.duration += data;
                        Data.SAVE();
                        break;

                }
            }
        }
        private static void on_pack(string ser,string p) {
            var type = JObject.Parse(p);
            var param = JsonConvert.DeserializeObject<@params>(type["params"].ToString());
            if (type["cause"] == null)
                return;
            switch (type["cause"].ToString())
            {
                case "join":
                    SendPack.bcText(ser, Lang.get("SERVER_MEMBER_JOIN", ser, param.sender));
                    add_time(0, param.sender, 1);
                    if (!Main.tmp.joinenable)
                        return;
                    send(1, Lang.get("MEMBER_JOIN", ser, param.sender,Data.CheckJoinTime(param.sender).ToString()));                                       
                    break;
                case "left":
                    SendPack.bcText(ser, Lang.get("SERVER_MEMBER_LEFT", ser, param.sender));
                    if (!Main.tmp.leftenable)
                        return;
                    send(1, Lang.get("MEMBER_LEFT", ser, param.sender));               
                    break;
                case "chat":
                    SendPack.bcText(ser, Lang.get("SERVER_MEMBER_CHAT", ser, param.sender, param.text));
                    if (!Main.tmp.chatenable)
                        return;
                    send(1, Lang.get("MEMBER_CHAT", ser, param.sender,param.text));    
                    break;
                case "runcmdfeedback":
                    if (Main.runcmdid[ser] == param.id) {
                        send(0, Lang.get("CMD_FEEDBACK", ser,param.result));
                    }
                    break;
                case "mobdie":
                    if (!Main.tmp.mobdieenable)
                        return;
                    if (param.mobtype != "" && param.mobname != "")
                    {
                        string mob = "entity." + param.srctype.ToLower() + ".name";
                        if (Main.mobs.ContainsKey(mob) && param.srctype.ToLower() != "unknown")
                        {
                            send(1, Lang.get("MEMBER_KILL_BY_MOBS", ser, param.mobname,Main.mobs[mob]));
                        }
                        else if (Main.die_id.ContainsKey(param.dmcase.ToString()))
                        {
                            send(1, Lang.get("MEMBER_KILL_BY_SELF", ser, param.mobname, Main.die_id[param.dmcase.ToString()]));
                        }
                        else
                        {
                            send(1, Lang.get("MEMBER_KILL_BY_UNKNOWN", ser, param.mobname));
                        }
                        add_time(1, param.mobname, 1);
                    }
                    break;
                case "decodefailed":
                    send(0,Lang.get("WSPACK_RECEIVE_ERROR",ser,param.msg));
                    break;
            }
            
        }
        private static Table GetTable(JObject p)
        {
            var t = new Table(Main.lua);
            t.Set("cause", DynValue.NewString(p["cause"].ToString()));
            var pa = new Table(Main.lua);
            System.Reflection.MemberInfo[] members = typeof(@params).GetProperties();
            foreach (var i in members)
            {
                if(p["params"][i.Name] != null)
                {
                    try { pa.Set(i.Name, DynValue.NewString(p["params"][i.Name].ToString())); }
                    catch { }
                }
            }
            //t.Set("server", DynValue.NewString(ser));
            t.Set("params", DynValue.NewTable(pa));
            return t;
        }
        private static void lua_on_pack(string ser, string p)
        {
            var type = JObject.Parse(p);
            var pa = JsonConvert.DeserializeObject<@params>(type["params"].ToString());
            if (type["cause"] == null)
                return;
            var t = GetTable( type);
            LUAAPI.func["ws"].ForEach(s =>
            {
                try
                {
                    s.Call(ser,t);
                }
                catch (Exception ex) {send(0,$"[Error][xb_lua] {ex}"); }
            });
        }
    }
}
