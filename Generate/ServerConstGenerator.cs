using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelConvertLib
{
    public class ServerConstGenerator : CTFDataConvert
    {
        bool IsSheetForServer(Excel.Worksheet ws)
        {
            string type = ConstGenerator.GetSheetType(ws);
            return (type == "common") || (type == "server");
        }

        string GetFilePath(string filename)
        {
            return MainModule.Inst.GetServerCodePath(filename);
        }

        List<Excel.Worksheet> CollectWorkSheet()
        {
            List<Excel.Worksheet> result = new List<Excel.Worksheet>();

            foreach (Excel.Worksheet ws in MainModule.Inst.GetWorkSheets())
            {
                if (ws == null)
                    continue;

                if (!IsSheetForServer(ws))
                    continue;

                result.Add(ws);
            }
            return result;
        }

        protected override int GetStartIndexConst()
        {
            return 3;
        }

        public bool OnGenerateConstServer(string constExcel)
        {
            string path = Utility.GetPath(MainModule.Inst.GetServerOutPath("common", ""));
            string dir = Path.GetDirectoryName(path);

            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            ServerGenerator ServerCodeGen = new ServerGenerator();

            foreach (Excel.Worksheet ws in CollectWorkSheet())
            {
                var filename = string.Format("{0}.ctf", ws.Name.ToLower());
                object[,] values = (object[,])ws.UsedRange.Value2;

                string output = Utility.GetPath(MainModule.Inst.GetServerOutPath("common", filename));

                StreamWriter writer = null;
                if (File.Exists(output))
                {
                    if (false == MainModule.CheckOutCTFFile(filename))
                    {
                        if (false == MainModule.AddCTFFile(filename, true))
                        {
                            LogSystem.Inst.Add(string.Format("perforce checkout error:{0}", output));
                            return false;
                        }
                    }
                    writer = new StreamWriter(new FileStream(@output, FileMode.Create, FileAccess.ReadWrite), new UTF8Encoding(true));
                }
                else
                {
                    writer = new StreamWriter(new FileStream(@output, FileMode.Create, FileAccess.ReadWrite), new UTF8Encoding(true));
                    if (false == MainModule.AddCTFFile(filename, true))
                    {
                        writer.Close();
                        LogSystem.Inst.Add(string.Format("perforce checkout error:{0}", output));
                        return false;
                    }
                }

                WriteHeader(writer, filename);

                writer.WriteLine(TAGDefines.ConstNames);

                WriteConstValue(writer, ref values);

                writer.WriteLine();

                writer.Flush();
                writer.Close();

                LogSystem.Inst.Add(string.Format("Make file:{0}", output));


                // header file generate
                string serverPath = MainModule.Inst.GetServerCodePath("");
                if (false == ServerCodeGen.MakeServerCode(ws.Name.ToLower(), output, serverPath, ""))
                {
                    LogSystem.Inst.Add("CTF -> ServerCode Fail!!");
                    return false;
                }

            }

            return true;
        }
    }
}