using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;

namespace Ezaurum.Http.Client
{
    public static class HttpRequestUtils
    {
        private const string Post = "POST";
        private const string Delete = "DELETE";
        private const string Put = "PUT";
        private const string Get = "GET";

        public static bool IsSuccess(this HttpStatusCode httpStatusCode)
        {
            return 0x19 == ((int) httpStatusCode >> 3);
        }

        #region Send methods

        public static bool PostJson<TReq, TRes>(this Uri uri, TReq request, out TRes response, out HttpStatusCode code,
            out string jsonResult, string authorization = null)
        {
            return SendRequest(uri, request, out response, out code, out jsonResult, authorization, Post);
        }

        public static bool PostJson<TRes>(this Uri uri, out TRes response, out HttpStatusCode code,
            out string jsonResult, string authorization = null)
        {
            return SendRequest(uri, out response, out code, out jsonResult, authorization, Post);
        }

        public static bool DeleteJson<TReq, TRes>(this Uri uri, TReq request, out TRes response, out HttpStatusCode code,
            out string jsonResult, string authorization = null)
        {
            return SendRequest(uri, request, out response, out code, out jsonResult, authorization, Delete);
        }

        public static bool DeleteJson<TRes>(this Uri uri, out TRes response, out HttpStatusCode code,
            out string jsonResult, string authorization = null)
        {
            return SendRequest(uri, out response, out code, out jsonResult, authorization, Delete);
        }

        public static bool PutJson<TRes>(this Uri uri, out TRes response, out HttpStatusCode code, out string jsonResult,
            string authorization = null)
        {
            return SendRequest(uri, out response, out code, out jsonResult, authorization, Put);
        }

        public static bool PutJson<TReq, TRes>(this Uri uri, TReq request, out TRes response, out HttpStatusCode code,
            out string jsonResult, string authorization = null)
        {
            return SendRequest(uri, request, out response, out code, out jsonResult, authorization, Put);
        }

        public static bool GetJson<TRes>(this Uri uri, out TRes response, out HttpStatusCode code, out string jsonResult,
            string authorization = null)
        {
            return SendRequest(uri, out response, out code, out jsonResult, authorization, Get);
        }

        #endregion

        #region inner methods

        /// <summary>
        /// Send request without request data
        /// </summary>
        /// <typeparam name="TRes"></typeparam>
        /// <param name="uri"></param>
        /// <param name="response"></param>
        /// <param name="code"></param>
        /// <param name="jsonResult"></param>
        /// <param name="authorization"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static bool SendRequest<TRes>(Uri uri, out TRes response, out HttpStatusCode code, out string jsonResult,
            string authorization, string method)
        {
            return MakeJsonRequest(uri, authorization, method).MakeResponse(out response, out code, out jsonResult);
        }

        /// <summary>
        /// Send request with request data
        /// </summary>
        /// <typeparam name="TReq"></typeparam>
        /// <typeparam name="TRes"></typeparam>
        /// <param name="uri"></param>
        /// <param name="request"></param>
        /// <param name="response"></param>
        /// <param name="code"></param>
        /// <param name="jsonResult"></param>
        /// <param name="authorization"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        private static bool SendRequest<TReq, TRes>(Uri uri, TReq request, out TRes response, out HttpStatusCode code,
            out string jsonResult, string authorization, string method)
        {
            HttpWebRequest httpWebRequest = MakeJsonRequest(uri, authorization, method);

            using (var sw = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                sw.Write(JsonConvert.SerializeObject(request));
                sw.Flush();
            }

            return httpWebRequest.MakeResponse(out response, out code, out jsonResult);
        }

        private static HttpWebRequest MakeJsonRequest(Uri uri, string authorization, string method)
        {
            HttpWebRequest httpWebRequest = WebRequest.CreateHttp(uri);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Accept = "application/json";
            if (null != authorization)
            {
                httpWebRequest.Headers.Add("Authorization", authorization);
            }
            httpWebRequest.Method = method;
            return httpWebRequest;
        }

        private static bool MakeResponse<TRes>(this HttpWebRequest httpWebRequest, out TRes response,
            out HttpStatusCode code, out string jsonResult)
        {
            jsonResult = null;
            try
            {
                using (var webResponse = httpWebRequest.GetResponse() as HttpWebResponse)
                {
                    return webResponse.ConvertJsonStringToObject(out response, out code);
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
                        code = (HttpStatusCode) e.Status;
                        return false;
                }
            }
        }

        private static bool ConvertJsonStringToObject<TRes>(this HttpWebResponse webResponse, out TRes response,
            out HttpStatusCode code)
        {
            Debug.Assert(webResponse != null, "webResponse != null");
            code = webResponse.StatusCode;

            //response stream will be disposed when stream reader dipose.
            Stream responseStream = webResponse.GetResponseStream();
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
            return true;
        }

        private static bool MakeStringResponse(this HttpWebResponse webResponse, out string response,
            out HttpStatusCode code)
        {
            Debug.Assert(webResponse != null, "webResponse != null");
            code = webResponse.StatusCode;

            //response stream will be disposed when stream reader dipose.
            Stream responseStream = webResponse.GetResponseStream();
            if (null == responseStream)
            {
                response = null;
                return false;
            }

            using (var sr = new StreamReader(responseStream, Encoding.UTF8))
            {
                response = sr.ReadToEnd();
            }

            return true;
        }

        #endregion
    }
}