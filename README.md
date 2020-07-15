## User Registration and Login

### Create Models

```csharp
namespace ModuleUserRegistration.Models
{
    public class Department
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Deparment Name")]
        public string DepartmentName { get; set; }
    }
}
```

```csharp
namespace ModuleUserRegistration.Models
{
    public class Division
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Division Name")]
        public string DivisionName { get; set; }
    }
}
```

```csharp
namespace ModuleUserRegistration.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string Name { get; set; }
        public int DepartmentId { get; set; }
        public int DivisionId { get; set; }

        [NotMapped] // Not Added to the database
        public string Role { get; set; }

        [ForeignKey("DepartmentId")]
        public Department Department { get; set; }
        [ForeignKey("DivisionId")]
        public Division Division { get; set; }
    }
}
```

### Create Static Roles Class

```csharp
namespace ModuleUserRegistration
{
    public static class StaticRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string EE = "ExecutiveEngineer";
    }
}
```

### Add Models to Application DB Context

```csharp
namespace ModuleUserRegistration.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Department> Department { get; set; }
        public DbSet<Division> Division { get; set; }
        public DbSet<ApplicationUser> ApplicationUser { get; set; }
    }
}
```

### Migrate and Update Database
```bash
add-migration AddModels
update-database
```

### Creating Repository
IDepartmentRepository.cs
```csharp
namespace ModuleUserRegistration.Models
{
    public interface IDepartmentRepository
    {
        IEnumerable<Department> GetAll();
    }
}
```

DepartmentRepository.cs
```csharp
namespace ModuleUserRegistration.Models
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _db;

        public DepartmentRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public IEnumerable<Department> GetAll()
        {
            var all_dept = _db.Department.ToList();
            return all_dept;
        }
    }
}
```
Configure Startup.cs

```csharp
services.AddScoped<IDepartmentRepository, DepartmentRepository>();
```

### Scaffolding Account Pages

* Right click on project and select new scafolded item.
* Select identity > add.
* Select layout.
* Select all pages and select application db context.

### Edit Register.cs

```csharp
public class InputModel
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Required]
    public string Name { get; set; }
    public int DepartmentId { get; set; }
    public int DivisionId { get; set; }
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
}
```

### Edit Register.cshtml

```csharp
<div class="form-group">
    <label asp-for="Input.Name"></label>
    <input asp-for="Input.Name" class="form-control" />
    <span asp-validation-for="Input.Name" class="text-danger"></span>
</div>
```

### Edit OnPostAsync()

**REMOVE**
```csharp
 var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
```
> Also Comment Email Confirmation Code.

**ADD**
```csharp
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
 }
```

### Inject RoleManager and Database Accessing Class

```csharp
public class RegisterModel : PageModel
{
    private readonly RoleManager<IdentityRole> _roleManager;
    public RegisterModel(RoleManager<IdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }
}
```

### Create Role and Assign to the User 
(Not Good Practice, Just for Demo)\
Register.cs

```csharp
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
       await _userManager.AddToRoleAsync(user, StaticRoles.EE);
}
```

### Edit Startup.cs

```csharp
services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders().AddEntityFrameworkStores<ApplicationDbContext>();
```


### Email Providers Error Solution
Microsoft.AspNetCore.Identity.UI.Services.IEmailSender

**Configure under Startup.cs**
```csharp
services.AddSingleton<IEmailSender, EmailSender>();
```

**Create Class**
```csharp
namespace BulkyBook.Utility
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            throw new NotImplementedException();
        }
    }
}
```

### Dropdown Selection Field

Register.cs

Declaration
```csharp
private readonly IDepartmentRepository _deptRepo;
private readonly IDivisionRepository _divRepo;
```

Under InputModel
```csharp
public IEnumerable<SelectListItem> DepartmentList { get; set; }
public IEnumerable<SelectListItem> DivisionList { get; set; }
public IEnumerable<SelectListItem> RoleList { get; set; }
```

Modify OnGetAsync()
```csharp
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
```

Edit Register.cshtml

```csharp
<div class="form-group">
  <label asp-for="Input.DepartmentId"></label>
  @Html.DropDownListFor(m => m.Input.DepartmentId, Model.Input.DepartmentList, "-Please select department", new { @class = "form-control" })
</div>
<div class="form-group">
  <label asp-for="Input.DivisionId"></label>
  @Html.DropDownListFor(m => m.Input.DivisionId, Model.Input.DivisionList, "-Please select division", new { @class = "form-control" })
</div>
<div class="form-group">
  <label asp-for="Input.Role"></label>
  @Html.DropDownListFor(m => m.Input.Role, Model.Input.RoleList, "-Please select a role", new { @class = "form-control" })
</div>

  @* if (User.IsInRole(SD.Role_Admin))
  {
      // Display FILTER Based on Roles
  }*@
```

Edit Register.cs (OnPostAsync)

```csharp
**REMOVE**
// Assigning User to Some Role
await _userManager.AddToRoleAsync(user, StaticRoles.EE);

**ADD**
await _userManager.AddToRoleAsync(user, user.Role);
```
