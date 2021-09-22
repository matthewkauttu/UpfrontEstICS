using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace UpfrontEstFileUpload1.Models
{
    public class EnteredDate
    {
        [Column("Insert Date")]
        public DateTime InsertedDate { get; set; }
    }
}