using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using csw_lib;

namespace csw_wstest
{
    class Program
    {

        static void Main(string[] args)
        {
            csw_connector connector= new csw_connector();

            string token;
            List<City> citylist;
            List<Switchboard> swblist;
            List<Luminaire> lumlist;
            List<LuminaireStatus> lumstatuslist;
            
            //Legge il token
            token = connector.csw_login(Properties.Settings.Default.ws_USER, Properties.Settings.Default.ws_PWD);
            Console.WriteLine();

            //Legge la CitiesList
            citylist = connector.csw_cities(token);
            Console.WriteLine();

            //Legge la switchboardslist
            swblist = connector.csw_switchboards(token, citylist[0].city_id);
            Console.WriteLine();

            //Legge la luminarieslist
            lumlist = connector.csw_luminaries(token, swblist[0].swb_id);
            Console.WriteLine();

            //Legge lo stato dei luminaries
            //lumstatuslist = connector.csw_lumstatus(token, swblist[0].swb_id);
            //Console.WriteLine();
        }
    }
}
