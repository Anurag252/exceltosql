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
            builder.DataSource = dbName + @"\SQLEXPRESS";
            builder.InitialCatalog = "DB_Alstom";           
           
            builder.IntegratedSecurity = true;
            
            return builder.ConnectionString.ToString();
        }

        //data source = (LocalDb)\MSSQLLocalDB;initial catalog = ExcelToSql.GridViewModelDB; integrated security = True; MultipleActiveResultSets=True;App=EntityFramework" providerName="System.Data.SqlClient"
        [HttpPost]
        public ActionResult Connect(string dbName)
        {
            string conneString = DynamicConnectionString(dbName);
            DataTable t = (DataTable)Session["dataTable"];
            dbWrite(conneString,t);
            return View("Index");
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