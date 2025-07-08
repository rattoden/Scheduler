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
        public class ScheduleNPublInputModel
        {
            public int GROUPNO { get; set; }
            public string DEN { get; set; }
            public string VREM { get; set; }
            public string DATA { get; set; }
            public int DISCIPL_NUM { get; set; }
            public string FORM_ZAN { get; set; }
            public string ZDANIE { get; set; }
            public string AUDITORIYA { get; set; }
            public string PREPODAVATEL { get; set; }
            public int TIP { get; set; }
            public int SEMESTR { get; set; }
            public int YEARF { get; set; }
        }
        private string CleanString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return "";
            var trimmed = input.Trim();
            return System.Text.RegularExpressions.Regex.Replace(trimmed, @"\s+", " ");
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(ScheduleNPublEntity input)
        {
            if (input == null)
                return BadRequest("Неверные данные.");

            // Найти группу по номеру группы
            var group = await _context.GROUPS.FirstOrDefaultAsync(g => g.GROUPNO == input.GROUPNO);
            if (group == null)
                return BadRequest("Группа не найдена.");

            input.GROUPID = group.GROUPID;

            // Очистка полей для сравнения
            string trimmedDen = CleanString(input.DEN?.ToLower());
            string trimmedVrem = CleanString(input.VREM);
            string trimmedAud = CleanString(input.AUDITORIYA?.ToLower());
            string trimmedZdan = CleanString(input.ZDANIE?.ToLower());

            int semestr = input.SEMESTR;
            int yearf = input.YEARF;

            // --- Проверка конфликта по группе ---
            var groupSchedules = await _context.SHEDULE_N_PUBL
                .Where(s => s.GROUPID == input.GROUPID && s.SEMESTR == semestr && s.YEARF == yearf)
                .ToListAsync();

            var groupConflictEntry = groupSchedules.FirstOrDefault(s =>
                CleanString(s.DEN?.ToLower()) == trimmedDen &&
                CleanString(s.VREM) == trimmedVrem);

            if (groupConflictEntry != null)
            {
                var discipline = await _context.DISCIPLINES.FirstOrDefaultAsync(d => d.ID == groupConflictEntry.DISCIPL_NUM);
                string disciplineName = CleanString(discipline?.NAME ?? "неизвестна");

                string prepodName = CleanString(groupConflictEntry.PREPODAVATEL ?? "неизвестен");
                string auditory = CleanString(groupConflictEntry.AUDITORIYA ?? "неизвестна");
                int building = groupConflictEntry.BUILDING_ID;
                string groupNo = CleanString(group.GROUPNO.ToString());

                return BadRequest($"У группы {groupNo} в это время есть занятие \"{disciplineName}\" у преподавателя {prepodName} в {auditory} аудитории {building} здания.");
            }

            // --- Проверка конфликта по аудитории ---
            var auditorySchedules = await _context.SHEDULE_N_PUBL
                .Where(s => s.GROUPID != input.GROUPID && s.SEMESTR == semestr && s.YEARF == yearf)
                .ToListAsync();

            var auditoryConflictEntry = auditorySchedules.FirstOrDefault(s =>
                CleanString(s.DEN?.ToLower()) == trimmedDen &&
                CleanString(s.VREM) == trimmedVrem &&
                CleanString(s.ZDANIE?.ToLower()) == trimmedZdan &&
                CleanString(s.AUDITORIYA?.ToLower()) == trimmedAud);

            if (auditoryConflictEntry != null)
            {
                var discipline = await _context.DISCIPLINES.FirstOrDefaultAsync(d => d.ID == auditoryConflictEntry.DISCIPL_NUM);
                string disciplineName = CleanString(discipline?.NAME ?? "неизвестна");

                string prepodName = CleanString(auditoryConflictEntry.PREPODAVATEL ?? "неизвестен");
                string groupNo = CleanString(_context.GROUPS.FirstOrDefault(g => g.GROUPID == auditoryConflictEntry.GROUPID)?.GROUPNO.ToString() ?? "неизвестна");

                return BadRequest($"Эта аудитория в это время занята группой {groupNo}: дисциплина \"{disciplineName}\" у преподавателя {prepodName}.");
            }

            // --- Найти преподавателя ---
            var prepod = await _context.SOTRUDNIK.FirstOrDefaultAsync(p =>
                CleanString((p.FIRSTNAME + " " + p.MIDDLENAME + " " + p.LASTNAME)) == CleanString(input.PREPODAVATEL));

            if (prepod == null)
                return BadRequest("Преподаватель не найден.");

            input.PREPOD_ID = prepod.ID_SOTR;
            input.TABNUM = prepod.TAB_NO ?? "NULL";
            input.DOLZNOST = prepod.RANG ?? "NULL";

            // --- Проверка конфликта по преподавателю ---
            var teacherSchedules = await _context.SHEDULE_N_PUBL
                .Where(s => s.PREPOD_ID == input.PREPOD_ID && s.SEMESTR == semestr && s.YEARF == yearf)
                .ToListAsync();

            var teacherConflictEntry = teacherSchedules.FirstOrDefault(s =>
                CleanString(s.DEN?.ToLower()) == trimmedDen &&
                CleanString(s.VREM) == trimmedVrem);

            if (teacherConflictEntry != null)
            {
                var discipline = await _context.DISCIPLINES.FirstOrDefaultAsync(d => d.ID == teacherConflictEntry.DISCIPL_NUM);
                string disciplineName = CleanString(discipline?.NAME ?? "неизвестна");

                string groupNo = CleanString(_context.GROUPS.FirstOrDefault(g => g.GROUPID == teacherConflictEntry.GROUPID)?.GROUPNO.ToString() ?? "неизвестна");
                string auditory = CleanString(teacherConflictEntry.AUDITORIYA ?? "неизвестна");
                int building = teacherConflictEntry.BUILDING_ID;

                return BadRequest($"У выбранного преподавателя уже есть занятие в это время с группой {groupNo} по дисциплине \"{disciplineName}\" в {auditory} аудитории {building} здания.");
            }

            // --- Найти аудиторию ---
            var aud = _context.SPR_AUDITORY
    .AsEnumerable() // Принудительно переносим выполнение в память
    .FirstOrDefault(a => (a.NOMER ?? "").Trim().Equals(trimmedAud, StringComparison.InvariantCultureIgnoreCase));

            input.AUDITORY_ID = aud?.ID_AUDITORY ?? 0;

            // --- Найти здание ---
            var zdan = _context.SPR_BUILDING
                .FirstOrDefault(z => (z.NAME ?? "").Trim().ToLower() == trimmedZdan);

            input.BUILDING_ID = zdan?.ID_BUILDING ?? 0;

            // --- Генерация нового ID (LESSON_ID) ---
            int maxId = _context.SHEDULE_N_PUBL.Count() > 0 ? _context.SHEDULE_N_PUBL.Max(s => s.LESSON_ID) : 0;
            input.LESSON_ID = maxId + 1;

            // --- Заполнение позиций ---
            input.DEN_POS = GetDayPosition(input.DEN);
            input.TIME_POS = GetTimePosition(input.VREM);
            input.NUM_DISCIPL_GUIDE = 1;

            try
            {
                _context.SHEDULE_N_PUBL.Add(input);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка при сохранении занятия: " + ex.Message);
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

        public async Task<IActionResult> Delete(int id, int? groupId, int? tip, int? semester, int? year)
        {
            var schedule = await _context.SHEDULE_N_PUBL.FindAsync(id);
            if (schedule != null)
            {
                _context.SHEDULE_N_PUBL.Remove(schedule);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index), new { groupId, tip, semester, year });
        }

        [HttpGet]
        public IActionResult GetAuditoriesByBuilding(string buildingName)
        {
            if (string.IsNullOrEmpty(buildingName))
                return Json(new List<SelectListItem>());

            var auditories = _context.SPR_AUDITORY
                .Where(a => _context.SPR_BUILDING
                    .Count(b => b.NAME == buildingName && b.ID_BUILDING == a.ID_BUILDING) > 0)
                .Select(a => new SelectListItem
                {
                    Value = a.NOMER,
                    Text = a.NOMER
                })
                .ToList();

            return Json(auditories);
        }
    }
}



