using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace InventtiWs.Controllers
{
    [RoutePrefix("api/StringManipulator")]
    public class StringManipulatorController : ApiController
    {
        [HttpGet]
        [Route("OddStrings")]
        public HttpResponseMessage OddStrings(string pCompare, string pOdd)
        {
            if (pCompare.Length != pOdd.Length)
                return Request.CreateResponse(HttpStatusCode.InternalServerError, false);

            List<char> listaCompare = new List<char>();
            List<char> listaOdd = new List<char>();

            for (int i = 0; i < pCompare.Length; i = i + 2)
            {
                listaCompare.Add(pCompare[i]);
                listaOdd.Add(pOdd[i]);
            }

            listaCompare = listaCompare.OrderBy(x => x).ToList();
            listaOdd = listaOdd.OrderBy(x => x).ToList();

            for (int i = 0; i < listaCompare.Count; i++)
            {
                if (listaCompare[i] != listaOdd[i])
                    return Request.CreateResponse(HttpStatusCode.InternalServerError, false);
            }

            return Request.CreateResponse(HttpStatusCode.OK, true);
        }
    }
}