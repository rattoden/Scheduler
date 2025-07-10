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
            ViewBag.Buildings1 = _context.SPR_BUILDING
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
        private bool IsParityConflict(string existingData, string newData)
        {
            existingData = existingData?.Trim().ToLower() ?? "";
            newData = newData?.Trim().ToLower() ?? "";

            // если один "чет", другой "неч" — НЕ конфликт
            if ((existingData == "чет" && newData == "неч") || (existingData == "неч" && newData == "чет"))
                return false;

            // во всех остальных случаях — конфликт
            return true;
        }

        // POST: Create
        [HttpPost]
        public async Task<IActionResult> Create(ScheduleNPublEntity input)
        {
            if (input == null)
                return BadRequest("Неверные данные.");

            var group = await _context.GROUPS.FirstOrDefaultAsync(g => g.GROUPNO == input.GROUPNO);
            if (group == null)
                return BadRequest("Группа не найдена.");

            input.GROUPID = group.GROUPID;

            string trimmedDen = CleanString(input.DEN?.ToLower());
            string trimmedVrem = CleanString(input.VREM);
            string trimmedAud = CleanString(input.AUDITORIYA?.ToLower());
            string trimmedZdan = CleanString(input.ZDANIE?.ToLower());
            string trimmedData = CleanString(input.DATA?.ToLower());

            int semestr = input.SEMESTR;
            int yearf = input.YEARF;

            // --- Проверка конфликта по группе ---
            var groupSchedules = await _context.SHEDULE_N_PUBL
                .Where(s => s.GROUPID == input.GROUPID && s.SEMESTR == semestr && s.YEARF == yearf)
                .ToListAsync();

            var groupConflictEntry = groupSchedules.FirstOrDefault(s =>
                CleanString(s.DEN?.ToLower()) == trimmedDen &&
                CleanString(s.VREM) == trimmedVrem &&
                IsParityConflict(CleanString(s.DATA), trimmedData));

            if (groupConflictEntry != null)
            {
                var discipline = await _context.DISCIPLINES.FirstOrDefaultAsync(d => d.ID == groupConflictEntry.DISCIPL_NUM);
                string disciplineName = CleanString(discipline?.NAME ?? "");

                string prepodName = CleanString(groupConflictEntry.PREPODAVATEL ?? "");
                string auditory = CleanString(groupConflictEntry.AUDITORIYA ?? "");
                int building = groupConflictEntry.BUILDING_ID;
                string groupNo = CleanString(group.GROUPNO.ToString());
                string conflictData = CleanString(groupConflictEntry.DATA ?? "");

                return BadRequest($"У этой группы в это время есть занятие \"{disciplineName}\" у преподавателя {prepodName} в {auditory} аудитории {building} здания.");
            }

            // --- Проверка конфликта по аудитории ---
            var auditorySchedules = await _context.SHEDULE_N_PUBL
                .Where(s => s.GROUPID != input.GROUPID && s.SEMESTR == semestr && s.YEARF == yearf)
                .ToListAsync();

            var auditoryConflictEntry = auditorySchedules.FirstOrDefault(s =>
                CleanString(s.DEN?.ToLower()) == trimmedDen &&
                CleanString(s.VREM) == trimmedVrem &&
                CleanString(s.ZDANIE?.ToLower()) == trimmedZdan &&
                CleanString(s.AUDITORIYA?.ToLower()) == trimmedAud &&
                IsParityConflict(CleanString(s.DATA), trimmedData));

            if (auditoryConflictEntry != null)
            {
                var discipline = await _context.DISCIPLINES.FirstOrDefaultAsync(d => d.ID == auditoryConflictEntry.DISCIPL_NUM);
                string disciplineName = CleanString(discipline?.NAME ?? "");

                string prepodName = CleanString(auditoryConflictEntry.PREPODAVATEL ?? "");
                string groupNo = CleanString(_context.GROUPS.FirstOrDefault(g => g.GROUPID == auditoryConflictEntry.GROUPID)?.GROUPNO.ToString() ?? "");

                return BadRequest($"Эта аудитория в это время занята группой {groupNo}: дисциплина \"{disciplineName}\" у преподавателя {prepodName}.");
            }

            // --- Найти преподавателя ---
            var trimmedPrepodName = CleanString(input.PREPODAVATEL);
            var prepod = _context.SOTRUDNIK
                .AsEnumerable()
                .FirstOrDefault(p =>
                    CleanString($"{p.FIRSTNAME} {p.MIDDLENAME} {p.LASTNAME}") == trimmedPrepodName);

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
                CleanString(s.VREM) == trimmedVrem &&
                IsParityConflict(CleanString(s.DATA), trimmedData));

            if (teacherConflictEntry != null)
            {
                var discipline = await _context.DISCIPLINES.FirstOrDefaultAsync(d => d.ID == teacherConflictEntry.DISCIPL_NUM);
                string disciplineName = CleanString(discipline?.NAME ?? "");

                string groupNo = CleanString(_context.GROUPS.FirstOrDefault(g => g.GROUPID == teacherConflictEntry.GROUPID)?.GROUPNO.ToString() ?? "");
                string auditory = CleanString(teacherConflictEntry.AUDITORIYA ?? "");
                int building = teacherConflictEntry.BUILDING_ID;

                return BadRequest($"У выбранного преподавателя уже есть занятие в это время с группой {groupNo} по дисциплине \"{disciplineName}\" в {auditory} аудитории {building} здания.");
            }

            // --- Найти аудиторию ---
            var aud = _context.SPR_AUDITORY
                .AsEnumerable()
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
            var order = new List<string> { "08:00", "09:40", "11:20", "13:30", "15:10", "16:50", "18:25", "20:00" };
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
        public IActionResult GetAuditoriesByBuilding(string buildingName, string day, string time, int semester, int year, string parity)
        {
            if (string.IsNullOrEmpty(buildingName))
                return Json(new List<object>());

            var normalizedBuilding = buildingName.Trim().ToLower();
            var normalizedDay = (day ?? "").Trim().ToLower();
            var normalizedTime = (time ?? "").Trim();
            var normalizedParity = (parity ?? "").Trim().ToLower();

            // Предзагрузка справочников
            var groupDict = _context.GROUPS.ToDictionary(
    g => g.GROUPID,
    g => g.GROUPNO.ToString()
);
            var discDict = _context.DISCIPLINES.ToDictionary(d => d.ID, d => d.NAME ?? "");

            // Все аудитории здания
            var allAuditories = _context.SPR_AUDITORY
                .Where(a => _context.SPR_BUILDING.Any(b => b.NAME == buildingName && b.ID_BUILDING == a.ID_BUILDING))
                .AsNoTracking()
                .ToList();

            // Все занятия в семестре/году
            var busySchedulesRaw = _context.SHEDULE_N_PUBL
                .AsNoTracking()
                .Where(s =>
                    s.ZDANIE != null &&
                    s.DEN != null &&
                    s.VREM != null &&
                    s.SEMESTR == semester &&
                    s.YEARF == year)
                .ToList();

            // Фильтрация по занятым
            var busySchedules = busySchedulesRaw
                .Where(s =>
                    s.ZDANIE.Trim().ToLower() == normalizedBuilding &&
                    s.DEN.Trim().ToLower() == normalizedDay &&
                    s.VREM.Trim() == normalizedTime &&
                    IsParityConflict((s.DATA ?? "").ToLower(), normalizedParity))
                .ToList();

            // Информация по занятым аудиториям
            var busyAuditoriesInfo = busySchedules
                .GroupBy(s => (s.AUDITORIYA ?? "").Trim().ToLower())
                .ToDictionary(g => g.Key, g =>
                {
                    var s = g.First();
                    var groupNo = groupDict.ContainsKey(s.GROUPID) ? groupDict[s.GROUPID] : "";
                    var discipline = discDict.ContainsKey(s.DISCIPL_NUM) ? discDict[s.DISCIPL_NUM] : "";
                    var prepod = s.PREPODAVATEL ?? "";
                    return $"Группа {groupNo}, дисциплина: \"{discipline}\", преподаватель: {prepod}";
                });

            // Формируем результат
            var result = allAuditories.Select(a =>
            {
                var audName = (a.NOMER ?? "").Trim();
                var key = audName.ToLower();
                var isBusy = busyAuditoriesInfo.ContainsKey(key);
                return new
                {
                    value = audName,
                    text = isBusy ? $"{audName} (занята)" : audName,
                    busy = isBusy,
                    tooltip = isBusy ? busyAuditoriesInfo[key] : ""
                };
            });

            return Json(result);
        }


        [HttpGet]
        public IActionResult GetBusyTeachers(string day, string time, int semester, int year, string parity)
        {
            if (string.IsNullOrWhiteSpace(day) || string.IsNullOrWhiteSpace(time) || string.IsNullOrWhiteSpace(parity))
                return Json(new List<object>());

            var normalizedDay = day.Trim().ToLower();
            var normalizedTime = time.Trim();
            var normalizedParity = parity.Trim().ToLower();

            // Предзагрузка справочников
            var groupDict = _context.GROUPS.ToDictionary(
    g => g.GROUPID,
    g => g.GROUPNO.ToString());
            var discDict = _context.DISCIPLINES.ToDictionary(d => d.ID, d => d.NAME ?? "");

            // Получаем расписание
            var allSchedules = _context.SHEDULE_N_PUBL
                .AsNoTracking()
                .Where(s =>
                    s.SEMESTR == semester &&
                    s.YEARF == year &&
                    s.DEN != null &&
                    s.VREM != null)
                .ToList();

            // Фильтрация в памяти
            var busySchedules = allSchedules
                .Where(s =>
                    s.DEN.Trim().ToLower() == normalizedDay &&
                    s.VREM.Trim() == normalizedTime &&
                    IsParityConflict((s.DATA ?? "").ToLower(), normalizedParity))
                .ToList();

            // Группировка по преподавателю
            var busyInfo = busySchedules
                .Where(s => !string.IsNullOrWhiteSpace(s.PREPODAVATEL))
                .GroupBy(s => s.PREPODAVATEL.Trim())
                .ToDictionary(g => g.Key, g =>
                {
                    var s = g.First();
                    var groupNo = groupDict.ContainsKey(s.GROUPID) ? groupDict[s.GROUPID] : "";
                    var discipline = discDict.ContainsKey(s.DISCIPL_NUM) ? discDict[s.DISCIPL_NUM] : "";
                    return $"Группа {groupNo}, дисциплина: \"{discipline}\"";
                });

            // Список всех преподавателей
            var allPreps = _context.SOTRUDNIK
                .Select(p => new
                {
                    FullName = (p.FIRSTNAME + " " + p.MIDDLENAME + " " + p.LASTNAME).Trim()
                })
                .ToList();

            var result = allPreps.Select(p => new
            {
                value = p.FullName,
                text = busyInfo.ContainsKey(p.FullName) ? $"{p.FullName} (занят)" : p.FullName,
                busy = busyInfo.ContainsKey(p.FullName),
                tooltip = busyInfo.ContainsKey(p.FullName) ? busyInfo[p.FullName] : ""
            });

            return Json(result);
        }

        [HttpGet]
        public IActionResult GetFreeTimes(string day, string parity, int semester, int year, int groupNo)
        {
            if (string.IsNullOrWhiteSpace(day) || string.IsNullOrWhiteSpace(parity))
                return Json(new List<string>());

            var dayLower = day.Trim().ToLower();
            var parityLower = parity.Trim().ToLower();

            // Получаем ID группы
            var groupId = _context.GROUPS
                .AsNoTracking()
                .Where(g => g.GROUPNO == groupNo)
                .Select(g => g.GROUPID)
                .FirstOrDefault();

            // Если не нашли группу, возвращаем пусто
            if (groupId == 0)
                return Json(new List<string>());

            var allTimes = new List<string> { "08:00", "09:40", "11:20", "13:30", "15:10", "16:50", "18:25", "20:00" };

            // Загружаем в память, т.к. IsParityConflict — метод C#, а не SQL
            var schedules = _context.SHEDULE_N_PUBL
                .AsNoTracking()
                .Where(s =>
                    s.DEN != null &&
                    s.VREM != null &&
                    s.DATA != null &&
                    s.SEMESTR == semester &&
                    s.YEARF == year &&
                    s.DEN.Trim().ToLower() == dayLower &&
                    s.GROUPID == groupId)
                .ToList();

            var busyTimes = schedules
                .Where(s => IsParityConflict(s.DATA.ToLower(), parityLower))
                .Select(s => s.VREM.Trim())
                .Distinct()
                .ToList();

            var freeTimes = allTimes.Except(busyTimes).ToList();

            return Json(freeTimes);
        }


        [HttpGet]
        public async Task<IActionResult> GetLessonDetailsAsync(int lessonId)
        {
            var lesson = await _context.SHEDULE_N_PUBL.FirstOrDefaultAsync(l => l.LESSON_ID == lessonId);
            if (lesson == null)
                return NotFound();

            var group = await _context.GROUPS.FirstOrDefaultAsync(g => g.GROUPID == lesson.GROUPID);

            return Json(new
            {
                lessonId = lesson.LESSON_ID,
                groupId = lesson.GROUPID,
                groupNo = group?.GROUPNO,
                den = lesson.DEN,
                vrem = lesson.VREM,
                data = lesson.DATA,
                zdanie = lesson.ZDANIE,
                auditoriya = lesson.AUDITORIYA,
                prepodavatel = lesson.PREPODAVATEL
            });
        }

        [HttpPost]
        public async Task<IActionResult> Create1(ScheduleNPublEntity input)
        {
            if (input == null)
                return BadRequest("Неверные данные.");

            var lesson = await _context.SHEDULE_N_PUBL.FirstOrDefaultAsync(s => s.LESSON_ID == input.LESSON_ID);
            if (lesson == null)
                return BadRequest("Занятие не найдено.");

            var group = await _context.GROUPS.FirstOrDefaultAsync(g => g.GROUPNO == input.GROUPNO);
            if (group == null)
                return BadRequest("Группа не найдена.");

            string trimmedDen = CleanString(input.DEN?.ToLower());
            string trimmedData = CleanString(input.DATA?.ToLower());
            string trimmedVrem = CleanString(input.VREM);
            string trimmedAud = CleanString(input.AUDITORIYA?.ToLower());
            string trimmedZdan = CleanString(input.ZDANIE?.ToLower());

            int semestr = lesson.SEMESTR;
            int yearf = lesson.YEARF;
            int groupId = lesson.GROUPID;

            // --- Проверка конфликта по группе ---
            var groupSchedules = await _context.SHEDULE_N_PUBL
                .Where(s => s.GROUPID == input.GROUPID && s.SEMESTR == semestr && s.YEARF == yearf && s.LESSON_ID != input.LESSON_ID)
                .ToListAsync();

            var groupConflictEntry = groupSchedules.FirstOrDefault(s =>
                CleanString(s.DEN?.ToLower()) == trimmedDen &&
                CleanString(s.VREM) == trimmedVrem &&
                IsParityConflict(CleanString(s.DATA), trimmedData));

            if (groupConflictEntry != null)
            {
                var discipline = await _context.DISCIPLINES.FirstOrDefaultAsync(d => d.ID == groupConflictEntry.DISCIPL_NUM);
                string disciplineName = CleanString(discipline?.NAME ?? "");

                string prepodName = CleanString(groupConflictEntry.PREPODAVATEL ?? "");
                string auditory = CleanString(groupConflictEntry.AUDITORIYA ?? "");
                int building = groupConflictEntry.BUILDING_ID;
                string groupNo = CleanString(group.GROUPNO.ToString());
                string conflictData = CleanString(groupConflictEntry.DATA ?? "");

                return BadRequest($"У этой группы в это время есть занятие \"{disciplineName}\" у преподавателя {prepodName} в {auditory} аудитории {building} здания.");
            }

            // --- Проверка конфликта по аудитории ---
            var auditorySchedules = await _context.SHEDULE_N_PUBL
                .Where(s => s.GROUPID != input.GROUPID && s.SEMESTR == semestr && s.YEARF == yearf)
                .ToListAsync();

            var auditoryConflictEntry = auditorySchedules.FirstOrDefault(s =>
                CleanString(s.DEN?.ToLower()) == trimmedDen &&
                CleanString(s.VREM) == trimmedVrem &&
                CleanString(s.ZDANIE?.ToLower()) == trimmedZdan &&
                CleanString(s.AUDITORIYA?.ToLower()) == trimmedAud &&
                IsParityConflict(CleanString(s.DATA), trimmedData));

            if (auditoryConflictEntry != null)
            {
                var discipline = await _context.DISCIPLINES.FirstOrDefaultAsync(d => d.ID == auditoryConflictEntry.DISCIPL_NUM);
                string disciplineName = CleanString(discipline?.NAME ?? "");

                string prepodName = CleanString(auditoryConflictEntry.PREPODAVATEL ?? "");
                string groupNo = CleanString(_context.GROUPS.FirstOrDefault(g => g.GROUPID == auditoryConflictEntry.GROUPID)?.GROUPNO.ToString() ?? "");

                return BadRequest($"Эта аудитория в это время занята группой {groupNo}: дисциплина \"{disciplineName}\" у преподавателя {prepodName}.");
            }

            // --- Найти преподавателя ---
            var trimmedPrepodName = CleanString(input.PREPODAVATEL);
            var prepod = _context.SOTRUDNIK
                .AsEnumerable()
                .FirstOrDefault(p =>
                    CleanString($"{p.FIRSTNAME} {p.MIDDLENAME} {p.LASTNAME}") == trimmedPrepodName);

            if (prepod == null)
                return BadRequest("Преподаватель не найден.");

            input.PREPOD_ID = prepod.ID_SOTR;
            input.TABNUM = prepod.TAB_NO ?? "NULL";
            input.DOLZNOST = prepod.RANG ?? "NULL";

            // --- Проверка конфликта по преподавателю ---
            var teacherSchedules = await _context.SHEDULE_N_PUBL
                .Where(s => s.PREPOD_ID == lesson.PREPOD_ID && s.SEMESTR == semestr && s.YEARF == yearf && s.LESSON_ID != input.LESSON_ID)
                .ToListAsync();

            var teacherConflictEntry = teacherSchedules.FirstOrDefault(s =>
                CleanString(s.DEN?.ToLower()) == trimmedDen &&
                CleanString(s.VREM) == trimmedVrem &&
                IsParityConflict(CleanString(s.DATA), trimmedData));

            if (teacherConflictEntry != null)
            {
                var discipline = await _context.DISCIPLINES.FirstOrDefaultAsync(d => d.ID == teacherConflictEntry.DISCIPL_NUM);
                string disciplineName = CleanString(discipline?.NAME ?? "");

                string groupNo = CleanString(_context.GROUPS.FirstOrDefault(g => g.GROUPID == teacherConflictEntry.GROUPID)?.GROUPNO.ToString() ?? "");
                string auditory = CleanString(teacherConflictEntry.AUDITORIYA ?? "");
                int building = teacherConflictEntry.BUILDING_ID;

                return BadRequest($"У выбранного преподавателя уже есть занятие в это время с группой {groupNo} по дисциплине \"{disciplineName}\" в {auditory} аудитории {building} здания.");
            }

            // --- Найти аудиторию ---
            var aud = _context.SPR_AUDITORY
                .AsEnumerable()
                .FirstOrDefault(a => (a.NOMER ?? "").Trim().Equals(trimmedAud, StringComparison.InvariantCultureIgnoreCase));
            lesson.AUDITORY_ID = aud?.ID_AUDITORY ?? 0;

            // --- Найти здание ---
            var zdan = _context.SPR_BUILDING
                .FirstOrDefault(z => (z.NAME ?? "").Trim().ToLower() == trimmedZdan);
            lesson.BUILDING_ID = zdan?.ID_BUILDING ?? 0;

            lesson.DEN = input.DEN;
            lesson.VREM = input.VREM;
            lesson.DATA = input.DATA;
            lesson.ZDANIE = input.ZDANIE;
            lesson.AUDITORIYA = input.AUDITORIYA;

            // Обновляем позиции
            lesson.DEN_POS = GetDayPosition(input.DEN);
            lesson.TIME_POS = GetTimePosition(input.VREM);
            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Ошибка при сохранении изменений: " + ex.Message);
            }
        }
        [HttpGet]
        public IActionResult GetAuditoriesByBuilding1(string buildingName, string day, string time, int semester, int year, string parity)
        {
            if (string.IsNullOrEmpty(buildingName))
                return Json(new List<object>());

            var allAuditories = _context.SPR_AUDITORY
                .Where(a => _context.SPR_BUILDING.Any(b => b.NAME == buildingName && b.ID_BUILDING == a.ID_BUILDING))
                .ToList();

            var busySchedulesRaw = _context.SHEDULE_N_PUBL
    .Where(s =>
        s.ZDANIE != null &&
        s.DEN != null &&
        s.VREM != null &&
        s.SEMESTR == semester &&
        s.YEARF == year)
    .ToList(); // <-- сначала забрали все записи

            var busySchedules = busySchedulesRaw
                .Where(s =>
                    s.ZDANIE.Trim().ToLower() == buildingName.Trim().ToLower() &&
                    s.DEN.Trim().ToLower() == day.Trim().ToLower() &&
                    s.VREM.Trim() == time.Trim() &&
                    IsParityConflict((s.DATA ?? "").ToLower(), parity.ToLower()))
                .ToList();

            var busyAuditoriesInfo = busySchedules
                .GroupBy(s => (s.AUDITORIYA ?? "").Trim().ToLower())
                .ToDictionary(g => g.Key, g =>
                {
                    var s = g.First(); // Берем первое занятие
                    var groupNo = _context.GROUPS.FirstOrDefault(gr => gr.GROUPID == s.GROUPID)?.GROUPNO.ToString() ?? "";
                    var discipline = _context.DISCIPLINES.FirstOrDefault(d => d.ID == s.DISCIPL_NUM)?.NAME ?? "";
                    var prepod = s.PREPODAVATEL ?? "";
                    return $"Группа {groupNo}, дисциплина: \"{discipline}\", преподаватель: {prepod}";
                });

            var result = allAuditories.Select(a =>
            {
                var audName = (a.NOMER ?? "").Trim();
                var key = audName.ToLower();
                var isBusy = busyAuditoriesInfo.ContainsKey(key);
                return new
                {
                    value = audName,
                    text = isBusy ? $"{audName} (занята)" : audName,
                    busy = isBusy,
                    tooltip = isBusy ? busyAuditoriesInfo[key] : ""
                };
            });

            return Json(result);
        }
    }
}