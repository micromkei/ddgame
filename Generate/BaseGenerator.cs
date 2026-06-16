using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelConvertLib
{
    public enum EConvertType : uint
    {
        CTF = 0,
        UEDTHeader,     //UE DataTable Header    
        CSVExport,
        CSV,
        Enum,
        Const,
        LocaleTable,
        ServerConst,
        ServerCode,
        ServerEnum,
        MAX,
    };

    public struct JoinInfo
    {
        public string tableName;
        public string sectionName;
    }

    public struct EnumInfo
    {
        public string Name;
        public string Desc;
        public uint Value;        
    }

    public abstract class BaseGenerator
    {
        protected StringBuilder sb = new StringBuilder();
               
        public Dictionary<string,int> HeaderColNums = new Dictionary<string,int>();
        public List<HeaderInfo> ClientHeaderInfos = new List<HeaderInfo>();
        public List<HeaderInfo> ServerHeaderInfos = new List<HeaderInfo>();
        public List<HeaderInfo> CommonHeaderInfos = new List<HeaderInfo>();
        public List<HeaderInfo> AllHeaderInfos = new List<HeaderInfo>(); // check used
        public List<HeaderInfo> KeyInfos = new List<HeaderInfo>();
        public List<string> ColumnNames = new List<string>();
        
        public bool bHasContentIDKey = false;
        public int recordNum = 0;
        public bool bCTF = false;

        public string ServerFileName { get; set; }
        public string ClientFileName { get; set; }
        public string CombineFileName { get; set; }
        public string ServerSection { get; set; }

        public bool IsCommentEnumInfo(EnumInfo e)
        {
            if (e.Name == null) return false;
            return (e.Name[0] == '/');
        }

        virtual public bool MakeDataConvert(IProgress<double> progress, string tableName)
        {
            return false;
        }

        virtual public bool MakeConvertByData(string tableName, IProgress<double> progress)
        {           
            return false;
        }

        virtual public bool OnGenerate(string tableName)
        {
            return true;
        }

        public void CollectHeader(ref object[,] values)
        {
            bHasContentIDKey = false;

            int colMax = values.GetLength(1);
            ClearInfos();
            for (int j = 1; j <= colMax; ++j)
            {
                string strType = Convert.ToString(values[CommonDefines.UseTypeNum , j]);
                string fieldName = Convert.ToString(values[CommonDefines.FieldNameNum, j]);
                if (strType == null || string.IsNullOrEmpty(fieldName))
                {
                    break;
                }
                HeaderInfo headerInfo = new HeaderInfo(j);
                string server = Convert.ToString(values[CommonDefines.ServerDefNum, j]);
                string client = Utility.FirstUpper(Convert.ToString(values[CommonDefines.ClientDefNum, j]));

                headerInfo.serverType = EnumUtils.ToEnum<EnumValueType>(Utility.FirstUpper(server));
                headerInfo.clientType = EnumUtils.ToEnum<EnumValueType>(client);
                headerInfo.clientTypeName = client;
                headerInfo.serverTypeName = server;
                headerInfo.VarName = fieldName;
                headerInfo.type = EnumUtils.ToEnum<EUsedType>(strType);
                
                if (string.IsNullOrEmpty( client ) && string.IsNullOrEmpty(server))
                {
                    continue;
                }
                if (headerInfo.HasServer())
                {
                    ServerHeaderInfos.Add(headerInfo);
                }
                if (headerInfo.HasClient() )
                {
                    ClientHeaderInfos.Add(headerInfo);
                }
                if (headerInfo.HasCommon() )
                {
                    CommonHeaderInfos.Add(headerInfo);
                }

                AllHeaderInfos.Add(headerInfo);

                if (j == 1 &&
                    headerInfo.clientType == EnumValueType.ContentID &&
                    headerInfo.type == EUsedType.Key)
                {
                    bHasContentIDKey = true;
                }

                if (headerInfo.type == EUsedType.Key || j == 1) // first always key
                {
                    KeyInfos.Add(headerInfo);
                }
            }
        }

        public void ClearInfos()
        {
            CommonHeaderInfos.Clear();
            AllHeaderInfos.Clear();
            ClientHeaderInfos.Clear();
            ServerHeaderInfos.Clear();
            KeyInfos.Clear();           
        }

        internal bool CollectColumName(ref object[,] values)
        {
            ColumnNames.Clear();

            int colMax = values.GetLength(1);
            for (int j = 1; j <= colMax; ++j)
            {
                string strName = Convert.ToString(values[1, j]);
                if (strName == null || strName == "")
                    break;

                ColumnNames.Add(strName);
            }
            bool bHasError = false;
            foreach (var header in AllHeaderInfos)
            {
                header.colNum = ColumnNames.FindIndex(x => x == header.VarName);
                if (!string.IsNullOrWhiteSpace(header.RefColumnName))
                {
                    header.colNum = ColumnNames.FindIndex(x => x == header.RefColumnName);
                }
                if (header.colNum < 0 && !header.bSkip)
                {
                    // 없어도 디폴트 처리하게 변경
                    //bHasError = true;
                    //LogSystem.Inst.Add("Not Found Column =" + header.VarName);
                }
            }
            return bHasError;
        }

        internal void AddUnrealResPath(ref string value, string resPath)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            string[] arValue = value.Split(new char[] { '/' });
            if (arValue.Length == 1)
            {
                value = resPath + value + "." + value + "'";
            }
            else
            {
                int LastIndex = arValue.Length - 1;
                value = resPath;
                for (int Index = 0; Index < LastIndex; ++Index)
                {
                    value += arValue[Index] + "/";
                }
                value += arValue[LastIndex] + "." + arValue[LastIndex] + "'";
            }
        }

        public bool CollectInfoSheet()
        {
            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("Info");
            if (ws == null)
            {
                LogSystem.Inst.Add("Not Found Info sheet !", ELOGType.Error);
                return false;
            }
            object[,] values = (object[,])ws.UsedRange.Value2;
            bHasContentIDKey = false;

            int rowMax = values.GetLength(0);
            int NumRow = 3;

            ClearInfos();

            ClientFileName = Convert.ToString(values[1, 2]);
            ServerFileName = Convert.ToString(values[1, 4]).ToLower();
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
            bool bCheckMerge = false;   // merge column client
            if (ColumnCount >= CommonDefines.InfoMerge)
            {
                bCheckMerge = Convert.ToString(values[2, CommonDefines.InfoMerge]) == "UseMerge" ? true : false;
            }

            Dictionary<string, int> CTFArrayCheck = new Dictionary<string, int>();
            int j = 1;
            while (NumRow <= rowMax)
            {
                string strType = Convert.ToString(values[NumRow,CommonDefines.InfoExportType]);
                if (strType == null || strType == "")
                {
                    ++NumRow;
                    continue;
                }
               
                string server = Convert.ToString(values[NumRow,CommonDefines.InfoServerType]).Trim();
                string servercolname = Convert.ToString(values[NumRow, CommonDefines.InfoCTFColumn]).Trim();
                string client = Utility.FirstUpper(Convert.ToString(values[NumRow, CommonDefines.InfoClientType]).Trim());

                HeaderInfo headerInfo = new HeaderInfo(j);
                headerInfo.type = EnumUtils.ToEnum<EUsedType>(strType);
                if (headerInfo.type == EUsedType.memo)
                    headerInfo.bSkip = true;
                headerInfo.VarName = Convert.ToString(values[NumRow, 1]);
                headerInfo.serverType = EnumUtils.ToEnum<EnumValueType>(Utility.FirstUpper(server));
                headerInfo.clientType = EnumUtils.ToEnum<EnumValueType>(client);
                headerInfo.clientTypeName = client;
                headerInfo.serverTypeName = server;
                headerInfo.CTFColumnName = servercolname;
                if (bCheckMerge)
                {
                    string useName = Convert.ToString(values[NumRow, CommonDefines.InfoMerge]);
                    headerInfo.bUseMerge = (useName.ToUpper() == "TRUE") ? true : false;
                }

                if ((bCheckCTFColumn && bCTF) || headerInfo.bUseMerge )
                {
                    if ( servercolname != "" )
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
                            int headerindex = CTFArrayCheck[servercolname];
                            if (headerindex >= ServerHeaderInfos.Count)
                                return false;

                            ServerHeaderInfos[headerindex].CTFHeaderIndices.Add(ServerHeaderIndex);
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

                if (j == 1 && headerInfo.clientType == EnumValueType.ContentID &&
                    headerInfo.type == EUsedType.Key)
                {
                    bHasContentIDKey = true;
                }

                if (headerInfo.type == EUsedType.Key || j == 1) // first always key
                {
                    KeyInfos.Add(headerInfo);
                }
                ++NumRow;
                ++j;
            }

            //enyheid : 다 등록한 후 CTFArray를 재구성한다
            j = 0;
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

        public HeaderInfo FindHeaderInfo(string name)
        {
            foreach (var header in AllHeaderInfos)
            {
                if (header.VarName == name)
                    return header;
            }

            return null;
        }
    }
}
