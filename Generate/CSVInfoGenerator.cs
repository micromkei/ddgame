using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelConvertLib
{
    public class CSVInfoGenerator : BaseGenerator
    {
        public bool IsOnlyOneKey { get; set; }

        private bool MakeRecord( object[,] values, int NumRow,  StreamWriter writer)
        {
            bool bHasError = false;
            sb.Clear();
            int i = 0;
            foreach (var header in ClientHeaderInfos)
            {
                if (header.bSkip)
                {
                    continue;
                }
                int col = header.colNum + 1;
                if (col <= 0)
                {
                    bHasError = true;
                    LogSystem.Inst.Add("Not Found Header Name " + header.VarName);
                    continue;
                }
                string value = Convert.ToString(values[NumRow, col]).Trim();

                if (header.bUseDataName)
                {
                    value = ContentIDChecker.Inst.GetAliasValue(value);
                }
                else
                {
                    value = ContentIDChecker.Inst.ConvertAliasString(value, header);
                }

                if (value.Contains(",") == true)
                {
                    if (!value.Contains(";") && !value.Contains("="))
                    {
                        LogSystem.Inst.Add(string.Format("value [{0}] contains symbol ','", value));

                        if (header.clientType == EnumValueType.Name)
                        {
                            value = $"\"{value}\"";
                        }
                    }
                }

                if (header.clientType == EnumValueType.Strings)
                {
                    value = "\"" + value.Replace("\"", "\"\"") + "\"";
                }
                else if ( !string.IsNullOrWhiteSpace(header.ResPath) )
                {
                    AddUnrealResPath(ref value, header.ResPath);
                }
               
                if (i == 0)
                {
                    if (value.Length < 1 || value[0] == ';') //skip line
                        break;

                    if (IsOnlyOneKey)
                    {
                        if (header.clientType == EnumValueType.ContentID)
                        {
                            value = Utility.StripContentID(value);
                        }

                        if (bHasContentIDKey || header.type == EUsedType.Key)
                        {
                            sb.Append(string.Format("\"{0}\"", value));
                            ++i;
                            continue;
                        }
                        else
                        {
                            sb.Append(string.Format("\"{0}\",", (NumRow - CommonDefines.StartRowNum + 1)));
                        }
                    }
                    else
                    {
                        sb.AppendFormat("\"{0}\",", recordNum + 1);
                    }
                }
                else
                {
                    if (header.bUseMerge && header.CTFHeaderIndices.Count > 1)
                    {
                        value = Utility.GenMergeArray(ServerHeaderInfos, ref values, NumRow, header);
                    }
                    sb.Append(",");
                }

                switch (header.clientType)
                {
                    case EnumValueType.Vector:
                    case EnumValueType.Vector2D:
                        {
                            sb.AppendFormat("\"{0}\"", value);
                        }
                        break;
                    default:
                        {
                            if (header.clientTypeName == "FSlateBrush" || header.clientTypeName.StartsWith("TSubclassOf") ||
                                header.clientTypeName == "FVector2D" || header.clientTypeName == "FVector"
                                )
                            {
                                sb.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                            }
                            else if (header.clientTypeName == "FColor" && value.Length == 0)
                            {
                                string defaultValue = "(R=0, G=0, B=0, A=0)";
                                sb.AppendFormat("\"{0}\"", defaultValue);
                            }
                            else
                            {
                                sb.AppendFormat("\"{0}\"", value);
                            }
                        }
                        break;
                }
                ++i;
            }
            ++recordNum;
            if (sb.Length > 1)
                writer.WriteLine(sb); 
            return bHasError;
        }

        public override bool MakeConvertByData(string tableName, IProgress<double> progress)
        {
            if (false == CollectInfoSheet())
            {
                LogSystem.Inst.Add(string.Format("Failed to load info : {0}", tableName));
                return false;
            }
            if ( string.IsNullOrWhiteSpace(ClientFileName) )
            {
                return true;
            }

            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("Data");
            if (ws == null)
            {
                LogSystem.Inst.Add("Not Found Data Sheet !");
                return false;
            }
            recordNum = 0;
            sb.Clear();
            object[,] values = (object[,])ws.UsedRange.Value2;
            bool bHasError = CollectColumName(ref values);
            if ( bHasError )
            {
                LogSystem.Inst.Add(string.Format("Failed to load info : {0}", tableName));
                return false;
            }
            // Converting Error Log 
            bool bValidID = ValidCheckGenerator.IsValidCheck(AllHeaderInfos, ref values);

            string output = MainModule.Inst.GetClientOutPath(ClientFileName);
            string dir = Path.GetDirectoryName(output);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            MainModule.CheckOutClientCSVFile(ClientFileName);

            StreamWriter writer = new StreamWriter(
                    new FileStream(@output, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);

            int max = values.GetLength(0); // ws.UsedRange.Rows.Count;            
          
            WriteHeader(writer);
          
            int NumRow = 2;
            while (NumRow <= max)
            {
                bHasError = MakeRecord( values, NumRow, writer);
                ++NumRow;
            }

            writer.Flush();
            writer.Close();

            LogSystem.Inst.Add(string.Format("Make file:{0} ", output));
            progress.Report(1.0f);
            return ( bHasError || bValidID == false ) ? false : true;
        }

        void WriteHeader(StreamWriter writer)
        {
            IsOnlyOneKey = KeyInfos.Count == 1;

            if (IsOnlyOneKey == false)
            {
                sb.Append("---");
            }
            int i = 0;
            foreach (var header in ClientHeaderInfos)
            {
                if (header.bSkip)
                    continue;
                if (i == 0 && IsOnlyOneKey)
                {
                    if (bHasContentIDKey || header.type == EUsedType.Key)
                    {
                        sb.Append("KEY");
                        ++i;
                        continue;
                    }
                    else
                        sb.Append("KEY,");
                }
                else
                {
                    sb.Append(",");
                }

                if (header.bUseDataName && !string.IsNullOrEmpty(header.CTFColumnName))
                {
                    sb.Append(header.CTFColumnName);
                }
                else if ( header.bUseMerge && header.CTFHeaderIndices.Count > 1 )
                {
                    sb.Append(header.CTFColumnName);
                }
                else
                {
                    sb.Append(header.VarName);
                }
                ++i;
            }
            writer.WriteLine(sb);
            sb.Clear();
        }

        public bool MergeStart(StreamWriter writer)
        {
            CollectInfoSheet();
            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("Data");
            if (ws == null)
            {
                return false;
            }
            recordNum = 0;
            sb.Clear();
            object[,] values = (object[,])ws.UsedRange.Value2;

            bool bHasError = CollectColumName(ref values);
            int max = values.GetLength(0); // ws.UsedRange.Rows.Count;            

            WriteHeader(writer);

            int NumRow = 2;
            while (NumRow <= max)
            {
                bHasError = MakeRecord(values, NumRow, writer);
                ++NumRow;
            }
            return bHasError;
        }

        public bool MergeFile(StreamWriter writer)
        {           
            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("Data");
            if (ws == null)
            {
                return false;
            }
            int NumRow = 2;
            object[,] values = (object[,])ws.UsedRange.Value2;

            bool bHasError = CollectColumName(ref values);
            int max = values.GetLength(0);
            while (NumRow <= max)
            {
                bHasError = MakeRecord(values, NumRow, writer);
                ++NumRow;
            }
            return bHasError;
        }
    }
}
