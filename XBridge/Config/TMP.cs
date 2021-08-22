using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XBridge.Config
{
    class TMP
    {
        /// <summary>
        /// 是否转发聊天
        /// </summary>
        public bool chatenable = true;
        /// <summary>
        /// 是否转发加入服务器
        /// </summary>
        public bool joinenable = true;
        /// <summary>
        /// 是否转发离开服务器
        /// </summary>
        public bool leftenable = true;
        /// <summary>
        /// 是否转发生物死亡
        /// </summary>
        public bool mobdieenable = true;
        /// <summary>
        /// 聊天索引
        /// </summary>
        public string chatindex = "*";
        /// <summary>
        /// 群聊长度截取
        /// </summary>
        public int chatsub = 20;
        /// <summary>
        /// 自动白名单
        /// </summary>
        public bool autowl = false;
    }
}
