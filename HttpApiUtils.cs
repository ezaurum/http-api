using System.Net;

namespace Ezaurum.HttpAPI
{
    public static class HttpApiUtils
    {
        public static bool IsSuccess(this HttpStatusCode httpStatusCode)
        {
            return 0x19 == ((int)httpStatusCode >> 3);
        }
    }
}