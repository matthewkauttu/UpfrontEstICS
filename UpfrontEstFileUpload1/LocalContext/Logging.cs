using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace UpfrontEstFileUpload1.LocalContext
{
    public class Logging
    {
        public static void Write(string message)
        {
            var generator = DateTime.Today.ToString("MM_dd_yyyy");

            //path of file
            var path = @"\\hca\jrsc\Transfers\PDF\Log" + generator + ".txt";
            using (StreamWriter sw = new StreamWriter(path, append: true))
            {

                sw.WriteLine(message);

            }
        }

    }
}