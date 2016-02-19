using GarageOpener.Classes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace GarageOpener.Controllers
{
    public class GarageOpennerController : Controller
    {
        public ActionResult Alive()
        {
            return View("~/views/garageopenner/ipaddress.cshtml", new GarageOpener.Classes.LoginModel(""));
        }

        [HttpPost]
        public ActionResult CheckIP(LoginModel model)
        {
            if (model != null && model.User != null && model.Parola != null && model.User.ToLower() == "stoichincatalin@gmail.com" && model.Parola.ToLower() == "mycoolstuff1982")
            {
                return View("~/views/garageopenner/ipaddress.cshtml", new GarageOpener.Classes.LoginModel(getSetting("RaspiIP").Replace("http://", "").Replace(":", "")));
            }
            else
            {
                string ip = "";
                try
                {
                    ip = GetUserIP();
                    saveHacker(ip,model);
                }
                catch(Exception ex)
                {
                    ip = "exeption";
                    if (ex != null)
                    {

                    }
                }
                return View("~/views/garageopenner/hacker.cshtml", new GarageOpener.Classes.LoginModel(ip));
            }

        }

        private string GetUserIP()
        {
            string ipList = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipList))
            {
                return ipList.Split(',')[0];
            }

            return Request.ServerVariables["REMOTE_ADDR"].ToString();
        }

        public bool UpdateazaAdresa(string ip)
        {
            bool returnValue = false;
            string ipDatabase = "";
            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();
            try
            {
                //string ip = GetPublicIP();

                IPAddress ipA;
                returnValue = IPAddress.TryParse(ip, out ipA);

                if (returnValue)
                {
                    //Update IP Address
                    ipDatabase = getSetting("RaspiIP");
                    if (!ipDatabase.Contains(ipA.MapToIPv4().ToString()))
                    {
                        returnValue = updateSetting("RaspiIP", ipA.MapToIPv4().ToString());
                        //Update Database IP

                    }
                }

            }
            catch (Exception e)
            {
                if (e != null)
                {

                }
            }

            return returnValue;
        }

        public static string GetPublicIP()
        {
            string url = "http://checkip.dyndns.org";
            System.Net.WebRequest req = System.Net.WebRequest.Create(url);
            System.Net.WebResponse resp = req.GetResponse();
            System.IO.StreamReader sr = new System.IO.StreamReader(resp.GetResponseStream());
            string response = sr.ReadToEnd().Trim();
            string[] a = response.Split(':');
            string a2 = a[1].Substring(1);
            string[] a3 = a2.Split('<');
            string a4 = a3[0];
            return a4;
        }

        // GET: GarageOpenner/Create
        [HttpPost]
        public JsonResult CheckLogin(String value1, String value2)
        {
            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();
            int userID = -1;
            LoginResponse returnValue = new LoginResponse();

            IQueryable<int> temp = from data in context.Users
                                   where data.Email.ToUpper() == value1.ToUpper() && data.Password.ToUpper() == value2.ToUpper()
                                   select data.ID;
            try
            {
                userID = temp.FirstOrDefault<int>();
            }
            catch (Exception ex)
            {
                returnValue.Value = ex.Message;
            }

            if (userID > 0)
                returnValue = getSession(userID);

            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        // GET: GarageOpenner/Create
        [HttpPost]
        public JsonResult GetStatus(String value1)
        {
            CheckStatusResponse returnValue = new CheckStatusResponse();
            System.Threading.Thread.Sleep(1000);
            returnValue = GetStatusRaspberry(value1);
            System.Threading.Thread.Sleep(1000);
            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        // GET: GarageOpenner/Create
        [HttpPost]
        public JsonResult Close(String value1)
        {
            CheckStatusResponse returnValue = new CheckStatusResponse();
            StatusRaspiResponse call = new StatusRaspiResponse();
            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();
            Database.User usr = new Database.User();

            usr = getUser(value1);

            if (usr != null && usr.ID > 0)
            {
                call = Helpers.SerializeHelper.StreamToObjectJSON<StatusRaspiResponse>(WebRequestRaspiPost(getSetting("RaspiIP") + getSetting("RaspiPort"), new StringBuilder().Append("value1=").Append(usr.Email.ToLower()).Append("&value2=").Append(usr.Password.ToLower()).Append("&value3=close").ToString()));

                if (call.Response.ToUpper() == "Bad Data".ToUpper())
                {
                    returnValue.NotLoggedIn = true;
                }
                else if (call.Value == "0")
                {
                    //fully closed all systems correct
                    returnValue.Valid = false;
                    returnValue.StatusDoor = 0;
                    returnValue.Value = "Door is already closed";
                }
                else if (call.Value == "1")
                {
                    //fully openeed all systems correct
                    returnValue.Valid = true;
                    returnValue.StatusDoor = 0;
                }
                else if (call.Value == "2")
                {
                    //call overwrite
                    OverwriteDoor(value1);
                    //sleep for 7 sec so the door either closes or opens
                    System.Threading.Thread.Sleep(7000);
                    //check status again - if closed return valid else call open again
                    returnValue = GetStatusRaspberry(value1);
                    if (returnValue.Valid)
                    {
                        if (returnValue.StatusDoor == 1)
                        {
                            //call close again
                            OverwriteDoor(value1);
                            System.Threading.Thread.Sleep(7000);
                            returnValue = GetStatusRaspberry(value1);
                        }
                    }
                }
            }
            else
            {
                returnValue.NotLoggedIn = true;
            }

            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        // GET: GarageOpenner/Create
        [HttpGet]
        public JsonResult Open(String value1)
        {
            CheckStatusResponse returnValue = new CheckStatusResponse();
            StatusRaspiResponse call = new StatusRaspiResponse();
            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();
            Database.User usr = new Database.User();

            usr = getUser(value1);

            if (usr != null && usr.ID > 0)
            {
                call = Helpers.SerializeHelper.StreamToObjectJSON<StatusRaspiResponse>(WebRequestRaspiPost(getSetting("RaspiIP") + getSetting("RaspiPort"), new StringBuilder().Append("value1=").Append(usr.Email.ToLower()).Append("&value2=").Append(usr.Password.ToLower()).Append("&value3=open").ToString()));
                System.Threading.Thread.Sleep(5000);
                if (call.Response.ToUpper() == "Bad Data".ToUpper())
                {
                    returnValue.NotLoggedIn = true;
                }
                else if (call.Value == "0")
                {
                    //fully closed all systems correct
                    returnValue.Valid = false;
                    returnValue.StatusDoor = 0;
                    returnValue.Value = "Door is already openned.";
                }
                else if (call.Value == "1")
                {
                    //fully openeed all systems correct
                    returnValue.Valid = true;
                    returnValue.StatusDoor = 1;
                }
                else if (call.Value == "2")
                {
                    //call overwrite
                    OverwriteDoor(value1);
                    //sleep for 7 sec so the door either closes or opens
                    System.Threading.Thread.Sleep(7000);
                    //check status again - if closed return valid else call open again
                    returnValue = GetStatusRaspberry(value1);
                    if (returnValue.Valid)
                    {
                        if (returnValue.StatusDoor == 0)
                        {
                            //call close again
                            OverwriteDoor(value1);
                            System.Threading.Thread.Sleep(7000);
                            returnValue = GetStatusRaspberry(value1);
                        }
                    }

                }
            }
            else
            {
                returnValue.NotLoggedIn = true;
            }

            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }

        // GET: GarageOpenner/Create
        [HttpPost]
        public JsonResult OverWrite(String value1)
        {
            CheckStatusResponse returnValue = new CheckStatusResponse();

            returnValue = OverwriteDoor(value1);

            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }


        // GET: GarageOpenner/Create
        [HttpGet]
        public JsonResult GetStatusTest(String value1)
        {
            CheckStatusResponse returnValue = new CheckStatusResponse();
            System.Threading.Thread.Sleep(1000);
            returnValue = GetStatusRaspberry(value1);
            System.Threading.Thread.Sleep(1000);
            return this.Json(returnValue, JsonRequestBehavior.AllowGet);
        }


        #region "Support Methods"
        private CheckStatusResponse OverwriteDoor(String value1)
        {
            CheckStatusResponse returnValue = new CheckStatusResponse();
            StatusRaspiResponse call = new StatusRaspiResponse();
            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();
            Database.User usr = new Database.User();

            usr = getUser(value1);

            if (usr != null && usr.ID > 0)
            {
                call = Helpers.SerializeHelper.StreamToObjectJSON<StatusRaspiResponse>(WebRequestRaspiPost(getSetting("RaspiIP") + getSetting("RaspiPort"), new StringBuilder().Append("value1=").Append(usr.Email.ToLower()).Append("&value2=").Append(usr.Password.ToLower()).Append("&value3=overwrite").ToString()));
                System.Threading.Thread.Sleep(3000);
                if (call.Response.ToUpper() == "Bad Data".ToUpper())
                {
                    returnValue.NotLoggedIn = true;
                }
                else if (call.Value == "1")
                {
                    //fully openeed all systems correct
                    returnValue.Valid = true;
                }
            }
            else
            {
                returnValue.NotLoggedIn = true;
            }

            return returnValue;
        }


        private CheckStatusResponse GetStatusRaspberry(String value1)
        {
            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();
            Database.User usr = new Database.User();
            StatusRaspiResponse call = new StatusRaspiResponse();
            CheckStatusResponse returnValue = new CheckStatusResponse();

            usr = getUser(value1);
            if (usr != null && usr.ID > 0)
            {
                call = Helpers.SerializeHelper.StreamToObjectJSON<StatusRaspiResponse>(WebRequestRaspiPost(getSetting("RaspiIP") + getSetting("RaspiPort"), new StringBuilder().Append("value1=").Append(usr.Email.ToLower()).Append("&value2=").Append(usr.Password.ToLower()).Append("&value3=status").ToString()));
                if (call.Response.ToUpper() == "Bad Data".ToUpper())
                {
                    returnValue.NotLoggedIn = true;
                }
                else if (call.StatusLower == "1")
                {
                    //fully closed all systems correct
                    returnValue.Valid = true;
                    returnValue.StatusDoor = 0;
                }
                else if (call.StatusLower == "0")
                {
                    //fully openeed all systems correct
                    returnValue.Valid = true;
                    returnValue.StatusDoor = 1;
                }
                else
                {
                    returnValue.Valid = true;
                    returnValue.StatusDoor = 3;

                    //if ((call.StatusLower == "1" && call.StatusUpper == "1"))
                    //{
                    //    returnValue.Valid = true;
                    //    returnValue.StatusDoor = 3;
                    //}
                    //else if (call.StatusLower == "0" && call.StatusUpper == "0")
                    //{
                    //    returnValue.Valid = true;
                    //    returnValue.StatusDoor = 2;
                    //}
                }
            }
            else
            {
                returnValue.NotLoggedIn = true;
            }

            return returnValue;
        }

        private Database.User getUser(string sessionID)
        {
            Database.User ReturnValue = new Database.User();

            int userID = -1;

            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();

            IQueryable<int> temp = from data in context.Sessions
                                   where data.ID.ToString() == sessionID
                                   select data.UserID;

            try
            {
                userID = temp.FirstOrDefault();

                if (userID > 0)
                {
                    ReturnValue = (from data in context.Users
                                   where data.ID == userID
                                   select data).FirstOrDefault();

                    if (ReturnValue == null)
                    {
                        ReturnValue = new Database.User();
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != null)
                {

                }
            }

            return ReturnValue;
        }

        private string getSetting(string settingName)
        {

            string ReturnValue = "";
            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();

            IQueryable<string> temp = from data in context.Settings
                                      where data.Name.ToUpper() == settingName.ToUpper()
                                      select data.Description;

            try
            {
                ReturnValue = temp.FirstOrDefault();

                if (ReturnValue == null || ReturnValue.Length <= 0)
                {
                    ReturnValue = "";
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != null)
                {
                }
                ReturnValue = "";
            }

            return ReturnValue;
        }

        private Stream WebRequestRaspiPost(string url, string postData)
        {
            try
            {
                WebRequest request = WebRequest.Create(url);
                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                request.ContentType = "application/x-www-form-urlencoded";

                byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                // StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                // responseFromServer = reader.ReadToEnd();

                //System.Threading.Thread.Sleep(7000);
                return dataStream;
            }
            catch (Exception ex)
            {
                if (ex != null)
                {

                }
                // responseFromServer = ex.Message;
            }
            finally
            {

            }
            return null;
        }

        private StatusRaspiResponse getRaspberryStatus(int userid)
        {
            StatusRaspiResponse ReturnValue = new StatusRaspiResponse();
            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();

            Database.User usr = new Database.User();

            IQueryable<Database.User> temp = from data in context.Users
                                             where data.ID == userid
                                             select data;

            try
            {
                usr = temp.FirstOrDefault();
                if (usr != null && usr.ID > 0)
                {

                }
            }
            catch (Exception ex)
            {
                if (ex.Message != null)
                    ReturnValue.Status = "";
            }

            return ReturnValue;
        }

        private int isLoggedIn(string sessionID)
        {
            int ReturnValue = -1;
            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();

            IQueryable<int> temp = from data in context.Sessions
                                   where data.ID.ToString() == sessionID
                                   select data.UserID;

            try
            {
                ReturnValue = temp.FirstOrDefault();

                if (ReturnValue <= 0)
                {
                    ReturnValue = -1;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != null)
                    ReturnValue = -1;
            }

            return ReturnValue;
        }

        private LoginResponse getSession(int userID)
        {
            LoginResponse ReturnValue = new LoginResponse();

            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();

            IQueryable<string> temp = from data in context.Sessions
                                      where data.UserID == userID
                                      select data.ID.ToString();

            try
            {
                ReturnValue.Value = temp.FirstOrDefault<string>();
                if (isGuid(ReturnValue.Value))
                {
                    ReturnValue.Valid = true;

                    Database.Session dbS = new Database.Session();

                    dbS = (from data in context.Sessions
                           where data.UserID == userID
                           select data).FirstOrDefault();

                    if (dbS != null && dbS.UserID > 0)
                    {
                        context.Sessions.Remove(dbS);
                        context.SaveChanges();
                        Guid guid = Guid.NewGuid();
                        //Try and create new session
                        context.Sessions.Add(new Database.Session() { ID = guid, UserID = userID, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow, ExpirationDate = DateTime.UtcNow.AddMonths(1) });
                        context.SaveChanges();
                        ReturnValue.Value = guid.ToString();
                        ReturnValue.Valid = true;
                    }
                }
                else {
                    Guid guid = Guid.NewGuid();
                    //Try and create new session
                    context.Sessions.Add(new Database.Session() { ID = guid, UserID = userID, CreatedDate = DateTime.UtcNow, ModifiedDate = DateTime.UtcNow, ExpirationDate = DateTime.UtcNow.AddMonths(1) });
                    context.SaveChanges();
                    ReturnValue.Value = guid.ToString();
                    ReturnValue.Valid = true;
                }
            }
            catch (Exception ex)
            {
                ReturnValue.Value = ex.Message;
            }

            return ReturnValue;
        }

        private Boolean isGuid(string value)
        {
            Guid temp = Guid.Empty;
            return Guid.TryParse(value, out temp);
        }



        private bool updateSetting(string settingName, string settingvalue)
        {
            bool returnValue = false;
            GarageOpener.Database.Setting setting = new GarageOpener.Database.Setting();
            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();

            IQueryable<GarageOpener.Database.Setting> temp = from data in context.Settings
                                                             where data.Name.ToUpper() == settingName.ToUpper()
                                                             select data;

            try
            {
                setting = temp.FirstOrDefault();

                if (setting != null)
                {
                    setting.Description = "http://" + settingvalue + ":";
                    context.SaveChanges();
                    returnValue = true;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message != null)
                {
                }
            }

            return returnValue;
        }

        private bool saveHacker(string address, LoginModel model)
        {
            bool returnValue = false;
            GarageOpener.Database.Hacker hacker = new GarageOpener.Database.Hacker();
            GarageOpener.Database.DB_30349_garageEntities context = new GarageOpener.Database.DB_30349_garageEntities();

            
            try
            {              
                hacker.Address = address;
                hacker.User = model.User;
                hacker.Parola = model.Parola;
                hacker.CreatedDate = DateTime.UtcNow;
                context.Hackers.Add(hacker);
                context.SaveChanges();
                returnValue = true;
            }
            catch (Exception ex)
            {
                if (ex.Message != null)
                {
                }
            }

            return returnValue;
        }
    }
    #endregion

    [CLSCompliant(true)]
    public class LoginResponse
    {
        public Boolean Valid = false;
        public string Value = "";
    }

    [CLSCompliant(true)]
    public class CheckStatusResponse : LoginResponse
    {
        public int StatusDoor = -1;
        public Boolean NotLoggedIn = false;
    }
    [CLSCompliant(true)]
    public class StatusRaspiResponse
    {
        public string Status = "";
        public string Value = "";
        public string Response = "";
        public string StatusUpper = "";
        public string StatusLower = "";
    }

    [CLSCompliant(true)]
    public class StatusRaspBaseiResponse
    {
        public string Status = "";
        public string Response = "";
    }
}
