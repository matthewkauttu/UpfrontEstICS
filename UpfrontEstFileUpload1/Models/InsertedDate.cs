using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using UpfrontEstFileUpload1.LocalContext;

namespace UpfrontEstFileUpload1.Models
{
    public class InsertedDate
    {
        [Column("Insert Date")]
        public DateTime EnteredDateList { get; set; }

        public List<DateTime> SelectedList { get; set; }

        public DateTime SelectedDate { get; set; }

        [NotMapped]
        public SelectList SelectList { get; set; }


        public SelectList GetDateList()
        {
            try
            {

                IEnumerable<DateTime> queryResults;

                using (var context1 = new SQL())
                {
                    string insertedDateQuery = "SELECT DISTINCT [Insert Date]" +
                                               "FROM [Upfront_Estimate_ICS].[dbo].[Estimate Details]" +
                                               "ORDER BY [Insert Date] DESC";

                    queryResults = context1.Database.SqlQuery<DateTime>(insertedDateQuery);

                }

                foreach (DateTime result in queryResults)
                {
                    this.EnteredDateList.Append(result);
                }

                this.SelectList = new SelectList(this.EnteredDateList);

                return this.SelectList;

            }
            catch (Exception ex)
            {
                Logging.Write("Get Inserted Dates Query Error:\n" + ex.Message);
            }

            return null;
        }
    }

}