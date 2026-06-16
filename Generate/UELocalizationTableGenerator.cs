using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;

namespace ExcelConvertLib
{
    public class UELocalizationTableGenerator : BaseGenerator
    {
        private HashSet<string> _CollectKeyList = new HashSet<string>();

        string NamedParamToOrderParam(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            Dictionary<string, int> FoundOrderMap = new Dictionary<string, int>();
            int order = 0;
            string resultStr = Regex.Replace(str, @"\{([^\}]*)\}", match =>
            {
                int strOrder = order;
                if (FoundOrderMap.ContainsKey(match.Value))
                {
                    strOrder = FoundOrderMap[match.Value];
                }
                else
                {
                    strOrder = order++;
                    FoundOrderMap.Add(match.Value, strOrder);
                }
                return "{" + strOrder++ + "}";
            });

            return resultStr;
        }

        public void InitCollectKey()
        {
            _CollectKeyList.Clear();
        }

        public bool CheckHasLocaleString(string locale)
        {
            for (int i = 1; i < AllHeaderInfos.Count(); ++i)
            {
                int col = AllHeaderInfos[i].colNum;
                if (string.Compare(AllHeaderInfos[i].VarName, locale, true) == 0)
                {
                    return true;
                }
            }
            return false;
        }

        int GenerateStringTable(Excel.Worksheet ws, StreamWriter outputWriter , string locale , bool bSystemMessage)
        {
            if (ws == null)
            {
                return 0;
            }
            int record = 0;
            {
                object[,] values = (object[,])ws.UsedRange.Value2;
                int max = values.GetLength(0);
                int columnMax = AllHeaderInfos.Count();

                int NumRow = CommonDefines.StartStringRowNum;
               
                while (NumRow <= max)
                {
                    string strValue = "";
                    for (int i = 0; i < columnMax; ++i)
                    {
                        int col = AllHeaderInfos[i].colNum;
                        strValue = Convert.ToString(values[NumRow, col]);
                        if (string.IsNullOrEmpty(strValue) || strValue[0] == ';')
                        {                            
                            break;
                        }

                        if (AllHeaderInfos[i].type == EUsedType.Key)
                        {
                            if (strValue.Contains("="))
                            {
                                strValue = strValue.Split('=')[0].Trim();
                            }
                            if (_CollectKeyList.Contains(strValue))
                            {
                                LogSystem.Inst.Add("String Key Already Exist !! " + strValue, ELOGType.Error);
                                MainModule.Inst.HasError = true;
                                break;
                            }
                            if ( bSystemMessage )
                            {
                                //작업롤백 - 부활가능성이 있어서 주석처리
                                //strValue = strValue.ToUpper();
                            }
                            outputWriter.Write(string.Format("\"{0}\"",  strValue ));
                            _CollectKeyList.Add(strValue);
                        }
                        else 
                        {
                            if (string.Compare(AllHeaderInfos[i].VarName, locale, true) == 0)
                            {
                                outputWriter.Write(",");
                                outputWriter.Write(string.Format("\"{0}\"", NamedParamToOrderParam(strValue)));

                                ++record;
                                break;
                            }
                        }
                    }
                    if (strValue.Length < 5 && strValue == ";end")
                    {
                        break;
                    }

                    outputWriter.WriteLine();
                    ++NumRow;
                }

                outputWriter.Flush();
            }
            return record;
        }


        int GenerateSystemMessageInfoCSV(Excel.Worksheet ws, StreamWriter outputWriter)
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
                    case EUsedType.client:
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
            foreach (HeaderInfo info in SelectedColumns)
            {
                if (info.type == EUsedType.Key)
                {
                    outputWriter.Write("\"KEY\"");
                }
                else
                {
                    outputWriter.Write(",");
                    outputWriter.Write(string.Format("\"{0}\"", info.VarName));
                }
            }
            outputWriter.WriteLine();
            int NumRow = CommonDefines.StartStringRowNum;
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
                        outputWriter.Write(string.Format("\"{0}\"", strValue));
                    }
                    else
                    {
                        outputWriter.Write(",");
                        outputWriter.Write(string.Format("\"{0}\"", NamedParamToOrderParam(strValue)));
                    }
                }

                outputWriter.WriteLine();
                ++NumRow;
                outputWriter.Flush();
            }
            return _CollectKeyList.Count;
        }        

        public bool MakeDataConvert(StreamWriter writer , string locale , bool bSystemMessage )
        {
            // Data 만 Convert 하는 부분.
            // 2021.09.15. 기준으로 Data 에 해당하는 테이블의 경우는 해당 엑셀 파일을 기준으로 모든 L10N Sheet 를 Export 해야 한다.

            var wb = MainModule.Inst.WorkBook;
            if (wb == null)
            {
                return false;
            }              
            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("localization");
            if (ws!=null)
            {
                object[,] values = (object[,])ws.UsedRange.Value2;

                CollectHeader(ref values);
                ////// String Table
                if (CheckHasLocaleString(locale))
                {
                    int count = GenerateStringTable(ws, writer, locale , bSystemMessage);
                    writer.WriteLine("");
                    LogSystem.Inst.Add( string.Format("{0} : Num {1}", wb.Name, count));
                    return true;
                }                
            }
            return false;
        }

        public bool ExtractSystemMessage(IProgress<double> progress)
        {
            var wb = MainModule.Inst.WorkBook;
            if (wb == null)
            {
                return false;
            }
            string FileName = "system_message_info.csv";

            string output = MainModule.Inst.GetClientOutPath(FileName);
            string dir = Path.GetDirectoryName(output);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }
            MainModule.CheckOutClientCSVFile(FileName);
            StreamWriter writer = new StreamWriter(
                    new FileStream(output, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);

            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("Data");
            if (ws != null)
            {
                object[,] values = (object[,])ws.UsedRange.Value2;

                CollectHeader(ref values);
                GenerateSystemMessageInfoCSV(ws, writer);

                writer.WriteLine("");
            }            

            writer.Flush();
            writer.Close();
           
            return true;
        }
    }
}
