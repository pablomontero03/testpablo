using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace F2P.WAP.Controllers
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if (actionContext.ModelState.IsValid == false)
            {
                var resp = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    Content = new StringContent(string.Format("LOS ATRIBUTOS: {0}", actionContext.ModelState.Keys), System.Text.Encoding.UTF8, "application/json"),
                    ReasonPhrase = string.Format("FALTAN PARAMETROS MINIMOS REQUERIDOS {0}", JsonConvert.SerializeObject(actionContext.ModelState.Keys))
                };
                // throw new resp.CreateErrorResponse(HttpStatusCode.BadRequest, ModelState);
                throw new HttpResponseException(resp);

                //actionContext.Response = actionContext.Request.CreateErrorResponse(
                //    HttpStatusCode.BadRequest, actionContext.ModelState);
            }
        }
    }
}
