using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RestEase;

namespace ChatBot.Api.Controllers
{
    [Route("_api/[controller]")]
    [ApiController]   
    public abstract class BaseController : ControllerBase
    {
        protected BaseController() { }

        //This base action helps to handle 200 & 400
        //todo: comment and replace all base with this
        protected ActionResult<T> Single<T>(T data)
        {
            if (data == null)
            {
                return NotFound();
            }

            return Ok(data);
        }

        protected ActionResult Single<T>(Response<T> response)
        {
            if (response == null || response.ResponseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return NotFound();
            }
            else if (response.ResponseMessage.IsSuccessStatusCode)
            {
                return Ok(response.StringContent);
            }
            else
            {
                var problemDetails = JsonConvert.DeserializeObject<ProblemDetails>(response.StringContent);
                return Problem(detail: problemDetails.Detail, title: problemDetails.Title, statusCode: problemDetails.Status);
            }
        }

    
    }
}