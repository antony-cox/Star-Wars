using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using HtmlAgilityPack;

namespace Star_Wars_Checker
{
    class Program
    {
        private static string knownHours = "19:30";

        static void Main(string[] args)
        {
            Console.Write("Set check interval in seconds: ");
            int timeout = int.Parse(Console.ReadLine()) * 1000;

            while (true)
            {
                log("Checking ...");
                checkTimes("https://kinepolis.be/nl/films/star-wars-episode-vii-force-awakens?date=2015-12-19&theater=49#showtimes", "19/12/2015");
                Thread.Sleep(timeout);
            }
        }

        private static void checkTimes(string url, string date)
        {
            //GOTO WEBSITE
            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            //GET MOVIE VERSIONS
            HtmlNodeCollection info = doc.DocumentNode.SelectNodes("//div[@class='movie-version-info-extra']");
            HtmlNodeCollection program = doc.DocumentNode.SelectNodes("//div[@class='programs']");
            HtmlNodeCollection zalen = doc.DocumentNode.SelectNodes("//div[@class='screen-title']");

            //REMOVE 3D SCREENS
            int i = 0;
            foreach (HtmlNode movieInfo in info)
            {
                if (movieInfo.InnerText.ToLower().Contains("3d"))
                {
                    program.RemoveAt(i);
                    zalen.RemoveAt(i);
                }
                i++;
            }

            i = 0;
           
            //LIST ROOMS + HOURS
            foreach(HtmlNode zaal in zalen)
            {
                HtmlNode prog = program[i];
                string uren = "";
                bool alert = false;
                foreach(HtmlNode uur in prog.ChildNodes)
                {
                    if(uur.Name.Equals("div"))
                    {
                        foreach(HtmlAttribute a in uur.Attributes)
                        {
                            if(a.Value.Contains("upcoming"))
                            {
                                string trimmedUur = uur.InnerText.Trim();
                                uren += trimmedUur + " ";

                                if (!knownHours.Contains(trimmedUur))
                                {
                                    alert = true;
                                    knownHours += trimmedUur;
                                }
                            }
                        }
                    }
                }

                string msg = date + " - " + zaal.InnerText.Trim() + ": " + uren;
                log(msg);
                if(alert)
                {
                    alertUser(msg, "");
                }
                i++;  
            }
        }

        private static void alertUser(string body, string email)
        {
            String apiKey = "";
            String type = "note", title = "Star Wars";
            byte[] data = Encoding.ASCII.GetBytes(String.Format("{{ \"email\": \"{0}\", \"type\": \"{1}\", \"title\": \"{2}\", \"body\": \"{3}\" }}", email, type, title, body));

            var request = System.Net.WebRequest.Create("https://api.pushbullet.com/v2/pushes") as System.Net.HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/json";
            request.Credentials = new System.Net.NetworkCredential(apiKey, "");

            request.ContentLength = data.Length;
            String responseJson = null;

            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();
            }

            using (var response = request.GetResponse() as System.Net.HttpWebResponse)
            {
                using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
                {
                    responseJson = reader.ReadToEnd();
                }
            }
        }

        private static void log(string msg)
        {
            Console.WriteLine(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " - " + msg);
        }
    }
}
