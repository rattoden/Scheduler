using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulerV4.Models;

namespace SchedulerV4.Controllers
{
    public class SotrudnikController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SotrudnikController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()

        {
            List<SotrudnikEntity> auditories = _context.SOTRUDNIK.ToList();
            return View(auditories);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SotrudnikEntity sotrudnik)
        {
            // Автоинкремент ID: находим максимальный ID и увеличиваем
            int maxId = _context.SOTRUDNIK.Count() > 0 ? _context.SOTRUDNIK.Max(s => s.ID_SOTR) : 0;
            sotrudnik.ID_SOTR = maxId + 1;
            // Обработка чекбокса "является деканом"
            sotrudnik.DEKAN = Request.Form["DEKAN"].Count > 0 ? 'Д' : 'Н';
            // Проверка уникальности табельного номера
            bool exists = _context.SOTRUDNIK.Count(a => a.TAB_NO == sotrudnik.TAB_NO) > 0;
            if (exists)
            {
                TempData["ErrorMessage"] = "Сотрудник с таким табельным номером уже существует.";
            }
            else
            {
                try
                {
                    _context.SOTRUDNIK.Add(sotrudnik);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Преподаватель успешно добавлен.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Ошибка при добавлении сотрудника: " + ex.Message;
                }
            }

            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Delete(int id)
        {
            var sotrudniki = await _context.SOTRUDNIK.FindAsync(id);
            if (sotrudniki != null)
            {
                _context.SOTRUDNIK.Remove(sotrudniki);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int id)
        {
            var building = await _context.SOTRUDNIK.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }
            return View(building);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(SotrudnikEntity sotrudnik)
        {
            sotrudnik.DEKAN = Request.Form["DEKAN"].Count > 0 ? 'Д' : 'Н';

            try
            {
                // Проверка на дубликат табельного номера, исключая текущую запись
                bool duplicateExists = await _context.SOTRUDNIK
                    .CountAsync(s => s.ID_SOTR != sotrudnik.ID_SOTR && s.TAB_NO == sotrudnik.TAB_NO) > 0;

                if (duplicateExists)
                {
                    TempData["ErrorMessage"] = "Сотрудник с таким табельным номером уже существует.";
                }
                else
                {
                    var existingSotrudnik = await _context.SOTRUDNIK.FindAsync(sotrudnik.ID_SOTR);
                    if (existingSotrudnik != null)
                    {
                        _context.Entry(existingSotrudnik).CurrentValues.SetValues(sotrudnik);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Информация о сотруднике успешно обновлена.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Сотрудник с таким идентификатором не найден.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении сотрудника: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }


    }
}
