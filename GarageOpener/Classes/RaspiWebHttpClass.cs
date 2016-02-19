using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace GarageOpener.Classes
{
    public class Class1
    {

        protected string getStatus(int i)
        {
            string responseFromServer = "";
            try
            {
                WebRequest request = WebRequest.Create(ConfigurationManager.AppSettings["IPAddress"].ToString());
                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                request.ContentType = "application/x-www-form-urlencoded";
                string postData = "";

                switch (i)
                {
                    case 1:
                        postData = "value1=raspberry@home.com&value2=astaeparolade1234567890&value3=status";
                        break;
                    case 2:
                        postData = "value1=raspberry@home.com&value2=astaeparolade1234567890&value3=status_upper";
                        break;
                    case 3:
                        postData = "value1=raspberry@home.com&value2=astaeparolade1234567890&value3=status_lower";
                        break;
                }


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
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                responseFromServer = reader.ReadToEnd();

                //System.Threading.Thread.Sleep(7000);
            }
            catch (Exception ex)
            {
                if (ex != null)
                {

                }
                responseFromServer = "Server is not online.";
            }
            finally
            {

            }
            return responseFromServer;
        }

        protected void Button1_Click(object sender, EventArgs e)
        {

            try
            {
                WebRequest request = WebRequest.Create(ConfigurationManager.AppSettings["IPAddress"].ToString());
                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                request.ContentType = "application/x-www-form-urlencoded";
                string postData = "value1=raspberry@home.com&value2=astaeparolade1234567890&value3=open";
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
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
               // statusUpperID.Text = responseFromServer;
                System.Threading.Thread.Sleep(7000);
                //  getStatus();
            }
            catch (Exception ex)
            {
              //  statusUpperID.Text = "Server is not online.";
              if (ex !=null){

                }
            }
            finally
            {

            }
        }

        protected void Button2_Click(object sender, EventArgs e)
        {

            try
            {
                WebRequest request = WebRequest.Create(ConfigurationManager.AppSettings["IPAddress"].ToString());
                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                request.ContentType = "application/x-www-form-urlencoded";
                string postData = "value1=raspberry@home.com&value2=astaeparolade1234567890&value3=close";
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
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
               // statusUpperID.Text = responseFromServer;
                System.Threading.Thread.Sleep(7000);
                // getStatus();
            }
            catch (Exception ex)
            {
                if (ex != null)
                {

                }
                // statusUpperID.Text = "Server is not online.";
            }
            finally
            {

            }




        }
    }
}