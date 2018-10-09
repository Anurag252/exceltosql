using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using ExcelDataReader.Core;
using ExcelToSql.Models;
using System.Data;

namespace ExcelToSql.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [ValidateAntiForgeryToken]
        public ActionResult Read(HttpPostedFileBase excelFile)
        {
            GridViewModel g = null;
            try
            {
                string path = Path.Combine(Server.MapPath("~/Excel"),
                                           Path.GetFileName(excelFile.FileName));
                excelFile.SaveAs(path);
                g = readExcelFile(path);
                ViewBag.Message = "Excel file was read successfully";

            }
            catch (Exception ex)
            {
                ViewBag.Message = "Following error occured while reading the file " + ex.Message;
            }

            return View("Index", g);
        }

        private static GridViewModel populateViewModel(IExcelDataReader reader)
        {



            return new GridViewModel()
            {
                header = new List<string>()
                {

                    "head1","head2","head3","head3","head3","head3","head3","head3","head3","head3","head3","head3","head3","head3","head3","head3","head3"
                },
                value = new List<List<string>>()
                {
                   new List<string>()
                   {
                       "test1","test2","test3"
                   },

                   new List<string>()
                   {
                       "test1","test2","test3"
                   },

                   new List<string>()
                   {
                       "test1","test2","test3"
                   }
                }
            };
        }

        private GridViewModel readExcelFile(string filePath)
        {
            GridViewModel grdiViewModel = new GridViewModel();
            DataTable dtProductCatalog = null;
            List<List<string>> rowvalue = new List<List<string>>();
            using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
            {

               
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                     dtProductCatalog = reader.AsDataSet().Tables[0];

                    Session["dataTable"] = dtProductCatalog;
                    do
                    {
                        while (reader.Read())
                        {
                            
                            try
                            {
                                List<string> cell = new List<string>();
                                for (int i = 0; i < reader.FieldCount; i++)
                                {                                   
                                    cell.Add(reader[i] != null ? reader[i].ToString() : "");
                                }
                                rowvalue.Add(cell);
                            }
                            catch (Exception ex)
                            {

                            }
                           
                           
                        }
                    } while (reader.NextResult());

                }
            }

           
            grdiViewModel.value = rowvalue;
            grdiViewModel.header = grdiViewModel.value[0];
            grdiViewModel.value.RemoveAt(0);
            return grdiViewModel;

        }
}
}