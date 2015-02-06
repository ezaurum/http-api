using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Ezaurum.HttpAPI
{
public static class HttpApiUtils
    {
        public static bool IsSuccess(this HttpStatusCode httpStatusCode)
        {
            return 0x19 == ((int)httpStatusCode >> 3);
        }

        #region request property setters

        public static HttpWebRequest Method(this HttpWebRequest request, string method = "GET")
        {
            request.Method = method;
            return request;
        }

        public static HttpWebRequest UserAgent(this HttpWebRequest request, string userAgent)
        {
            request.UserAgent = userAgent;
            return request;
        }

        public static HttpWebRequest Accept(this HttpWebRequest request, string accept)
        {
            request.Accept = accept;
            return request;
        }

        #endregion

        public static bool PostJson<TReq, TRes>(this Uri uri, TReq request, out TRes response, out HttpStatusCode code, out string jsonResult, string authorization = null)
        {
            var httpWebRequest = WebRequest.CreateHttp(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";

            if (null != authorization)
            {
                httpWebRequest.Headers.Add("Authorization", authorization);
            }

            httpWebRequest.Method = "POST";

            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string serializeObject = JsonConvert.SerializeObject(request);
                sw.Write(serializeObject);
                sw.Flush();
            }

            return httpWebRequest.MakeResponse(out response, out code, out jsonResult);
        }

        public static bool DeleteJson<TReq, TRes>(this Uri uri, TReq request, out TRes response, out HttpStatusCode code, out string jsonResult, string authorization = null)
        {
            var httpWebRequest = WebRequest.CreateHttp(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";

            if (null != authorization)
            {
                httpWebRequest.Headers.Add("Authorization", authorization);
            }

            httpWebRequest.Method = "POST";

            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string serializeObject = JsonConvert.SerializeObject(request);
                sw.Write(serializeObject);
                sw.Flush();
            }

            return httpWebRequest.MakeResponse(out response, out code, out jsonResult);
        }

        public static bool DeleteJson<TRes>(this Uri uri, out TRes response, out HttpStatusCode code, out string jsonResult, string authorization = null)
        {
            var httpWebRequest = WebRequest.CreateHttp(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";

            if (null != authorization)
            {
                httpWebRequest.Headers.Add("Authorization", authorization);
            }

            httpWebRequest.Method = "DELETE";
            return httpWebRequest.MakeResponse(out response, out code, out jsonResult);
        }

        public static bool PutJson<TRes>(this Uri uri, out TRes response, out HttpStatusCode code, out string jsonResult, string authorization = null)
        {
            var httpWebRequest = WebRequest.CreateHttp(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";

            if (null != authorization)
            {
                httpWebRequest.Headers.Add("Authorization", authorization);
            }

            httpWebRequest.Method = "PUT";
            return httpWebRequest.MakeResponse(out response, out code, out jsonResult);
        }

        public static bool PutJson<TReq, TRes>(this Uri uri, TReq request, out TRes response, out HttpStatusCode code, out string jsonResult, string authorization = null)
        {
            var httpWebRequest = WebRequest.CreateHttp(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";

            if (null != authorization)
            {
                httpWebRequest.Headers.Add("Authorization", authorization);
            }

            httpWebRequest.Method = "PUT";

            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string serializeObject = JsonConvert.SerializeObject(request);
                sw.Write(serializeObject);
                sw.Flush();
            }

            return httpWebRequest.MakeResponse(out response, out code, out jsonResult);
        }

        public static bool GetJson<TRes>(this Uri uri, out TRes response, out HttpStatusCode code, out string jsonResult, string authorization = null)
        {
            var httpWebRequest = WebRequest.CreateHttp(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";

            httpWebRequest.Method = "GET";

            if (null != authorization)
            {
                httpWebRequest.Headers.Add("Authorization", authorization);
            }

            return httpWebRequest.MakeResponse(out response, out code, out jsonResult);
        }

        private static bool MakeResponse<TRes>(this HttpWebRequest httpWebRequest, out TRes response, out HttpStatusCode code, out string jsonResult)
        {
            jsonResult = null;
            try
            {
                using (var webResponse = httpWebRequest.GetResponse() as HttpWebResponse)
                {
                    return webResponse.MakeResponse(out response, out code);
                }
            }
            catch (WebException e)
            {
                response = default(TRes);
                switch (e.Status)
                {
                    case WebExceptionStatus.ProtocolError:
                        return (e.Response as HttpWebResponse).MakeStringResponse(out jsonResult, out code);
                    default:
                        response = default(TRes);
                        jsonResult = e.Message;
                        code = (HttpStatusCode)e.Status;
                        return false;
                }
            }
        }

        private static bool MakeResponse<TRes>(this HttpWebResponse webResponse, out TRes response, out HttpStatusCode code)
        {
            Debug.Assert(webResponse != null, "webResponse != null");
            code = webResponse.StatusCode;

            using (Stream responseStream = webResponse.GetResponseStream())
            {
                if (null == responseStream)
                {
                    response = default(TRes);
                    return false;
                }

                string readToEnd;
                using (var sr = new StreamReader(responseStream, Encoding.UTF8))
                {
                    readToEnd = sr.ReadToEnd();
                }
                response = JsonConvert.DeserializeObject<TRes>(readToEnd);
            }
            return true;
        }

        private static bool MakeStringResponse(this HttpWebResponse webResponse, out string response, out HttpStatusCode code)
        {
            Debug.Assert(webResponse != null, "webResponse != null");
            code = webResponse.StatusCode;

            using (Stream responseStream = webResponse.GetResponseStream())
            {
                if (null == responseStream)
                {
                    response = null;
                    return false;
                }

                using (var sr = new StreamReader(responseStream, Encoding.UTF8))
                {
                    response = sr.ReadToEnd();
                }
            }
            return true;
        }
    }
}