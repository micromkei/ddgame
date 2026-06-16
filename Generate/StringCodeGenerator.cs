using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using Excel = Microsoft.Office.Interop.Excel;
using System.Text.RegularExpressions;
using System.ComponentModel;


namespace ExcelConvertLib
{
    public class StringCodeGenerator : BaseGenerator
    {
        private HashSet<string> _CollectKeyList = new HashSet<string>();

        static string GetPathAndCreateDirectory(string path)
        {
            string dir = Path.GetDirectoryName(path);
            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }
            return path;
        }

        static public string GetL10NOutPathAndCreateDirectory(string fileName)
        {
            string output = MainModule.Inst.GetL10NPath(fileName);
            return GetPathAndCreateDirectory(output);
        }

        static public string GetL10NOutPathAndCreateDirectory(string fileName, string extName)
        {
            string output = MainModule.Inst.GetL10NPath(fileName, extName);
            return GetPathAndCreateDirectory(output);
        }

        static public string GetL10NEnumCodePathAndCreateDirectory(string fileName)
        {
            string output = MainModule.Inst.GetOutPath(@"../ProjectG_Client/Games/ProjectG/Source/TACTAN/Public/", fileName, "h");
            return GetPathAndCreateDirectory(output);
        }

        void GenerateEnumCode(Excel.Worksheet ws, StreamWriter outputWriter)
        {
            if (ws == null)
            {
                return;
            }
            outputWriter.WriteLine("\t// {0} sheets", ws.Name);

            object[,] values = (object[,])ws.UsedRange.Value2;
            int max = values.GetLength(0);
            int NumRow = CommonDefines.StartStringRowNum;
            while (NumRow <= max)
            {
                string strValue = Convert.ToString(values[NumRow, 1]);
                if (string.IsNullOrEmpty(strValue) || strValue[0] == ';')
                {
                    if (strValue != null && strValue == ";end")
                    {
                        break;
                    }

                    ++NumRow;
                    continue;
                }

                outputWriter.Write(string.Format("\t{0},", strValue));               
                outputWriter.WriteLine();
                ++NumRow;
            }

            outputWriter.WriteLine();
            outputWriter.WriteLine();

            LogSystem.Inst.Add(string.Format("Generated EnumCode :{0} ", ws.Name));
        }

