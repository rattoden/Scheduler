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

        public IActionResult Schedule(int groupId)
        {
            var group = _context.GROUPS.FirstOrDefault(g => g.GROUPID == groupId);
            if (group == null)
            {
                return NotFound();
            }

            ViewBag.GroupNo = group.GROUPNO;

            var schedule = _context.SHEDULE_N_PUBL
                .Where(s => s.GROUPID == groupId)
                .Include(s => s.Discipline)
                .ToList();

            return View(schedule);
        }

        // GET: Index
        public IActionResult Index(int? groupId, int? tip, int? semester, int? year)
        {
            // Список групп
            var groups = _context.GROUPS
                .OrderBy(g => g.GROUPNO)
                .Select(g => new SelectListItem
                {
                    Value = g.GROUPID.ToString(),
                    Text = g.GROUPNO.ToString()
                })
                .ToList();
            ViewBag.GroupList = groups;

            // Справочники
            ViewBag.Disciplins = _context.DISCIPLINES
                .Select(d => new SelectListItem
                {
                    Value = d.ID.ToString(),
                    Text = d.NAME
                }).ToList();

            ViewBag.Auditories = _context.SPR_AUDITORY
                .Select(a => new SelectListItem
                {
                    Value = a.NOMER,
                    Text = a.NOMER
                }).ToList();

            ViewBag.Buildings = _context.SPR_BUILDING
                .Select(b => new SelectListItem
                {
                    Value = b.NAME,
                    Text = b.NAME
                }).ToList();

            ViewBag.Prepodavatels = _context.SOTRUDNIK
                .Select(p => new SelectListItem
                {
                    Value = p.FIRSTNAME + " " + p.MIDDLENAME + " " + p.LASTNAME,
                    Text = p.FIRSTNAME + " " + p.MIDDLENAME + " " + p.LASTNAME,
                }).ToList();

            var schedule = _context.SHEDULE_N_PUBL
                .Include(s => s.Discipline)
                .AsQueryable();
            ViewBag.SelectedGroupId = groupId;
            ViewBag.SelectedTip = tip;
            ViewBag.SelectedSemester = semester;
            ViewBag.SelectedYear = year;

            // ФИЛЬТРАЦИЯ по groupId
            if (groupId.HasValue)
            {
                schedule = schedule.Where(s => s.GROUPID == groupId.Value);
                ViewBag.FilteredGroupNo = _context.GROUPS.FirstOrDefault(g => g.GROUPID == groupId)?.GROUPNO;
            }

            // ФИЛЬТРАЦИЯ по типу занятия
            if (tip.HasValue)
            {
                schedule = schedule.Where(s => s.TIP == tip.Value);
            }

            // ФИЛЬТРАЦИЯ по семестру
            if (semester.HasValue)
            {
                schedule = schedule.Where(s => s.SEMESTR == semester.Value);
            }

            // ФИЛЬТРАЦИЯ по году
            if (year.HasValue)
            {
                schedule = schedule.Where(s => s.YEARF == year.Value);
            }

            var orderedSchedule = schedule
                .OrderBy(s => s.GROUPID)
                .ThenBy(s => s.DEN_POS)
                .ThenBy(s => s.TIME_POS)
                .ToList();

            ViewBag.ShowAddButton = groupId.HasValue;

            return View(orderedSchedule);
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(ScheduleNPublEntity schedule)
        {
            // Генерация нового ID
            int maxId = _context.SHEDULE_N_PUBL.Count() > 0 ? _context.SHEDULE_N_PUBL.Max(s => s.LESSON_ID) : 0;
            schedule.LESSON_ID = maxId + 1;

            // Заполнение GROUPID по GROUPNO
            var group = _context.GROUPS.AsEnumerable().FirstOrDefault(g => g.GROUPNO == schedule.GROUPNO);
            schedule.GROUPID = group?.GROUPID ?? 0;
            
            // Получение ID аудитории и здания, если необходимо по имени
            var aud = _context.SPR_AUDITORY
    .AsEnumerable()
    .FirstOrDefault(a => a.NOMER.Trim().Equals(schedule.AUDITORIYA?.Trim(), StringComparison.InvariantCultureIgnoreCase));
            schedule.AUDITORY_ID = aud?.ID_AUDITORY ?? 0;
            var zdan = _context.SPR_BUILDING.AsEnumerable().FirstOrDefault(z => z.NAME == schedule.ZDANIE);
            schedule.BUILDING_ID = zdan?.ID_BUILDING ?? 0;

            // Получение ID преподавателя, если возможно
            var prepod = _context.SOTRUDNIK.AsEnumerable().FirstOrDefault(p => (p.FIRSTNAME + " " + p.MIDDLENAME + " " + p.LASTNAME) == schedule.PREPODAVATEL);
            schedule.PREPOD_ID = prepod?.ID_SOTR ?? 0;
            schedule.TABNUM = prepod?.TAB_NO ?? "NULL";
            schedule.DOLZNOST = prepod?.RANG ?? "NULL";

            // Обработка позиций (можно улучшить при необходимости)
            schedule.DEN_POS = GetDayPosition(schedule.DEN); // Понедельник=1, вторник=2 и т.д.
            schedule.TIME_POS = GetTimePosition(schedule.VREM); // 08:00=1, 09:40=2 и т.д.

            // Значения по умолчанию для GUIDE
            schedule.NUM_DISCIPL_GUIDE = 1;

            try
            {
                _context.SHEDULE_N_PUBL.Add(schedule);
                await _context.SaveChangesAsync();
                return Ok(); // Возвращаем успешный результат
            }
            catch
            {
                return StatusCode(500); // Ошибка
            }
        }

        // Вспомогательные методы
        private int GetDayPosition(string day)
        {
            return day switch
            {
                "пн" => 1,
                "вт" => 2,
                "ср" => 3,
                "чт" => 4,
                "пт" => 5,
                "сб" => 6,
                _ => 0
            };
        }

        private int GetTimePosition(string time)
        {
            var order = new List<string> { "08:00", "09:40", "11:20", "13:30", "15:10", "16:50", "18:30" };
            return order.IndexOf(time) + 1;
        }

        public async Task<IActionResult> Delete(int id)
        {
            var schedule = await _context.SHEDULE_N_PUBL.FindAsync(id);
            if (schedule != null)
            {
                _context.SHEDULE_N_PUBL.Remove(schedule);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}



