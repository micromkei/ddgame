using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using System.Data.Common;
using System.Runtime.Remoting.Messaging;
using ExcelConvertLib;

namespace ExcelConvertLib
{
    public static class Utility
    {
        // 추가규칙  https://nckorea.atlassian.net/wiki/spaces/PROJECTG/pages/281732613/ID+ContentID

        public static List<string> CategoryEnums = new List<string>()
            {
                CommonDefines.EnumCate,
                CommonDefines.EnumEffect,
                CommonDefines.EnumUnit,
                CommonDefines.EnumRace,
                CommonDefines.EnumObjStatus,
                CommonDefines.EnumConditionCate
            };
        public static List<string> KINDEnums = new List<string>()
            {
                CommonDefines.EnumBuilding,
                CommonDefines.EnumUnit,
                CommonDefines.EnumAsset,
                CommonDefines.EnumRace,
                CommonDefines.EnumWorldTag,
                CommonDefines.EnumGuildBuilding,
                CommonDefines.EnumTokenType
            };
        public static List<string> VarietyEnums = new List<string>()
            {
                CommonDefines.EnumAsset,
                CommonDefines.EnumBuilding
            };


        public static Color ToColor(this string str)
        {
            string[] splittedStr = str.Split(',');

            if (splittedStr.Length != 4 && splittedStr.Length != 3) throw new ArgumentException();

            int[] values = new int[splittedStr.Length];
            for (int i = 0; i < splittedStr.Length; ++i)
            {
                values[i] = int.Parse(splittedStr[i]);
            }

            if (values.Length == 4) return System.Drawing.Color.FromArgb(values[0], values[1], values[2], values[3]);
            else return Color.FromArgb(values[0], values[1], values[2]);
        }

        public static void AddTab(StringBuilder sb,int num)
        {
            for (int i = 0; i < num; ++i)
                sb.Append("\t");
        }

        public static string GetPath(string path)
        {
            if (Path.IsPathRooted(path))
                return path;
#if DEBUG
            return Path.Combine(System.IO.Directory.GetCurrentDirectory(), path);
#else
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
#endif
        }

        public static string FirstUpper( string name )
        {
            if (name.Length < 3) return name;
            return name.Substring(0,1).ToUpper() + name.Substring(1,name.Length - 1);
        }

        public static string StripContentID(string id)  //"() 제거"
        {            
            if (id == null) return id;

            string[] arValue = id.Split(new char[] { '(', ')', });
            if (arValue.Length > 1)
                return arValue[1];
            return id;
        }

        public static string PascalCase(string word)
        {
            return string.Join("", word.Split('_')
                         .Select(w => w.Trim())
                         .Where(w => w.Length > 0)
                         .Select(w => w.Substring(0, 1).ToUpper() + w.Substring(1).ToLower()));
        }

        public static string GenMergeArray( List<HeaderInfo> HeaderInfos, ref object[,] values, int NumRow, HeaderInfo info)
        {
            int count = info.CTFHeaderIndices.Count();
            if ( count == 0)
            {
                return "";
            }
            int i = 0;
            StringBuilder sb = new StringBuilder();
            foreach (int idx in info.CTFHeaderIndices)
            {
                HeaderInfo hInfo = HeaderInfos[idx];
                if (hInfo.colNum < 0) continue;

                string value = Convert.ToString(values[NumRow, hInfo.colNum + 1]);

                if (hInfo.bUseDataName)
                {
                    value = ContentIDChecker.Inst.GetAliasValue(value);
                }
                else
                {
                    value = ContentIDChecker.Inst.ConvertAliasString(value, hInfo);
                }              

                if (string.IsNullOrWhiteSpace(value)) continue;

                if (hInfo.CTFFormat.Contains(";") == false)
                {
                    if (i > 0)
                    {
                        sb.Append(",");
                    }
                }

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
                ++i;
            }        

            return sb.ToString();
        }


        public static bool IsEnumEffect(string value)
        {
            string[] arValue = value.Split(new char[] { '.' });
            if (arValue.Length == 0)
            {
                return false;
            }
            int val = MainModule.Inst.FindEnumValue(CommonDefines.EnumEffect, arValue[0]);
            if (val == -1)
            {
                return false;
            }
            return true;
        }

