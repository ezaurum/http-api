using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;

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

        #region get method

        private HttpWebRequest MakeGetRequest(string url, string contentType = "application/json")
        {
            var httpWebRequest =
                (HttpWebRequest) WebRequest.Create(_requestServer + url);

            httpWebRequest.Method = "GET";
            httpWebRequest.ContentType = contentType;
            httpWebRequest.UserAgent = _userAgent;

            return httpWebRequest;
        }

        public bool IsSuccessGet<TRes>(string uri, out TRes response)
            where TRes : class
        {
            return Get(uri, out response).IsSuccess();
        }

        public HttpStatusCode Get<TRes>(string uri, out TRes res)
            where TRes : class
        {
            HttpWebRequest request = MakeGetRequest(uri);
            return MakeResponse(out res, request);
        }

        #endregion

        #region post method

        public bool IsSuccessPost<TReq, TRes>(string uri, TReq request, out TRes response)
            where TRes : class
            where TReq : class
        {
            return Post(uri,
                request, out response).IsSuccess();
        }

        public HttpStatusCode Post<TReq, TRes>(string billOrder, TReq body,
            out TRes res)
            where TReq : class
            where TRes : class
        {
            HttpWebRequest request = MakePostRequest(billOrder, body);
            return MakeResponse(out res, request);
        }

        private HttpWebRequest MakePostRequest<TReq>(string url, TReq body,
            string contentType = "application/json")
        {
            var stream = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof (TReq));

            ser.WriteObject(stream, body);
            stream.Position = 0;
            var sr = new StreamReader(stream);
            var httpWebRequest =
                (HttpWebRequest) WebRequest.Create(_requestServer + url);

            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = contentType;
            httpWebRequest.UserAgent = _userAgent;

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

        #endregion

        #region put method

        public bool IsSuccessPut<TReq, TRes>(string uri, TReq request, out TRes response)
            where TRes : class
            where TReq : class
        {
            return Put(uri,
                request, out response).IsSuccess();
        }

        public HttpStatusCode Put<TReq, TRes>(string uri, TReq body,
            out TRes res)
            where TReq : class
            where TRes : class
        {
            HttpWebRequest request = MakePutRequest(uri, body);
            return MakeResponse(out res, request);
        }

        private HttpWebRequest MakePutRequest<TReq>(string url, TReq body,
            string contentType = "application/json")
        {
            var stream = new MemoryStream();
            var ser = new DataContractJsonSerializer(typeof (TReq));

            ser.WriteObject(stream, body);
            stream.Position = 0;
            var sr = new StreamReader(stream);
            var httpWebRequest =
                (HttpWebRequest) WebRequest.Create(_requestServer + url);

            httpWebRequest.Method = "PUT";
            httpWebRequest.ContentType = contentType;
            httpWebRequest.UserAgent = _userAgent;

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

        #endregion

        #region common methods

        private static HttpStatusCode MakeResponse<TRes>(out TRes res,
            HttpWebRequest request) where TRes : class
        {
            HttpWebResponse response = null;
            HttpStatusCode result = 0;
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
                if (null != response)
                {
                    result = GetResponseObject(out res, response);
                    response.Close();
                }
                else
                {
                    result = HttpStatusCode.BadRequest;
                    res = null;
                }
            }

            return result;
        }

        private static HttpStatusCode GetResponseObject<TRes>(out TRes res,
            HttpWebResponse response) where TRes : class
        {
            Stream responseStream = response.GetResponseStream();
            if (null == responseStream)
                throw new InvalidOperationException("response stream is null.");

            HttpStatusCode resultCode;
            string readToEnd;
            using (var sr = new StreamReader(responseStream, Encoding.UTF8))
            {
                resultCode = response.StatusCode;

                if (resultCode == HttpStatusCode.NoContent)
                {
                    res = null;
                    return resultCode;
                }

                readToEnd = sr.ReadToEnd();
            }
            byte[] b = Encoding.UTF8.GetBytes(readToEnd);

            var jsonSerializer = new DataContractJsonSerializer(typeof (TRes));

            res = jsonSerializer.ReadObject(new MemoryStream(b)) as TRes;

            return resultCode;
        }

        #endregion
    }
}