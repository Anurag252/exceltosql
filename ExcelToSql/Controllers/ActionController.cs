using ExcelToSql.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExcelToSql.Controllers
{
    public class ActionController : Controller
    {
        // GET: Action
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult executeProcedure(string procString)
        {
            try
            {
                string connectionString = (string)TempData["connectionString"];
                TempData.Keep();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand sqlcmd = new SqlCommand(procString, conn))
                    {
                        sqlcmd.CommandType = System.Data.CommandType.StoredProcedure;
                        sqlcmd.ExecuteNonQuery();
                    }
                }
            }
            catch(Exception ex)
            {

            }
                return View();
        }

        [HttpPost]
        public JsonResult selectQuery(string selectCommands)
        {
            try
            {

                string connectionString = (string)TempData["connectionString"];
                TempData.Keep();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand sqlcmd = new SqlCommand(selectCommands, conn))
                    {
                        sqlcmd.CommandType = System.Data.CommandType.Text;
                        conn.Open();
                        SqlDataReader reader = sqlcmd.ExecuteReader();
                        List<List<string>> rowvalue = new List<List<string>>();
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


                        var json = JsonConvert.SerializeObject(rowvalue);
                        return new JsonResult()
                        {
                            Data = json
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                return new JsonResult()
                {
                    Data = "{\"error\" :\"something did not work\" }"
                };
            }

        }
        public ActionResult selectCommands(string selectCommands)
        {
            ActionViewModel actionViewModel = new ActionViewModel();
            GridViewModel gridViewModel = new GridViewModel();
            try
            {
              
                string connectionString = (string)TempData["connectionString"];
                TempData.Keep();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand sqlcmd = new SqlCommand(selectCommands, conn))
                    {
                        sqlcmd.CommandType = System.Data.CommandType.Text;
                        conn.Open();
                        SqlDataReader reader = sqlcmd.ExecuteReader();

                        List<List<string>> rowvalue = new List<List<string>>();

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
                        gridViewModel.value = rowvalue;
                        gridViewModel.header = gridViewModel.value[0];
                        gridViewModel.value.RemoveAt(0);
                        actionViewModel.gridViewModel = gridViewModel;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return View("Index" , actionViewModel);
        }

        public ActionResult executeNonQuery(string executeNonQuery)
        {
            try
            {
                string connectionString = (string)TempData["connectionString"];
                TempData.Keep();
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    using (SqlCommand sqlcmd = new SqlCommand(executeNonQuery, conn))
                    {
                        sqlcmd.CommandType = System.Data.CommandType.StoredProcedure;
                        sqlcmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return View();
        }
    }
}