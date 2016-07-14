using System;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;
using AECSentinel.Properties;
using System.Diagnostics;

namespace csw_lib
{

    /// <summary>
    /// CSW LOGIN
    /// </summary>
    public class Login
    {
        public string token { get; set; }
    }

    /// <summary>
    /// CSW CITY
    /// </summary>
    public class City
    {
        public int city_id { get; set; }
        public string city_code { get; set; }
        public string city_description { get; set; }
    }

    /// <summary>
    /// CSW SWITCHBOARD
    /// </summary>
    public class Switchboard
    {
        public int swb_id { get; set; }
        public string swb_code { get; set; }
        public string swb_description { get; set; }
    }

    /// <summary>
    /// CSW SWITCHBOARD DASHBOARD
    /// </summary>
    public class SwitchboardStatus
    {
        public int swb_status { get; set; }
        public string swb_measure_upd { get; set; }
        public string swb_last_on { get; set; }
        public string swb_last_off { get; set; }
        public string swb_theor_on { get; set; }
        public string swb_theor_off { get; set; }
        public string swb_energy { get; set; }
        public float swb_maxpw_l2 { get; set; }
        public float swb_maxpw_l3 { get; set; }
        public int swb_lamp_status { get; set; }
        public string swb_committed_pwr { get; set; }
        public float swb_lamppwr_sum { get; set; }
        public float swb_lamplosses_sum { get; set; }

    }

    /// <summary>
    /// CSW LAMP
    /// </summary>
    public class Luminaire
    {
        public string element { get; set; }
        public int element_id { get; set; }
        public string element_code { get; set; }
        public string element_description { get; set; }
        public int lamp_id { get; set; }
    }

    /// <summary>
    /// CSW LAMPS STATUS
    /// </summary>
    public class LuminaireStatus
    {
        public int lamp_id { get; set; }
        public string lamp_code { get; set; }
        public string lamp_description { get; set; }
        public int lamp_status { get; set; }
        public string lamp_upd_date { get; set; }
    }
    


    /// <summary>
    /// Implement methods to retrieve/update data from CSW webservice
    /// </summary>
    public class csw_connector
    {
        /// <summary>
        /// Retrieve the token string to access the other methods
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns>Token String</returns>
        public string csw_login(string user, string password, string URL)
        {
            string tk;

            try
            {
                RestClient client;
                RestRequest request;
                client = new RestClient(Settings.Default.ws_URL);
                request = new RestRequest();

                request.RequestFormat = RestSharp.DataFormat.Json;
                request.Resource = "/login.json";
                request.Method = Method.GET;

                request.AddParameter("username", user); // adds to POST or URL querystring based on Method
                request.AddParameter("password", password); // adds to POST or URL querystring based on Method

                IRestResponse response = client.Execute(request);

                if ((response.Content == "") || (response.Content == "[]"))
                    tk = "";
                else
                    tk = JsonConvert.DeserializeObject<Login>(response.Content.Trim().Trim('[', ']')).token;

                Debug.WriteLine("login:" + tk);
                return tk;
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message.ToString());
                Debug.WriteLine("login-"+ex.Message.ToString());
                throw;
                //return "";
                
            }


        }

        /// <summary>
       /// CSW_CITIES - Retrieve the city list of an user
       /// </summary>
       /// <param name="token">The Token String</param>
       /// <returns>The list of cities</returns>
        public List<City> csw_cities(string token)
        {
            
                List<City> citylist = new List<City>();

                try
                {
                    RestClient client;
                    RestRequest request;
                    client = new RestClient(Settings.Default.ws_URL);
                    request = new RestRequest();

                    request.RequestFormat = RestSharp.DataFormat.Json;
                    request.Resource = "/cities.json";
                    request.Method = Method.GET;
                    
                    request.AddParameter("token", token);

                    IRestResponse response = client.Execute(request);

                    if ((response.Content == "") || (response.Content == "[]"))
                            citylist = null;
                    else
                        citylist = JsonConvert.DeserializeObject<List<City>>(response.Content);
                    
                    foreach (City c in citylist)
                    {
                        Debug.WriteLine("city_id:" + c.city_id);
                        Debug.WriteLine("city_code:" + c.city_code);
                        Debug.WriteLine("city_description:" + c.city_description);
                    }

                    return citylist;
                }
                catch (Exception ex)
                {

                    //MessageBox.Show(ex.Message.ToString());
                    Debug.WriteLine("cities-"+ex.Message.ToString());
                    throw;
                //    return null;

            }
        }

        /// <summary>
        /// Retrieve the switchboards list in the same city
        /// </summary>
        /// <param name="token">Token String</param>
        /// <param name="city_id">The ID of the city</param>
        /// <returns>The list of the switchboards</returns>
        public List<Switchboard> csw_switchboards(string token, int city_id)
        {
            
                List<Switchboard> swblist = new List<Switchboard>();

                try
                {
                    RestClient client;
                    RestRequest request;
                    client = new RestClient(Settings.Default.ws_URL);
                    request = new RestRequest();

                    request.RequestFormat = RestSharp.DataFormat.Json;
                    request.Resource = "cities/" + city_id + "/switchboards.json";
                    request.Method = Method.GET;

                    request.AddParameter("token", token);

                    IRestResponse response = client.Execute(request);
               
                    if ((response.Content == "") || (response.Content == "[]"))
                        swblist = null;
                    else
                        swblist = JsonConvert.DeserializeObject<List<Switchboard>>(response.Content);

                    foreach (Switchboard c in swblist)
                    {
                        Debug.WriteLine("swb_id:" + c.swb_id);
                        Debug.WriteLine("swb_code:" + c.swb_code);
                        Debug.WriteLine("swb_description:" + c.swb_description);
                    }

                    return swblist;
                }
                catch (Exception ex)
                {

                    //MessageBox.Show(ex.Message.ToString());
                    Debug.WriteLine("switchboard-"+ex.Message.ToString());
                    throw;
                    //return null;
                    
            }
        }

