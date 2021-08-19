using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Checkout.Controllers
{
    public class PayController : ApiController
    {
        // GET: api/Pay
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/Pay/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/Pay
        public void Post([FromBody]string value)
        {
        }

        
    }
}
