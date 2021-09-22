using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace UpfrontEstFileUpload1.Models
{

    public class EnteredDates
    {

        public List<DateTime> InsertedDateList { get; set; }

        public IEnumerable<SelectListItem> DateSelectList { get; set; }


    }

}