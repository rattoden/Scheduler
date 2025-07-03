using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulerV4.Models;

namespace SchedulerV4.Controllers
{
    public class GroupsController : Controller
    {
        // GET: GroupsController


        private readonly ApplicationDbContext _context;

        public GroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()

        {
            List<GroupEntity> groups = await _context.STUDY_GROUP.ToListAsync();
            return View(groups);
        }



        [HttpPost]
        public async Task<IActionResult> Create(GroupEntity group)
        {
            if (ModelState.IsValid)
            {
                _context.STUDY_GROUP.Add(group);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(group); // Вернуть форму снова для исправления ошибок ввода
        }


        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.STUDY_GROUP.FindAsync(id);
            if (group != null)
            {
                _context.STUDY_GROUP.Remove(group);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public IActionResult Edit(GroupEntity group)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Убедитесь, что только один экземпляр группы отслеживается
                    var existingGroup = _context.STUDY_GROUP.Find(group.ID);
                    if (existingGroup != null)
                    {
                        _context.Entry(existingGroup).CurrentValues.SetValues(group);
                    }
                    else
                    {
                        _context.STUDY_GROUP.Update(group);
                    }

                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbUpdateConcurrencyException)
                {
                    // Обработка исключения при необходимости
                    throw;
                }
            }

            return View(group);
        }


    }
}
