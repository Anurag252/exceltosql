using ExcelToSql.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ExcelToSql.Controllers
{
    public class ConfigureDBController : Controller
    {
        string _connecString;
       
        // GET: ConfigureDB
        public ActionResult Index()
        {
            return View();
        }


        public static string DynamicConnectionString(string dbName)
        {
            //Data Source = pun - anuragd02\SQLEXPRESS; Integrated Security = True
            //data Source = pun - anuragd02\SQLEXPRESS; Initial Catalog = DB_Alstom; Integrated Security = True
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
            builder.DataSource = dbName;
           // builder.InitialCatalog = "DB_Alstom";           
           
            builder.IntegratedSecurity = true;
            
            return builder.ConnectionString.ToString();
        }

        //data source = (LocalDb)\MSSQLLocalDB;initial catalog = ExcelToSql.GridViewModelDB; integrated security = True; MultipleActiveResultSets=True;App=EntityFramework" providerName="System.Data.SqlClient"
        [HttpPost]
        public ActionResult Connect(string dbName)
        {
            DBConfigureViewModel viewModel = new DBConfigureViewModel();
            viewModel.ConnectionAttempted = true;

            _connecString = DynamicConnectionString(dbName);
            TempData["connectionString"] = _connecString;
            using (SqlConnection sqlconn = new SqlConnection(_connecString))
            {
                try
                {
                    sqlconn.Open();
                    //Will be closed automatically once sqlconn is disposed
                    //sqlconn.Close();

                    
                    viewModel.messageOnConnection = "Connection to database was successful";
                    
                }
                catch(Exception ex)
                {
                    viewModel.messageOnConnection = ex.Message;
                    
                   
                }

            }
            return View("Index", viewModel);


            //DataTable t = (DataTable)Session["dataTable"];
            //dbWrite(conneString,t);
            //return View("Index");
        }

        private  bool CheckDatabaseExists( string databaseName)
        {
           
            string sqlCreateDBQuery;
            bool result = false;
            SqlConnection tmpConn;

            try
            {
                tmpConn = new SqlConnection(_connecString);

                sqlCreateDBQuery = string.Format("SELECT database_id FROM sys.databases WHERE Name   = '{0}'", databaseName);
        
        using (tmpConn)
                {
                    using (SqlCommand sqlCmd = new SqlCommand(sqlCreateDBQuery, tmpConn))
                    {
                        tmpConn.Open();

                        object resultObj = sqlCmd.ExecuteScalar();

                        int databaseID = 0;

                        if (resultObj != null)
                        {
                            int.TryParse(resultObj.ToString(), out databaseID);
                        }

                        tmpConn.Close();

                        result = (databaseID > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                result = false;
            }

            return result;
        }

        [HttpPost]
        public ActionResult pushToDb(string dbName , string tblName)
        {
            DBConfigureViewModel viewModel = new DBConfigureViewModel();
            _connecString = (string)TempData["connectionString"];
            TempData.Keep();


            using (SqlConnection sqlconn = new SqlConnection(_connecString))
            {
                try
                {
                    if (CheckDatabaseExists(dbName) == false)
                    {
                        sqlconn.Open();
                        using (SqlCommand sql = new SqlCommand())
                        {
                            try
                            {
                                sql.CommandText = "CREATE database @dbname";
                                sql.Parameters.Add(new SqlParameter("@dbname", dbName));
                                sql.Connection = sqlconn;
                                sql.ExecuteNonQuery();
                                viewModel.messageOnConnection = "Created a database with name" + dbName;
                            }
                            catch (Exception ex)
                            {
                                viewModel.messageOnConnection = ex.Message;
                            }
                        }
                    }
                    else
                    {
                        viewModel.messageOnConnection = "Found an exisitng databse with name as " + dbName;
                    }
                    //Will be closed automatically once sqlconn is disposed
                    //sqlconn.Close();


                    

                }
                catch (Exception ex)
                {
                    viewModel.messageOnConnection = ex.Message;


                }

            }
            return View("Index", viewModel);


            //DataTable t = (DataTable)Session["dataTable"];
            //dbWrite(conneString,t);
            //return View("Index");
        }
        

        public static string CreateTABLE(string tableName, DataTable table)
        {
            string sqlsc;
            sqlsc = "CREATE TABLE " + tableName + "(";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                sqlsc += "\n [" + table.Columns[i].ColumnName + "] ";
                string columnType = table.Columns[i].DataType.ToString();
                switch (columnType)
                {
                    case "System.Int32":
                        sqlsc += " int ";
                        break;
                    case "System.Int64":
                        sqlsc += " bigint ";
                        break;
                    case "System.Int16":
                        sqlsc += " smallint";
                        break;
                    case "System.Byte":
                        sqlsc += " tinyint";
                        break;
                    case "System.Decimal":
                        sqlsc += " decimal ";
                        break;
                    case "System.DateTime":
                        sqlsc += " datetime ";
                        break;
                    case "System.String":
                    default:
                        sqlsc += string.Format(" nvarchar({0}) ", table.Columns[i].MaxLength == -1 ? "max" : table.Columns[i].MaxLength.ToString());
                        break;
                }
                if (table.Columns[i].AutoIncrement)
                    sqlsc += " IDENTITY(" + table.Columns[i].AutoIncrementSeed.ToString() + "," + table.Columns[i].AutoIncrementStep.ToString() + ") ";
                if (!table.Columns[i].AllowDBNull)
                    sqlsc += " NOT NULL ";
                sqlsc += ",";
            }
            return sqlsc.Substring(0, sqlsc.Length - 1) + "\n)";
        }

        private void dbWrite(string conneString, DataTable t)
        {
            try
            {


                using (SqlDataAdapter da = new SqlDataAdapter())
                {
                    var c = CreateTABLE("mytable", t);

                    da.SelectCommand = new SqlCommand();
                    da.SelectCommand.CommandText = c;
                    da.SelectCommand.Connection = new SqlConnection(conneString);
                    da.Fill(t.DataSet);
                    using (SqlBulkCopy bulkCopy =
                          new SqlBulkCopy(conneString))
                    {
                        bulkCopy.DestinationTableName =
                            "mytable";

                        try
                        {
                            using (var reader = t.CreateDataReader())
                            {
                                // Write from the source to the destination.
                                bulkCopy.WriteToServer(reader);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }
            
        }
    }
}