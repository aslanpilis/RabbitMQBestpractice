using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Models;
using RabbitMQ.Watermark.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitMQ.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly RabbitMQPublisher _rabbitMQPublisher;
        public HomeController(ILogger<HomeController> logger, RabbitMQPublisher rabbitMQPublisher)
        {
            _logger = logger;
            _rabbitMQPublisher = rabbitMQPublisher;
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ImageName")] Product product, IFormFile ImageFile)
        {

            if (!ModelState.IsValid) return View(product);


            if (ImageFile.Length>0 )
            {
                var randomImageName = Guid.NewGuid() + Path.GetExtension(ImageFile.FileName);


                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", randomImageName);


                await using FileStream stream = new FileStream(path, FileMode.Create);


                await ImageFile.CopyToAsync(stream);


                _rabbitMQPublisher.Publish(new productImageCreatedEvent() { ImageName = randomImageName });

                product.ImageName = randomImageName;
            }




            return RedirectToAction(nameof(Index));

            return View(product);
        }


        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
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
