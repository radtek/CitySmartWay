using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Data.SqlClient;

namespace pingService.Controllers
{
    public class PingController : ApiController
    {

        List<bool> ping_result;
        string DataSource = "AECW8VM001\\AECW8VM001";
        string InitialCatalog = "AEC_DBUT";
        string UserID = "dmzuser2";
        string Password = "dmzuser2";
        

        // GET: api/Ping
        public List<bool> Get()
        {
            getsql();
            return ping_result;
        }

        // GET: api/Ping/5
        public string Get(int id)
        {
            string rtn = "";
            bool ping_alarm = false;

            getsql();

            switch (id)
            {
                case 0:
                        for (int i = 0; i < ping_result.Count; i++)
                        ping_alarm = ping_alarm || (bool)ping_result[i];

                        if (ping_alarm)
                            rtn = "PING ALARM";
                        else
                            rtn = "PING OK";
                    
                        break;

                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7:
                case 8:
                case 9:
                case 10:
                        rtn = "Panel " + (id) + ":" + ping_result[id - 1];
                    break;

                default:
                        rtn = "Invalid Panel";

                        break;
            }

            return rtn;
        }

        // POST: api/Ping
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/Ping/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/Ping/5
        public void Delete(int id)
        {
        }


        private void getsql()
        {
            ping_result = new List<bool>();

            string con_str = "Data Source=" + DataSource +
                            ";Initial Catalog=" + InitialCatalog +
                            ";User ID=" + UserID +
                            ";Password=" + Password + ";";

            string cmd_str = "SELECT STATUS_BOOL FROM PING_PANEL_LIST";

            using (SqlConnection con = new SqlConnection(con_str))
            {

                con.Open();
                
                SqlCommand cmd = new SqlCommand(cmd_str, con);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            ping_result.Add((bool)reader.GetValue(i));
                        }
                        ;
                    }
                }

                con.Close();

            }
        }
    }
}
