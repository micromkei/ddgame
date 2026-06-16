using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelConvertLib
{
    public class ServerEnumInfo
    {
        public string EnumName;
        public string type;
        public string asFunc;
        public string seekFunc;
        public string appendFunc;
        public string classname;
    }

    public class ServerEnumGenerator : BaseGenerator
    {
        Dictionary<string, List<ServerEnumInfo>> mHeaderFileInfos = new Dictionary<string, List<ServerEnumInfo>>();

        private int GetMaxLengthEnumName(List<EnumInfo> infos, int offset = 0)
        {
            int maxLen = 0;
            for (int i = 0; i < infos.Count; ++i)
            {
                if (IsCommentEnumInfo(infos[i])) continue;
                maxLen = infos[i].Name.Length > maxLen ? infos[i].Name.Length : maxLen;
            }

            return maxLen;
        }

        public bool OnGenerateEnum(bool bCheckOutExcel, bool bReadOnlyExcel)
        {
            // 띄워둔 상태에서 수정 후 추출할 수 있으므로 다시 로딩한다
            if (!MainModule.Inst.ReadExcel(CommonDefines.ExcelFolder, ConfigSystem.Inst.EnumTable, bCheckOutExcel, bReadOnlyExcel))
                return false;

            MainModule.Inst.ReadConfig();

            if (MainModule.Inst.CheckEnumUniqueByContentID() == false)
            {
                return false;
            }

            ReadHeaderFileInfo();

            foreach (KeyValuePair<string, List<ServerEnumInfo>> eHeader in mHeaderFileInfos)
            {
                string filename = eHeader.Key.Split('.')[0];

                if (!MainModule.CheckOutServerEnumHeaderFile(filename + ".h"))      //perforce check   
                {
                    LogSystem.Inst.Add(string.Format("P4Error failed check {0}", filename));
                    return false;
                }
                if (!MainModule.CheckOutServerEnumHeaderFile(filename + ".cpp"))      //perforce check   
                {
                    LogSystem.Inst.Add(string.Format("P4Error failed check {0}", filename));
                    return false;
                }

                string outputHeader = MainModule.Inst.GetServerEnumHeaderPath(filename + ".h");
                string outputCpp = MainModule.Inst.GetServerEnumHeaderPath(filename + ".cpp");

                StreamWriter writerHeader = new StreamWriter(
                         new FileStream(outputHeader, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
                StreamWriter writerCpp = new StreamWriter(
                         new FileStream(outputCpp, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);

                FileServerHeader(writerHeader);
                FileServerCpp(writerCpp);

                foreach (ServerEnumInfo eEnumInfo in eHeader.Value)
                {
                    List<EnumInfo> value = MainModule.Inst.EnumDefInfos[eEnumInfo.EnumName];

                    MakeServerEnumClass(writerHeader, eEnumInfo, ref value);

                    if (!(eEnumInfo.asFunc == null || eEnumInfo.asFunc == ""))
                    {
                        writerHeader.WriteLine(string.Format("extern std::string {0}(const {1}& type);", eEnumInfo.asFunc, GetEnumClassName(eEnumInfo)));
                        MakeAsFunc(writerCpp, eEnumInfo, ref value);
                    }
                    if (!(eEnumInfo.seekFunc == null || eEnumInfo.seekFunc == ""))
                    {
                        writerHeader.WriteLine(string.Format("extern bool {0}(const std::string& s, {1}& retValue);", eEnumInfo.seekFunc, GetEnumClassName(eEnumInfo)));
                        MakeSeekFunc(writerCpp, eEnumInfo, ref value);
                    }
                    if (!(eEnumInfo.appendFunc == null || eEnumInfo.appendFunc == ""))
                    {
                        writerHeader.WriteLine(string.Format("extern void {0}(core::Switcher<std::string, Int64>& m_Switcher);", eEnumInfo.appendFunc));
                        MakeAppendFunc(writerCpp, eEnumInfo, ref value);
                    }

                    writerHeader.WriteLine();
                }

                writerHeader.Flush();
                writerHeader.Close();
                writerCpp.Flush();
                writerCpp.Close();

                LogSystem.Inst.Add(string.Format("Generate :{0} ", outputHeader));
            }

            return !MainModule.Inst.HasError;
        }

        public bool MakeProtocolErrorEnum(IProgress<double> progress)
        {
            // main module에서 excel load 되어있어야 한다.
            var wb = MainModule.Inst.WorkBook;
            if (wb == null)
            {
                return false;
            }

            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("Data");
            if (ws != null)
            {
                object[,] values = (object[,])ws.UsedRange.Value2;

                string FileName = "ProtocolError.h";
                if (!MainModule.CheckOutServerEnumErrorHeaderFile(FileName))      //perforce check   
                {
                    LogSystem.Inst.Add(string.Format("P4Error failed check CommonEnumDefines "));
                    return false;
                }
                string output = MainModule.Inst.GetServerEnumHeaderPath(FileName);

                StreamWriter writer = new StreamWriter(
                            new FileStream(output, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);

                FileServerHeader(writer);

                sb.Append("namespace Error\r\n");
                sb.Append("{\r\n");

                sb.Append("\tenum : uint32\r\n");
                sb.Append("\t{\r\n");

                // enum body
                int max = values.GetLength(0);
                int NumRow = 6;
                while (NumRow <= max)
                {
                    string strValue = Convert.ToString(values[NumRow, 1]);
                    if (strValue == ";end")
                        break;

                    if (string.IsNullOrEmpty(strValue))
                        sb.Append(string.Format("\r\n", strValue));
                    else
                        sb.Append(string.Format("\t\t{0},\r\n", strValue));

                    NumRow++;
                }
                //

                sb.Append("\t};\r\n");
                //sb.Append("\tvoid Send(const SessionSharedPtr&pSession, PROTOCOL::ID ReqProtocolID, int ErrorNo, StreamSharedPtr pStream = nullptr);\r\n");
                //sb.Append("\tvoid Send(const SessionSharedPtr&pSession, uint32 errCode);\t//for auth\r\n");
                sb.Append("};\r\n");

                writer.WriteLine(sb.ToString()); sb.Clear();

                writer.Flush();
                writer.Close();

                LogSystem.Inst.Add(string.Format("Generate :{0} ", output));
            }

            return false;
        }

        bool ReadHeaderFileInfo()
        {
            if (!MainModule.Inst.ReadExcel(CommonDefines.ExcelFolder, ConfigSystem.Inst.EnumTable, false))
                return false;

            mHeaderFileInfos.Clear();
            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("#ServerConfig");
            object[,] values = (object[,])ws.UsedRange.Value2;

            int numRow = values.GetLength(0);
            for (int i=2;  i <= numRow; ++i)
            {
                string filename = Convert.ToString(values[i, 2]);
                if (filename == null || filename == "")
                    continue;

                ServerEnumInfo info = new ServerEnumInfo();
                info.EnumName = Convert.ToString(values[i, 1]);
                info.type = Convert.ToString(values[i, 3]);
                info.asFunc = Convert.ToString(values[i, 4]);
                info.seekFunc = Convert.ToString(values[i, 5]);
                info.appendFunc = Convert.ToString(values[i, 6]);
                info.classname = Convert.ToString(values[i, 7]);

                if (mHeaderFileInfos.ContainsKey(filename))
                {
                    mHeaderFileInfos[filename].Add(info);
                }
                else
                {
                    List<ServerEnumInfo> infoList = new List<ServerEnumInfo>();
                    infoList.Add(info);
                    mHeaderFileInfos.Add(filename, infoList);
                }
            }

            return true;
        }

        void FileServerHeader(StreamWriter writer)
        {
            writer.WriteLine("#pragma once");
            writer.WriteLine();
        }
        void FileServerCpp(StreamWriter writer)
        {
            writer.WriteLine("#include \"CommonPCH.h\"");
            writer.WriteLine();
        }

        void MakeServerEnumClass(StreamWriter writer, ServerEnumInfo enumInfo, ref List<EnumInfo> infos)
        {
            string section = GetEnumClassName(enumInfo);

            sb.Append(string.Format("enum class {0}", section));
            if (!string.IsNullOrWhiteSpace(enumInfo.type))
            {
                sb.Append(string.Format(" : {0}", enumInfo.type));
            }
            sb.Append("\r\n");
            sb.Append("{\r\n");

            int maxTab = GetMaxLengthEnumName(infos) / 4 + 2;
            
            for (int i = 0; i < infos.Count; ++i)
            {
                EnumInfo info = infos[i];
                Utility.AddTab(sb, 1);

                if (IsCommentEnumInfo(infos[i]))
                {
                    sb.Append(string.Format("{0}", info.Name));
                }
                else
                {
                    sb.Append(string.Format("{0}", info.Name));
                    Utility.AddTab(sb, maxTab - info.Name.Length / 4);
                    sb.Append(string.Format("= {0},", info.Value));
                    if (info.Desc.Length > 0)
                        sb.Append(string.Format("\t\t//{0}", info.Desc));
                }
                sb.Append("\r\n");
            }
            sb.Append("};");

            writer.WriteLine(sb.ToString()); sb.Clear();
            writer.WriteLine();
        }

        void MakeAsFunc(StreamWriter writer, ServerEnumInfo enumInfo, ref List<EnumInfo> infos)
        {
            sb.Append(string.Format("std::string {0}(const {1}& type)\r\n", enumInfo.asFunc, GetEnumClassName(enumInfo)));
            sb.Append("{\r\n");
            Utility.AddTab(sb, 1); sb.Append("switch (type) //-V719\r\n");
            Utility.AddTab(sb, 1); sb.Append("{\r\n");

            int maxTab = (GetMaxLengthEnumName(infos) + GetEnumClassName(enumInfo).Length) / 4 + 2;

            for (int i = 0; i < infos.Count; ++i)
            {
                EnumInfo info = infos[i];
                if (info.Name[0] == '_' || IsCommentEnumInfo(infos[i])) continue;

                Utility.AddTab(sb, 1);
                sb.Append(string.Format("case {0}::{1}:", GetEnumClassName(enumInfo), info.Name));
                Utility.AddTab(sb, maxTab - (info.Name.Length + GetEnumClassName(enumInfo).Length) / 4);
                sb.Append(string.Format("return \"{0}\";\r\n", info.Name));
            }
            Utility.AddTab(sb, 1); sb.Append("}\r\n");
            sb.Append("\r\n\treturn std::string();\r\n");
            sb.Append("}");

            writer.WriteLine(sb.ToString()); sb.Clear();
            writer.WriteLine();
        }

        void MakeSeekFunc(StreamWriter writer, ServerEnumInfo enumInfo, ref List<EnumInfo> infos)
        {
            sb.Append(string.Format("bool {0}(const std::string& s, {1}& retValue)\r\n", enumInfo.seekFunc, GetEnumClassName(enumInfo)));
            sb.Append("{\r\n");
            Utility.AddTab(sb, 1); sb.Append("std::string key = s;\r\n");
            Utility.AddTab(sb, 1); sb.Append("core::toupper(key);\r\n\r\n");

            Utility.AddTab(sb, 1); sb.Append(string.Format("static core::Switcher<std::string, {0}>\tm_Switcher;\r\n", GetEnumClassName(enumInfo)));
            Utility.AddTab(sb, 1); sb.Append("std::call_once(m_Switcher.onceflags_, [&]() -> auto\r\n");
            Utility.AddTab(sb, 1); sb.Append("{\r\n");

            int defalutInfo = 0;
            for (int i = 0; i < infos.Count; ++i)
            {
                if (infos[i].Value == 0) defalutInfo = i;
            }
            int maxTab = (GetMaxLengthEnumName(infos) + 2) / 4 + 2;

            for (int i = 0; i < infos.Count; ++i)
            {
                EnumInfo info = infos[i];
                if (info.Name[0] == '_' || IsCommentEnumInfo(infos[i])) continue;

                Utility.AddTab(sb, 2);
                sb.Append(string.Format("AppendEnum(\"{0}\",", info.Name.ToUpper()));
                Utility.AddTab(sb, maxTab - (info.Name.Length + 2) / 4);
                sb.Append(string.Format("{0}::{1});\r\n", GetEnumClassName(enumInfo), info.Name));
            }
            sb.Append("\r\n");
            Utility.AddTab(sb, 2); sb.Append("m_Switcher.Default([]() -> auto { ");
            sb.Append(string.Format("return {0}::{1}; ", GetEnumClassName(enumInfo), infos[defalutInfo].Name));
            sb.Append("});\r\n");

            Utility.AddTab(sb, 1); sb.Append("});\r\n\r\n");
            Utility.AddTab(sb, 1); sb.Append("retValue = m_Switcher.Execute(key);\r\n");
            Utility.AddTab(sb, 1); sb.Append("return true;\r\n");
            sb.Append("}");

            writer.WriteLine(sb.ToString()); sb.Clear();
            writer.WriteLine();
        }

        void MakeAppendFunc(StreamWriter writer, ServerEnumInfo enumInfo, ref List<EnumInfo> infos)
        {
            sb.Append(string.Format("void {0}(core::Switcher<std::string, Int64>& m_Switcher)\r\n", enumInfo.appendFunc));
            sb.Append("{\r\n");

            Utility.AddTab(sb, 1); sb.Append("// [주의사항]\r\n");
            Utility.AddTab(sb, 1); sb.Append("// ALL 과 NONE은 모든 Enum에 동일하게 적용\r\n");

            Utility.AddTab(sb, 1); sb.Append("m_Switcher.Insert(\"ALL\", \t\t\t\t[]() -> auto { return 0; });\r\n");
            Utility.AddTab(sb, 1); sb.Append("m_Switcher.Insert(\"NONE\", \t\t\t\t[]() -> auto { return 0; });\r\n");
            Utility.AddTab(sb, 1); sb.Append("m_Switcher.Insert(\"UNLIMITED\", \t\t\t[]() -> auto { return -2; });\r\n\r\n");

            int maxTab = (GetMaxLengthEnumName(infos)+1) / 4 + 2;

            for (int i = 0; i < infos.Count; ++i)
            {
                EnumInfo info = infos[i];
                if (info.Name[0] == '_' || IsCommentEnumInfo(infos[i])) continue;

                Utility.AddTab(sb, 1);
                sb.Append(string.Format(string.Format("AppendEnumAsInt(\"{0}\",", info.Name.ToUpper())));
                Utility.AddTab(sb, maxTab - (info.Name.Length+1) / 4);
                sb.Append(string.Format(string.Format("{0}::{1});\r\n", GetEnumClassName(enumInfo), info.Name)));
            }
            sb.Append("}");

            writer.WriteLine(sb.ToString()); sb.Clear();
            writer.WriteLine();
        }

        string GetEnumClassName(ServerEnumInfo enumInfo)
        {
            if (string.IsNullOrWhiteSpace(enumInfo.classname))
                return enumInfo.EnumName;
            else
                return enumInfo.classname;
        }
    }
}