        public static bool IsContentIDCheck(string value)
        {
            string id = StripContentID(value);
            if (id == "") //empty use
                return true;
            string[] arValue = id.Split(new char[] { '.' });
            if (arValue.Length == 0)
            {
                return false;
            }

            if (FindCate(arValue[0]) < 0) //EnumEffect
            {
                return false;
            }

            if (arValue.Length == 2)
            {
                if (FindKIND(arValue[1]) < 0)
                    return false;
            }
            else if (arValue.Length == 3)
            {
                if (FindVariety(arValue[1]) < 0)
                    return false;
                if (FindKIND(arValue[2]) < 0)
                    return false;
            }
            else if (arValue.Length > 3)
            {
                return false;
            }
            return true;
        }


        public static int FindCate(string val)
        {
            int value = -1;
            foreach (string EnumName in CategoryEnums)
            {                
                if (value == -1)
                {
                    value = MainModule.Inst.FindEnumValue(EnumName, val);
                }
            }
            return value;
        }

        public static int FindKIND(string val)
        {
            int value = -1;
            foreach (string EnumName in KINDEnums)
            {
                if (value == -1)
                {
                    value = MainModule.Inst.FindEnumValue(EnumName, val);
                }
            }
            if (value == -1)
            {
                if (int.TryParse(val, out value) == false)
                {
                    LogSystem.Inst.Add(string.Format("ContentID Kind Parse Error {0}", val), ELOGType.Error);
                    return -1;
                }
            }
            return value;
        }

        public static int FindVariety(string val)
        {           
            //enyheid : variety는 기본적으로 category 조건을 따라간다
            int value = FindCate(val);//check category first
            foreach (string EnumName in VarietyEnums)
            {
                if (value == -1)
                {
                    value = MainModule.Inst.FindEnumValue(EnumName, val);
                }
            }
            return value;
        }
    }

    public static class InitializeUtility
    {
        private static bool bInited = false;

        public static void InitDir(string InRootPath)
        {
            if (bInited == true)
            {
                return;
            }

            string Dir = Path.GetDirectoryName(InRootPath);
            ConfigSystem.Inst.Init(Dir);
            CTFInfoSystem.Inst.Init(Dir);

            string outPath = Path.Combine(ConfigSystem.Inst.Data.RootFolder, ConfigSystem.Inst.Data.OutFolder);
            string csvPath = Path.Combine(ConfigSystem.Inst.Data.RootFolder, ConfigSystem.Inst.Data.CSVFolder);
            string uePath = Path.Combine(ConfigSystem.Inst.Data.RootFolder, ConfigSystem.Inst.Data.UEFolder);
            string l10nPath = Path.Combine(ConfigSystem.Inst.Data.RootFolder, ConfigSystem.Inst.Data.L10NFolder);
            string guidePath = Path.Combine(ConfigSystem.Inst.Data.RootFolder, ConfigSystem.Inst.Data.GuideFolder);

            MainModule.Inst.Init(ConfigSystem.Inst.Data.RootFolder, outPath, csvPath, uePath, l10nPath, guidePath);

            MainModule.Inst.SetPerforce(
                ConfigSystem.Inst.Data.PerforceURL,
                ConfigSystem.Inst.Data.PerforceUser,
                ConfigSystem.Inst.Data.PerforceWork,
                ConfigSystem.Inst.Data.PerforceBranch
            );

            bInited = true;
        }
        public static void InitDir(string InRootPath, string P4URL, string P4User, string P4Stream, string P4WorkSpace)
        {
            if (bInited == true)
            {
                return;
            }

            Console.WriteLine("Init {0} {1} {2} {3}", InRootPath, P4User, P4Stream, P4WorkSpace);

            string Dir = Path.GetDirectoryName(InRootPath);
            ConfigSystem.Inst.Init(Dir);
            CTFInfoSystem.Inst.Init(Dir);

            ConfigSystem.Inst.Data.PerforceURL = P4URL;
            ConfigSystem.Inst.Data.PerforceBranch = P4Stream;
            ConfigSystem.Inst.Data.PerforceUser = P4User;
            ConfigSystem.Inst.Data.PerforceWork = P4WorkSpace;
            ConfigSystem.Inst.Data.PerforceCheck = true;

            string outPath = Path.Combine(ConfigSystem.Inst.Data.RootFolder, ConfigSystem.Inst.Data.OutFolder);
            string csvPath = Path.Combine(ConfigSystem.Inst.Data.RootFolder, ConfigSystem.Inst.Data.CSVFolder);
            string uePath = Path.Combine(ConfigSystem.Inst.Data.RootFolder, ConfigSystem.Inst.Data.UEFolder);
            string l10nPath = Path.Combine(ConfigSystem.Inst.Data.RootFolder, ConfigSystem.Inst.Data.L10NFolder);
            string guidePath = Path.Combine(ConfigSystem.Inst.Data.RootFolder, ConfigSystem.Inst.Data.GuideFolder);

            MainModule.Inst.Init(ConfigSystem.Inst.Data.RootFolder, outPath, csvPath, uePath, l10nPath, guidePath);

            MainModule.Inst.SetPerforce(
                ConfigSystem.Inst.Data.PerforceURL,
                ConfigSystem.Inst.Data.PerforceUser,
                ConfigSystem.Inst.Data.PerforceWork,
                ConfigSystem.Inst.Data.PerforceBranch
            );

            bInited = true;
        }
    }

