﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using proyecto_core.Models;
using proyecto_core.Models.ManageViewModels;
using proyecto_core.Services;
using proyecto_core.Data;
using proyecto_core.Consts;

namespace proyecto_core.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly string _externalCookieScheme;
        private readonly ILogger _logger;

        public ManageController(
          UserManager<ApplicationUser> userManager,
          SignInManager<ApplicationUser> signInManager,
          IOptions<IdentityCookieOptions> identityCookieOptions,
          ILoggerFactory loggerFactory)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _externalCookieScheme = identityCookieOptions.Value.ExternalCookieAuthenticationScheme;
            _logger = loggerFactory.CreateLogger<ManageController>();
        }

        //
        // GET: /Manage/Index
        [HttpGet]
        public async Task<IActionResult> Index(ManageMessageId? message = null)
        {
            //Asigna el text de error en función del mensaje de error proporcionado
            ViewData["StatusMessage"] =
                message == ManageMessageId.ChangePasswordSuccess ? "Tu contraseña se ha cambiado con éxito."
                : message == ManageMessageId.Error ? "Ha courrido un error inesperado."
                : message == ManageMessageId.ChangeEmailSuccess ? "Tu correo electrónico se ha cambiado con éxito"
                : "";

            var user = await GetCurrentUserAsync();
            if (user == null)
            {
                return View("Error");
            }
            var model = new IndexViewModel
            {
                ApplicationUser = user,
                //Devuelve si el usuario actual es administrador
                IsAdmin = await _userManager.IsInRoleAsync(user, WebRoles.Admin),
                
                Logins = await _userManager.GetLoginsAsync(user),
                BrowserRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user)
            };
            return View(model);
        }

        //
        // GET: /Manage/ChangeName
        [HttpGet]
        public async Task<IActionResult> ChangeName()
        {
            var user = await GetCurrentUserAsync();
            var model = new ChangeNameViewModel
            {
                ApplicationUser = user
            };
            return View(model);
        }

        // 
        // POST: /Manage/ChangeName
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeName(ChangeNameViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            model.ApplicationUser = user;

            if (user != null)
            {
                user.Name = model.NewName;
                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "El usuario ha cambiado con éxito su nombre.");
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangeNameSuccess});
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //
        // GET: /Manage/ChangeEmail
        [HttpGet]
        public async Task<IActionResult> ChangeEmail()
        {
            var user = await GetCurrentUserAsync();
            var model = new ChangeEmailViewModel
            {
                ApplicationUser = user
            };
            return View(model);
        }

        //
        // POST: /Manage/ChangeEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await GetCurrentUserAsync();
            model.ApplicationUser = user;

            //Verificación conforme el correo electrónico esta en uso y su error correspondiente
            if (await IsEmailInUse(model.NewEmail))
            {
                AddErrors(getEmailInUseResult());
                return View(model);
            }

            if (user != null)
            {
                //TODO Se deberia enviar el token al correo para poder hacer una confirmación posterior, no cambiar directamente aquí
                var token = _userManager.GenerateChangeEmailTokenAsync(user, model.NewEmail).Result;
                var result = await _userManager.ChangeEmailAsync(user, model.NewEmail, token);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "El usuario ha cambiado con éxito su correo electrónico.");
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangeEmailSuccess });
                }
                AddErrors(result);
                return View(model);
            }

            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await GetCurrentUserAsync();
            if (user != null)
            {
                var result = await _userManager.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    _logger.LogInformation(3, "El usuario ha cambiado con éxito su contraseña.");
                    return RedirectToAction(nameof(Index), new { Message = ManageMessageId.ChangePasswordSuccess });
                }
                AddErrors(result);
                return View(model);
            }
            return RedirectToAction(nameof(Index), new { Message = ManageMessageId.Error });
        }

        #region Helpers
        //Añade errores al modelo actual
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        //Enumeración de los mensajes disponibles
        public enum ManageMessageId
        {
            AddLoginSuccess,
            ChangePasswordSuccess,
            ChangeEmailSuccess,
            ChangeNameSuccess,
            Error
        }
        //Devuelve al usuario actual
        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        //verifica si el correo ya esta en uso
        private async Task<bool> IsEmailInUse(string email)
        {
            var emailIsInUse = await _userManager.FindByEmailAsync(email);

            return (emailIsInUse != null);
        }

        //Devuelve un error por correo electrónico en uso
        private IdentityResult getEmailInUseResult()
        {
            return IdentityResult.Failed(new IdentityError[] {
                new IdentityError() {
                    Description = "El correo electrónico ya esta en uso."
                }
            });
        }

        #endregion
    }
}
