using System;
using System.Collections.Generic;
using RestSharp;
using Newtonsoft.Json;
using AECSentinel.Properties;


namespace csw_lib
{

    public class Login
    {
        public string token { get; set; }
    }

    public class City
    {
        public int city_id { get; set; }
        public string city_code { get; set; }
        public string city_description { get; set; }
    }

    public class CityList
    {
        public List<City> Cities { get; set; }
        
    }

    public class Switchboard
    {
        public int swb_id { get; set; }
        public string swb_code { get; set; }
        public string swb_description { get; set; }
    }

    public class Luminaire
    {
        public string element { get; set; }
        public int element_id { get; set; }
        public string element_code { get; set; }
        public string element_description { get; set; }
        public int lamp_id { get; set; }
    }

    public class LuminaireStatus
    {
        public int lamp_id { get; set; }
        public string lamp_code { get; set; }
        public string lamp_description { get; set; }
        public int lamp_status { get; set; }
        public string lamp_upd_date { get; set; }
    }
    



    public class csw_connector
    {
        public string csw_login(string user, string password)
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

                Console.WriteLine("login:" + tk);
                return tk;
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message.ToString());
                Console.WriteLine(ex.Message.ToString());
                return "";
            }


        }

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
                        Console.WriteLine("city_id:" + c.city_id);
                        Console.WriteLine("city_code:" + c.city_code);
                        Console.WriteLine("city_description:" + c.city_description);
                    }

                    return citylist;
                }
                catch (Exception ex)
                {

                    //MessageBox.Show(ex.Message.ToString());
                    Console.WriteLine(ex.Message.ToString());
                    return null;
                }
            }

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
                        Console.WriteLine("swb_id:" + c.swb_id);
                        Console.WriteLine("swb_code:" + c.swb_code);
                        Console.WriteLine("swb_description:" + c.swb_description);
                    }

                    return swblist;
                }
                catch (Exception ex)
                {

                    //MessageBox.Show(ex.Message.ToString());
                    Console.WriteLine(ex.Message.ToString());
                    return null;
                }
            }

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
                    Console.WriteLine("element:" + c.element);
                    Console.WriteLine("element_id:" + c.element_id);
                    Console.WriteLine("element_code:" + c.element_code);
                    Console.WriteLine("element_description:" + c.element_description);
                    Console.WriteLine("lamp_id:" + c.lamp_id);
                }

                return lumlist;
            }
            catch (Exception ex)
            {

                //MessageBox.Show(ex.Message.ToString());
                Console.WriteLine(ex.Message.ToString());
                return null;
            }
        }

        public List<LuminaireStatus> csw_lumstatus(string token, int swb_id)
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
                Console.WriteLine(ex.Message.ToString());
                return null;
            }
        }
    }

}
