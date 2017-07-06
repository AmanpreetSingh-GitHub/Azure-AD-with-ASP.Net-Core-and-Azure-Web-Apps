using System;
using System.Collections.Generic;
using System.Net;

namespace FrameworkThree.BusinessLayer.Common
{
    public class ResponseMessage<T> where T : class
    {
        public Exception Exception { get; set; }

        public int Total { get; set; }

        public T Result { get; set; }

        public string StatusText { get; set; }

        public List<string> Messages { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public bool Success
        {
            get
            {
                return StatusCode == HttpStatusCode.OK;
            }
        }

        public bool Unauthorized
        {
            get
            {
                return StatusCode == HttpStatusCode.Unauthorized;
            }
        }

        public void SetAsBadRequest()
        {
            StatusCode = HttpStatusCode.BadRequest;
        }

        public void SetAsGoodRequest()
        {
            StatusCode = HttpStatusCode.OK;
        }
    }
}
