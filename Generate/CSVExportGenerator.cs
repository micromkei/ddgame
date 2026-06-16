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
    public class CSVExportGenerator : BaseGenerator
    {
        char[] seperators = { ',' };
        char[] removechar = { '\"' };

        public bool MakeDataExport( StreamReader sr)
        {
            CollectInfoSheet();

            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("Data");
            if (ws == null)
                return false;
            
            sb.Clear();
            object[,] values = (object[,])ws.UsedRange.Value2;
            CollectColumName(ref values);
            
            int rowLength = values.GetLength(0); // ws.UsedRange.Rows.Count;
           
            int NumRow = 2;

            string data = sr.ReadLine(); //header
            string[] headers = Regex.Split(data, ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");

            List<int> columList = FindColumeIndices(ref headers);
            
            string[] readline;           
            while ((data = sr.ReadLine()) != null)
            {
                readline = Regex.Split(data, ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
                // readline = data.Split(seperators, StringSplitOptions.None);

                for (int i = 1;  i< headers.Length; ++i) // except index
                {
                    string cellData = readline[i].Trim(removechar);
                    // Excel->CSV 에서 \" -> \"\" 로 변환을 하므로
                    // 여기서는 반대처리를 한다.
                    cellData = cellData.Replace("\"\"", "\"");
                    int columeIdx = columList[i] + 1; // one base

                    ws.Cells[NumRow, columeIdx].Value = cellData;
                }
                ++NumRow;
            }

            if(NumRow < rowLength)
            {
                // CSV 데이터보다 Excel 에 더 많은 데이터가 있다.
                // CSV->Excel 이므로 Excel 에 있는 데이터들을 지우도록 하자.

                for (int i = NumRow; i < rowLength; ++i)
                {
                    ws.Rows[i].Clear();
                }
            }

            return true;
        }

        internal List<int> FindColumeIndices(ref string[] inHeaders)
        {
            List<int> columList = new List<int>();
            foreach ( var head in inHeaders )
            {
                int colNum = ColumnNames.FindIndex(x => x == head);
                columList.Add(colNum);
            }
            return columList;
        }

        private int NextColumeIndex(int beginIndex)
        {
            for(int i=beginIndex-1; i < AllHeaderInfos.Count; ++i)
            {
                var headerInfo = AllHeaderInfos[i];
                if(headerInfo.type == EUsedType.server || headerInfo.type == EUsedType.memo)
                {
                    continue;
                }

                return i+1;
            }

            return -1;
        }

        //(""Base/Attack"",""Base/Attack_2"")
        int MultiValueCheck(string[] readline, int i,ref string cellData)
        {
            string outValue = "";
            for (int k = i; k < readline.Length; ++k)
            {                
                cellData = readline[k];
                               
                outValue += cellData;
                if (cellData[cellData.Length - 2] == ')')
                {
                    i = k;
                    break;
                }
                outValue += ",";
            }
                        
            cellData = outValue.Replace("\"\"", "\"");
            cellData = cellData.TrimStart(removechar);
            return i;
        }
    }


}