        public bool MakeCodeGen()
        {
            string OutfileName = "EnumStringTable";
            string output = StringCodeGenerator.GetL10NEnumCodePathAndCreateDirectory(OutfileName);
            if (!MainModule.CheckOutClientCodeFile(OutfileName + ".h"))
            {
                return false;
            }
            // MainModule.CheckOutFilePath(output);
            
            StreamWriter writer = new StreamWriter(
                     new FileStream(output, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
            
            writer.WriteLine("//Generated From Convert Tools");
            writer.WriteLine();
            writer.WriteLine("#pragma once");
            writer.WriteLine("#include \"CoreMinimal.h\"");
            writer.WriteLine("#include \"EnumStringTable.generated.h\"");
            writer.WriteLine();
            var selectedExcels = ConfigSystem.Inst.StringFiles;
            foreach (var filename in selectedExcels)
            {
                string tableName = filename.FileName.Split(new char[] { '.' })[0];
                if (tableName.Contains("string_error"))
                {
                    if (MainModule.Inst.ReadExcel(CommonDefines.StringFolder, filename))
                    {
                        Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("localization");
                        string className = MainModule.Inst.GetClassName(ws);
                        OnGenerateCode(className,writer);
                        MainModule.Inst.CloseWorkBook();
                    }
                }
            }
            writer.Flush();
            writer.Close();
            return true;
        }

        public bool OnGenerateCode(string className, StreamWriter writer)
        {
            MainModule.Inst.HasError = false;
            Excel.Worksheet ws = MainModule.Inst.GetWorkSheet("localization");
            if (ws != null)
            {
                writer.WriteLine("UENUM()");
                writer.WriteLine( string.Format("enum class E{0} : int32",className) );
                writer.WriteLine("{");

                object[,] values = (object[,])ws.UsedRange.Value2;
                GenerateEnumCode(ws, writer);

                writer.WriteLine("};");
            }
            writer.WriteLine();
            return !MainModule.Inst.HasError;
        }

        public bool MakeStringData( string locale, IProgress<double> progress)
        {
            string fileName = "string_table";
            string output = GetL10NOutPathAndCreateDirectory(Path.GetFileNameWithoutExtension(fileName));
            if ( MainModule.CheckOutFilePath(output) == false )
            {
                return false;
            }

            UELocalizationTableGenerator stringconvert = new UELocalizationTableGenerator();
            stringconvert.InitCollectKey();

            StreamWriter writer = new StreamWriter(
                     new FileStream(output, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);
                        
            // L10N Header
            writer.WriteLine("Key,SourceString,Comment");
            var selectedExcels = ConfigSystem.Inst.StringFiles;
            int total = selectedExcels.Count;
            int i = 1;
            foreach (var filename in selectedExcels)
            {
                if (MainModule.Inst.ReadExcel(CommonDefines.StringFolder, filename))
                {
                    string tableName = filename.FileName.Split(new char[] { '.' })[0];
                    bool bSystemMessage = false;
                    if (tableName.Contains("string_message"))
                    {
                        bSystemMessage = true;
                        stringconvert.ExtractSystemMessage(progress);                       
                    }
                    stringconvert.MakeDataConvert(writer, locale , bSystemMessage);
                    MainModule.Inst.CloseWorkBook();

                    progress.Report((float)i / total); ++i;
                }
            }
            writer.Flush();
            writer.Close();
            LogSystem.Inst.Add("=== string_message ===");
            return false;
        }

        public bool MakeStringDataByLocale(string locale, IProgress<double> progress)
        {
            string fileName = "string." + locale + ".csv";
            string output = MainModule.Inst.GetLocalizePath(fileName);
            MainModule.CheckOutFilePath(output);

            UELocalizationTableGenerator stringconvert = new UELocalizationTableGenerator();
            stringconvert.InitCollectKey();

            StreamWriter writer = new StreamWriter(
                     new FileStream(output, FileMode.Create, FileAccess.ReadWrite), Encoding.UTF8);

            // L10N Header
            writer.WriteLine("Key,SourceString");
            var selectedExcels = ConfigSystem.Inst.StringFiles;
            int total = selectedExcels.Count;
            int i = 1;
            foreach (var filename in selectedExcels)
            {
                if (MainModule.Inst.ReadExcel(CommonDefines.StringFolder, filename))
                {                    
                    string tableName = filename.FileName.Split(new char[] { '.' })[0];
                    bool MessageFlag = string.Compare(tableName, "string_message") == 0 ? true : false;
                    stringconvert.MakeDataConvert(writer, locale  , MessageFlag);
                    MainModule.Inst.CloseWorkBook();

                    progress.Report((float)i/ total); ++i;
                }
            }
            writer.Flush();
            writer.Close();
            return false;
        }

        public bool MakeStringDataServer(IProgress<double> progress)
        {         
            CTFDataConvert ctfconvert = new CTFDataConvert();
            ServerEnumGenerator serverEnumGenerator = new ServerEnumGenerator();
           
            var selectedExcels = ConfigSystem.Inst.StringFiles;

            foreach (var filename in selectedExcels)
            {
                string tableName = filename.FileName.Split(new char[] { '.' })[0];
                if (tableName.Contains("string_message"))
                {
                    if (MainModule.Inst.ReadExcel(CommonDefines.StringFolder, filename))
                    {
                        ctfconvert.ExtractSystemMessage(progress);
                        MainModule.Inst.CloseWorkBook();
                    }                    
                }
                else if (tableName.Contains("string_error_protocol"))
                {
                    if (MainModule.Inst.ReadExcel(CommonDefines.StringFolder, filename))
                    {
                        serverEnumGenerator.MakeProtocolErrorEnum(progress);
                        MainModule.Inst.CloseWorkBook();
                    }
                }
            }
            return false;
        }
    }
}
