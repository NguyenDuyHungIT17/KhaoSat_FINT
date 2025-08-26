using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using KhaoSat.Models;

namespace KhaoSat.Controllers
{
    public class SystemsettingsController : Controller
    {
        private readonly AppDbContext _context;

        public SystemsettingsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Systemsettings
        public async Task<IActionResult> Index()
        {
            return View(await _context.Systemsettings.ToListAsync());
        }

        // GET: Systemsettings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemsetting = await _context.Systemsettings
                .FirstOrDefaultAsync(m => m.SettingId == id);
            if (systemsetting == null)
            {
                return NotFound();
            }

            return View(systemsetting);
        }

        // GET: Systemsettings/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Systemsettings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SettingId,Key,Value")] Systemsetting systemsetting)
        {
            if (ModelState.IsValid)
            {
                _context.Add(systemsetting);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(systemsetting);
        }

        // GET: Systemsettings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemsetting = await _context.Systemsettings.FindAsync(id);
            if (systemsetting == null)
            {
                return NotFound();
            }
            return View(systemsetting);
        }

        // POST: Systemsettings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SettingId,Key,Value")] Systemsetting systemsetting)
        {
            if (id != systemsetting.SettingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(systemsetting);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SystemsettingExists(systemsetting.SettingId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(systemsetting);
        }

        // GET: Systemsettings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var systemsetting = await _context.Systemsettings
                .FirstOrDefaultAsync(m => m.SettingId == id);
            if (systemsetting == null)
            {
                return NotFound();
            }

            return View(systemsetting);
        }

        // POST: Systemsettings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var systemsetting = await _context.Systemsettings.FindAsync(id);
            if (systemsetting != null)
            {
                _context.Systemsettings.Remove(systemsetting);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SystemsettingExists(int id)
        {
            return _context.Systemsettings.Any(e => e.SettingId == id);
        }
    }
}
