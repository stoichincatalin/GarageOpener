#region "Imports"
using System.Text;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Xml.Serialization;
using System.Xml;
using Newtonsoft.Json;
using System;
#endregion

[assembly: System.CLSCompliant(true)]
namespace Helpers
{
    /// <summary>
    /// Serialize and deserialize
    /// </summary>
    /// <remarks></remarks>
    [CLSCompliant(true)]
    public class SerializeHelper
    {
        /// <summary>
        /// WinRT friendly way to deserialize
        /// </summary>
        /// <typeparam name="ClassType"></typeparam>
        /// <param name="JSONToDeserialize"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static ClassType DeserializeJSON<ClassType>(string JSONToDeserialize)
        {
            ClassType ReturnValue = default(ClassType);
            dynamic _Bytes = Encoding.Unicode.GetBytes(JSONToDeserialize);
            using (MemoryStream _Stream = new MemoryStream(_Bytes))
            {
                dynamic _Serializer = new DataContractJsonSerializer(typeof(ClassType));
                ReturnValue = (ClassType)_Serializer.ReadObject(_Stream);
            }
            return ReturnValue;
        }

        /// <summary>
        /// WinRT friendly way to serialize
        /// </summary>
        /// <param name="ClassToSerialize"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string SerializeJSON<ClassType>(ClassType ClassToSerialize)
        {
            string ReturnValue = null;
            using (MemoryStream _Stream = new MemoryStream())
            {
                dynamic _Serializer = new DataContractJsonSerializer(typeof(ClassType));
                _Serializer.WriteObject(_Stream, ClassToSerialize);
                _Stream.Position = 0;
                using (StreamReader _Reader = new StreamReader(_Stream))
                {
                    ReturnValue = _Reader.ReadToEnd();
                }
            }

            return ReturnValue;
        }

        /// <summary>
        /// Serializes and returns the XML
        /// </summary>
        /// <param name="ObjectToSerialize"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static string Serialize<ObjectType>(ObjectType ObjectToSerialize)
        {
            // Local variables
            MemoryStream MemoryStream = new MemoryStream();
            XmlSerializer XS = new XmlSerializer(ObjectToSerialize.GetType());
            XmlTextWriter TextWriter = new XmlTextWriter(MemoryStream, System.Text.Encoding.UTF8);
            System.Text.UTF8Encoding Encoding = new System.Text.UTF8Encoding();
            XS.Serialize(MemoryStream, ObjectToSerialize);
            MemoryStream = (MemoryStream)TextWriter.BaseStream;
            return Encoding.GetString(MemoryStream.ToArray());
        }

        /// <summary>
        /// Converts XML into the type passed
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static ObjectType Deserialize<ObjectType>(string Value)
        {
            // Local variables
            ObjectType ReturnValue = (ObjectType)Activator.CreateInstance(typeof(ObjectType));

            try
            {
                System.Text.UTF8Encoding Encoding = new System.Text.UTF8Encoding();
                byte[] ByteArray = (byte[])Encoding.GetBytes(Value);
                MemoryStream MemoryStream = new MemoryStream(ByteArray);
                XmlSerializer XS = new XmlSerializer(typeof(ObjectType));
                ReturnValue = (ObjectType)XS.Deserialize(MemoryStream);
            }
            catch (System.InvalidOperationException ex)
            {
                if (ex != null)
                {

                }
                // Bad XML Format, ignore error and just dont serialize
                ReturnValue = (ObjectType)Activator.CreateInstance(typeof(ObjectType));
            }
            return ReturnValue;
        }

        /// <summary>
        /// Used to convert Request.InputStream to an object
        /// I.e. Request form submit to a JSON object
        /// </summary>
        /// <typeparam name="ObjectType"></typeparam>
        /// <param name="StreamToConvert"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static ObjectType StreamToObjectJSON<ObjectType>(System.IO.Stream StreamToConvert)
        {
            // Local variables
            StreamReader Reader = default(StreamReader);
            ObjectType ReturnValue = default(ObjectType);

            try
            {
                // Convert
                Reader = new StreamReader(StreamToConvert);
                ReturnValue = SerializeHelper.DeserializeJSON<ObjectType>(Reader.ReadToEnd());
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // Return data
            return ReturnValue;
        }

        /// <summary>
        /// Used to convert an object into a stream
        /// I.e. Returning an object to a Response.Stream from a web service
        /// </summary>
        /// <typeparam name="ObjectType"></typeparam>
        /// <returns></returns>
        /// <remarks></remarks>
        public static System.IO.Stream ObjectToStreamJSON<ObjectType>(ObjectType ObjectToConvert)
        {
            // Local variables
            byte[] ResultBytes = null;
            MemoryStream ReturnValue = default(MemoryStream);

            try
            {
                // Convert
                ResultBytes = Encoding.UTF8.GetBytes(SerializeHelper.SerializeJSON<ObjectType>(ObjectToConvert));
                ReturnValue = new MemoryStream(ResultBytes);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // Return data
            return ReturnValue;
        }

        /// <summary>
        /// Used to convert Request.InputStream to an object - JsonConvert 
        /// I.e. Request form submit to a JSON object
        /// </summary>
        /// <typeparam name="ObjectType"></typeparam>
        /// <param name="StreamToConvert"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static ObjectType JSONStreamToObject<ObjectType>(System.IO.Stream StreamToConvert)
        {
            // Local variables
            StreamReader Reader = default(StreamReader);
            ObjectType ReturnValue = default(ObjectType);

            try
            {
                // Convert
                Reader = new StreamReader(StreamToConvert);
                ReturnValue = JsonConvert.DeserializeObject<ObjectType>(Reader.ReadToEnd(), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // Return data
            return ReturnValue;
        }
    }
}