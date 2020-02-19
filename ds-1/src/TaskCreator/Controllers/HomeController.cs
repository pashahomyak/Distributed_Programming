using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TaskCreator.Models;
using Grpc.Net.Client;
using BackendApi;
using Grpc.Core;

namespace TaskCreator.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> SubmitFormAsync(string taskDescription)
        {
            string taskResult = String.Empty;

            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            using var channel = GrpcChannel.ForAddress("http://localhost:5000");
            var client = new Job.JobClient(channel);
            try
            {
                var reply = await client.RegisterAsync(
                              new RegisterRequest { Description = taskDescription });
                taskResult = reply.Id;
                ViewBag.TaskResult = "Задача создана с идентификатором: " + taskResult;
            }
            catch (RpcException)
            {
                ViewBag.TaskResult = "Подключение не установлено, т.к. конечный компьютер отверг запрос на подключение";
            }
            catch (ArgumentNullException)
            {
                ViewBag.TaskResult = "Описание задачи не может быть пустым";
            }

            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}