    public static class IniUtility
    {
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        public static void ClearIniSection(string section, string filePath)
        {
            WritePrivateProfileString(section, null, null, filePath);
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
    struct SContentID
    {
        [FieldOffset(0)]
        public UInt64 Code;

#region Uint32
        [FieldOffset(0)]
        public uint second;
        [FieldOffset(4)]
        public uint first;
#endregion Uint32


        // 
        [FieldOffset(0)]
        public uint KIND;          //	카인드
        [FieldOffset(4)]
        public ushort Variety;       //	종류 (서버카테고리)
        [FieldOffset(6)]
        public ushort Category;
        
    }

    public class HeaderInfo
	{
		public EnumValueType clientType;
		public EnumValueType serverType;
        public string clientTypeName;
        public string serverTypeName;
        public string VarName;
		public EUsedType type;
		public int colNum;
        public string CTFColumnName;
        public bool bUseDataName = false;
        public bool bSkip = false;
        public bool bUseMerge = false;
        public List<int> CTFHeaderIndices = new List<int>();
        public string ResPath;
        public string CTFFormat;
        public string RefColumnName;

        public HeaderInfo(int col) 
		{
			colNum = col;
		}

		public bool IsID()
		{
            if (clientType == EnumValueType.Int32 ||
               clientType == EnumValueType.Double ||
               (clientType != EnumValueType.Undefined && clientTypeName[0] == 'E') )
                return false;

            if (serverType == EnumValueType.Default ||
				serverType >= EnumValueType.ContentID)
                return true;
			return false;
		}

		public string GetServerType()
		{
			return serverType.ToString();
		}

		public string GetClientType()
		{
			switch (clientType)
			{
                case EnumValueType.Default:
				case EnumValueType.String:
					return "FString";
				case EnumValueType.Strings:
					return "TArray<FString>";
				case EnumValueType.ContentID:
				case EnumValueType.ObjID:
					return string.Format("core::{0}", clientType);				
				case EnumValueType.ContentIDs:
                    return string.Format("TArray<core::{0}>", (clientType - 1).ToString());
                case EnumValueType.ObjIDs:
                case EnumValueType.Properties:
                    return "FProperties";
                case EnumValueType.Int32:
                    return "int32";
                case EnumValueType.Int64:
                case EnumValueType.ObjID64:
                    return "int64";
                case EnumValueType.UInt32:
                    return "uint";
                case EnumValueType.Int32s:
					return "TArray<int32>";
				case EnumValueType.UInt32s:
					return "TArray<uint32>";
				case EnumValueType.Int64s:
					return "TArray<int64>";
				case EnumValueType.Bool:
					return "bool";
				case EnumValueType.Bools:
					return "TArray<bool>";				
				case EnumValueType.ObjID64s:
					return "TArray<int64>";			
				case EnumValueType.Vector:
					return "FVector";
				case EnumValueType.Vectors:
					return "TArray<FVector>"; 
				case EnumValueType.Double:
					return "float";
				case EnumValueType.Doubles:
					return "TArray<float>";
				case EnumValueType.Range:
				case EnumValueType.Vector2D:
					return "FVector2D";
                case EnumValueType.Name:
                    return "FName";
			}
			return clientTypeName;
		}

        public string ConvertDBReadType()
        {
            string head = VarName;
            switch (clientType)
            {
               
                case EnumValueType.ContentID:
                case EnumValueType.ObjID:
                    return string.Format("\tcore::{0} {1};" ,clientType, head);
                case EnumValueType.Default:
                case EnumValueType.String:
                    return string.Format("\tFString {0};", head);
                case EnumValueType.Int32s:
                    return string.Format("\tTArray<int> {0};", head);
                case EnumValueType.UInt32s:
                    return string.Format("\tTArray<uint> {0};", head);
                case EnumValueType.Int64s:
                case EnumValueType.ObjID64s:
                    return string.Format("\tTArray<int64> {0};", head);
                case EnumValueType.ContentIDs:
                    return string.Format("\tTArray<core::ContentID> {0};",  head);
                case EnumValueType.Properties:
                case EnumValueType.ObjIDs:
                    return string.Format("\tFProperties {0};",  head) ;
                case EnumValueType.Strings:
                    return string.Format("\tTArray<FString> {0};", head);
                case EnumValueType.Doubles:
                    return string.Format("\tTArray<float> {0};", head);
                case EnumValueType.Vectors:
                    return string.Format("\tTArray<FVector> {0};", head);
                case EnumValueType.Bools:
                    return string.Format("\tTArray<bool> {0};", head);
                case EnumValueType.Int32:
                    return string.Format("\tint32 {0};", head);              
                case EnumValueType.Int64:
                case EnumValueType.ObjID64:
                    return string.Format("\tint64 {0};", head);
                case EnumValueType.UInt32:
                    return string.Format("\tuint {0};", head);
                case EnumValueType.Double:
                    return string.Format("\tfloat {0};", head);
                case EnumValueType.Bool:
                    return string.Format("\tuint32 {0} : 1;", head);
                case EnumValueType.Vector:
                    return string.Format("\tFVector {0};", head);
                case EnumValueType.Range:
                case EnumValueType.Vector2D:
                    return string.Format("\tFVector2D {0};", head);
                case EnumValueType.Name:
                    return string.Format("\tFName {0};", head);
                case EnumValueType.Undefined:
                    {
                        if (clientTypeName.StartsWith("Enum"))
                        {
                            return string.Format("\tE{0} {1};", clientTypeName, head);
                        }
                        return string.Format("\t{0} {1};", clientTypeName, head);
                    }
            }
            return "default";
        }

        public string ConvertSQLTYPE()
        {
            switch (clientType)
            {
                case EnumValueType.ContentID:
                case EnumValueType.String:
                case EnumValueType.ObjID:
                case EnumValueType.Vector:
                case EnumValueType.Range:
                case EnumValueType.Name:
                    return "VARCHAR(128)";
                case EnumValueType.Int64:
                case EnumValueType.ObjID64:
                case EnumValueType.UInt32:
                    return "INTEGER";
                case EnumValueType.Int32:
                    return "TINYINT(4)";
                case EnumValueType.Bool:
                    return "BOOL";
                case EnumValueType.Double:
                    return "FLOAT";
                case EnumValueType.Default: //?
                case EnumValueType.ContentIDs:
                case EnumValueType.ObjIDs:
                case EnumValueType.Doubles:
                case EnumValueType.Vectors:
                case EnumValueType.Int64s:
                case EnumValueType.ObjID64s:
                case EnumValueType.Int32s:
                case EnumValueType.UInt32s:
                case EnumValueType.Bools:
                    return "VARCHAR(256)";
                case EnumValueType.Strings:
                case EnumValueType.Properties:
                    return "TEXT";
            }
            return "VARCHAR(128)";
        }

        public bool HasClient()
		{
            if (type == EUsedType.client)
                return true;
            if (type == EUsedType.Key) 
                return true;
            if (clientType == EnumValueType.Undefined && clientTypeName.Length < 1)
                return false;
            if (type == EUsedType.common)
                return true;           
            if (type == EUsedType.DataName) //tool use
                return true;            
            return false;
		}

		public bool HasServer()
		{
            if (type == EUsedType.memo)
                return false;
            if (type == EUsedType.Key)
                return true;
            if (type == EUsedType.DataName) 
                return false;
            if (type == EUsedType.None)
                return false;
            //if (serverType == EnumValueType.Undefined)
            //	return false;

            return (type != EUsedType.client);
		}

		public bool HasCommon()
		{
            return HasClient() || HasServer();

            //if (type == EUsedType.Key) return true;
            //if (type == EUsedType.memo) return false;
            
            //return (type != EUsedType.client);
        }


    }


}
