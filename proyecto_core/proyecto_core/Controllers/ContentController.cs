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
            return View();
        }

        // GET: Content/Download/:guid
        [Authorize]
        public ActionResult Download(Guid guid)
        {
            return View();
        }

        // GET: Content/Error
        public ActionResult Error()
        {
            return View();
        }

        // GET: Content/DownloadDemo/:guid
        public ActionResult DownloadDemo(Guid guid)
        {
            return View();
        }

        // GET: Content/Details/:guid
        public ActionResult Details(Guid guid)
        {
            var contentQuery =
                from _content in _context.Content
                //where _content.Guid == guid
                select _content;

            // No encuentra ningun contenido con esa guid
            /*if(contentQuery.Count<ApplicationContent>() == 0)
            {
                return RedirectToAction($"Error");
            }*/

            var model = new DetailsViewModel()
            {
                ApplicationContent = contentQuery.First<ApplicationContent>()
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
            StreamReader sr = null;
            try
            {
                st = model.File.OpenReadStream();
                sr = new StreamReader(st);
            }
            catch
            {
                // Error
                AddError("Error");
                return View(model);
            }

            if (IsFileBinary(ReadBytesFromStream(st)))
            {
                // Error Non binari
                AddError("El archivo no tiene un formato valido.");
                return View(model);
            }

            // TODO Comprobar que el archivo es de tipo audiodescripción en el interior

            string audioDescriptionText = sr.ReadToEnd();
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