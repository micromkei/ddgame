using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;

namespace ExcelConvertLib
{
    public class ValidCheckGenerator 
    {        
        public static bool IsValidCheck(List<HeaderInfo> AllHeaderInfos,ref object[,] values)
        {
            int max = values.GetLength(0);
            if (max == 0 || AllHeaderInfos.Count == 0)
                return false;
            int NumRow = CommonDefines.StartRowNum;
            int colMax = values.GetLength(1);
            bool bResult = true;
            while (NumRow <= max)
            {
                foreach (var header in AllHeaderInfos)
                {
                    int col = header.colNum + 1;
                    if (colMax < col || col == 0)
                    {
                        continue;
                    }   
                    string value = Convert.ToString(values[NumRow, col]).TrimEnd();

                    if (col == 1)
                    {
                        if (value.Length < 1 || value[0] == ';') //skip line
                            continue;
                    }

                    if (header.clientTypeName.Length > 1 && header.clientTypeName.Substring(0, 4) == "Enum")
                    {
                        string enumTable = header.clientTypeName;
                        if (value.Length > 1 && header.HasClient() )
                        {
                            if (!MainModule.Inst.IsEnumCheck(enumTable, value))
                            {
                                LogSystem.Inst.Add(string.Format("Line {0} {1}: [{2}] = {3}", NumRow + CommonDefines.StartRowNum, header.VarName, enumTable, value), ELOGType.Error);
                                bResult = false;
                            }
                        }
                    }
                    else if (header.clientType == EnumValueType.ContentID && !header.bUseDataName)
                    {
                        value = ContentIDChecker.Inst.ConvertAliasString(value, header);
                        if ( !Utility.IsContentIDCheck(value) )
                        {
                            LogSystem.Inst.Add(string.Format("Line {0} {1}: [{2}]  contentID Error", NumRow + CommonDefines.StartRowNum, header.VarName, value), ELOGType.Error);
                            bResult = false;
                        }
                    }
                }
                ++NumRow;
            }
            return bResult;
        }

    }
}