        /// <summary>
        /// Retrieve the status and the most important data of specific swichboard 
        /// </summary>
        /// <param name="token">Token String</param>
        /// <param name="switchboard_id">The ID of the switchboard</param>
        /// <returns>the switchboard status</returns>
        public List<SwitchboardStatus> csw_switchboardStatus(string token, int switchboard_id)
        {

            List<SwitchboardStatus> swbStatuslist = new List<SwitchboardStatus>();

            try
            {
                RestClient client;
                RestRequest request;
                client = new RestClient(Settings.Default.ws_URL);
                request = new RestRequest();

                request.RequestFormat = RestSharp.DataFormat.Json;
                request.Resource = "switchboards/" + switchboard_id + "/dashboard.json";
                request.Method = Method.GET;

                request.AddParameter("token", token);

                IRestResponse response = client.Execute(request);

                if ((response.Content == "") || (response.Content == "[]"))
                {
                    swbStatuslist = null;
                }
                else
                {
                    swbStatuslist = JsonConvert.DeserializeObject<List<SwitchboardStatus>>(response.Content);
                    foreach (SwitchboardStatus c in swbStatuslist)
                    {
                        Debug.WriteLine("swb_id:" + switchboard_id);
                        Debug.WriteLine("swb_status:" + c.swb_status);
                    }
                }
           
                return swbStatuslist;
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message.ToString());
                
                Debug.WriteLine("swbstatus-" + ex.Message.ToString());

                return null;

            }

            
        }
        
        /// <summary>
        /// Retrieve the list of the luminaries under the same switchboard
        /// </summary>
        /// <param name="token">The Token string</param>
        /// <param name="swb_id">The switchboard ID</param>
        /// <returns>The luminaries list</returns>
        public List<Luminaire> csw_luminaries(string token, int swb_id)
        {

            List<Luminaire> lumlist = new List<Luminaire>();

            try
            {
                RestClient client;
                RestRequest request;
                client = new RestClient(Settings.Default.ws_URL);
                request = new RestRequest();

                request.RequestFormat = RestSharp.DataFormat.Json;
                request.Resource = "switchboards/" + swb_id + "/lamps.json.1000";
                request.Method = Method.GET;

                request.AddParameter("token", token);

                IRestResponse response = client.Execute(request);

                if ((response.Content == "") || (response.Content == "[]"))
                    lumlist = null;
                else
                    lumlist = JsonConvert.DeserializeObject<List<Luminaire>>(response.Content);

                foreach (Luminaire c in lumlist)
                {
                    Debug.WriteLine("element:" + c.element);
                    Debug.WriteLine("element_id:" + c.element_id);
                    Debug.WriteLine("element_code:" + c.element_code);
                    Debug.WriteLine("element_description:" + c.element_description);
                    Debug.WriteLine("lamp_id:" + c.lamp_id);
                }

                return lumlist;
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message.ToString());
                Debug.WriteLine("luminaries-"+ex.Message.ToString());
                throw;
                //return null;
                
            }
        }

        /// <summary>
        /// Retrieve the list of the luminaries under the same switchboard and their status
        /// </summary>
        /// <param name="token">The Token string</param>
        /// <param name="swb_id">The switchboard ID</param>
        /// <returns>The luminaries list</returns>
        public List<LuminaireStatus> csw_luminaries_status(string token, int swb_id)
        {

            List<LuminaireStatus> lumstatuslist = new List<LuminaireStatus>();

            try
            {
                RestClient client;
                RestRequest request;
                client = new RestClient(Settings.Default.ws_URL);
                request = new RestRequest();

                request.RequestFormat = RestSharp.DataFormat.Json;
                request.Resource = "switchboards/" + swb_id + "/lampstatus.json.1000";
                request.Method = Method.GET;

                request.AddParameter("token", token);

                IRestResponse response = client.Execute(request);

                if ((response.Content == "") || (response.Content == "[]"))
                    lumstatuslist = null;
                else
                    lumstatuslist = JsonConvert.DeserializeObject<List<LuminaireStatus>>(response.Content);

                foreach (LuminaireStatus c in lumstatuslist)
                {
                    Console.WriteLine("lamp_id:" + c.lamp_id);
                    Console.WriteLine("lamp_code:" + c.lamp_code);
                    Console.WriteLine("lamp_description:" + c.lamp_description);
                    Console.WriteLine("lamp_status:" + c.lamp_status);
                    Console.WriteLine("lamp_upd_date:" + c.lamp_upd_date);
                }

                return lumstatuslist;
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message.ToString());
                Console.WriteLine("lumstatus-"+ex.Message.ToString());
                throw;
                //return null;
                
            }
        }
    }

}
