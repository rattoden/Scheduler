using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SchedulerV4.Models;

namespace SchedulerV4.Controllers
{
    public class SprAuditoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SprAuditoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()

        {
            List<SprAuditoryEntity> auditories = _context.SPR_AUDITORY.ToList();
            ViewBag.Buildings = _context.SPR_BUILDING.Select(b => new SelectListItem
        {
            Value = b.ID_BUILDING.ToString(),
            Text = b.NAME
        })
        .ToList();
            return View(auditories);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SprAuditoryEntity auditory)
        {
            int maxId = _context.SPR_AUDITORY.Count() > 0 ? _context.SPR_AUDITORY.Max(a => a.ID_AUDITORY) : 0;
            auditory.ID_AUDITORY = maxId + 1;
            // Приведение чекбоксов к 0/1, если не пришли — будут 0
            auditory.MULTI_INVENTORY = auditory.MULTI_INVENTORY == 1 ? 1 : 0;
            auditory.INVALID = auditory.INVALID == "Да" ? "Да" : "Нет";
            // Проверяем, существует ли аудитория с таким номером в том же здании
            bool exists = _context.SPR_AUDITORY.Count(a => a.ID_BUILDING == auditory.ID_BUILDING && a.NOMER == auditory.NOMER) > 0;
            if (exists)
            {
                TempData["ErrorMessage"] = "Аудитория с таким номером в этом здании уже существует.";
            }
            else
            {
                try
                {
                    _context.SPR_AUDITORY.Add(auditory);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Аудитория добавлена.";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Произошла ошибка при добавлении аудитории: " + ex.Message;
                }
            }
            return RedirectToAction(nameof(Index));
        }



        public async Task<IActionResult> Delete(int id)
        {
            var auditory = await _context.SPR_AUDITORY.FindAsync(id);
            if (auditory != null)
            {
                _context.SPR_AUDITORY.Remove(auditory);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }


        public async Task<IActionResult> Edit(int id)
        {
            var auditory = await _context.SPR_AUDITORY.FindAsync(id);
            if (auditory == null)
            {
                return NotFound();
            }
            ViewBag.Buildings = _context.SPR_BUILDING.Select(b => new SelectListItem
            {
            Value = b.ID_BUILDING.ToString(),
            Text = b.NAME
            })
            .ToList();
            return View(auditory);
        }

        // POST: Edit
        [HttpPost]
        public async Task<IActionResult> Edit(SprAuditoryEntity auditory)
        {
            // Обработка чекбоксов вручную
            auditory.MULTI_INVENTORY = Request.Form["MULTI_INVENTORY"].Count > 0 ? 1 : 0;
            auditory.INVALID = Request.Form["INVALID"].Count > 0 ? "Да" : "Нет";

            try
            {
                // Проверка на дубликат НОМЕР + ID_BUILDING, исключая текущую запись
                bool duplicateExists = await _context.SPR_AUDITORY
                    .CountAsync(a => a.ID_AUDITORY != auditory.ID_AUDITORY &&
                                   a.NOMER == auditory.NOMER &&
                                   a.ID_BUILDING == auditory.ID_BUILDING) > 0;

                if (duplicateExists)
                {
                    TempData["ErrorMessage"] = "Аудитория с таким номером уже существует в выбранном здании.";
                }
                else
                {
                    var existingAuditory = await _context.SPR_AUDITORY.FindAsync(auditory.ID_AUDITORY);
                    if (existingAuditory != null)
                    {
                        _context.Entry(existingAuditory).CurrentValues.SetValues(auditory);
                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Информация об аудитории успешно обновлена.";
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Аудитории с таким номером не найдено.";
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Произошла ошибка при обновлении аудитории: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }


}

