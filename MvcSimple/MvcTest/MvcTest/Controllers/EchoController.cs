using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MvcTest.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EchoController : ControllerBase
    {
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(string id)
        {
            ResponesModel.SimpleEchoResponse response = new ResponesModel.SimpleEchoResponse()
            {
                ErrorCode = "0",
                ErrorMsg = "",
                ResultObjects = $"hello {id}"
            };
            return Newtonsoft.Json.JsonConvert.SerializeObject(response);
        }
    }
}
