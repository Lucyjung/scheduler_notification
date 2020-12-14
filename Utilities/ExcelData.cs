using ExcelDataReader;
using System;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using Excel = Microsoft.Office.Interop.Excel;
using ClosedXML.Excel;

namespace ScheduleNoti.Utilities
{
    class ExcelData
    {
        static Excel.Application xlApp;
        static Excel.Workbook xlBook;
        static Excel.Range xlRange;
        public static Excel.Worksheet xlSheet;
        public static DataTable readInputFile(string filePath, int sheetIndex, ref string sheetName)
        {
            DataTable dt;
            xlApp = new Excel.Application();
            xlBook = xlApp.Workbooks.Open(filePath);
            xlSheet = xlBook.Sheets[sheetIndex];
            sheetName = xlSheet.Name;

            xlSheet.UsedRange.Copy();
            dt = GetSheetDataAsDataTable();
            xlBook.Close();
            xlApp.Quit();
            Marshal.FinalReleaseComObject(xlBook);
            xlBook = null;
            Marshal.FinalReleaseComObject(xlApp);
            xlApp = null;
            return dt;
        }
        public static DataTable readData(string filePath, int sheetIndex=0)
        {
            using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                // Auto-detect format, supports:
                //  - Binary Excel files (2.0-2003 format; *.xls)
                //  - OpenXml Excel files (2007 format; *.xlsx)
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    // Choose one of either 1 or 2:

                    // 1. Use the reader methods
                    do
                    {
                        while (reader.Read())
                        {
                            // reader.GetDouble(0);
                        }
                    } while (reader.NextResult());

                    // 2. Use the AsDataSet extension method
                    var result = reader.AsDataSet();

                    // The result of each spreadsheet is in result.Tables
                    var dt = result.Tables[sheetIndex];

                    foreach (DataColumn column in dt.Columns)
                    {
                        string cName = dt.Rows[0][column.ColumnName].ToString();
                        if (!dt.Columns.Contains(cName) && cName != "")
                        {
                            column.ColumnName = cName;
                        }

                    }
                    dt.Rows.Remove(dt.Rows[0]);
                    return dt;
                }
            }
        }
        public static int getNumSheets(string filePath)
        {
            int num = 0;
            xlApp = new Excel.Application();
            xlBook = xlApp.Workbooks.Open(filePath);
            num = xlBook.Sheets.Count;
            xlBook.Close();
            xlApp.Quit();
            Marshal.FinalReleaseComObject(xlBook);
            xlBook = null;
            Marshal.FinalReleaseComObject(xlApp);
            xlApp = null;
            return num;
        }
        private static DataTable GetSheetDataAsDataTable()
        {
            DataTable dt = new DataTable();
            try
            {
                xlRange = xlSheet.UsedRange;
                DataRow row = null;
                for (int i = 1; i <= xlRange.Rows.Count; i++)
                {
                    if (i != 1)
                        row = dt.NewRow();
                    for (int j = 1; j <= xlRange.Columns.Count; j++)
                    {
                        if (i == 1)
                            dt.Columns.Add(xlRange.Cells[1, j].value);
                        else
                            row[j - 1] = xlRange.Cells[i, j].value;
                    }
                    if (row != null)
                        dt.Rows.Add(row);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return dt;
        }
        public static void saveTableToExcel(string outputFile, string worksheetName, DataTable dt)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(dt, worksheetName);
                int i = 1;
                foreach (DataColumn dc in dt.Columns)
                {
                    worksheet.Cell(1, i).Value = dc.ColumnName;
                    i++;
                }
                worksheet.Columns(1, i).AdjustToContents();
                workbook.SaveAs(outputFile);
            }
        }
    }
}
