using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;


namespace Mallenom.test
{
    /// <summary>
    /// ОПИСАНИЕ СТРОКИ ДАННЫХ ДЛЯ ТАБЛИЦЫ
    /// </summary>
    public class GridItem
    {
        public int id { get; set; }
        public string plate { get; set; }
        public DateTime time { get; set; }
        public string camera { get; set; }
        public string direction { get; set; }
        public int image { get; set; }
    }

    class WebAPI
    {
        static public string urlAPI = "http://распознаваниеномеров.рф:45555";
        static public List<GridItem> dsGrid = new List<GridItem>();
        static private string _cookies = string.Empty;

        /// <summary>
        /// АВТОРИЗАЦИЯ И ПОЛУЧЕНИЕ COOKIE
        /// </summary>
        static public bool Autorize(string userName, string userPass)
        {
            bool ret = false;

            try
            {
                System.Net.WebClient ExchangeWC = new System.Net.WebClient();
                ExchangeWC.Headers.Add("Accept", "application/json");
                ExchangeWC.Headers.Add("Content-Type", "application/json");
                var Results = ExchangeWC.UploadString(new System.Uri(urlAPI + "/login"), "{username: '" + userName + "', password: '" + userPass + "', isRememberMe: false}");
                _cookies = ExchangeWC.ResponseHeaders["Set-Cookie"];
                JObject objRet = JsonConvert.DeserializeObject<JObject>(Results);

                ret = (bool)objRet["isAutorized"]; 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return (ret);
        }

        /// <summary>
        /// ПОЛУЧЕНИЕ ДАННЫХ ПО ЗАДАННОМУ ПЕРИОДУ
        /// </summary>
        static public void GetFromPeriod(DateTime startDT, DateTime stopDT)
        {
            // ОЧИСТКА СТАРЫХ ДАННЫХ
            dsGrid.Clear();

            string postData = "Count:30, Offset:{0}, Plate:\"\", Videochannels:[], ServerGuid:\"\", Status:\"recognized\", Direction:\"\", From:\"{1}\", To:\"{2}\"";
            string s_startDT = new DateTime(startDT.Ticks, DateTimeKind.Utc).ToString("o");
            string s_stopDT = new DateTime(stopDT.Ticks, DateTimeKind.Utc).ToString("o");

            // ЧТЕНИЕ ПО 30 ЗАПИСЕЙ, ПОКА НЕ ЗАКОНЧАТСЯ
            int pos = 0;
            int retCount = 30;
            while (retCount == 30)
            {
                retCount = GetPreparedData("{" + String.Format(postData, pos, s_startDT, s_stopDT) + "}");
                pos += retCount;
            }
        }

        /// <summary>
        /// ЧТЕНИЕ ОДНОЙ ПОРЦИИ ИЗ API
        /// </summary>
        static private int GetPreparedData(string json)
        {
            int ret = 0;

            try
            {
                System.Net.WebClient ExchangeWC = new System.Net.WebClient();
                ExchangeWC.Headers.Add("Accept", "application/json");
                ExchangeWC.Headers.Add("Content-Type", "application/json");
                ExchangeWC.Headers.Add("Cookie", _cookies);
                var Results = ExchangeWC.UploadString(new System.Uri(urlAPI + "/api/v1/vehicles"), json);
                JArray objRet = (JArray)JsonConvert.DeserializeObject<JObject>(Results)["entries"];

                ret = objRet.Count;
                for (int i = 0; i < objRet.Count; i++)
                {
                    dsGrid.Add(new GridItem
                    {
                        id = (int)objRet[i]["id"],
                        camera = Converter(objRet[i]["videoChannel"]["name"].ToString(), Encoding.UTF8, Encoding.Default),
                        plate = objRet[i]["plate"].ToString(),
                        direction = Converter(objRet[i]["directionName"].ToString(), Encoding.UTF8, Encoding.Default),
                        time = (DateTime)objRet[i]["timestamp"],
                        image = (int)objRet[i]["vehicleImages"]["mainImage"]
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return (ret);
        }

        /// <summary>
        /// ПОЛУЧЕНИЕ ПОТОКА ДЛЯ ИЗОБРАЖЕНИЯ
        /// </summary>
        static public MemoryStream GetPhoto(int id)
        {
            MemoryStream ret;

            try
            {
                System.Net.WebClient ExchangeWC = new System.Net.WebClient();
                ExchangeWC.Headers.Add("Cookie", _cookies);
                ret = new MemoryStream(ExchangeWC.DownloadData(new System.Uri(urlAPI + "/api/v1/vehicle/image?entryid=" + id.ToString())));
            }
            catch (Exception ex)
            {
                ret = new MemoryStream();
            }

            return (ret);
        }

        /// <summary>
        /// UTF8 -> WIN1251
        /// </summary>
        public static string Converter(string value, Encoding src, Encoding trg) //функция
        {
            Decoder dec = src.GetDecoder();
            byte[] ba = trg.GetBytes(value);
            int len = dec.GetCharCount(ba, 0, ba.Length);
            char[] ca = new char[len];
            dec.GetChars(ba, 0, ba.Length, ca, 0);
            return new string(ca);
        }
    }
}
