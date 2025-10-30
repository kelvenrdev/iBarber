using iBarber.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace iBarber.Controllers
{
    [Authorize]
    public class ProfissionaisController : Controller
    {
        private readonly AppDbContext _context;

        public ProfissionaisController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Profissionais
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Profissionais.Include(p => p.Barbearia);
            return View(await appDbContext.ToListAsync());
        }

        // GET: Profissionais/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profissional = await _context.Profissionais
                .Include(p => p.Barbearia)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (profissional == null)
            {
                return NotFound();
            }

            return View(profissional);
        }

        // GET: Profissionais/Create
        public IActionResult Create()
        {
            // CORRIGIDO: Trocando "Bairro" por "Nome" para a exibição no Dropdown
            ViewData["BarbeariaId"] = new SelectList(_context.Barbearias, "Id", "Nome");
            return View();
        }

        // POST: Profissionais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Telefone,Email,BarbeariaId")] Profissional profissional)
        {
            if (ModelState.IsValid)
            {
                _context.Add(profissional);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // CORRIGIDO: Trocando "Bairro" por "Nome" para a exibição no Dropdown
            ViewData["BarbeariaId"] = new SelectList(_context.Barbearias, "Id", "Nome", profissional.BarbeariaId);
            return View(profissional);
        }

        // GET: Profissionais/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profissional = await _context.Profissionais.FindAsync(id);
            if (profissional == null)
            {
                return NotFound();
            }
            // CORRIGIDO: Trocando "Bairro" por "Nome" para a exibição no Dropdown
            ViewData["BarbeariaId"] = new SelectList(_context.Barbearias, "Id", "Nome", profissional.BarbeariaId);
            return View(profissional);
        }

        // POST: Profissionais/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Telefone,Email,BarbeariaId")] Profissional profissional)
        {
            if (id != profissional.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(profissional);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProfissionalExists(profissional.Id))
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
            // CORRIGIDO: Trocando "Bairro" por "Nome" para a exibição no Dropdown
            ViewData["BarbeariaId"] = new SelectList(_context.Barbearias, "Id", "Nome", profissional.BarbeariaId);
            return View(profissional);
        }

        // GET: Profissionais/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var profissional = await _context.Profissionais
                .Include(p => p.Barbearia)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (profissional == null)
            {
                return NotFound();
            }

            return View(profissional);
        }

        // POST: Profissionais/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var profissional = await _context.Profissionais.FindAsync(id);
            if (profissional != null)
            {
                _context.Profissionais.Remove(profissional);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProfissionalExists(int id)
        {
            return _context.Profissionais.Any(e => e.Id == id);
        }
    }
}