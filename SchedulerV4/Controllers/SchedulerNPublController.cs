using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchedulerV4.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SchedulerV4.Controllers
{
    public class SchedulerNPublController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SchedulerNPublController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Index
        public IActionResult Index(int? selectedGroup)
        {
            // Получение списка групп для селектбокса
            var groups = _context.GROUPS
                .Select(g => new SelectListItem
                {
                    Value = g.GROUPID.ToString(),
                    Text = g.GROUPNO.ToString()
                })
                .ToList();
            ViewBag.Groups = groups;

            // Передача выбранной группы обратно в представление
            ViewBag.SelectedGroup = selectedGroup?.ToString();

            // Получаем расписание с включением дисциплин
            var scheduler = _context.SHEDULE_N_PUBL
                .Include(s => s.Discipline) // Включаем данные дисциплин
                .Where(s => !selectedGroup.HasValue || s.GROUPID == selectedGroup.Value)
                .ToList();

            return View(scheduler);
        }


        // GET: Create
        // GET: Create
        // GET: Create
        // GET: Create
        // GET: Create
        public IActionResult Create()
        {
            // Получение списка групп
            var groups = _context.GROUPS
                .Select(g => new SelectListItem
                {
                    Value = g.GROUPID.ToString(),
                    Text = g.GROUPNO.ToString()
                })
                .ToList();
            ViewBag.Groups = groups;

            // Список дней недели
            var daysOfWeek = new List<SelectListItem>
    {
        new SelectListItem { Value = "ПОНЕДЕЛЬНИК", Text = "ПОНЕДЕЛЬНИК" },
        new SelectListItem { Value = "ВТОРНИК", Text = "ВТОРНИК" },
        new SelectListItem { Value = "СРЕДА", Text = "СРЕДА" },
        new SelectListItem { Value = "ЧЕТВЕРГ", Text = "ЧЕТВЕРГ" },
        new SelectListItem { Value = "ПЯТНИЦА", Text = "ПЯТНИЦА" },
        new SelectListItem { Value = "СУББОТА", Text = "СУББОТА" }
    };
            ViewBag.DaysOfWeek = daysOfWeek;

            // Изначально возвращаем все возможные временные слоты
            ViewBag.Times = GetAllTimeSlots();

            // Опции для поля DATA
            ViewBag.DataOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "NULL", Text = "NULL" },
        new SelectListItem { Value = "Чет", Text = "Чет" },
        new SelectListItem { Value = "Неч", Text = "Неч" }
    };

            // Опции для FORM_ZAN
            ViewBag.FormZanOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "Лек.", Text = "Лек." },
        new SelectListItem { Value = "Прак.", Text = "Прак." },
        new SelectListItem { Value = "Л.р.", Text = "Л.р." }
    };

            // Опции для ZDANIE
            var buildings = _context.SPR_BUILDING
                .Select(b => new SelectListItem
                {
                    Value = b.ID_BUILDING.ToString(),
                    Text = b.NAME.ToString(),
                })
                .ToList();

            ViewBag.Buildings = buildings;

            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ScheduleNPublEntity schedule)
        {
            if (!ModelState.IsValid)
            {
                // Перезаполнение данных для формы в случае ошибки
                ViewBag.Groups = _context.GROUPS
                    .Select(g => new SelectListItem
                    {
                        Value = g.GROUPID.ToString(),
                        Text = g.GROUPNO.ToString()
                    })
                    .ToList();

                ViewBag.DaysOfWeek = new List<SelectListItem>
        {
            new SelectListItem { Value = "ПОНЕДЕЛЬНИК", Text = "ПОНЕДЕЛЬНИК" },
            new SelectListItem { Value = "ВТОРНИК", Text = "ВТОРНИК" },
            new SelectListItem { Value = "СРЕДА", Text = "СРЕДА" },
            new SelectListItem { Value = "ЧЕТВЕРГ", Text = "ЧЕТВЕРГ" },
            new SelectListItem { Value = "ПЯТНИЦА", Text = "ПЯТНИЦА" },
            new SelectListItem { Value = "СУББОТА", Text = "СУББОТА" }
        };
                ViewBag.Times = GetAllTimeSlots();

                ViewBag.DataOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "NULL", Text = "NULL" },
            new SelectListItem { Value = "Чет", Text = "Чет" },
            new SelectListItem { Value = "Неч", Text = "Неч" }
        };

                ViewBag.FormZanOptions = new List<SelectListItem>
        {
            new SelectListItem { Value = "Лек.", Text = "Лек." },
            new SelectListItem { Value = "Прак.", Text = "Прак." },
            new SelectListItem { Value = "Л.р.", Text = "Л.р." }
        };

                // Опции для ZDANIE
                var buildings = _context.SPR_BUILDING
                    .Select(b => new SelectListItem
                    {
                        Value = b.ID_BUILDING.ToString(),
                        Text = b.NAME
                    })
                    .ToList();

                ViewBag.Buildings = buildings;

                return View(schedule);
            }

            try
            {
                _context.SHEDULE_N_PUBL.Add(schedule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Произошла ошибка при добавлении расписания.");
                return View(schedule);
            }
        }







        /// GET: Method to filter available times based on the selected group and day of the week
        public IActionResult GetAvailableTimeSlots(int groupId, string dayOfWeek)
        {
            try
            {
                Console.WriteLine($"Group ID: {groupId}, Day of Week: {dayOfWeek}");

                // Получаем занятые временные слоты для указанной группы и дня недели
                var occupiedTimes = _context.SHEDULE_N_PUBL
                    .Where(s => s.GROUPID == groupId && s.DEN.Trim().ToUpper() == dayOfWeek.Trim().ToUpper())
                    .Select(s => s.VREM.Trim())
                    .ToList();

                if (occupiedTimes.Any())
                {
                    Console.WriteLine($"Occupied time slots: {string.Join(", ", occupiedTimes)}");
                }
                else
                {
                    Console.WriteLine("No occupied time slots found for this group and day.");
                }

                // Получаем все возможные временные слоты
                var allTimeSlots = GetAllTimeSlots();

                // Фильтруем свободные временные слоты
                var availableTimeSlots = allTimeSlots
                    .Where(t => !occupiedTimes.Contains(t.Value.Trim()))
                    .Select(t => new { t.Value, t.Text })  // Отправляем Value и Text
                    .ToList();

                if (availableTimeSlots.Any())
                {
                    Console.WriteLine($"Available time slots: {string.Join(", ", availableTimeSlots.Select(t => t.Value))}");
                }
                else
                {
                    Console.WriteLine("No available time slots.");
                }

                return Json(availableTimeSlots);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return StatusCode(500, new { error = "An error occurred while fetching available time slots." });
            }
        }




        // GET: Edit
        public async Task<IActionResult> Edit(int id)
        {
            var schedule = await _context.SHEDULE_N_PUBL.FindAsync(id);
            if (schedule == null)
            {
                return NotFound();
            }
            return View(schedule);
        }

        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ScheduleNPublEntity schedule)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingSchedule = await _context.SHEDULE_N_PUBL.FindAsync(schedule.LESSON_ID);
                    if (existingSchedule != null)
                    {
                        _context.Entry(existingSchedule).CurrentValues.SetValues(schedule);
                        await _context.SaveChangesAsync();
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        return NotFound();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
            }
            return View(schedule);
        }

        // POST: DeleteConfirmed
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var lesson = await _context.SHEDULE_N_PUBL.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }
            _context.SHEDULE_N_PUBL.Remove(lesson);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // Метод для получения всех возможных временных интервалов
        private List<SelectListItem> GetAllTimeSlots()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "08:00", Text = "08:00" },
                new SelectListItem { Value = "09:40", Text = "09:40" },
                new SelectListItem { Value = "11:20", Text = "11:20" },
                new SelectListItem { Value = "13:30", Text = "13:30" },
                new SelectListItem { Value = "15:10", Text = "15:10" },
                new SelectListItem { Value = "16:50", Text = "16:50" },
                new SelectListItem { Value = "18:20", Text = "18:20" },
                new SelectListItem { Value = "20:00", Text = "20:00" }
            };
        }
    }
}



