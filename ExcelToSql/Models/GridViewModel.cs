using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExcelToSql.Models
{
    public class dataWithHeader
    {
       public string header;
       public string Value;
    }
    public class GridViewModel
    {
        public List<string> header;
        public List<List<string>> value;
      
    }
}