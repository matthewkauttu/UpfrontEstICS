using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Validation;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UpfrontEstFileUpload1.LocalContext;
using UpfrontEstFileUpload1.Models;
using static System.Net.Mime.MediaTypeNames;

namespace UpfrontEstFileUpload1.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {

            using (var context1 = new SQL())
            {


                string insertedDateQuery = "SELECT DISTINCT [Insert Date]" +
                                           "FROM [Upfront_Estimate_ICS].[dbo].[Estimate Details]" +
                                           "ORDER BY [Insert Date] DESC";

                var data = context1.Database.SqlQuery<DateTime>(insertedDateQuery).ToList();

                var model = new EnteredDates
                {
                    InsertedDateList = data,
                    DateSelectList = GetDateSelectList(data)
                };

                return View(model);

            }

        }
        
        [HttpPost]
        public ActionResult Index(HttpPostedFileBase FileUpload, EnteredDates model)
        {

            ImportUploadToDB(FileUpload);

            using (var context1 = new SQL())
            {
                string insertedDateQuery = "SELECT DISTINCT [Insert Date]" +
                                           "FROM [Upfront_Estimate_ICS].[dbo].[Estimate Details]" +
                                           "ORDER BY [Insert Date] DESC";

                var data = context1.Database.SqlQuery<DateTime>(insertedDateQuery).ToList();

                model.InsertedDateList = data;
                model.DateSelectList = GetDateSelectList(data);

                return View(model);

            }



        }


        #region InsertExcelDataToDB

        private void ImportUploadToDB(HttpPostedFileBase fileUpload)
        {
            string targetFilename = "a.xlsx";
            string targetpath = @"\\hca\jrsc\Transfers\PDF\";
            string stagingFileAbsolutePath = targetpath + targetFilename;
            fileUpload.SaveAs(stagingFileAbsolutePath);

            var connectionString = "";
            if (targetFilename.EndsWith(".xls"))
            {
                connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", stagingFileAbsolutePath);
                //connectionString = string.Format("Provider=Microsoft.Jet.OLEDB.4.0; data source={0}; Extended Properties=Excel 8.0;", filename);
            }
            else if (targetFilename.EndsWith(".xlsx"))
            {
                connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", stagingFileAbsolutePath);
                //connectionString = string.Format("Provider=Microsoft.ACE.OLEDB.12.0;Data Source={0};Extended Properties=\"Excel 12.0 Xml;HDR=YES;IMEX=1\";", filename);
            }

            string sheetName = null;

            try
            {
                sheetName = SelectCorrectSheetName(targetpath, targetFilename, connectionString);

                if (sheetName == null)
                {
                    throw new NullReferenceException("There was a problem obtaining the correct sheet name from the staging spreadsheet.");
                }
            }
            catch (NullReferenceException ex)
            {
                Logging.Write(ex.Message);
            }


            var adapter = new OleDbDataAdapter("SELECT * FROM [" + sheetName + "]", connectionString);
            var ds = new DataSet();
            adapter.Fill(ds, "ExcelTable");
            DataTable dtable = ds.Tables["ExcelTable"];
            var excelFile = new ExcelQueryFactory(stagingFileAbsolutePath);
            var UpfrontEstimateList = from a in excelFile.Worksheet<UpfrontEstimate>(sheetName.TrimEnd('$')) select a;

            foreach (var a in UpfrontEstimateList)
            {
                try
                {
                    if (a.Facility != "" && a.PatientAccount != "")
                    {
                        InsertUpfrontEstimate(a);

                    }

                }
                catch (DbEntityValidationException ex)
                {
                    Logging.Write(ex.Message);
                    foreach (var entityValidationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in entityValidationErrors.ValidationErrors)
                        {
                            Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                        }
                    }
                }
            }

            ////Delete temp file
            //if ((System.IO.File.Exists(targetpath + filename)))
            //{
            //    System.IO.File.Delete(targetpath + filename);
            //}

            // Move temp file to archive and rename
            if ((System.IO.File.Exists(stagingFileAbsolutePath)))
            {
                var srcFile = stagingFileAbsolutePath;
                var destFile = string.Concat(
                                        @"\\hca\jrsc\Shared\1_WebProcess\Upfront Estimate ICS\Estimate Spreadsheet Archive\"
                                        ,$"{DateTime.Now:MM-dd-yyyy_HH-mm-ss}"
                                        ,".xlsx"
                                        );
                System.IO.File.Move(srcFile, destFile);
            }

            try
            {
                // Execute stored procedure to move data from Staging table to Estimate Details table
                ExecuteStagingSP();
            }
            catch (DbEntityValidationException ex)
            {
                Logging.Write(ex.Message);
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        Response.Write("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
        }

        private string SelectCorrectSheetName(string targetPath, string targetFilename, string connectionString)
        {
            
            OleDbConnection objConn = null;
            System.Data.DataTable dt = null;

            try
            {
                
                objConn = new OleDbConnection(connectionString);

                // Open connection with the database.
                objConn.Open();

                // Get the data table containg the schema guid.
                dt = objConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                if (dt == null)
                {
                    return null;
                }

                String[] excelSheets = new String[dt.Rows.Count];
                int i = 0;

                // Add the sheet name to the string array.
                foreach (DataRow row in dt.Rows)
                {
                    excelSheets[i] = row["TABLE_NAME"].ToString();
                    i++;
                }
                
                

                // Loop through all of the sheets if you want to
                for (int j = 0; j < excelSheets.Length; j++)
                {
                    OleDbDataAdapter dbAdapter = new OleDbDataAdapter("SELECT top 10 * FROM [" + excelSheets[j] + "]", connectionString);
                    DataTable worksheet = new DataTable();
                    dbAdapter.Fill(worksheet);

                    List<string> columnNames = new List<string>();

                    foreach (DataColumn col in worksheet.Columns)
                    {
                        columnNames.Add(col.ColumnName);
                    }

                    List<string> correctColumnNames = new List<string>
                                                            {
                                                                "Facility",
                                                                "Patient Account #",
                                                                "Date of Service",
                                                                "Created By",
                                                                "Date Created",
                                                                "Co-Insurance Amt Owed",
                                                                "Co-Pay Amt Owed",
                                                                "Deductible Amt Owed",
                                                                "Total Est Patient Amount"
                                                            };

                    if (CheckEquality(columnNames, correctColumnNames))
                    {
                        return excelSheets[j];
                    }

                }

                return null;

            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                // Clean up.
                if (objConn != null)
                {
                    objConn.Close();
                    objConn.Dispose();
                }
                if (dt != null)
                {
                    dt.Dispose();
                }
            }

        }

        private bool CheckEquality(List<string> columnNames, List<string> correctColumnNames)
        {
            if (columnNames.Count != correctColumnNames.Count)
                return false;

            for (int i = 0; i < columnNames.Count; i++)
            {
                if (columnNames[i] != correctColumnNames[i])
                    return false;
            }

            return true;
        }

        private void ExecuteStagingSP()
        {
            try
            {
                using (var context1 = new SQL())
                {
                    var a = context1.Database.ExecuteSqlCommand("EXEC [Upfront_Estimate_ICS].[dbo].[sp_StagingToMain]");
                }
            }
            catch (Exception ex)
            {
                Logging.Write(ex.Message);
            }
        }


        private void InsertUpfrontEstimate(UpfrontEstimate a)
        {
            int count = 0;

            if (a.PatientAccount != null)
            {
                try
                {

                    using (var context1 = new SQL())
                    {

                        SqlParameter Facility = new SqlParameter("@Facility", a.Facility);
                        SqlParameter PatientAccount = new SqlParameter("@PatientAccount", a.PatientAccount);
                        SqlParameter DateofService = new SqlParameter("@DateofService", a.DateofService);
                        SqlParameter CreatedBy = new SqlParameter("@CreatedBy", a.CreatedBy);
                        SqlParameter DateCreated = new SqlParameter("@DateCreated", a.DateCreated);
                        SqlParameter CoInsuranceAmtOwed = new SqlParameter("@CoInsuranceAmtOwed", a.CoInsuranceAmtOwed);
                        SqlParameter CoPayAmtOwed = new SqlParameter("@CoPayAmtOwed", a.CoPayAmtOwed);
                        SqlParameter DeductibleAmtOwed = new SqlParameter("@DeductibleAmtOwed", a.DeductibleAmtOwed);
                        SqlParameter TotalEstPatientAmount = new SqlParameter("@TotalEstPatientAmount", a.TotalEstPatientAmount);

                        context1.Database.ExecuteSqlCommand(@"
                                                        INSERT INTO [Upfront_Estimate_ICS].[dbo].[Estimate Details Staging]
                                                        ([Facility]
                                                        ,[Patient Account #]
                                                        ,[Date of Service]
                                                        ,[Created By]
                                                        ,[Date Created]
                                                        ,[Co-Insurance Amt Owed]
                                                        ,[Co-Pay Amt Owed]
                                                        ,[Deductible Amt Owed]
                                                        ,[Total Est Patient Amount]
                                                        )
                                                        VALUES
                                                        (

                                                        @Facility,
                                                        @PatientAccount,
                                                        @DateofService,
                                                        @CreatedBy,
                                                        @DateCreated,
                                                        @CoInsuranceAmtOwed,
                                                        @CoPayAmtOwed,
                                                        @DeductibleAmtOwed,
                                                        @TotalEstPatientAmount
                                                        )

                                                        ",
                        Facility, PatientAccount, DateofService, CreatedBy, DateCreated, CoInsuranceAmtOwed, CoPayAmtOwed, DeductibleAmtOwed, TotalEstPatientAmount);
                        count = count + 1;
                    }


                }
                catch (Exception ex)
                {
                    Logging.Write(ex.Message);
                }
            }

            
        }

        #endregion

        public List<SelectListItem> GetDateSelectList(List<DateTime> queryResultList)
        {

            List<SelectListItem> items = new List<SelectListItem>();

            for (var i = 0; i < queryResultList.Count; i++)
            {
                var currentDate = queryResultList[i];

                items.Add(new SelectListItem { Text = queryResultList[i].ToString(), Value = (i + 1).ToString() });
            }

            return items;

        }

    }
}