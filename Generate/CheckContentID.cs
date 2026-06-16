using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelConvertLib
{
    public class ContentIDContext
    {
        public string _contentID;
        public string _fullContext;
        public int _row;
        public int _col;
    }

    public class ContentIDChecker : Singleton<ContentIDChecker>
    {
        bool mLoading = false;

        public bool IsLoading() { return mLoading; }

        // 키값으로 확정된 데이터만 넣음
        private HashSet<string> _ContentIDKeySet = new HashSet<string>();

        // 중복 키 set 확인용 hash
        private HashSet<string> _UniqueKeySet = new HashSet<string>();
        private HashSet<string> _LocalKeySet = new HashSet<string>();
        private bool IsMainKeyDefine = false;

        // 예외 케이스의 파일들 
        private HashSet<string> _exceptExcelList = new HashSet<string>();
        private HashSet<string> _exceptCategoryList = new HashSet<string>();

        // 키값으로 등록되지 않은 데이터만 넣는다.
        private List<ContentIDContext> _checkContentData = new List<ContentIDContext>();

        private Dictionary<string, string> _KeyAliasTable = new Dictionary<string, string>();

        private Dictionary<string, List<ExcelFile>> _CombineTable = new Dictionary<string, List<ExcelFile>>();
        private Dictionary<string, ExcelFile> _ExportTableInfo = new Dictionary<string, ExcelFile>();

        HeaderInfo mContentIDkeyInfo = new HeaderInfo(0);
        List<HeaderInfo> mListKeyInfo = new List<HeaderInfo>();
        HeaderInfo mAliasInfo = new HeaderInfo(0);

        public Dictionary<string, List<ExcelFile>> CombineTable { get => _CombineTable; set => _CombineTable = value; }
        public Dictionary<string, ExcelFile> ExportTableInfo { get => _ExportTableInfo; set => _ExportTableInfo = value; }

        public ContentIDChecker()
        {
        }

        public async Task Init(IProgress<double> progress)
        {
            // 코드에 하드코딩된 정보
            _ContentIDKeySet.Clear();
            _ContentIDKeySet.Add("(Asset.Wood)");        // 자원 예외처리
            _ContentIDKeySet.Add("(Asset.Stone)");        // 자원 예외처리
            _ContentIDKeySet.Add("(Asset.Ether)");      // 자원 예외처리
            _ContentIDKeySet.Add("(Asset.DragonFeed)");       // 자원 예외처리
            _ContentIDKeySet.Add("(Asset.Iron)");       // 자원 예외처리
            _ContentIDKeySet.Add("(Asset.Dia)");        // 자원 예외처리
            _ContentIDKeySet.Add("(Asset.Cash)");        // 자원 예외처리

            _ContentIDKeySet.Add("(Squad)");        // 고유값 미리 등록
            _ContentIDKeySet.Add("(User)");
            _ContentIDKeySet.Add("(Guild)");
            _ContentIDKeySet.Add("(Hero)");
            _ContentIDKeySet.Add("(SubHero)");

            _exceptCategoryList.Clear();
            _exceptCategoryList.Add("BuffGroup");

            // 예외 엑셀 리스트
            _exceptExcelList.Clear();
            _exceptExcelList.Add(ConfigSystem.Inst.ConstTable);   // 컨텐츠 아이디 사용안함
            _exceptExcelList.Add(ConfigSystem.Inst.EnumTable);  // 컨텐츠 아이디 사용안함

            _KeyAliasTable.Clear();
            _UniqueKeySet.Clear();
            _LocalKeySet.Clear();

            _CombineTable.Clear();
            _ExportTableInfo.Clear();

            if (progress == null)
            {
                ReadAllContentID();
            }
            else
            {
                await AsyncReadAllContentID(progress);
            }
        }

        public bool TryGetCombineTable(ExcelFile excelFile, out List<ExcelFile> outFiles, out string csvName)
        {
            foreach (var info in _CombineTable)
            {
                if (info.Value.FindIndex(x => x == excelFile) >= 0)
                {
                    outFiles = info.Value;
                    csvName = info.Key;
                    return true;
                }
            }
            outFiles = null;
            csvName = null;
            return false;
        }

        public string GetAliasValue(string alias)
        {
            if (alias == null || alias == "")
                return "";

            if (false == _KeyAliasTable.ContainsKey(alias))
            {
                LogSystem.Inst.Add(string.Format("Cannot Find DataName : {0}", alias), ELOGType.Error);
                MainModule.Inst.HasError = true;
                return "N/A";
            }

            return _KeyAliasTable[alias];
        }

        public string ConvertAliasString(string value, HeaderInfo headerInfo)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (headerInfo == null)
                return value;

            if (headerInfo.clientType == EnumValueType.String || headerInfo.clientType == EnumValueType.Strings || headerInfo.clientType == EnumValueType.Name)
                return value;

            if (headerInfo.serverType == EnumValueType.String || headerInfo.clientType == EnumValueType.Strings || headerInfo.serverTypeName == "std::string")
                return value;

            // @로 시작하는 토큰을 찾아서 alias 변환한다.
            Regex regex = new Regex(@"@\w+");
            MatchCollection matches = regex.Matches(value);
            string[] tokens = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                var newValue = GetAliasValue(matches[i].Value.Substring(1));

                char[] trimstr = { '(', ')' };      // @ convert는 ( )를 제거한다. 모두 제거해도 되면 aliasTable에서 아예 제거
                newValue = newValue.Trim(trimstr);
                if (!string.IsNullOrEmpty(newValue))
                {
                    value = value.Replace(matches[i].Value, newValue);
                }
            }
            
            return value;
        }

        public bool FindContentID(string value)
        {
            char[] trimstr = { ' ', ';' };
            value = value.Trim(trimstr);

            if (string.IsNullOrWhiteSpace(value) == true)
                return true;

            // (Asset.Ether;15000);  (Asset.Ether,GE,1); 같은 예외적인 케이스 제외하기 위함
            var splitList = value.Split(';', ',');
            if (splitList.Length >= 2)
            {
                var replaceId = splitList[0].Replace("(", "");
                value = $"({replaceId})";
            }

            var cIDList = value.Split('.', '(', ')');
            foreach (var keyword in cIDList)
            {
                if (string.IsNullOrWhiteSpace(keyword) == true)
                    continue;

                if (_exceptCategoryList.Contains(keyword))   // 하나라도 있으면 예외처리
                    return true;
            }

            return _ContentIDKeySet.Contains(value);
        }

        public async Task AsyncReadAllContentID(IProgress<double> progress)
        {
            if (mLoading == true)
                return;

            mLoading = true;

            LogSystem.Inst.Add("--Read Excel and Check ContentID");

            progress.Report(0.0f);
            int total = ConfigSystem.Inst.ExcelFiles.Count;

            int processCount = await Task.Run<int>(() =>
            {
                int i = 0;
                foreach (var excelPath in ConfigSystem.Inst.ExcelFiles)
                {
                    // 예외 처리
                    var excelFileName = Path.GetFileName(excelPath.FileName);
                    if (true == _exceptExcelList.Contains(excelFileName))
                        continue;

                    ReadContentID(CommonDefines.ExcelFolder, excelPath);
                    ++i;
                    progress.Report((double)i / total);
                }
                return i;
            });
            //Combine Log
            foreach (var info in _CombineTable)
            {
                string msg = "Combine csv [" + info.Key + "] : ";
                if (info.Value.Count == 1)
                {
                    msg += "병합할 파일이 하나뿐입니다. " + info.Value[0] + " info 를 확인하세요";
                }
                else
                {
                    foreach (var name in info.Value)
                    {
                        msg += name + " / ";
                    }
                }
                LogSystem.Inst.Add(msg);
            }

            MainModule.Inst.CheckEnumUniqueByContentID();

            LogSystem.Inst.Add("--Check ContentID Done");

            mLoading = false;
        }

        public void ReadAllContentID()
        {
            foreach (var excelPath in ConfigSystem.Inst.ExcelFiles)
            {
                // 예외 처리
                var excelFileName = Path.GetFileName(excelPath.FileName);
                if (true == _exceptExcelList.Contains(excelFileName))
                    continue;

                ReadContentID(CommonDefines.ExcelFolder, excelPath);
            }
        }

        public void ReadTableInfo()
        {
            LogSystem.Inst.Add("ContentIDChecker.ReadTableInfo Begin");
            _ExportTableInfo.Clear();
            foreach (var excelPath in ConfigSystem.Inst.ExcelFiles)
            {
                // 예외 처리
                var excelFileName = Path.GetFileName(excelPath.FileName);
                if (true == _exceptExcelList.Contains(excelFileName))
                    continue;

                if ( MainModule.Inst.ReadExcel(CommonDefines.ExcelFolder, excelPath, false))
                {
                    CollectInfo(excelPath);
                    MainModule.Inst.CloseWorkBook();
                }
            }
            LogSystem.Inst.Add("ContentIDChecker.ReadTableInfo End");
        }

        private bool CollectInfo(ExcelFile excelPath)
        {
            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("Info");
            if (ws == null)
                return false;
            object[,] values = (object[,])ws.UsedRange.Value2;

            int rowMax = values.GetLength(0);
            int NumRow = 3;

            // 키 체크 옵션 : Main으로 정의하는 파일인지
            IsMainKeyDefine = false;
            string MainKeyDefine = Convert.ToString(values[1, CommonDefines.InfoDefineKey]);
            if (!string.IsNullOrWhiteSpace(MainKeyDefine))
            {
                if (MainKeyDefine == "Main")
                {
                    IsMainKeyDefine = true;
                }
            }
                
            // Collect Combine 파일 정보
            string CombineFileName = Convert.ToString(values[1, CommonDefines.InfoCombineCol]); // 공용 파일
            if (!string.IsNullOrWhiteSpace(CombineFileName))
            {
                if (_CombineTable.TryGetValue(CombineFileName, out var files))
                {
                    files.Add(excelPath);
                }
                else
                {
                    files = new List<ExcelFile>() { excelPath };
                    _CombineTable.Add(CombineFileName, files);
                }
            }
            // Collect Export 파일 정보 ( client ) ExcelToCSV
            string convertName = Convert.ToString(values[1, CommonDefines.ClientDefNum]);
            if (!string.IsNullOrWhiteSpace(convertName))
            {
                convertName = Path.GetFileNameWithoutExtension(convertName);
            }
            if (!string.IsNullOrWhiteSpace(CombineFileName))
            {
                convertName = CombineFileName;
            }

            if (!string.IsNullOrWhiteSpace(convertName))
            {
                if (!_ExportTableInfo.ContainsKey(convertName))
                    _ExportTableInfo.Add(convertName, excelPath);
                else
                    LogSystem.Inst.Add(string.Format("multi convert file ! - {0} {1} <= {2}  ", convertName, excelPath, _ExportTableInfo[convertName]));
            }
            //////////////////////////////////

            _LocalKeySet.Clear();
            mContentIDkeyInfo = null;
            mListKeyInfo.Clear();
            long ColumnCount = values.GetLongLength(1);
            while (NumRow <= rowMax)
            {
                string strType = Convert.ToString(values[NumRow, CommonDefines.InfoExportType]);
                if (strType == null || strType == "")
                {
                    ++NumRow;
                    continue;
                }

                string server = Convert.ToString(values[NumRow, CommonDefines.InfoServerType]);
                string servercolname = Convert.ToString(values[NumRow, CommonDefines.InfoCTFColumn]);
                string client = Utility.FirstUpper(Convert.ToString(values[NumRow, CommonDefines.InfoClientType]));

                HeaderInfo headerInfo = new HeaderInfo(NumRow - 2);
                headerInfo.type = EnumUtils.ToEnum<EUsedType>(strType);
                headerInfo.VarName = Convert.ToString(values[NumRow, 1]);
                headerInfo.serverType = EnumUtils.ToEnum<EnumValueType>(Utility.FirstUpper(server));
                headerInfo.clientType = EnumUtils.ToEnum<EnumValueType>(client);
                headerInfo.clientTypeName = client;
                headerInfo.serverTypeName = server;

                if (headerInfo.type == EUsedType.Key)
                {
                    if (mContentIDkeyInfo == null && headerInfo.clientType == EnumValueType.ContentID)
                        mContentIDkeyInfo = headerInfo;

                    mListKeyInfo.Add(headerInfo);
                }
                if (headerInfo.type == EUsedType.DataName)
                {
                    mAliasInfo = headerInfo;
                }

                ++NumRow;
            }

            return true;
        }

        private bool ReadContentID(string folder, ExcelFile excelPath)
        {
            MainModule.Inst.ReadExcel(folder, excelPath, false);

            // info 정보 확인
            if (CollectInfo(excelPath) == false)
            {
                LogSystem.Inst.Add(string.Format("Failed to load info : {0}", excelPath), ELOGType.Error);
                return false;
            }

            if (mContentIDkeyInfo == null)
                return true;

            // 데이터 취합
            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("Data");
            if (ws == null)
            {
                LogSystem.Inst.Add("Not Found Data Sheet !", ELOGType.Error);
                return false;
            }
            object[,] values = (object[,])ws.UsedRange.Value2;

            int colMax = values.GetLength(1);
            int keyIDIndex = 0;
            List<int> subkeyIndex = new List<int>();
            int aliasIndex = 0;
            for (int j = 1; j <= colMax; ++j)
            {
                string strName = Convert.ToString(values[1, j]);
                if (strName == mContentIDkeyInfo.VarName)
                    keyIDIndex = j;

                if (mAliasInfo != null && strName == mAliasInfo.VarName)
                    aliasIndex = j;

                foreach (var keyHeader in mListKeyInfo)
                {
                    if (strName == keyHeader.VarName)
                        subkeyIndex.Add(j);
                }
            }

            if (keyIDIndex <= 0)
            {
                LogSystem.Inst.Add($"ContentID not Found: File:{excelPath}", ELOGType.Error);
                return false;
            }

            int rowMax = values.GetLength(0);
            for (int j = 2; j <= rowMax; ++j)
            {
                char[] trimstr = { ' ', ';' };
                var keyString = Convert.ToString(values[j, keyIDIndex]).Trim(trimstr);
                if (keyString == null || keyString == "")
                    continue;

                // 고유 ContentID 저장
                _ContentIDKeySet.Add(keyString);

                // DataName 매핑 및 체크
                if (aliasIndex > 0)
                {
                    var aliasString = Convert.ToString(values[j, aliasIndex]).Trim(trimstr);
                    if (string.IsNullOrEmpty(aliasString))
                        continue;

                    if (_KeyAliasTable.ContainsKey(aliasString))
                    {
                        if (GetAliasValue(aliasString) != keyString)
                        {
                            var msg = string.Format("중복된 DataName [{0}] : 파일:[{1}] - 중복키 [{2}, {3}]", aliasString, excelPath, GetAliasValue(aliasString), keyString);
                            LogSystem.Inst.Add(msg, ELOGType.Error);
                        }
                    }
                    else
                    {
                        _KeyAliasTable.Add(aliasString, keyString);
                    }
                }

                // 키 선언 중복 체크
                var fullKeyStringSB = new StringBuilder();
                foreach (var keyindex in subkeyIndex)
                {
                    fullKeyStringSB.Append(Convert.ToString(values[j, keyindex]).Trim(trimstr));
                    fullKeyStringSB.Append(", ");
                }
                if (string.IsNullOrEmpty(fullKeyStringSB.ToString()))
                    continue;
                fullKeyStringSB.Remove(fullKeyStringSB.Length - 2, 2);  // 뒤에 콤마 삭제

                HashSet<string> KeySet = _LocalKeySet;
                if (IsMainKeyDefine == true)
                {
                    KeySet = _UniqueKeySet;
                }

                var fullKeyString = fullKeyStringSB.ToString();
                if (KeySet.Contains(fullKeyString))
                {
                    var msg = string.Format("Key [{0}]가 중복됩니다. : 파일:[{1}]", fullKeyString, excelPath);
                    LogSystem.Inst.Add(msg, ELOGType.Error);

                }
                else
                {
                    KeySet.Add(fullKeyString);
                }
            }

            return true;
        }


        private int CheckContentIDList()
        {
            int errorCount = 0;

            foreach (var contentData in _checkContentData)
            {
                string contentID = contentData._contentID;

                // 기존 키로 등록 되어 있으면 제외함
                if (true == _ContentIDKeySet.Contains(contentID))
                    continue;

                // (Asset.Ether;15000);  (Asset.Ether,GE,1); 같은 예외적인 케이스 제외하기 위함
                var splitList = contentID.Split(';', ',');
                if (splitList.Length >= 2)
                {
                    var replaceId = splitList[0].Replace("(", "");
                    if (true == _ContentIDKeySet.Contains($"({replaceId})"))
                        continue;
                }

                //LogSystem.Inst.Add($"No Exist ContentID: {contentData._fullContext} Table:{contentData._tableName} Row:{contentData._row} Col:{contentData._col}", ELOGType.Error);
                errorCount++;
            }

            return errorCount;

        }

        public int ExcelToContentIDCheck()
        {
            // Temporary disabled : #2431
#if false
            // 1. 엑셀 파일을 전부 읽는다.
            foreach ( var excelPath in ConfigSystem.Inst.Files)
            {
                // 예외 처리
                var excelFileName = Path.GetFileName(excelPath);
                if (true == _exceptExcelList.Contains(excelFileName))
                    continue;

                //LogSystem.Inst.Add($"ReadExcel: {excelPath}");

                ReadExcel(CommonDefines.ExcelFolder, excelPath);   
            }
#endif

            // 2. 사용중인 컨텐츠 아이디가 선언이 누락된 경우를 찾는다.
            return 0;
            //return CheckContentIDList();
        }
    }
}
