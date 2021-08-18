using Checkout.Common;
using Checkout.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Checkout.Controllers
{
    public class CheckoutController : Controller
    {
        public ActionResult Index()
        {
            var model = PrepareModel();
            return View(model);
        }

        public ActionResult Post()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
        private PaymentModel PrepareModel(PaymentModel existingModel = null)
        {
            var model = existingModel ?? new PaymentModel();
            model.Currencies = new[]
            {
                new SelectListItem() {Value = Currency.USD, Text = Currency.USD},
                new SelectListItem() {Value = Currency.EUR, Text = Currency.EUR},
                new SelectListItem() {Value = Currency.GBP, Text = Currency.GBP}
            };
            return model;
        }
    }
}