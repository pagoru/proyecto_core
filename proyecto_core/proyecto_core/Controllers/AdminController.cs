using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using proyecto_core.Consts;
using proyecto_core.Models;
using proyecto_core.Models.AdminViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace proyecto_core.Controllers
{
    /**
     * Solo se autoriza a estas páginas a administradores
     **/
    [Authorize(Roles = WebRoles.Admin)]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //
        // GET: /Admin
        public IActionResult Index()
        {
            var model = new IndexViewModel()
            {
                IdentityRoleList = _roleManager.Roles.ToList()
            };
            return View(model);
        }

        //
        // GET: /Admin/CreateRole
        public IActionResult CreateRole()
        {
            return View();
        }

        //
        // GET: /Admin/RemoveUserRole:id [id = "roleName-userName"]
        public async Task<IActionResult> RemoveUserRole(string id)
        {
            if(id == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");

            }
            if(id.Length == 0)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            var idParts = id.Split('-');
            //Se comprueba que la id este compuesta de dos partes desde un guión de división.
            if(idParts.Length != 2)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            var roleName = idParts[0];
            var userName = idParts[1];
            if (roleName.Length == 0)
            {
                AddError("Error");
            }
            if (userName.Length == 0)
            {
                AddError("Error");
            }
            //Devuelve un usuario especifico a raiz de su nombre de usuario unico
            var user = _userManager.Users.FirstOrDefault(u => u.UserName == userName);
            if (user == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            //Devuelve un rol especifico a raiz del nombre de este rol
            var role = _roleManager.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            //Comprueba si el usuario esta en el rol especificado
            if (!await _userManager.IsInRoleAsync(user, role.Name))
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            //Eliminar al usuario del rol
            await _userManager.RemoveFromRoleAsync(user, roleName);
            //Cierra la sesión del usuario
            await CloseUserSession(userName);

            return RedirectToAction(nameof(AdminController.DetailsRole) + $"/{idParts[0]}", "Admin");
        }

        //
        // GET: /Admin/RemoveRole:id
        public async Task<IActionResult> RemoveRole(string id)
        {
            //Comprueba si el Rol indicado no existe
            if (!await _roleManager.RoleExistsAsync(id))
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            //Selecciona el rol indicado
            var role = _roleManager.Roles.First(r => r.Name == id);
            //Elimina el rol
            await _roleManager.DeleteAsync(role);

            return RedirectToAction(nameof(AdminController.Index), "Admin");
        }

        //
        // GET: /Admin/AddUserRole:id 
        public async Task<IActionResult> AddUserRole(string id)
        {
            //Comprueba si el Rol indicado no existe
            if (!await _roleManager.RoleExistsAsync(id))
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            //Construye el nuevo modelo a raiz del rol seleccionado 
            //y de los usuarios que aun no pertenecen a ese rol
            var model = new AddUserRoleViewModel()
            {
                IdentityRole = GetRole(id),
                ApplicationUserList = await GetUsersNotInRole(id)
            };
            return View(model);
        }

        //
        // POST: /Admin/AddUserRole:id 
        [HttpPost]
        public async Task<IActionResult> AddUserRole(AddUserRoleViewModel model)
        {
            if (!ModelState.IsValid) {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }

            //Comprueba si el Rol indicado no existe dentro del modelo
            if (!await _roleManager.RoleExistsAsync(model.RoleName))
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }

            //Devuelve el usuario a partir del nombre de usuario unico
            var user = GetUser(model.UserName);
            if (user == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            //Añade al usuario especificado un rol
            await _userManager.AddToRoleAsync(user, model.RoleName);
            //Cierra la sessión del usuario para aplicar cambios
            await CloseUserSession(model.UserName);

            return RedirectToAction(nameof(AdminController.DetailsRole) + $"/{model.RoleName}", "Admin");
        }

        //
        // POST: /Admin/AddUserRole -> CreateRoleViewModel 
        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            //Comprueba si el nombre del rol existe, en ese caso, dará error
            if(await _roleManager.RoleExistsAsync(model.Name))
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            //Crea el rol
            await _roleManager.CreateAsync(new IdentityRole(model.Name));

            return RedirectToAction(nameof(AdminController.DetailsRole) + $"/{model.Name}", "Admin");
        }

        //
        // POST: /Admin/AddUserRole:id [id = Nombre del rol] 
        public async Task<IActionResult> DetailsRole(string id)
        {
            if(id == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            if(id.Length == 0)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            if(!await _roleManager.RoleExistsAsync(id))
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }

            var model = new DetailsRoleViewModel()
            {
                IdentityRole = GetRole(id),
                ApplicationUserList = await GetUsersInRole(id)
            };
            return View(model);
        }


        #region utils
        //Añade un único error al contexto de modelo actual
        private void AddError(String description)
        {
            AddErrors(IdentityResult.Failed(new IdentityError[] {
                    new IdentityError() {
                        Description = description
                    }
            }));
        }
        //Aañde errores al contexto de modelo actual
        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }
        //Cierra la sessión del usuario, cambiando una clave de seguridad del usuario
        public async Task<IdentityResult> CloseUserSession(string UserName)
        {
            return await _userManager.UpdateSecurityStampAsync(GetUser(UserName));
        }
        //Devuelve al usuario pasado el nombre único
        public ApplicationUser GetUser(string UserName)
        {
            return _userManager.Users.FirstOrDefault(u => u.UserName == UserName);
        }
        //Devuelve un rol pasada su nombre
        public IdentityRole GetRole(string roleName)
        {
            return _roleManager.Roles.First(r => r.Name == roleName);
        }
        //Devuelve una lista de usuarios que no pertenecen a un determinado rol
        public async Task<List<ApplicationUser>> GetUsersNotInRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            var usersRole = role.Users;
            var usersInRole = _userManager.Users.Where(u => u.Roles.Where(r => r.RoleId == role.Id).FirstOrDefault() == null).ToList();
            return usersInRole;
        }
        //Alternativa => '_userManager.GetUsersInRoleAsync(roleName)'
        public async Task<List<ApplicationUser>> GetUsersInRole(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);
            var usersRole = role.Users;
            var usersInRole = _userManager.Users.Where(u => u.Roles.Where(r => r.RoleId == role.Id).FirstOrDefault() != null).ToList();
            return usersInRole;
        }

        #endregion
    }
}
