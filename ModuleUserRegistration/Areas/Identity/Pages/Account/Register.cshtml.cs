﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using ModuleUserRegistration.Models;

namespace ModuleUserRegistration.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly IDepartmentRepository _deptRepo;
        private readonly IDivisionRepository _divRepo;


        public RegisterModel(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            IDepartmentRepository deptRepo,
            IDivisionRepository divRepo)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;

            _deptRepo = deptRepo;
            _divRepo = divRepo;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            public string Name { get; set; }
            [Required]
            public int DepartmentId { get; set; }
            [Required]
            public int DivisionId{ get; set; }
            [Required]
            public string Role { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            public IEnumerable<SelectListItem> DepartmentList { get; set; }
            public IEnumerable<SelectListItem> DivisionList { get; set; }
            public IEnumerable<SelectListItem> RoleList { get; set; }

        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            // Include dropdown in register page
            Input = new InputModel()
            {
                DepartmentList = _deptRepo.GetAll().Select(i => new SelectListItem
                {
                    Text = i.DepartmentName,
                    Value = i.Id.ToString()
                }),

                DivisionList = _divRepo.GetAll().Select(i => new SelectListItem
                {
                    Text = i.DivisionName,
                    Value = i.Id.ToString()
                }),

                //RoleList = _roleManager.Roles.Where(u => u.Name != StaticRoles.Admin).Select(x => x.Name).Select(i => new SelectListItem
                //{
                //    Text = i,
                //    Value = i
                //})

                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id
                })

            };
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                //var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
                var user = new ApplicationUser
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    DepartmentId = Input.DepartmentId,
                    DivisionId = Input.DivisionId,
                    Name = Input.Name,
                    Role = Input.Role
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");

                    if (!await _roleManager.RoleExistsAsync(StaticRoles.Admin))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(StaticRoles.Admin));
                    }
                    if (!await _roleManager.RoleExistsAsync(StaticRoles.EE))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(StaticRoles.EE));
                    }
                    if (!await _roleManager.RoleExistsAsync(StaticRoles.SuperAdmin))
                    {
                        await _roleManager.CreateAsync(new IdentityRole(StaticRoles.SuperAdmin));
                    }

                    // Assigning User to Some Role
                    //await _userManager.AddToRoleAsync(user, StaticRoles.EE);

                    await _userManager.AddToRoleAsync(user, StaticRoles.Admin);
                   

                    //var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    //code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    //var callbackUrl = Url.Page(
                    //    "/Account/ConfirmEmail",
                    //    pageHandler: null,
                    //    values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
                    //    protocol: Request.Scheme);

                    //await _emailSender.SendEmailAsync(Input.Email, "Confirm your email",
                    //    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
