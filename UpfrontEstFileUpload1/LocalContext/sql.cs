using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace UpfrontEstFileUpload1.LocalContext
{
    public class SQL : DbContext
    {
        public SQL() : base("DBContext_Local")
        {

        }
    }
}