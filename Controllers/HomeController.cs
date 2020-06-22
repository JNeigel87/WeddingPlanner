using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using WeddingPlanner.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace WeddingPlanner.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext { get; set; }

        public HomeController(MyContext context)
        {
            dbContext = context;
        }

        public  User GetUser()
        {
            return dbContext.Users.FirstOrDefault(u => u.UserId == HttpContext.Session.GetInt32("UserId"));
        }

        [HttpGet("")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("process/registration")]
        public IActionResult ProcessRegistration(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (dbContext.Users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                else
                {
                    // Initializing a PasswordHasher object, providing our class as its type
                    PasswordHasher<User> Hasher = new PasswordHasher<User>();
                    newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                    //Save your user object to the database
                    dbContext.Add(newUser);
                    dbContext.SaveChanges();
                    HttpContext.Session.SetInt32("UserId", newUser.UserId);
                    return Redirect("/dashboard");
                }
            }
            else
            {
                return View("Index");
            }
        }

        [HttpPost("process/login")]
        public IActionResult ProcessLogin(Login newLog)
        {
            if (ModelState.IsValid)
            {
                // If inital ModelState is valid, query for a user with provided email
                var userInDb = dbContext.Users.FirstOrDefault(u => u.Email == newLog.LoginEmail);
                // If no user exists with provided email
                if (userInDb == null)
                {
                    // Add an error to ModelState and return to View!
                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("Index");
                }
                else
                {
                    // Initialize hasher object
                    var hasher = new PasswordHasher<Login>();
                    // verify provided password against hash stored in db
                    var result = hasher.VerifyHashedPassword(newLog, userInDb.Password, newLog.LoginPassword);
                    if (result == 0)
                    {
                        ModelState.AddModelError("LoginPassword", "Invalid Email/Password");
                        return View("Index");
                    }
                    else
                    {
                        //store userId in session
                        HttpContext.Session.SetInt32("UserId", userInDb.UserId);
                        return RedirectToAction("Dashboard");
                    }
                }
            }
            else
            {
                return View("Index");
            }
        }

        [HttpGet("dashboard")]
        public IActionResult Dashboard(Login newLog)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect("/");
            }
            ViewBag.User = userInDb;
            List<Wedding> AllWeddings = dbContext.Weddings
                                                .Include(w => w.Planner)
                                                .Include(w => w.Guests)
                                                .ThenInclude(gl => gl.WeddingGuest)
                                                .Where(w => w.Date >= DateTime.Now)
                                                .OrderBy(w => w.Date)
                                                .ToList();
            return View(AllWeddings);
        }

        [HttpGet("wedding/new")]
        public IActionResult NewWedding()
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            return View();
        }

        [HttpPost("wedding/create")]
        public IActionResult CreateWedding(Wedding newWedding)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            if(ModelState.IsValid)
            {
                newWedding.UserId = userInDb.UserId;
                dbContext.Weddings.Add(newWedding);
                dbContext.SaveChanges();
                GuestList g = new GuestList();
                g.UserId = userInDb.UserId;
                g.WeddingId = newWedding.WeddingId;
                dbContext.GuestLists.Add(g);
                dbContext.SaveChanges();
                return RedirectToAction("Dashboard");
            }
            return View("NewWedding");
        }

        [HttpGet("wedding/{weddingId}/delete")]
        public IActionResult DeleteWedding(int weddingId)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            Wedding remove = dbContext.Weddings.FirstOrDefault(w => w.WeddingId == weddingId);
            dbContext.Weddings.Remove(remove);
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("wedding/{weddingId}/{status}")]
        public IActionResult ToggleStatus(int weddingId, string status)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            if(status == "join")
            {
                GuestList newStatus = new GuestList();
                newStatus.UserId = userInDb.UserId;
                newStatus.WeddingId = weddingId;
                dbContext.GuestLists.Add(newStatus);
            }
            else if(status == "leave")
            {
                GuestList backout = dbContext.GuestLists.FirstOrDefault(gl => gl.UserId == userInDb.UserId && gl.WeddingId == weddingId);
                dbContext.GuestLists.Remove(backout);
            }
            dbContext.SaveChanges();
            return RedirectToAction("Dashboard");
        }

        [HttpGet("wedding/{weddingId}")]
        public IActionResult DisplayWedding(int weddingId)
        {
            User userInDb = GetUser();
            if (userInDb == null)
            {
                return Redirect ("/");
            }
            Wedding displaying = dbContext.Weddings
                                    .Include(w => w.Guests)
                                    .ThenInclude(gl => gl.WeddingGuest)
                                    .Include(w => w.Planner)
                                    .FirstOrDefault(w => w.WeddingId == weddingId );
            return View(displaying);
        }


        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return View("Index");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
