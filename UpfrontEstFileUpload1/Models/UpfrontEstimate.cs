using LinqToExcel.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace UpfrontEstFileUpload1.Models
{
    public class UpfrontEstimate
    {
        public string Facility { get; set; }

        [ExcelColumn("Patient Account #")]
        public string PatientAccount { get; set; }

        [ExcelColumn("Date of Service")]
        public string DateofService { get; set; }

        [ExcelColumn("Created By")]
        public string CreatedBy { get; set; }

        [ExcelColumn("Date Created")]
        public string DateCreated { get; set; }

        [ExcelColumn("Co-Insurance Amt Owed")]
        public string CoInsuranceAmtOwed { get; set; }

        [ExcelColumn("Co-Pay Amt Owed")]
        public string CoPayAmtOwed { get; set; }

        [ExcelColumn("Deductible Amt Owed")]
        public string DeductibleAmtOwed { get; set; }

        [ExcelColumn("Total Est Patient Amount")]
        public string TotalEstPatientAmount { get; set; }
    }
}