﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MqttHomeWeb.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StreamController : ControllerBase
    {

    }
}