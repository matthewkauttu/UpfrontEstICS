using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using UpfrontEstFileUpload1.Controllers;
using UpfrontEstFileUpload1.LocalContext;

namespace UpfrontEstFileUpload1.Models
{
    public class GetInsertedDateList
    {
        public IEnumerable<InsertedDate> GetDateList()
        {
            IEnumerable<InsertedDate> queryResults;

            try
            {

                using (var context1 = new SQL())
                {
                    string insertedDateQuery = "SELECT DISTINCT [Insert Date]" +
                                               "FROM [Upfront_Estimate_ICS].[dbo].[Estimate Details]" +
                                               "ORDER BY [Insert Date] DESC";

                    queryResults = context1.Database.SqlQuery<InsertedDate>(insertedDateQuery);

                }

                return queryResults;

            }
            catch (Exception ex)
            {
                Logging.Write("Get Inserted Dates Query Error:\n" + ex.Message);
            }

            return null;
        }
    }
}