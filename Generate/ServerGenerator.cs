using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;


namespace ExcelConvertLib
{
    // CTFDLL 로 로드해서 변환한다.
    // 서버 header 파일을 excel -> ctf-> header 코드로 변환 되도록 수정
    public class ServerGenerator : BaseGenerator
    {
        public ServerGenerator()
        {

        }

        [DllImport("CTFExport.dll")]
        private static extern bool CTFExport(string inputFilePath, string serverFilePath, string clientFilePath);
        

        public bool MakeServerCode(string FileName, string inputFilePath, string serverFilePath, string clientFilePath)
        {
            string HeaderFileName = "common_" + FileName + ".h";

            if (false == MainModule.CheckOutServerCTFHeaderFile(HeaderFileName))
            { 
                return false;
            }

            var curDir = Environment.CurrentDirectory;

			try
            {
				if (false == CTFExport(inputFilePath, serverFilePath, clientFilePath))
				{
					LogSystem.Inst.Add($"CTFExport Error");
					return false;
				}
			}
			catch (DllNotFoundException e)
            {
				LogSystem.Inst.Add($"Exception: DLL Not found! {e.Message}");
				return false;
			}
            catch( Exception e )
            {
				LogSystem.Inst.Add($"Exception: {e.Message}");
				return false;
			}

			// 생성된 파일의 로그를 남기기 위함
			var outputFullPath = Path.GetFullPath(serverFilePath);      // 아웃 풀 패스 

            LogSystem.Inst.Add($"Make ServerCode :{outputFullPath}/{HeaderFileName}");

            return true;

        }
    }
}
