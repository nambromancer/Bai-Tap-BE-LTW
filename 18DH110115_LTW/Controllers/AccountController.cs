using _18DH110115_LTW.Models;
using _18DH110115_LTW.Models.ViewModel;
using System;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;

namespace _18DH110115_LTW.Controllers
{
    public class AccountController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // GET: Account/Register
        public ActionResult Register()
        {
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = db.Users.SingleOrDefault(u => u.Username == model.Username);
                    if (existingUser != null)
                    {
                        ModelState.AddModelError("Username", "Tên đăng nhập này đã tồn tại!");
                        return View(model);
                    }

                    var user = new User
                    {
                        Username = model.Username,
                        Password = model.Password,
                        UserRole = "Customer"
                    };
                    db.Users.Add(user);

                    var customer = new Customer
                    {
                        CustomerName = model.CustomerName,
                        CustomerEmail = model.CustomerEmail,
                        CustomerPhone = model.CustomerPhone,
                        CustomerAddress = model.CustomerAddress,
                        Username = model.Username
                    };
                    db.Customers.Add(customer);

                    db.SaveChanges();

                    // Lưu thông tin vào Session và Cookie
                    Session["Username"] = user.Username;
                    Session["UserRole"] = user.UserRole;
                    Session["CustomerName"] = customer.CustomerName;
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    return RedirectToAction("Index", "Home");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName,
                                $"{validationError.PropertyName}: {validationError.ErrorMessage}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi khi đăng ký: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Account/Login
        public ActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginVM model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var user = db.Users.SingleOrDefault(u => u.Username == model.Username
                                                          && u.Password == model.Password
                                                          && u.UserRole == "Customer");
                    if (user != null)
                    {
                        var customer = db.Customers.SingleOrDefault(c => c.Username == user.Username);

                        // Lưu thông tin vào Session và Cookie
                        Session["Username"] = user.Username;
                        Session["UserRole"] = user.UserRole;
                        Session["CustomerName"] = customer?.CustomerName;
                        FormsAuthentication.SetAuthCookie(user.Username, false);

                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Lỗi đăng nhập: {ex.Message}");
                }
            }

            return View(model);
        }

        // GET: Account/Logout
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}