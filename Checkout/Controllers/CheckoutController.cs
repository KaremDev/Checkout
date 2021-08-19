using Checkout;
using Checkout.Common;
using Checkout_Test.Models;
using Checkout.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Checkout_Test.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICheckoutApi _checkoutApi;
        private readonly ISerializer _serializer;
        public CheckoutController():this(new JsonSerializer())
        {

        }
        public CheckoutController(ISerializer serializer )
        {
            var config = new Configration();
            CheckoutConfiguration configuration = new CheckoutConfiguration(config.Sk, true);
            ApiClient api = new ApiClient(configuration);
            _checkoutApi = new CheckoutApi(api, configuration);
            _serializer = serializer;
        }
        public ActionResult Index()
        {
            var model = PrepareModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Post(PaymentModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    PrepareModel(model);
                    return View(nameof(Index), model);
                }

                if (string.IsNullOrWhiteSpace(model.CardToken))
                    throw new ArgumentException($"{nameof(model.CardToken)} is missing.", nameof(model));

                var source = new TokenSource(model.CardToken);
                var paymentRequest = new PaymentRequest<TokenSource>(source, model.Currency, model.Amount)
                {
                    Capture = model.Capture,
                    Reference = model.Reference,
                    ThreeDS = model.DoThreeDS,
                    SuccessUrl = BuildUrl(nameof(ThreeDSSuccess)),
                    FailureUrl = BuildUrl(nameof(ThreeDSFailure))
                };

                var response = await _checkoutApi.Payments.RequestAsync(paymentRequest);

                if (response.IsPending && response.Pending.RequiresRedirect())
                {
                    return Redirect(response.Pending.GetRedirectLink().Href);
                }

                StorePaymentInTempData(response.Payment);

                if (response.Payment.Approved)
                {
                    return RedirectToAction(nameof(NonThreeDSSuccess));
                }

                return RedirectToAction(nameof(NonThreeDSFailure));
            }
            catch (Exception ex)
            {
                return View(nameof(NonThreeDSFailure));
                //return View(nameof(Error), new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext?.TraceIdentifier, Message = ex.Message });
            }
        }
        private PaymentModel PrepareModel(PaymentModel existingModel = null)
        {
            var model = existingModel ?? new PaymentModel();
            model.Currencies = new[]
            {
                new SelectListItem() {Value = Currency.USD, Text = Currency.USD},
                new SelectListItem() {Value = Currency.EUR, Text = Currency.EUR},
                new SelectListItem() {Value = Currency.GBP, Text = Currency.GBP},
                new SelectListItem() {Value = Currency.SAR, Text = Currency.SAR}
            };
            return model;
        }
        private void StorePaymentInTempData(PaymentProcessed payment)
        {
            TempData[nameof(PaymentProcessed)] = _serializer.Serialize(payment);
        }

        public Task<ActionResult> ThreeDSSuccess()
    => GetThreeDsPaymentAsync();

        [HttpGet]
        public Task<ActionResult> ThreeDSFailure()
            => GetThreeDsPaymentAsync();
        private async Task<ActionResult> GetThreeDsPaymentAsync()
        {
            var sessionId = Request.QueryString["cko-session-id"];
            GetPaymentResponse payment = await _checkoutApi.Payments.GetAsync(sessionId);

            if (payment == null)
                return RedirectToAction(nameof(Index));

            return View("NonThreeDSSuccess", payment);
        }

        [HttpGet]
        public ActionResult NonThreeDSSuccess() => GetPaymentFromTempData();

        [HttpGet]
        public ActionResult NonThreeDSFailure() => GetPaymentFromTempData();
        private ActionResult GetPaymentFromTempData()
        {
            if (TempData.TryGetValue(nameof(PaymentProcessed), out var serializedPayment))
            {
                var _serializer = new JsonSerializer();
                var payment = _serializer.Deserialize(serializedPayment.ToString(), typeof(PaymentProcessed)) as PaymentProcessed;
                return View(payment);
            }

            return RedirectToAction(nameof(Index));
        }

        private string BuildUrl(string actionName)
        {
            return Url.Action(actionName, "Checkout", null, Request.Url.Scheme);
        }
    }
}