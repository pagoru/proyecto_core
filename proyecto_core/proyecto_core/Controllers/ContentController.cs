using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using proyecto_core.Models.ContentViewModels;
using System.IO;
using proyecto_core.Models;
using Microsoft.AspNetCore.Identity;
using proyecto_core.Data;

namespace proyecto_core.Controllers
{
    public class ContentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public ContentController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context
            )
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: Content
        public ActionResult Index()
        {
            var applicationContentList =
                (from _content in _context.Content
                select _content).ToList();

            var model = new IndexViewModel()
            {
                ApplicationContentList = applicationContentList
            };

            return View(model);
        }

        // GET: Content/Download/:guid
        [Authorize]
        public FileStreamResult Download(String id)
        {
            var applicationContent =
                (from _content in _context.Content
                 where _content.Guid == Guid.Parse(id)
                 select _content).FirstOrDefault();
            //Hacer mas comprobaciones

            var fileName = applicationContent.Title.Replace(' ', '_');

            Response.Headers.Add("content-disposition", "attachment; filename=" + fileName + ".txt");
            return GetFileFromText(applicationContent.AudioDescription); // or "application/x-rar-compressed"
        }

        private FileStreamResult GetFileFromText(String text)
        {
            MemoryStream ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(text));
            return File(ms, "application/octet-stream"); // or "application/x-rar-compressed"
        }

        // GET: Content/Error
        public ActionResult Error()
        {
            return View();
        }

        // GET: Content/DownloadDemo/:guid
        public FileStreamResult DownloadDemo(String id)
        {
            var applicationContent =
                (from _content in _context.Content
                 where _content.Guid == Guid.Parse(id)
                 select _content).FirstOrDefault();
            //Hacer mas comprobaciones

            var fileName = applicationContent.Title.Replace(' ', '_');
            var ad = applicationContent.AudioDescription;
            ad = ad.Substring(0, Math.Min(150, ad.Length)) + "... - Para descargar el contenido completo... PAGA! >:)";

            Response.Headers.Add("content-disposition", "attachment; filename=demo-" + fileName + ".txt");
            return GetFileFromText(ad); // or "application/x-rar-compressed"
        }

        // GET: Content/Details/:guid
        public ActionResult Details(String id)
        {
            Guid guid;
            try
            {
                guid = Guid.Parse(id);
            } catch
            {
                AddError("No existe el contenido solicitado.");
                return View();
            }

            var applicationContent = 
                (from _content in _context.Content
                where _content.Guid == guid
                 select _content).FirstOrDefault();

            if (applicationContent == null)
            {
                AddError("No se ha encontrado el contenido solicitado.");
                return View();
            }

            var model = new DetailsViewModel()
            {
                ApplicationContent = applicationContent
            };

            return View(model);
        }

        // GET: Content/Create
        // [Authorize]
        public ActionResult Create()
        {
            return View();
        }

        // POST: Content/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(CreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Stream st = null;
            try
            {
                st = model.File.OpenReadStream();
            }
            catch
            {
                // Error
                AddError("Error");
                return View(model);
            }

            byte[] resultBytes = ReadBytesFromStream(st);

            if (IsFileBinary(resultBytes))
            {
                // Error binari file
                AddError("El archivo no tiene un formato valido.");
                return View(model);
            }

            string audioDescriptionText = System.Text.Encoding.UTF8.GetString(resultBytes);

            // TODO Comprobar que el archivo es de tipo audiodescripción en el interior
            //Comprobar que no haya un duplicado

            /*if(audioDescriptionText.Length < 50)
            {
                // Error
                AddError("El archivo contiene muy poco contenido.");
                return View(model);
            }*/
            var user = await GetCurrentUserAsync();

            var applicationUser = new ApplicationContent()
            {
                Guid = new Guid(),
                UserId = user.Id,
                Title = model.Title,
                Description = model.Description,
                AudioDescription = audioDescriptionText

            };

            // Añade el contenido nuevo
            _context.Content.Add(applicationUser);
            _context.SaveChanges();
            // Redirige a la página del contenido
            return RedirectToAction($"Details/{applicationUser.Guid}");
        }

        // GET: Content/Edit/:id
        [Authorize]
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: Content/Edit/:id
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: Content/Delete/:id
        [Authorize]
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: Content/Delete/:id
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        #region utils

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        private void AddError(String description)
        {
            AddErrors(IdentityResult.Failed(new IdentityError[] {
                    new IdentityError() {
                        Description = description
                    }
            }));
        }

        private bool IsFileBinary(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
                if (bytes[i] > 127)
                    return true;
            return false;
        }

        public static byte[] ReadBytesFromStream(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        #endregion
    }
}