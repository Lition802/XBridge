using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XBridge.Utils;
using System.Windows;
using System.Windows.Forms;
using System.Threading;

namespace XDBTool
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("请选择一个要打开的.xdb文件");
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Multiselect = false;//该值确定是否可以选择多个文件
                dialog.Title = "请选择文件夹";
                dialog.Filter = "xdb文件(*.xdb)|*.xdb";
                Thread t = new Thread(() =>
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        string file = dialog.FileName;
                        var db = new XDB(file, "20040614");
                        foreach (var u in db.getAll())
                        {
                            Console.WriteLine($"{u.Key}\t\t{u.Value.xboxid}\tJ:{u.Value.count.join}\tD:{u.Value.count.death}");
                        }
                        Console.ReadKey();
                    }
                });
                t.SetApartmentState(ApartmentState.STA);
                t.Start();
            }
            
        }
    }
}
