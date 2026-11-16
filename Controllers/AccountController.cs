using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartStock.Models;
using SmartStock.ViewModels;

namespace SmartStock.Controllers
{
    /// <summary>
    /// MVC authentication pages backed by ASP.NET Core Identity
    /// (replaces the scaffolded Identity Razor Pages).
    /// </summary>
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }

        // ------------------------------------------------------------------ Login

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} logged in.", model.Email);
                return await RedirectToLocalDashboardAsync(model.ReturnUrl, model.Email);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User {Email} locked out.", model.Email);
                ModelState.AddModelError(string.Empty, "Account locked after too many failed attempts. Please wait a few minutes and try again.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        // ---------------------------------------------------------------- Register

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User))
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Only the two roles defined by the system may be chosen at signup.
            if (model.Role != RoleNames.Admin && model.Role != RoleNames.Cashier)
            {
                ModelState.AddModelError(nameof(model.Role), "Please choose a valid account type.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.Email,
                Email = model.Email,
                EmailConfirmed = true // no mail server in this deployment
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                await _signInManager.SignInAsync(user, isPersistent: false);
                _logger.LogInformation("User {Email} registered as {Role}.", model.Email, model.Role);

                return await RedirectToLocalDashboardAsync(null, model.Email);
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // ------------------------------------------------------------------ Logout

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out.");
            return RedirectToAction("Index", "Home");
        }

        // ----------------------------------------------------------- Access denied

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied(string? returnUrl = null)
        {
            return View(new AccessDeniedViewModel { ReturnUrl = returnUrl });
        }

        // ----------------------------------------------------------------- Helpers

        /// <summary>
        /// After authenticating, send the user to the dashboard that matches their role,
        /// unless an explicit local return URL was requested. The freshly issued cookie's
        /// claims are not on the current principal yet, so roles are resolved through the
        /// user manager for the account that just signed in.
        /// </summary>
        private async Task<IActionResult> RedirectToLocalDashboardAsync(string? returnUrl, string email)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.IsInRoleAsync(user, RoleNames.Admin))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (user != null && await _userManager.IsInRoleAsync(user, RoleNames.Cashier))
            {
                return RedirectToAction("Index", "Cashier");
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>Routes an authenticated user to the dashboard that matches their role.</summary>
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> DashboardRedirect()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && await _userManager.IsInRoleAsync(user, RoleNames.Admin))
            {
                return RedirectToAction("Index", "Admin");
            }

            if (user != null && await _userManager.IsInRoleAsync(user, RoleNames.Cashier))
            {
                return RedirectToAction("Index", "Cashier");
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
