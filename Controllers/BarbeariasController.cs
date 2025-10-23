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
    public class BarbeariasController : Controller
    {
        private readonly AppDbContext _context;

        public BarbeariasController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Barbearias
        public async Task<IActionResult> Index()
        {
            return View(await _context.Barbearias.ToListAsync());
        }

        // GET: Barbearias/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var barbearia = await _context.Barbearias
                .FirstOrDefaultAsync(m => m.Id == id);
            if (barbearia == null)
            {
                return NotFound();
            }

            return View(barbearia);
        }

        // GET: Barbearias/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Barbearias/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,Telefone,Endereco,Bairro,Cidade")] Barbearia barbearia)
        {
            if (ModelState.IsValid)
            {
                _context.Add(barbearia);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(barbearia);
        }

        // GET: Barbearias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var barbearia = await _context.Barbearias.FindAsync(id);
            if (barbearia == null)
            {
                return NotFound();
            }
            return View(barbearia);
        }

        // POST: Barbearias/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Telefone,Endereco,Bairro,Cidade")] Barbearia barbearia)
        {
            if (id != barbearia.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(barbearia);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BarbeariaExists(barbearia.Id))
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
            return View(barbearia);
        }

        // GET: Barbearias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var barbearia = await _context.Barbearias
                .FirstOrDefaultAsync(m => m.Id == id);
            if (barbearia == null)
            {
                return NotFound();
            }

            return View(barbearia);
        }

        // POST: Barbearias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var barbearia = await _context.Barbearias.FindAsync(id);
            if (barbearia != null)
            {
                _context.Barbearias.Remove(barbearia);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BarbeariaExists(int id)
        {
            return _context.Barbearias.Any(e => e.Id == id);
        }
    }
}
