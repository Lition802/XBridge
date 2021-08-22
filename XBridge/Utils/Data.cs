using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using File = System.IO.File;
using HuajiTech.CoolQ;
using System.IO;
using XBridge.Config;

namespace XBridge.Utils
{
    class Data
    {
        /// <summary>
        /// 判断是否是服务器
        /// </summary>
        /// <param name="ser">名称</param>
        /// <returns></returns>
        public  static bool is_server(string ser)
        {
            return Main.sockets.ContainsKey(ser);
        }
        public static bool wl_exsis(long q)
        {
            return Main.playerdatas.ContainsKey(q);
        }
        public static bool wl_add(long q,string xboxid)
        {
            if (!Main.playerdatas.ContainsKey(q))
            {
                var pl = new PlayerData() { xboxid = xboxid, count = new Count() };
                Main.playerdatas.Add(q, pl);
                SAVE();
                return true;
            }
            return false;
        }
        public static bool wl_remove(long q)
        {
            if (Main.playerdatas.ContainsKey(q))
            {
                Main.playerdatas.Remove(q);
                SAVE();
                return true;
            }
            return false;
        }
        public static bool xboxid_exsis(string id)
        {
            foreach(var i in Main.playerdatas)
            {
                if (i.Value.xboxid == id)
                    return true;
            }
            return false;
        }
        public static string get_xboxid(long q)
        {
            foreach (var i in Main.playerdatas)
            {
                if (i.Key == q)
                    return i.Value.xboxid;
            }
            return null;
        }
        public static bool is_admin(long q)
        {
            foreach (var i in Main.setting.Admins)
            {
                if (i == q)
                    return true;
            }
            return false;
        }
        public static long id2qq(string id) {
            foreach (var i in Main.playerdatas)
            {
                if (i.Value.xboxid == id)
                    return i.Key;
            }
            return 0;
        }
        public static int CheckJoinTime(string xboxid)
        {
            if (xboxid_exsis(xboxid))
            {
                return Main.playerdatas[id2qq(xboxid)].count.join;
            }
            return 0;
        }
        public static void SAVE()
        {
            File.WriteAllText(CurrentPluginContext.Bot.AppDirectory + "playerdata.json",JsonConvert.SerializeObject(Main.playerdatas));
        }
    }
}
