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

        // GET: Content/Download/:id
        [Authorize]
        public ActionResult Download(int id)
        {
            return View();
        }

        // GET: Content/Details/:id
        public ActionResult Details(Guid guid)
        {
            return View();
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

            StreamReader sr = null;
            try
            {
                sr = new StreamReader(model.File.OpenReadStream());
            }
            catch
            {
                // Error
                AddError("El correo electrónico ya esta en uso.");
                return View(model);
            }

            string audioDescriptionText = sr.ReadToEnd();
            if(audioDescriptionText.Length < 50)
            {
                // Error
                AddError("El archivo contiene muy poco contenido.");
                return View(model);
            }
            model.AudioDescription = audioDescriptionText;

            var user = await GetCurrentUserAsync();
            model.UserId = user.Id;

            model.Guid = new Guid();

            // Añade el contenido nuevo
            _context.Content.Add(model);
            _context.SaveChanges();
            // Redirige a la página del contenido
            return RedirectToAction($"Details/{model.Guid}");
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

        #endregion
    }
}