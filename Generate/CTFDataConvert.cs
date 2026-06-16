using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelConvertLib
{
    public class CTFDataConvert : BaseGenerator
    {
        private HashSet<string> _CollectKeyList = new HashSet<string>();

        public bool bSkipAlreadyProcessedAggregatedCTF = false;
        private List<string> _AggregatedProcessedHistory = new List<string>();

        string GetSection( string value )
        {
            string[] sections = value.Split(new char[] { '/' });
            if (sections.Length <= 1)
                return value;
            return sections[0].ToUpper();
        }

        protected void WriteHeader(StreamWriter writer, string tableName)
        {
            // 파일 생성 정보
            DateTime now = DateTime.Now;
            writer.WriteLine(string.Format(";{0}", tableName.ToUpper()));
            writer.WriteLine();
        }

        private void WriteRecord(ref object[,] values, int NumRow, int col, int i, HeaderInfo info)
        {
            string value = "";
            if (col > 0)
                value = Convert.ToString(values[NumRow, col]).Trim();

            if (info.bUseDataName)
                value = ContentIDChecker.Inst.GetAliasValue(value);
            else
                value = ContentIDChecker.Inst.ConvertAliasString(value, info);

            if (info.clientType == EnumValueType.ContentID && string.IsNullOrWhiteSpace(info.RefColumnName))
            {
                if (Utility.IsEnumEffect(value) == false)
                {
                    if (ContentIDChecker.Inst.FindContentID(value) == false)
                    {
                        LogSystem.Inst.Add(string.Format("Cannot Find ContentID : [{0}], row={1}", value, NumRow), ELOGType.Error);
                        MainModule.Inst.HasError = true;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(value))
            {
                sb.Append(MakeCTFFormat(value, ServerHeaderInfos[i]));
            }
            sb.Append("\t");
        }

        string MakeCTFFormat(string value, HeaderInfo info)
        {
            if ( !string.IsNullOrWhiteSpace(info.CTFFormat) )
            {
                value = ContentIDChecker.Inst.ConvertAliasString(value, info);
                return string.Format(info.CTFFormat, value);
            }
            if (value.Length > 1 &&
                 info.VarName == "ContentID" && !value.EndsWith(TAGDefines.End))
                return value + TAGDefines.End;
            return value;
        }

        void WriteCTFArray(ref object[,] values, int NumRow, HeaderInfo info)
        {
            int count = info.CTFHeaderIndices.Count();
            bool first = true;
            foreach (int idx in info.CTFHeaderIndices)
            {
                HeaderInfo hInfo = ServerHeaderInfos[idx];
                if (hInfo.colNum < 0)   continue;

                string value = Convert.ToString(values[NumRow, hInfo.colNum + 1]).Trim();

                if (hInfo.bUseDataName)
                    value = ContentIDChecker.Inst.GetAliasValue(value);
                else
                    value = ContentIDChecker.Inst.ConvertAliasString(value, info);

                if (string.IsNullOrWhiteSpace(value))   continue;

                if (first)
                    first = false;
                else if (!hInfo.bUseMerge)
                    sb.Append(",");


                if (!string.IsNullOrWhiteSpace(hInfo.CTFFormat))
                {
                    if (hInfo.CTFFormat.Contains(";"))
                    {
                        value = value.Replace("(", "");
                        value = value.Replace(")", "");
                    }
                    sb.Append(string.Format(hInfo.CTFFormat, value));
                }
                else
                {
                    sb.Append(value);
                }
            }
            sb.Append("\t");
        }

        // Info Sheet 
        public override bool MakeConvertByData(string tableName, IProgress<double> progress)
        {
            bCTF = true;
            if (CollectInfoSheet() == false)
            {
                LogSystem.Inst.Add(string.Format("Failed to load info : {0}", tableName));
                return false;
            }

            if (ServerFileName.Length == 0)
            {
                // client only
                return true;
            }

            // 파일 준비
            string output = MainModule.Inst.GetServerOutPath("common", ServerFileName);
            output = Utility.GetPath(output);
            string dir = Path.GetDirectoryName(output);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            if (bSkipAlreadyProcessedAggregatedCTF)
            {
                if (_AggregatedProcessedHistory.Contains(ServerFileName))
                {
                    return true;
                }
                _AggregatedProcessedHistory.Add(ServerFileName);
            }

            FileAttributes attr = (FileAttributes)(-1);
            StreamWriter writer = null;
            if (File.Exists(output))
            {
                if (false == MainModule.CheckOutCTFFile(ServerFileName))
                {
                    if (false == MainModule.AddCTFFile(ServerFileName, true))
                    {
                        return false;
                    }
                }

                //  readonly 계열은 exception 발생하니 잠깐 풀었다가 다시 잠근다
                attr = File.GetAttributes(output);
                if ((attr & FileAttributes.ReadOnly) != 0)
                {
                    File.SetAttributes(output, attr & ~FileAttributes.ReadOnly);
                }
                writer = new StreamWriter(new FileStream(@output, FileMode.Create, FileAccess.ReadWrite), new UTF8Encoding(true));  // 일단 기존과 같이 unicode로
            }
            else
            {
                writer = new StreamWriter(new FileStream(@output, FileMode.Create, FileAccess.ReadWrite), new UTF8Encoding(true));  // 일단 기존과 같이 unicode로
                if (false == MainModule.AddCTFFile(ServerFileName, true))
                {
                    writer.Close();
                    return false;
                }
            }

            var relatedCount = CTFInfoSystem.Inst.LoadRelatedExcel(ServerFileName);
            if (relatedCount > 0)
            {
                string RefFilename = CTFInfoSystem.Inst.GetReferenceFileName(ServerFileName);
                LogSystem.Inst.Add(string.Format("-- Aggregated ({0}) relatedCount : {1}", RefFilename, relatedCount));
                if (MainModule.Inst.ReadExcel(CommonDefines.ExcelFolder, RefFilename, true))
                {
                    MergeInfoSheet(relatedCount);
                }
                else
                {
                    LogSystem.Inst.Add(string.Format("Failed to load file : {0}", RefFilename));
                }
            }

            if (relatedCount > 0)
                WriteHeader(writer, CTFInfoSystem.Inst.GetRelatedFileNameString(ServerFileName));
            else
                WriteHeader(writer, tableName);

            // const 처리
            if (relatedCount <= 0)
            {
                Excel.Worksheet wsData = MainModule.Inst.GetWorkSheet("Const");
                if (wsData != null)         // const 는 없을 수도 있다
                {
                    writer.WriteLine(TAGDefines.ConstNames);

                    object[,] consts = (object[,])wsData.UsedRange.Value2;
                    WriteConstValue(writer, ref consts);
                }
            }
            else
            {
                bool bFirst = true;
                for (int i = 0; i < relatedCount; i++)
                {
                    Excel.Worksheet wsData = CTFInfoSystem.Inst.GetRelatedWorkSheet(i, "Const");
                    if (wsData != null)
                    {
                        if (bFirst)
                        {
                            writer.WriteLine(TAGDefines.ConstNames);
                            bFirst = false;
                        }

                        object[,] consts = (object[,])wsData.UsedRange.Value2;
                        WriteConstValue(writer, ref consts);
                    }
                }
            }
            writer.WriteLine();
            
            // data 처리(FieldName)

            Excel.Worksheet ws = null;
            if (relatedCount>0)
            {
                ws = CTFInfoSystem.Inst.GetRelatedWorkSheet(0, "Data"); // 첫번째 excel을 기준으로
            }
            else
            {
                ws = MainModule.Inst.GetWorkSheet("Data");
            }

            if (ws == null)
            {
                LogSystem.Inst.Add("Not Found Data Sheet !");
                writer.Close();
                if(attr > 0)
					File.SetAttributes(output, attr | FileAttributes.ReadOnly);
                return false;
            }
            object[,] values = (object[,])ws.UsedRange.Value2;

            bool bHasError = CollectColumName(ref values);      // data의 column과 header의 index를 맞춘다

            WriteFieldNames(writer);        // ref excel의 기준으로 맞춘다

            // data 처리(Table)
            writer.WriteLine("["+ServerSection+"]");

            if (relatedCount <= 0)
            {
                bHasError |= WriteEachData(writer, ref values);
            }
            else
            {
                for (int i = 0; i < relatedCount; i++)
                {
                    Excel.Worksheet wsData = CTFInfoSystem.Inst.GetRelatedWorkSheet(i, "Data");
                    if (wsData != null)
                    {
                        object[,] refValues = (object[,])wsData.UsedRange.Value2;
                        bHasError |= CollectColumName(ref refValues);      // data의 column과 header의 index를 맞춘다
                        bHasError |= WriteEachData(writer, ref refValues);
                    }
                }
            }

            writer.Flush();
            writer.Close();
			if (attr > 0)
				File.SetAttributes(output, attr | FileAttributes.ReadOnly);

			LogSystem.Inst.Add(string.Format("Make file:{0} ", output));
            progress.Report(1.0f);

            return !(bHasError || MainModule.Inst.HasError);
        }

        protected virtual int GetStartIndexConst()
        {
            return 2;
        }

        //enyheid - const 처리
        protected void WriteConstValue(StreamWriter writer, ref object[,] values)
        {
            int count = values.GetLength(0);

            StringBuilder sbConstValue = new StringBuilder();

            // 필드 갯수 확인
            long rowindex = GetStartIndexConst() - 1;
            long ColumnCount = 0;

            for (int i = 1; i <= values.GetLongLength(1); ++i)
            {
                string lvalue = Convert.ToString(values[rowindex, i]).Trim();

                if (string.IsNullOrEmpty(lvalue))
                    break;

                ++ColumnCount;
            }

            // Const 파싱
            for (int i = GetStartIndexConst(); i <= count; ++i)
            {
                sbConstValue.Clear();

                string lvalue = string.Empty;
                string servertype = string.Empty;
                string rvalue = string.Empty;
                string comment = string.Empty;

                switch (ColumnCount)
                {
                    case 3:
                        lvalue = Convert.ToString(values[i, 1]).Trim();
                        rvalue = Convert.ToString(values[i, 2]).Trim();
                        comment = Convert.ToString(values[i, 3]).Trim();
                        break;
                    case 4:
                        lvalue = Convert.ToString(values[i, 1]).Trim();
                        servertype = Convert.ToString(values[i, 2]).Trim();
                        rvalue = Convert.ToString(values[i, 3]).Trim();
                        comment = Convert.ToString(values[i, 4]).Trim();
                        break;
                }

                if (lvalue.Length <= 0 || lvalue[0] == ';')
                    continue;

                sbConstValue.Append(lvalue);

                if (!string.IsNullOrEmpty(servertype))
                    sbConstValue.Append("\t" + servertype);

                sbConstValue.Append("\t" + rvalue);
                if (comment.Length > 0)
                    comment = comment.Replace("\n", " / ");
                   
                sbConstValue.Append("\t;" + comment.Replace(";", ""));

                writer.WriteLine(sbConstValue);
            }
            writer.WriteLine();
        }

        void WriteFieldNames(StreamWriter writer)
        {
            StringBuilder head = new StringBuilder();
            StringBuilder usedType = new StringBuilder();
            StringBuilder server = new StringBuilder();

            int columnMAX = ServerHeaderInfos.Count();

            writer.WriteLine(TAGDefines.FieldNames);
            for (int i = 0; i < columnMAX; ++i)
            {
                HeaderInfo header = ServerHeaderInfos[i];
                if (header.bSkip)
                    continue;
                string serverhead = header.CTFColumnName;                                
                if (serverhead == null || serverhead.Length <= 0)
                    serverhead = header.VarName;
                head.Append(serverhead);
                head.Append("\t");

                // CTF파일에.. DataName이 key가 될 수 없는 문제 수정
                //if (header.bUseDataName)
                //    usedType.Append(EUsedType.DataName.ToString());
                //else
                    usedType.Append(header.type.ToString());
                usedType.Append("\t");

                server.Append(header.serverTypeName);
                server.Append("\t");
            }
            writer.WriteLine(head);
            writer.WriteLine(usedType);
            writer.WriteLine(server);
            writer.WriteLine();
        }

        bool WriteEachData(StreamWriter writer, ref object[,] values)
        {
            bCTF = true;
            bool bHasError = false;
            int max = values.GetLength(0);
            int NumRow = 2;
            while (NumRow <= max)
            {
                sb.Clear();

                string firstValue = Convert.ToString(values[NumRow, 1]);        // line skip
                if (firstValue.Length < 1 || firstValue[0] == ';')
                {
                    ++NumRow;
                    continue;
                }

                int i = 0;
                foreach (var header in ServerHeaderInfos)
                {
                    int col = header.colNum + 1;
                    if (!header.bSkip)
                    {
                        if (header.CTFHeaderIndices.Count > 1)
                        {
                            WriteCTFArray(ref values, NumRow, header);
                        }
                        else
                        {
                            WriteRecord(ref values, NumRow, col, i, header);
                        }
                    }
                    ++i;
                }

                if (sb.Length > 1)
                    writer.WriteLine(sb);
                ++NumRow;
            }
            return bHasError;
        }

        public bool MergeInfoSheet(int refCount)
        {
            ClearInfos();
            Dictionary<string, int> CTFArrayCheck = new Dictionary<string, int>();

            for (int refIndex = 0; refIndex < refCount; refIndex++)
            {
                Excel.Worksheet wsData = CTFInfoSystem.Inst.GetRelatedWorkSheet(refIndex, "Info");
                if (wsData == null)
                    continue;

                object[,] values = (object[,])wsData.UsedRange.Value2;

                bHasContentIDKey = false;

                int rowMax = values.GetLength(0);
                int NumRow = 3;

                ClientFileName = Convert.ToString(values[1, 2]);
                ServerFileName = Convert.ToString(values[1, 4]);
                CombineFileName = Convert.ToString(values[1, 5]); // 공용 파일 ( Table Code 생성시 이용 )
                ServerSection = Convert.ToString(values[1, 6]);
                if (ServerSection.Length <= 0)
                    ServerSection = "Table";

                //long RowCount = values.GetLongLength(0);
                long ColumnCount = values.GetLongLength(1);

                //Info 사용 check
                bool bCheckCTFColumn = false;  //CTF USE  Column Name
                if (ColumnCount >= CommonDefines.InfoCTFColumn)
                {
                    bCheckCTFColumn = Convert.ToString(values[2, CommonDefines.InfoCTFColumn]) == "CTF" ? true : false;
                }
                bool bCheckUseDataName = false; //Data Name Use
                if (ColumnCount >= CommonDefines.InfoUseDataName)
                {
                    bCheckUseDataName = Convert.ToString(values[2, CommonDefines.InfoUseDataName]) == "UseDataName" ? true : false;
                }
                bool bCheckResPath = false; //Client Resource Path Combine
                if (ColumnCount >= CommonDefines.InfoResPath)
                {
                    bCheckResPath = Convert.ToString(values[2, CommonDefines.InfoResPath]) == "ResPath" ? true : false;
                }
                bool bCheckCTFFormat = false;  // CTF USE  string Format 
                if (ColumnCount >= CommonDefines.InfoCTFFormat)
                {
                    bCheckCTFFormat = Convert.ToString(values[2, CommonDefines.InfoCTFFormat]) == "CTFFormat" ? true : false;
                }
                bool bCheckRefColumn = false;   // reference column
                if (ColumnCount >= CommonDefines.InfoRefColumn)
                {
                    bCheckRefColumn = Convert.ToString(values[2, CommonDefines.InfoRefColumn]) == "RefColumn" ? true : false;
                }

                int headerIdx = 1;
                while (NumRow <= rowMax)
                {
                    string strType = Convert.ToString(values[NumRow, CommonDefines.InfoExportType]);
                    if (strType == null || strType == "")
                    {
                        ++NumRow;
                        continue;
                    }
                    string colname = Convert.ToString(values[NumRow, 1]);
                    if (FindHeaderInfo(colname) != null)
                    {
                        ++NumRow;
                        continue;
                    }
//                     if (refIndex > 0)   
//                         LogSystem.Inst.Add(string.Format("column [{0}] merged", colname));

                    string server = Convert.ToString(values[NumRow, CommonDefines.InfoServerType]);
                    string servercolname = Convert.ToString(values[NumRow, CommonDefines.InfoCTFColumn]);
                    string client = Utility.FirstUpper(Convert.ToString(values[NumRow, CommonDefines.InfoClientType]));

                    HeaderInfo headerInfo = new HeaderInfo(headerIdx);
                    headerInfo.type = EnumUtils.ToEnum<EUsedType>(strType);
                    if (headerInfo.type == EUsedType.memo || headerInfo.type == EUsedType.DataName)
                        headerInfo.bSkip = true;
                    headerInfo.VarName = colname;
                    headerInfo.serverType = EnumUtils.ToEnum<EnumValueType>(Utility.FirstUpper(server));
                    headerInfo.clientType = EnumUtils.ToEnum<EnumValueType>(client);
                    headerInfo.clientTypeName = client;
                    headerInfo.serverTypeName = server;

                    if (bCheckCTFColumn && bCTF)
                    {
                        headerInfo.CTFColumnName = servercolname;
                        if (servercolname != "")
                        {
                            int ServerHeaderIndex = ServerHeaderInfos.Count();
                            if (!CTFArrayCheck.ContainsKey(servercolname)) // first find
                            {
                                CTFArrayCheck[servercolname] = ServerHeaderIndex;
                                headerInfo.CTFHeaderIndices.Add(ServerHeaderIndex);
                            }
                            else
                            {
                                headerInfo.bSkip = true;
                                ServerHeaderInfos[CTFArrayCheck[servercolname]].CTFHeaderIndices.Add(ServerHeaderIndex);
                            }

                        }
                    }
                    if (bCheckUseDataName)
                    {
                        string useName = Convert.ToString(values[NumRow, CommonDefines.InfoUseDataName]);
                        headerInfo.bUseDataName = (useName.ToUpper() == "TRUE") ? true : false;
                    }
                    if (bCheckResPath) headerInfo.ResPath = Convert.ToString(values[NumRow, CommonDefines.InfoResPath]);
                    if (bCheckCTFFormat) headerInfo.CTFFormat = Convert.ToString(values[NumRow, CommonDefines.InfoCTFFormat]);
                    if (bCheckRefColumn) headerInfo.RefColumnName = Convert.ToString(values[NumRow, CommonDefines.InfoRefColumn]);

                    if (headerInfo.HasClient())
                    {
                        ClientHeaderInfos.Add(headerInfo);
                    }
                    if (headerInfo.HasServer())
                    {
                        ServerHeaderInfos.Add(headerInfo);
                    }
                    if (headerInfo.HasCommon())
                    {
                        CommonHeaderInfos.Add(headerInfo);
                    }

                    AllHeaderInfos.Add(headerInfo);

                    if (headerIdx == 1 && headerInfo.clientType == EnumValueType.ContentID &&
                        headerInfo.type == EUsedType.Key)
                    {
                        bHasContentIDKey = true;
                    }

                    if (headerInfo.type == EUsedType.Key || headerIdx == 1) // first always key
                    {
                        KeyInfos.Add(headerInfo);
                    }
                    ++NumRow;
                    ++headerIdx;
                }
            }

            //enyheid : 다 등록한 후 CTFArray를 재구성한다
            int j = 0;
            foreach (var header in ServerHeaderInfos)
            {
                if (CTFArrayCheck.ContainsKey(header.VarName)) // 기존 컬럼과 중복
                {
                    int oldIndex = CTFArrayCheck[header.VarName];
                    CTFArrayCheck[header.VarName] = j;

                    header.CTFHeaderIndices.Clear();
                    header.CTFHeaderIndices.AddRange(ServerHeaderInfos[oldIndex].CTFHeaderIndices);
                    ServerHeaderInfos[oldIndex].CTFHeaderIndices.Clear();

                    header.bSkip = false;
                    ServerHeaderInfos[oldIndex].bSkip = true;
                }
                ++j;
            }

            return true;
        }

        public bool ExtractSystemMessage(IProgress<double> progress)
        {
            var wb = MainModule.Inst.WorkBook;
            if (wb == null)
            {
                return false;
            }

            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("Data");
            if (ws != null)
            {
                object[,] values = (object[,])ws.UsedRange.Value2;

                string FileName = "system_message_info.ctf";

                string output = MainModule.Inst.GetServerOutPath("common", FileName);
                string dir = Path.GetDirectoryName(output);
                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir);
                }
                MainModule.CheckOutCTFFile(FileName);

                StreamWriter writer = new StreamWriter(
                            new FileStream(output, FileMode.Create, FileAccess.ReadWrite), new UTF8Encoding(true));

                CollectHeader(ref values);

                int count = GenerateSystemMessageInfoCTF(ws, writer);

                writer.WriteLine("");
                writer.WriteLine("");

                writer.Flush();
                writer.Close();
            }
            return true;
        }

        int GenerateSystemMessageInfoCTF(Excel.Worksheet ws, StreamWriter outputWriter)
        {
            if (ws == null)
            {
                return 0;
            }

            List<HeaderInfo> SelectedColumns = new List<HeaderInfo>();

            for (int i = 0; i < AllHeaderInfos.Count; ++i)
            {
                switch (AllHeaderInfos[i].type)
                {
                    case EUsedType.Key:
                        {
                            SelectedColumns.Add(AllHeaderInfos[i]);
                        }
                        break;
                    case EUsedType.server:
                    case EUsedType.common:
                        {
                            if (AllHeaderInfos[i].clientTypeName.Length > 0)
                            {
                                SelectedColumns.Add(AllHeaderInfos[i]);
                            }
                        }
                        break;
                }
            }

            if (SelectedColumns.Count <= 0)
            {
                LogSystem.Inst.Add("Not found system message info table", ELOGType.Error);
                return 0;
            }

            object[,] values = (object[,])ws.UsedRange.Value2;
            int max = values.GetLength(0);
            int columnMax = AllHeaderInfos.Count();

            // write Header
            WriteHeader(outputWriter, "SYSTEM_MESSAGE_INFO");
            WriteFieldNames(outputWriter);
            outputWriter.WriteLine("[Table]");

            _CollectKeyList.Clear();
            int NumRow = CommonDefines.StartRowNum;
            while (NumRow <= max)
            {
                string KeyStr = Convert.ToString(values[NumRow, 1]);
                if (string.IsNullOrEmpty(KeyStr) || KeyStr[0] == ';')
                {
                    ++NumRow;
                    continue;
                }

                foreach (HeaderInfo info in SelectedColumns)
                {
                    int col = info.colNum;
                    string strValue = Convert.ToString(values[NumRow, col]);

                    if (info.type == EUsedType.Key)
                    {
                        if (_CollectKeyList.Contains(strValue))
                        {
                            LogSystem.Inst.Add(string.Format("Key Already Exist !! [{0}]", strValue), ELOGType.Error);
                            MainModule.Inst.HasError = true;
                            break;
                        }

                        outputWriter.Write(string.Format("{0}", strValue));
                        _CollectKeyList.Add(strValue);
                    }
                    else
                    {
                        outputWriter.Write("\t");
                        outputWriter.Write(string.Format("{0}", strValue));
                    }
                }

                outputWriter.WriteLine();
                ++NumRow;
                outputWriter.Flush();

                //LogSystem.Inst.Add(string.Format("Generated system message info ctf :{0} ", ws.Name));
            }

            return _CollectKeyList.Count;
        }
    }
}
