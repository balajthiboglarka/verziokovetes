using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Ceramista.Dnn.Ceramista.Dnn.HelloWorld.Models;
using Ceramista.Dnn.Ceramista.Dnn.HelloWorld.Services;
using DotNetNuke.Web.Mvc.Framework.ActionFilters;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace Ceramista.Dnn.Ceramista.Dnn.HelloWorld.Controllers
{
    [DnnHandleError]
    public class ItemController : DnnController
    {
        public ActionResult Index()
        {
            DotNetNuke.Framework.JavaScriptLibraries.JavaScript.RequestRegistration(
                DotNetNuke.Framework.JavaScriptLibraries.CommonJs.jQuery);

            var service = new HotcakesProductService();
            List<ProductViewModel> products;

            try
            {
                products = service.GetAllProducts();
            }
            catch
            {
                products = new List<ProductViewModel>();
            }

            var serializer = new JavaScriptSerializer();
            ViewBag.ProductsJson = serializer.Serialize(products);

            return View(products);
        }
    }
}
