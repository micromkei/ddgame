using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data;
using Perforce.P4;
using System.Windows;
using System.Linq;

namespace ExcelConvertLib
{
    public class MainModule : Singleton<MainModule>
    {
        private Excel.Application mExcelApp;
        private Excel.Workbook mWorkBook;

        public List<JoinInfo> JoinDefInfos = new List<JoinInfo>();
        public Dictionary<string, List<EnumInfo>> EnumDefInfos = new Dictionary<string, List<EnumInfo>>();


        private string mRootPath = "";
        private string mOutPath = "";
        private string mCSVPath = "";
        private string mUEPath = "";
        private string mL10NPath = "";
        private string mGuidePath = "";

        public bool HasError
        {
            get { return bHasError;  }
            set { bHasError = value; 
                if (value == false)  ErrorCount = 0;
            }
        }
        public Excel.Application ExcelApp { get => mExcelApp; }
        public Excel.Workbook WorkBook { get => mWorkBook; }

        static Repository _p4rep = null;
        static Connection _p4con = null;

        private bool bHasError = false;
        public int ErrorCount = 0;

        public string PerforceWork = "";
        public string PerforceUser = "";
        public string PerforceBranch = "";
        public static string PerforceURL = "172.18.112.63:1666";

        public MainModule()
        {
            mExcelApp = new Excel.Application();
            mExcelApp.ScreenUpdating = false;
            mExcelApp.EnableEvents = false;
            mExcelApp.DisplayAlerts = false;
            //mExcelApp.Calculation =  Excel.XlCalculation.xlCalculationManual;
        }

        ~MainModule()
        {
            Uninitialize();
        }

        public void Init(string _rootPath, string _oPath, string _csvPath, string _uePath, string _l10nPath, string _guidePath)
        {
            mRootPath = _rootPath;
            mOutPath = _oPath;
            mCSVPath = _csvPath;
            mUEPath = _uePath;
            mL10NPath = _l10nPath;
            mGuidePath = _guidePath;
        }

        public void Uninitialize()
        {
            ReleaseExcelObject(mExcelApp);
            mExcelApp = null;
        }

        public void SetPerforce(string url, string user, string work, string branch)
        {
            PerforceURL = url;
            PerforceWork = work;
            PerforceUser = user;
            PerforceBranch = branch;

            ClosePerforce();
            // P4 Setting
            P4Connection();

        }

        public void ClosePerforce()
        {
            if (_p4rep == null)
                return;

            _p4con.Dispose();
            _p4rep.Dispose();

            _p4rep = null;
        }
        static private bool InternalP4Add(DepotPath path, string fileName, bool utf8 = false)
        {
            if (false == P4Connection())
                return false;

            StringList paramList = new StringList();
            if (utf8 == true)
            {
                // add option(-t utf8)
                paramList.Add("-t");
                paramList.Add("utf8");
            }
            paramList.Add($"{path}/{fileName}");

            P4Command cmd = new P4Command(_p4rep, "add", true, paramList.ToArray());
            P4CommandResult result = cmd.Run();

            if (result.ErrorList != null)
            {
                LogSystem.Inst.Add($"add fail {result.ErrorList[0].ErrorMessage}({paramList.ToString()})", ELOGType.Error);
                return false;
            }

            LogSystem.Inst.Add($"add succ {paramList.ToString()}", ELOGType.Log);

            return true;
        }

        static private bool InternalP4CheckOut(DepotPath path, string fileName)
        {
            if (false == P4Connection())
                return false;

            string p4Param = $"{path}/{fileName}";
            P4Command cmd = new P4Command(_p4rep, "edit", true, p4Param);
            P4CommandResult result = cmd.Run();

            if (result.ErrorList != null)
            {
                LogSystem.Inst.Add($"checkout fail {result.ErrorList[0].ErrorMessage}({p4Param})", ELOGType.Error);
                return false;
            }

            LogSystem.Inst.Add($"checkout succ {p4Param}", ELOGType.Log);

            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath">상대 경로</param>
        /// <returns></returns>
        static public bool CheckOutFilePath(string filePath)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;
            var filePathInDepot = Path.GetFullPath(filePath);       // 절대 경로 

            var path = new DepotPath(Path.GetDirectoryName(filePathInDepot));
            var fileName = Path.GetFileName(filePathInDepot);

            return InternalP4CheckOut(path, fileName);
        }

