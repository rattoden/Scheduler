using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SchedulerV4.Models;

namespace SchedulerV4.Controllers
{
    public class SprBuildingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SprBuildingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()

        {
            List<SprBuildingEntity> buildings = _context.SPR_BUILDING.ToList();
            return View(buildings);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SprBuildingEntity building)
        {
            building.NAME = building.ID_BUILDING + " здание";
            bool exists = _context.SPR_BUILDING.Count(b => b.ID_BUILDING == building.ID_BUILDING) > 0;
            if (exists)
            {
                TempData["ErrorMessage"] = "Здание с таким номером уже существует.";
            }
            else
            {
                try
                {
                    _context.SPR_BUILDING.Add(building);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Здание добавлено.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Произошла ошибка при добавлении здания: " + ex.Message;
                }
            }
            return RedirectToAction(nameof(Index)); // Вернуть форму снова для исправления ошибок ввода
        }

        // 👇 вспомогательный метод для безопасного получения списка зданий
        private async Task<IActionResult> ReturnIndexWithData()
        {
            var buildings = await _context.SPR_BUILDING.ToListAsync();
            return View("Index", buildings);
        }


        public async Task<IActionResult> Delete(int id)
        {
            var building = await _context.SPR_BUILDING.FindAsync(id);
            if (building != null)
            {
                // Найти все аудитории, связанные с этим зданием
                var relatedAuditories = _context.SPR_AUDITORY
                    .Where(a => a.ID_BUILDING == id);

                // Обнулить ссылку на здание
                foreach (var auditory in relatedAuditories)
                {
                    auditory.ID_BUILDING = 0;
                }

                await _context.SaveChangesAsync(); // Сохранить изменения до удаления здания

                _context.SPR_BUILDING.Remove(building);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Edit(int id)
        {
            var building = await _context.SPR_BUILDING.FindAsync(id);
            if (building == null)
            {
                return NotFound();
            }
            return View(building);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(SprBuildingEntity building)
        {
            try
            {
                building.NAME = building.ID_BUILDING + " здание";
                // Проверка: существует ли здание с таким ID
                var existingBuilding = await _context.SPR_BUILDING.FindAsync(building.ID_BUILDING);

                if (existingBuilding != null)
                {
                    // Обновляем значения существующего здания
                    _context.Entry(existingBuilding).CurrentValues.SetValues(building);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Здание успешно обновлено.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Здание с таким номером не найдено.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении здания: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

