using Checkout.Payments;
using Checkout.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Checkout;

namespace Checkout_Test.Controllers
{
    public class PayController : ApiController
    {
        // GET: api/Pay
        public async Task<string> GetAsync()
        {
            var config = new Checkout_Test.Models.Configration();
            var api = CheckoutApi.Create(config.Sk, true , config.Pk);
            JsonSerializer json = new JsonSerializer();
            var request = new CardTokenRequest("4543474002249996", 6, 2025);    
                var response = await api.Tokens.RequestAsync(request);
                var token = response.Token;
            var tokenSrc = new TokenSource(token);
            var paymentRequest = new PaymentRequest<TokenSource>(tokenSrc,"SAR",600)
            {
                Reference = "ORD-090857",
                Capture = true,
                ThreeDS = false
            };


            try
            {
                var  res = await api.Payments.RequestAsync(paymentRequest);

                if (res.IsPending && res.Pending.RequiresRedirect())
                {
                    var lol = res.Pending.GetRedirectLink().Href;
                   // return Redirect(res.Pending.GetRedirectLink().Href);
                }
                
                if (res.Payment.Approved)
                    return json.Serialize(res.Payment);

                
            }
            catch (CheckoutValidationException validationEx)
            {
                //return ValidationError(validationEx.Error);
            }
            

return "error";
        }

        // POST: api/Pay
        public void Post([FromBody] string value)
        {
        }
        }
        


    }

