/*using Microsoft.AspNetCore.Mvc;
using SchedulerV4.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Расписание.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            List<User> users = await _context.USERS.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                _context.USERS.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user); // Вернуть форму снова для исправления ошибок ввода
        }


        [HttpPost]
        public async Task<IActionResult> Edit(User user)
        {
            if (ModelState.IsValid)
            {
                _context.USERS.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.USERS.FindAsync(id);
            if (user != null)
            {
                _context.USERS.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}*/


/*using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulerV4.Models;
using System.Linq;

namespace SchedulerV4.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.USERS.Include(u => u.STUDY_GROUP).ToList();
            ViewBag.StudyGroups = _context.STUDY_GROUP.ToList();
            return View(users);
        }

        [HttpPost]
        public IActionResult Create(User user)
        {
            if (ModelState.IsValid)
            {
                _context.USERS.Add(user);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.StudyGroups = _context.STUDY_GROUP.ToList();
            return View("Index", _context.USERS.Include(u => u.STUDY_GROUP).ToList());
        }

        [HttpPost]
        public IActionResult Edit(int id)
        {
            var user = _context.USERS.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            ViewBag.StudyGroups = _context.STUDY_GROUP.ToList();
            return View(user);
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            var user = _context.USERS.Find(id);
            if (user != null)
            {
                _context.USERS.Remove(user);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}*/

////////////////////////////

/*using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulerV4.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerV4.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var users = _context.USERS.Include(u => u.STUDY_GROUP).ToList();
            return View(users);
        }

        *//*public IActionResult Create()
        {
            ViewBag.StudyGroup = _context.STUDY_GROUP.ToList();
            return View();
        }*//*

        // GET: User/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: User/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("ID,NAME,EMAIL,GROUP_ID")] User user)
        {
            if (ModelState.IsValid)
            {
                _context.Add(user);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // GET: User
        *//*public IActionResult Index()
        {
            var users = _context.Users.Include(u => u.STUDY_GROUP).ToList();
            return View(users);
        }*//*

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = _context.USERS
                               .Include(u => u.STUDY_GROUP)
                               .FirstOrDefault(u => u.ID == id);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.StudyGroups = _context.STUDY_GROUP.ToList();
            return View(user);
        }

        // POST: User/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("ID,NAME,EMAIL,GROUP_ID")] User user)
        {
            if (id != user.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingUser = _context.USERS
                                               .Include(u => u.STUDY_GROUP)
                                               .FirstOrDefault(u => u.ID == id);

                    if (existingUser == null)
                    {
                        return NotFound();
                    }

                    existingUser.NAME = user.NAME;
                    existingUser.EMAIL = user.EMAIL;

                    var studyGroup = _context.STUDY_GROUP.FirstOrDefault(g => g.ID == user.GROUP_ID);
                    if (studyGroup == null)
                    {
                        ModelState.AddModelError("GROUP_ID", "Invalid Group ID");
                        ViewBag.StudyGroups = _context.STUDY_GROUP.ToList();
                        return View(user);
                    }

                    existingUser.GROUP_ID = user.GROUP_ID;
                    existingUser.STUDY_GROUP = studyGroup;

                    _context.Update(existingUser);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.StudyGroups = _context.STUDY_GROUP.ToList();
            return View(user);
        }

        private bool UserExists(int id)
        {
            return _context.USERS.Any(e => e.ID == id);
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.USERS.FindAsync(id);
            if (user != null)
            {
                _context.USERS.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}*/

///////////////////////

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulerV4.Models;
using System;
using System.Linq;

namespace SchedulerV4.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserController> _logger;

        public UserController(ApplicationDbContext context, ILogger<UserController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IActionResult Index()
        {
            var users = from u in _context.USERS
                       
                        join g in _context.STUDY_GROUP on u.GROUP_ID equals g.ID
                        select new UserWithGroups
                        {
                            UserID = u.ID,
                            UserName = u.NAME,
                            UserEmail = u.EMAIL,
                            GroupID = g.ID,
                            GroupName = g.NANE
                        };

            var userslist = users.ToList();
            

            return View(userslist);
        }

        public IActionResult Create()
        {
            /*            ViewBag.StudyGroups*//**//* = _context.USERS.ToList();
            */
            return View();
        }

        [HttpPost]
        public IActionResult Create(UserEntity user)
        {
            if (ModelState.IsValid)
            {
                _logger.LogInformation("ya bebta");

                _context.USERS.Add(user);
                _logger.LogInformation($"{_context.SaveChanges()}");
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }


        

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _context.USERS.FindAsync(id);
            if (user != null)
            {
                _context.USERS.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.USERS.Find(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        public IActionResult Edit(UserEntity user)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(user);
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }
            return View(user);
        }

    }
}








