using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AiXeduleApiGrabber.BackgroundTasks;
using AiXeduleApiGrabber.Models;
using Microsoft.AspNetCore.Mvc;

namespace AiXeduleApiGrabber.Controllers
{
    [Route("api/[controller]/[action]")]
    public class CredentialsController : Controller
    {
        public IActionResult Index()
        {
            return NotFound();
        }

        [HttpPost]
        // [RequireHttps]
        public IActionResult SetCredentials(CookieCredentials creds)
        {
            if (!(string.IsNullOrWhiteSpace(creds.Session) || string.IsNullOrWhiteSpace(creds.User)))
            {
                SessionReviver.SessionCookie = creds.Session;
                SessionReviver.UserCookie = creds.User;
            }

            return StatusCode(418);
        }
    }
}