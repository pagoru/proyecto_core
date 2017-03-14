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

        public IActionResult Index()
        {
            var model = new IndexViewModel()
            {
                IdentityRoleList = _roleManager.Roles.ToList()
            };
            return View(model);
        }
        
        public IActionResult CreateRole()
        {
            return View();
        }

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
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            if (userName.Length == 0)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            var user = _userManager.Users.FirstOrDefault(u => u.UserName == userName);
            if(user == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            var role = _roleManager.Roles.FirstOrDefault(r => r.Name == roleName);
            if(role == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            if (!user.IsInRole(role))
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }

            await _userManager.RemoveFromRoleAsync(user, roleName);
            await CloseUserSession(userName);

            return RedirectToAction(nameof(AdminController.DetailsRole) + $"/{idParts[0]}", "Admin");
        }

        public async Task<IActionResult> RemoveRole(string id)
        {
            if (!await _roleManager.RoleExistsAsync(id))
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }

            var role = _roleManager.Roles.First(r => r.Name == id);
            await _roleManager.DeleteAsync(role);

            return RedirectToAction(nameof(AdminController.Index), "Admin");
        }

        public async Task<IActionResult> AddUserRole(string id)
        {
            if (!await _roleManager.RoleExistsAsync(id))
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }

            var model = new AddUserRoleViewModel()
            {
                IdentityRole = GetRole(id),
                ApplicationUserList = await GetUsersNotInRole(id)
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddUserRole(AddUserRoleViewModel model)
        {
            if(model == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            if(model.RoleName == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            if(model.UserName == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            if(model.RoleName.Length == 0)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            if(model.UserName.Length == 0)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            
            if(!await _roleManager.RoleExistsAsync(model.RoleName))
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }

            var user = GetUser(model.UserName);
            if (user == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }

            await _userManager.AddToRoleAsync(user, model.RoleName);
            await CloseUserSession(model.UserName);

            return RedirectToAction(nameof(AdminController.DetailsRole) + $"/{model.RoleName}", "Admin");
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel model)
        {
            if(model.Name == null)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            if(model.Name.Length == 0)
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            
            if(await _roleManager.RoleExistsAsync(model.Name))
            {
                AddError("Error");
                return RedirectToAction(nameof(AdminController.Index), "Admin");
            }
            await _roleManager.CreateAsync(new IdentityRole(model.Name));

            return RedirectToAction(nameof(AdminController.DetailsRole) + $"/{model.Name}", "Admin");
        }
        
        // id => name of the role
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

        private void AddError(String description)
        {
            AddErrors(IdentityResult.Failed(new IdentityError[] {
                    new IdentityError() {
                        Description = description
                    }
            }));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        public async Task<IdentityResult> CloseUserSession(string UserName)
        {
            return await _userManager.UpdateSecurityStampAsync(GetUser(UserName));
        }

        public ApplicationUser GetUser(string UserName)
        {
            return _userManager.Users.FirstOrDefault(u => u.UserName == UserName);
        }

        public IdentityRole GetRole(string roleName)
        {
            return _roleManager.Roles.First(r => r.Name == roleName);
        }
        
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
