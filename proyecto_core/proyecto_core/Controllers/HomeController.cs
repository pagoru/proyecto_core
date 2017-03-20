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
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.RegularExpressions;
using proyecto_core.Consts;
using proyecto_core.Models.HomeViewModels;

namespace proyecto_core.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;

        public HomeController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context
            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        //
        // GET: Content/
        public async Task<ActionResult> Index()
        {
            //Se devuelve una lista con todo el contenido disponible
            var resultQuery =
                from _content in _context.Content
                select _content;

            if(resultQuery == null)
            {
                AddError("No hay contenido");
                return View();
            }

            var applicationContentList = resultQuery.ToList();

            //Comprobar si el usuario es administrador
            var isAdmin = false;
            var user = await GetCurrentUserAsync();
            if(user != null)
            {
                //Se comprueba si el usuario pertenece a la 
                //administración para poder mostrar las opciones de edición
                isAdmin = await _userManager.IsInRoleAsync(user, WebRoles.Admin);
            }

            var model = new IndexViewModel()
            {
                ApplicationContentList = applicationContentList,
                IsAdmin = isAdmin,
            };

            return View(model);
        }

        //
        // GET: Content/Download/:guid
        [Authorize]
        public FileStreamResult Download(string id)
        {
            //Devuelve el contenido en función de la id pasada
            var applicationContent =
                (from _content in _context.Content
                 where _content.Id == Guid.Parse(id)
                 select _content).FirstOrDefault();
            //TODO Hacer más comprobaciones

            //Se incrementa el contado de descargas
            applicationContent.Downloads++;
            _context.Update(applicationContent);
            _context.SaveChanges();

            //Devuelve el titulo del contenido parseado 
            //de tal forma que se pueda usar como nombre de archivo
            var fileName = DeleteNonAscii(applicationContent.Title.Replace(' ', '-').Replace('_', '-'));
            //Elimina cualquier carácter que no sea ascii para no provocar 
            //conflictos a la hora de crear el archivo
            var ad = DeleteNonAscii(applicationContent.AudioDescription);

            //Añade la cabecera al cliente con la información de que es un archivo .txt
            Response.Headers.Add("content-disposition", "attachment; filename=" + fileName + ".txt");
            //Genera y devuelve el archivo desde el texto
            return GetFileFromText(ad);
        }

        //
        // GET: Content/DownloadDemo/:id
        public FileStreamResult DownloadDemo(string id)
        {
            //Devuelve el contenido en función de la id pasada
            var applicationContent =
                (from _content in _context.Content
                 where _content.Id == Guid.Parse(id)
                 select _content).FirstOrDefault();
            //TODO Hacer más comprobaciones

            //Se incrementa el contado de descargas de demos
            applicationContent.DemoDownloads++;
            _context.Update(applicationContent);
            _context.SaveChanges();

            //Devuelve el titulo del contenido parseado 
            //de tal forma que se pueda usar como nombre de archivo
            var fileName = DeleteNonAscii(applicationContent.Title.Replace(' ', '-').Replace('_', '-'));
            //Elimina cualquier carácter que no sea ascii para no provocar 
            //conflictos a la hora de crear el archivo
            var ad = applicationContent.AudioDescription;
            //Trunca el contenido para que sea apto para la demo y añadé un mensaje 
            //informativo y 'poco directo' sobre que realice un pago para descargar
            //la versión completa
            ad = DeleteNonAscii(ad.Substring(0, Math.Min(150, ad.Length)) + "... - Para descargar el contenido completo... PAGA! >:)");

            //Añade la cabecera al cliente con la información de que es un archivo .txt
            Response.Headers.Add("content-disposition", "attachment; filename=" + fileName + ".txt");
            //Genera y devuelve el archivo desde el texto
            return GetFileFromText(ad);
        }

        //
        // GET: Content/Details/:guid
        public async Task<ActionResult> Details(string id)
        {
            //Se parsea la id para comprobar que es una guid valida
            Guid guid;
            try
            {
                guid = Guid.Parse(id);
            } catch
            {
                AddError("No existe el contenido solicitado.");
                return View();
            }

            //Devuelve todos los detalles del contenido especificado
            var applicationContent = 
                (from _content in _context.Content
                where _content.Id == guid
                 select _content).FirstOrDefault();

            //En caso de que el resultado de la busqueda no sea satisfactoria
            if (applicationContent == null)
            {
                AddError("No se ha encontrado el contenido solicitado.");
                return View();
            }

            //Incrementa el contado de visitas de la página
            applicationContent.Views++;
            _context.Update(applicationContent);
            _context.SaveChanges();
            
            //Comprobar si el usuario es administrador
            var isAdmin = false;
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                //Se comprueba si el usuario pertenece a la 
                //administración para poder mostrar las opciones de edición
                isAdmin = await _userManager.IsInRoleAsync(user, WebRoles.Admin);
            }

            var model = new DetailsViewModel()
            {
                ApplicationContent = applicationContent,
                IsAdmin = isAdmin
            };

            return View(model);
        }

        //
        // GET: Content/Create
        // [Authorize]
        //TODO Autorizar solo a los usuarios que puedan publicar contenido en la web
        public ActionResult Create()
        {
            return View();
        }

        //
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

            //Se abre un stream para leer el archivo.
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

            //Se leen los bytes del stream
            byte[] resultBytes = ReadBytesFromStream(st);
            //Se comprueban si estos bytes contienen una cabecera valida
            //y por lo tanto, que es un archivo de texto
            //En caso de que el archivo sea un binario, dará un error
            if (IsFileBinary(resultBytes))
            {
                AddError("El archivo no tiene un formato valido.");
                return View(model);
            }
            //Los bytes se convierten a texto
            string audioDescriptionText = System.Text.Encoding.UTF8.GetString(resultBytes);

            //TODO Comprobar que el archivo es de tipo audiodescripción en el interior
            //TODO Comprobar que no sea duplicado

            /*if(audioDescriptionText.Length < 50)
            {
                // Error
                AddError("El archivo contiene muy poco contenido.");
                return View(model);
            }*/
            var user = await GetCurrentUserAsync();

            var applicationUser = new ApplicationContent()
            {
                Id = new Guid(),
                Title = model.Title,
                Description = model.Description,
                AudioDescription = audioDescriptionText,
                AddedDateTime = DateTime.Now.ToString(),
                Views = 0,
                Downloads = 0

            };

            //Añade el contenido nuevo
            _context.Content.Add(applicationUser);
            await _context.SaveChangesAsync();
            //Redirige a la página del contenido
            return RedirectToAction($"Details/{applicationUser.Id}");
        }

        //
        // GET: Content/Edit/:id
        [Authorize]
        public ActionResult Edit(string id)
        {
            Guid guid;
            try
            {
                guid = Guid.Parse(id);
            }
            catch
            {
                //TODO Controlar error
                return RedirectToAction("Index");
            }

            //Devuelve el contenido en función de la id
            var applicationContent =
                (from _content in _context.Content
                 where _content.Id == guid
                 select _content).FirstOrDefault();

            if (applicationContent == null)
            {
                //TODO Controlar error
                return RedirectToAction("Index");
            }

            var model = new EditViewModel()
            {
                Id = applicationContent.Id,
                Title = applicationContent.Title,
                Description = applicationContent.Description,
                Audiodescription = applicationContent.AudioDescription
            };
            return View(model);
        }

        //
        // POST: Content/Edit
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(EditViewModel model)
        {
            if(!ModelState.IsValid)
            {
                return View(model);
            }
            //Devuelve el contenido en función de la id del modelo
            var applicationContent =
                (from _content in _context.Content
                 where _content.Id == model.Id
                 select _content).FirstOrDefault();
            //Se comprueba si el modelo existe
            if (applicationContent == null)
            {
                return RedirectToAction("Index");
            }
            //Se modifican las variables de la edición
            applicationContent.Title = model.Title;
            applicationContent.Description = model.Description;
            applicationContent.AudioDescription = model.Audiodescription;

            //TODO Añadir registro de cambios

            _context.Update(applicationContent);
            await _context.SaveChangesAsync();

            return RedirectToAction($"Details/{model.Id}");
        }

        // GET: Content/Delete/:id
        [Authorize]
        public async Task<ActionResult> Delete(string id)
        {
            Guid guid;
            try
            {
                guid = Guid.Parse(id);
            }
            catch
            {
                return RedirectToAction("Index");
            }
            //Devuelve el contenido en función de la id
            var applicationContent =
                (from _content in _context.Content
                 where _content.Id == guid
                 select _content).FirstOrDefault();
            
            if(applicationContent == null)
            {
                return RedirectToAction("Index");
            }

            //Elimina el contenido
            //TODO En lugar de eliminarlo, ocultarlo en una papelera
            _context.Remove(applicationContent);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

        #region utils
        //Pasado un texto, elimina todo el contenido no ascii que contenga
        private string DeleteNonAscii(string text)
        {
            return Regex.Replace(text, @"[^\u0000-\u007F]+", string.Empty);
        }
        //Devuelve al usuario actual
        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }
        //Añade errores
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        //Añade un solo error simple
        private void AddError(String description)
        {
            AddErrors(IdentityResult.Failed(new IdentityError[] {
                    new IdentityError() {
                        Description = description
                    }
            }));
        }
        //Comprueba si los bytes corresponden 
        //a un binario o si por el contrario son texto
        private bool IsFileBinary(byte[] bytes)
        {
            for (int i = 0; i < bytes.Length; i++)
                if (bytes[i] > 127)
                    return true;
            return false;
        }
        //Pasado un texto, devuelve un FileStream con el contenido de este
        private FileStreamResult GetFileFromText(String text)
        {
            MemoryStream ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(text));
            return File(ms, "application/octet-stream"); // or "application/x-rar-compressed"
        }
        //Devuelve un array de bytes en función del stream
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