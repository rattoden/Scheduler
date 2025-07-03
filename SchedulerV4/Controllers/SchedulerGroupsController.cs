using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulerV4.Models;

namespace SchedulerV4.Controllers
{
    public class SchedulerGroupsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SchedulerGroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()

        {
            List<GroupsEntity> groups = _context.GROUPS.ToList();
            return View(groups);
        }



        [HttpPost]
        public async Task<IActionResult> Create(GroupsEntity group)
        {
            int maxId = _context.GROUPS.Count() > 0 ? _context.GROUPS.Max(a => a.GROUPID) : 0;
            group.GROUPID = maxId + 1;
            if (Request.Form["GRINT"] == "null")
                group.GRINT = null;

            bool exists = _context.GROUPS.Count(g => g.GROUPNO == group.GROUPNO && g.YEARF == group.YEARF) > 0;
            if (exists)
            {
                TempData["ErrorMessage"] = "Группа с таким номером и учебным годом уже существует.";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                _context.GROUPS.Add(group);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Группа успешно добавлена.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при добавлении группы: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int id)
        {
            var group = await _context.GROUPS.FindAsync(id);
            if (group != null)
            {
                _context.GROUPS.Remove(group);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int id)
        {
            var auditory = await _context.GROUPS.FindAsync(id);
            if (auditory == null)
            {
                return NotFound();
            }
            return View(auditory);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(GroupsEntity group)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingAuditory = await _context.GROUPS.FindAsync(group.GROUPID);
                    if (existingAuditory != null)
                    {
                        _context.Entry(existingAuditory).CurrentValues.SetValues(group);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        return NotFound();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
            // Возвращаем представление с моделью при наличии ошибок
            return View(group);
        }


    }
}
