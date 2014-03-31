using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;

namespace Ezaurum.HttpAPI
{
    public class HttpApiService
    {
        private readonly string _requestServer;

        private readonly string _userAgent;

        public HttpApiService(string requestServer)
        {
            _requestServer = requestServer;
        }

        public HttpApiService(string requestServer, string userAgent) : this(requestServer)
        {
            _userAgent = userAgent;
        }

        private HttpWebRequest MakeGetRequest(string url,
            string sessionId = null, string contentType = "application/json")
        {
            var httpWebRequest =
                (HttpWebRequest)WebRequest.Create(_requestServer + url);

            httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = contentType;
            httpWebRequest.UserAgent = _userAgent;
            if (null != sessionId) httpWebRequest.Headers.Add("Session-Id", sessionId);

            return httpWebRequest;
        }

        private HttpWebRequest MakePostRequest<TReq>(string url, TReq body, string sessionId = null, string contentType = "application/json")
        {
            var stream = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof(TReq));

            ser.WriteObject(stream, body);
            stream.Position = 0;
            var sr = new StreamReader(stream);
            var httpWebRequest =
                (HttpWebRequest)WebRequest.Create(_requestServer + url);

            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = contentType;
            httpWebRequest.UserAgent = _userAgent;
            if (null != sessionId) httpWebRequest.Headers.Add("Session-Id", sessionId);

            using (
                var streamWriter =
                    new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(sr.ReadToEnd());
                streamWriter.Flush();
                streamWriter.Close();
            }

            return httpWebRequest;
        }

        public bool Post<TReq, TRes>(string uri, out TRes response, TReq request)
            where TRes : class
            where TReq : class
        {
            return Post(uri,
                request, out response).IsSuccess();
        }

        public bool Get<TRes>(string uri, out TRes response) where TRes : class 
        {
            HttpWebRequest request = MakeGetRequest(uri);
            return MakeResponse(out response, request).IsSuccess();
        }

        public HttpStatusCode Post<TReq, TRes>(string billOrder, TReq body, out TRes res)
            where TReq : class
            where TRes : class
        {
            HttpWebRequest request = MakePostRequest(billOrder, body);

            return MakeResponse(out res, request);
        }

        private static HttpStatusCode MakeResponse<TRes>(out TRes res,
            HttpWebRequest request) where TRes : class
        {
            HttpWebResponse response = null;
            HttpStatusCode result;
            try
            {
                response = request.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                response = ex.Response as HttpWebResponse;                
            }
            finally
            {
                result = GetResponseObject(out res, response);
            }
            return result;
        }

        private static HttpStatusCode GetResponseObject<TRes>(out TRes res,
            HttpWebResponse response) where TRes : class
        {
            if (null == response)
                throw new InvalidOperationException("response is null."); 

            var responseStream = response.GetResponseStream();
            if (null == responseStream)
                throw new InvalidOperationException("response stream is null.");

            HttpStatusCode resultCode = response.StatusCode;

            if (resultCode  == HttpStatusCode.NoContent)
            {
                res = null;
                return resultCode;
            }               

            var jsonSerializer = new DataContractJsonSerializer(typeof(TRes));
            res = jsonSerializer.ReadObject(responseStream) as TRes;
            response.Close();

            return resultCode;
        }
    }
}