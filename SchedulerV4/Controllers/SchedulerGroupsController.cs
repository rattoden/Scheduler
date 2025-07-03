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
            try
            {
                // Обработка nullable GRINT
                var grintValue = Request.Form["GRINT"].ToString();
                if (string.IsNullOrWhiteSpace(grintValue) || grintValue == "null")
                    group.GRINT = null;
                else if (int.TryParse(grintValue, out int parsedGrint))
                    group.GRINT = parsedGrint;

                // Проверка на дубликат GROUPNO + YEARF, исключая текущую группу
                bool duplicateExists = await _context.GROUPS
                    .CountAsync(g => g.GROUPID != group.GROUPID &&
                                   g.GROUPNO == group.GROUPNO &&
                                   g.YEARF == group.YEARF) > 0;

                if (duplicateExists)
                {
                    TempData["ErrorMessage"] = "Группа с таким номером и учебным годом уже существует.";
                    return RedirectToAction(nameof(Index));
                }

                // Обновление записи
                var existingGroup = await _context.GROUPS.FindAsync(group.GROUPID);
                if (existingGroup == null)
                {
                    TempData["ErrorMessage"] = "Группа не найдена.";
                    return RedirectToAction(nameof(Index));
                }

                _context.Entry(existingGroup).CurrentValues.SetValues(group);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Информация о группе успешно обновлена.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Ошибка при обновлении группы: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