        static public bool CheckOutFile(string prefix, string fileName)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var path = new DepotPath($"//ProjectG/{Inst.PerforceBranch}/ProjectG_ctf/CTF/{prefix}");

            return InternalP4CheckOut(path, fileName);
        }

        static public bool CheckOutExelFile(string folder, ExcelFile excelFile)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var depotPathString =
                $"//ProjectG/{Inst.PerforceBranch}/ProjectG_ctf/{folder}";

            if (excelFile.HasSubdirectory)
            {
                depotPathString += $"/{excelFile.Subdirectory}";
            }

            var path = new DepotPath(depotPathString);

            return InternalP4CheckOut(path, excelFile.FileName);
        }

        static public bool CheckOutUEFile(string folder, string fileName)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var path = new DepotPath($"//ProjectG/{Inst.PerforceBranch}/ProjectG_Client/Games/ProjectG/{folder}");

            return InternalP4CheckOut(path, fileName);
        }

        static public bool CheckOutClientCodeFile(string fileName)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var path = new DepotPath($"//ProjectG/{Inst.PerforceBranch}/ProjectG_Client/Games/ProjectG/Source/TACTAN/Public");

            return InternalP4CheckOut(path, fileName);
        }

        static public bool CheckOutToolCodeFile(string fileName)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var path = new DepotPath($"//ProjectG/{Inst.PerforceBranch}/ProjectG_tools/Client/DataConvert/ExcelConvertLib/");
            return InternalP4CheckOut(path, fileName);
        }

        static public bool CheckOutClientCSVFile(string fileName)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var path = new DepotPath($"//ProjectG/{Inst.PerforceBranch}/ProjectG_Client/Games/ProjectG/Content/CSV");

            return InternalP4CheckOut(path, fileName);
        }

        static public bool CheckOutServerEnumHeaderFile(string fileName)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var path = new DepotPath($"//ProjectG/{Inst.PerforceBranch}/ProjectG_common/common");

            if (System.IO.File.Exists(Inst.GetServerEnumHeaderPath(fileName)))
                return InternalP4CheckOut(path, fileName);
            else
                return InternalP4Add(path, fileName);
        }

        static public bool CheckOutServerEnumErrorHeaderFile(string fileName)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var path = new DepotPath($"//ProjectG/{Inst.PerforceBranch}/ProjectG_common/common");

            if (System.IO.File.Exists(Inst.GetServerEnumHeaderPath(fileName)))
                return InternalP4CheckOut(path, fileName);
            else
                return InternalP4Add(path, fileName);
        }

        static public bool CheckOutServerCTFHeaderFile(string fileName)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var path = new DepotPath($"//ProjectG/{Inst.PerforceBranch}/ProjectG_common/CTF/common");

            if (true == InternalP4CheckOut(path, fileName))
                return true;
            else
                return InternalP4Add(path, fileName, true);
        }

        static public bool CheckOutCTFFile(string fileName)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var path = new DepotPath($"//ProjectG/{Inst.PerforceBranch}/ProjectG_ctf/CTF/common");

            return InternalP4CheckOut(path, "common_" + fileName);
        }
        static public bool AddCTFFile(string fileName, bool utf8 = false)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var path = new DepotPath($"//ProjectG/{Inst.PerforceBranch}/ProjectG_ctf/CTF/common");

            return InternalP4Add(path, "common_" + fileName, utf8);
        }

        static public bool RevertIfUnchangedClientCodeFile(string fileName)
        {
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return true;

            var path = new DepotPath($"//ProjectG/{Inst.PerforceBranch}/ProjectG_Client/Games/ProjectG/Source/TACTAN/Public");

            return RevertIfUnchanged(path, fileName);
        }

        static private bool RevertIfUnchanged(DepotPath path, string fileName)
        {
            if (false == P4Connection())
                return false;

            string p4Param = $"-a {path}/{fileName}";
            P4Command cmd = new P4Command(_p4rep, "revert", true, p4Param);
            P4CommandResult result = cmd.Run();

            if (result.ErrorList != null)
            {
                LogSystem.Inst.Add($"revert fail {result.ErrorList[0].ErrorMessage}({p4Param})", ELOGType.Error);
                return false;
            }

            LogSystem.Inst.Add($"revert succ {p4Param}", ELOGType.Log);

            return true;
        }

        static public bool P4Connection()
        {
            // 이미 연결되어 있으면 제외함ㄴ
            if (_p4rep != null)
                return true;
            if (ConfigSystem.Inst.Data.PerforceCheck == false)
                return false;
            try
            {
                // ** Define the server, repository and connection **//
                var server = new Server(new ServerAddress(PerforceURL));
                var rep = new Repository(server);
                Connection con = rep.Connection;

                // ** Use the connection varaibles for this connection **//
                con.UserName = Inst.PerforceUser;
                con.Client = new Client();
                con.Client.Name = Inst.PerforceWork;
                //  con.Client.Stream = Inst.PerforceBranch;

                // ** Connect to the server **//
                if (false == con.Connect(null))
                    return false;

                _p4rep = rep;
                _p4con = con;

            }
            catch (Exception ex)
            {
                LogSystem.Inst.Add(ex.Message);
                return false;
            }
            return true;
        }

        public string GetP4User()
        {
            return _p4con.UserName;
        }

        public string GetP4Stream()
        {
            return _p4con.Client.Stream;
        }

        //public List<string> GetStreamWorkspaceList()
        //{
        //    List<string> workspaceList = new List<string>();

        //    ClientsCmdOptions opts = new ClientsCmdOptions(ClientsCmdFlags.IgnoreCase, ConfigSystem.Inst.Data.PerforceUser, null, int.MaxValue, null);
        //    IList<Client> clients = _p4rep.GetClients(opts);
        //    if (clients == null)
        //        return workspaceList;
        //    //
        //    foreach (Client client in clients)
        //    {
        //        if (client.Host != _p4rep.Connection.Client.Host)
        //            continue;

        //        if (string.IsNullOrEmpty(client.Stream) == true)
        //            continue;

        //        workspaceList.Add($"{client.Name}");
        //    }

        //    return workspaceList;
        //}

        //public List<string> GetStreamList()
        //{
        //    List<string> workspaceList = new List<string>();

        //    ClientsCmdOptions opts = new ClientsCmdOptions(ClientsCmdFlags.IgnoreCase, "", null, int.MaxValue, null);
        //    IList<Client> clients = _p4rep.GetClients(opts);
        //    if (clients == null)
        //        return workspaceList;
        //    //
        //    foreach (Client client in clients)
        //    {
        //        if (client.Host != _p4rep.Connection.Client.Host)
        //            continue;

        //        if (string.IsNullOrEmpty(client.Stream) == true)
        //            continue;

        //        workspaceList.Add($"{client.Stream}");
        //    }

        //    return workspaceList;
        //}

        public string GetOutPath(string prefix, string fileName, string ext)
        {
            if (prefix == "") return mOutPath + "/" + fileName + "." + ext;
            if (prefix.Contains("/"))
            {
                return mRootPath + "/" + prefix + fileName + "." + ext;
            }
            return mOutPath + "/" + prefix + fileName + "." + ext;
        }

        public string GetCSVPath( string fileName )
        {
            return mCSVPath + "/" +  fileName + ".csv";
        }

        public string GetClientOutPath(string fileName)
        {
            return mCSVPath + "/" + fileName ;
        }

        public string GetServerOutPath(string prefix, string fileName)
        {
            return mOutPath + $"/{prefix}/{prefix}_" + fileName;
        }

        public string GetUEPath(string fileName)
        {
            return Path.Combine(mUEPath, fileName);
        }

        public string GetClientCodePath( string fileName)
        {           
            return string.Format("{0}/Source/TacTan/Public/{1}", mUEPath, fileName);
        }

        public string GetToolCodePath(string fileName)
        {
            return string.Format("{0}/../../ProjectG_tools/Client/DataConvert/ExcelConvertLib/{1}", mOutPath, fileName);
        }

        public string GetServerCodePath(string fileName)
        {
            return string.Format("{0}/../../ProjectG_common/CTF/common/{1}", mOutPath, fileName);
        }

        public string GetServerEnumHeaderPath(string fileName)
        {
            return string.Format("{0}/../../ProjectG_common/common/{1}", mOutPath, fileName);
        }

        public string GetL10NPath(string fileName)
        {
            return Path.Combine(mL10NPath, string.Format("{0}.csv", fileName));
        }

        public string GetLocalizePath(string fileName)
        {
            return string.Format("{0}/Content/CSV/String/{1}", mUEPath, fileName);
        }

        public string GetL10NPath(string fileName, string extName)
        {
            return Path.Combine(mL10NPath, string.Format("{0}.{1}", fileName, extName));
        }

        public string GetGuildFolder()
        {
            return mGuidePath;
        }


        public bool ReadExcel(string folder, string fileName, bool bCheck = true, bool bReadOnly = true)
        {
            var excelFile = new ExcelFile()
            {
                FileName = fileName,
            };

            return ReadExcel(folder, excelFile, bCheck, bReadOnly);
        }

        public bool ReadExcel(string folder, ExcelFile excelFile, bool bCheck = true, bool bReadOnly = true)
        {
            if (mWorkBook != null)
            {
                CloseWorkBook();
            }
            HasError = false;
           // try
            {
                string excelPath = Utility.GetPath(mRootPath + $"/{folder}/{excelFile.CombinedPath}" );
//                 if (bCheck == true)
//                 {
//                     if ( !CheckOutExelFile(folder,excelFile) )
//                     {
//                         LogSystem.Inst.Add("Cannot CheckOut File " + excelFile, ELOGType.Error);
//                         HasError = true;
//                     }
//                 }
                string fullPath = Path.GetFullPath(excelPath);
                if (System.IO.File.Exists(fullPath) == false)
                {
                    LogSystem.Inst.Add("Not found " + fullPath, ELOGType.Error);
                    //    HasError = true;
                    return false;
                }
                mWorkBook = mExcelApp.Workbooks.Open(fullPath, 0, bReadOnly);                
            }
           // catch (Exception ex)
            //{
            //    LogSystem.Inst.Add(ex.ToString(), ELOGType.Error);
            //    HasError = true;
            //    return false;
            //}

            return true;
        }

        public object[,] GetRangeObject(Excel.Worksheet ws, int x, int y, int x1, int y1)
        {
            Excel.Range c1 = ws.Cells[y, x];
            Excel.Range c2 = ws.Cells[y1, x1];
            Excel.Range oRange = ws.get_Range(c1, c2);
            //oRange.EntireColumn.AutoFit();
            return (object[,])oRange.Value2;
        }

        public Excel.Worksheet GetWorkSheet(string tableName)
        {
            if (mWorkBook == null) return null;
            foreach(Excel.Worksheet sheet in mWorkBook.Sheets)
            {
                if(sheet.Name == tableName)
                {
                    return sheet;
                }
            }
            return null;
        }
		
        public Excel.Sheets GetWorkSheets()
        {
            if (mWorkBook == null) return null;

            return mWorkBook.Sheets;
        }

        public void SaveExcel()
        {
            if (mWorkBook == null) return;

            mWorkBook.Save();
        }

        public void CloseWorkBook()
        {
            if (mWorkBook == null)
                return;
            mWorkBook.Close(false);
            ReleaseExcelObject(mWorkBook);

            mWorkBook = null;

            GC.Collect();
        }

        public void OnRelease()
        {
            if (mExcelApp == null)
                return;
            ReleaseExcelObject(mWorkBook);
            //mExcelApp.Quit();
            ReleaseExcelObject(mExcelApp);

            LogSystem.Inst.Save();

            GC.Collect();
        }

        protected static void ReleaseExcelObject(object obj)
        {
            try
            {
                if (obj != null)
                {
                    Marshal.ReleaseComObject(obj);
                    obj = null;
                }
            }
            catch (Exception ex)
            {
                obj = null;
                throw ex;
            }
        }

        public string GetSheetType(Excel.Worksheet ws)
        {
            if(ws == null)
            {
                return string.Empty;
            }

            if (ws.Name == "config")
            {
                return string.Empty;
            }
            if (ws.Name.PadLeft(4).ToUpper() == "INFO")
            {
                return string.Empty;
            }

            object[,] values = GetRangeObject(ws, 1, 1, 2, 2);
            return Convert.ToString(values[1, 1]);
        }

        public string GetClassName(Excel.Worksheet ws)
        {
            if (ws == null)
            {
                return string.Empty;
            }
            object[,] values = GetRangeObject(ws, 1, 1, 2, 2);
            return Convert.ToString(values[1, 2]);
        }

        public bool CheckEnumUniqueByContentID()
        {
            HasError = false;

            Dictionary<string,string> uniqueIDs = new Dictionary<string, string>();
            foreach (string EnumName in Utility.CategoryEnums)
            {
                InternalCheckEnum(ref uniqueIDs, "Category", EnumName);
            }
            foreach (string EnumName in Utility.VarietyEnums)
            {
                InternalCheckEnum(ref uniqueIDs, "Variety", EnumName);                
            }

            uniqueIDs.Clear();
            foreach (string EnumName in Utility.KINDEnums)
            {
                InternalCheckEnum(ref uniqueIDs, "KIND", EnumName);
            }
            return !HasError;
        }

        private void InternalCheckEnum(ref Dictionary<string, string> uniqueIDs, string Tag, string EnumName)
        {
            List<EnumInfo> enumInfos;
            EnumDefInfos.TryGetValue(EnumName, out enumInfos);
            foreach (EnumInfo enumInfo in enumInfos)
            {
                if (enumInfo.Name.Contains("Undefine") || enumInfo.Name.Contains("None") || enumInfo.Name.Contains("_Enum"))
                {
                    continue;
                }

                if (uniqueIDs.ContainsKey(enumInfo.Name))
                {
                    LogSystem.Inst.Add(string.Format("Error {0} :  [{1} , {2}] {3} ",Tag, uniqueIDs[enumInfo.Name],
                        EnumName, enumInfo.Name ), ELOGType.Error);
                    HasError = true;
                    continue;
                }
                uniqueIDs.Add(enumInfo.Name,EnumName);
            }
        }

        public bool ReadConfig()
        {
            try
            {
                if (mWorkBook == null) return false;

                EnumDefInfos.Clear();
                foreach (Excel.Worksheet ws in mWorkBook.Sheets)
                {
                    string EnumName = ws.Name;
                    if (EnumName[0] == '#')
                        continue;

                    List<EnumInfo> enumInfos = new List<EnumInfo>();

                    object[,] values = (object[,])ws.UsedRange.Value2;
                    if (values == null)
                    {
                        LogSystem.Inst.Add(string.Format("error {0} Sheet Config", EnumName), ELOGType.Error);
                        return false;
                    }
                    int rowMax = values.GetLength(0);

                    int numRow = 2;
                    List<uint> uniqueCheck = new List<uint>();
                    while (numRow <= rowMax)
                    {
                        EnumInfo info = new EnumInfo();
                        info.Name = Convert.ToString(values[numRow, 1]).TrimEnd();
						if (string.IsNullOrEmpty(info.Name)) // End line
						{
							//LogSystem.Inst.Add(string.Format("Enum {0} Row {1} is null", EnumName, numRow), ELOGType.Error);
							break;
						}
						info.Value = (info.Name[0] == '/') ? 0 : uint.Parse(Convert.ToString(values[numRow, 2]));
                        info.Desc = Convert.ToString(values[numRow, 3]);

                      
                        if (!(info.Name[0] == '_' || info.Name[0] == '/'))
                        {
                            if (uniqueCheck.Contains(info.Value))
                            {
                                LogSystem.Inst.Add(string.Format("Enum {0} {1} {2}", EnumName, info.Name, info.Value), ELOGType.Error);
                                HasError = true;
                            }

                            uniqueCheck.Add(info.Value);
                        }
                        enumInfos.Add(info);

                        numRow++;
                    }
                    EnumDefInfos.Add(EnumName, enumInfos);
                }
                CloseWorkBook();
            }
            catch (Exception ex)
            {
                LogSystem.Inst.Add(ex.Message, ELOGType.Error);
                return false;
            }
            return true;
        }


        int ConfigTableDefinition(ref object[,] values, int rowIndex, int max, int colMax)
        {
            JoinDefInfos.Clear();

            int NumRow = rowIndex;
            while (NumRow < max)
            {
                if (values[NumRow, 1] == null)
                    break;
                string outName = Convert.ToString(values[NumRow, 1]);
                for (int j = 1; j < colMax; j += 2)
                {
                    if (values[NumRow, j] == null)
                        break;

                    JoinInfo info = new JoinInfo()
                    {
                        tableName = Convert.ToString(values[NumRow, j]),
                        sectionName = Convert.ToString(values[NumRow, j + 1])
                    };
                    JoinDefInfos.Add(info);
                }
                ++NumRow;
            }
            return NumRow + 1;
        }

        public int FindEnumValue(string typeName, string name)
        {
            var EnumList = EnumDefInfos[typeName];
            foreach (EnumInfo info in EnumList)
            {
                if (info.Name == name)
                    return (int)info.Value;
            }
            return -1;
        }

        /*
         *      struct
                {
                    UInt32	m_KIND;			//	카인드
                    UInt16	m_Variety;		//	종류 (서버카테고리)
                    UInt16	m_Category;		//	카테고리
                };
                struct
                {
                    UInt16	m_Sz;
                    UInt16	m_Sy;
                    UInt16	m_Sx;
                    UInt16	Unused;			//	카테고리 - 서버ID로 확장가능 (다른서버 정보보기)
                };
                struct
                {
                    UInt32	second;
                    UInt32	first;
                };
                UInt64	m_ContentID = 0;	//	컨텐츠ID
         */

        public bool IsEnumCheck(string typeName, string name)
        {
            foreach (EnumInfo info in EnumDefInfos[typeName])
            {
                if (info.Name == name)
                    return true;
            }
            return false;
        }

        public bool IsAllEnumCheck(string name)
        {
            foreach (List<EnumInfo> enumInfos in EnumDefInfos.Values)
            {
                foreach (EnumInfo info in enumInfos)
                {
                    if (info.Name == name)
                        return true;
                }
            }
            return false;
        }

        public UInt64 ConvertContentID( string id )
        {
            string[] arValue = id.Split(new char[] { '.' });

            SContentID contentID = new SContentID();
            contentID.Category = (ushort)Utility.FindCate(arValue[0]);
            if (arValue.Length == 1)
                return contentID.Code;

            if (arValue.Length == 2)
            {
                contentID.KIND = (uint)Utility.FindKIND(arValue[1]);
            }
            else //if (arValue.Length == 3)
            {
                contentID.Variety = (ushort)Utility.FindVariety(arValue[1]);
                contentID.KIND = (uint)Utility.FindKIND(arValue[2]);
            }          

            return contentID.Code;
        }

    }
}