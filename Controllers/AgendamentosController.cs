using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using iBarber.Models;

namespace iBarber.Controllers
{
    [Authorize]
    public class AgendamentosController : Controller
    {
        private readonly AppDbContext _context;

        public AgendamentosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Agendamentos (Carrega Barbearia via ThenInclude)
        public async Task<IActionResult> Index()
        {
            var appDbContext = _context.Agendamentos
                .Include(a => a.Profissional)
                    .ThenInclude(p => p.Barbearia) // <-- Necessário para mostrar o nome da Barbearia
                .Include(a => a.Servico)
                .Include(a => a.Usuario);

            return View(await appDbContext.ToListAsync());
        }

        // GET: Agendamentos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var agendamento = await _context.Agendamentos
                .Include(a => a.Profissional)
                .Include(a => a.Servico)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (agendamento == null) return NotFound();

            return View(agendamento);
        }

        // GET: Agendamentos/Create (Carrega Barbearias e Nome do Cliente)
        public IActionResult Create()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
            {
                var usuarioLogado = _context.Usuarios.FirstOrDefault(u => u.Id == userId);
                if (usuarioLogado != null)
                {
                    ViewData["NomeClienteLogado"] = usuarioLogado.Nome;
                }
            }

            ViewData["BarbeariaId"] = new SelectList(_context.Barbearias, "Id", "Nome");

            return View();
        }

        // POST: Agendamentos/Create (Injeta UsuarioId e trata erros)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DataHora,ProfissionalId,ServicoId")] Agendamento agendamento)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int usuarioId))
            {
                ModelState.AddModelError(string.Empty, "Não foi possível identificar o cliente logado. Por favor, faça login novamente.");
            }
            else
            {
                agendamento.UsuarioId = usuarioId;
            }

            if (ModelState.IsValid)
            {
                _context.Add(agendamento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Recarrega o ViewData em caso de erro de validação
            var usuarioLogado = _context.Usuarios.FirstOrDefault(u => u.Id == agendamento.UsuarioId);
            if (usuarioLogado != null)
            {
                ViewData["NomeClienteLogado"] = usuarioLogado.Nome;
            }
            ViewData["BarbeariaId"] = new SelectList(_context.Barbearias, "Id", "Nome");

            return View(agendamento);
        }

        // GET: Agendamentos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var agendamento = await _context.Agendamentos.FindAsync(id);
            if (agendamento == null) return NotFound();

            ViewData["ProfissionalId"] = new SelectList(_context.Profissionais, "Id", "Email", agendamento.ProfissionalId);
            ViewData["ServicoId"] = new SelectList(_context.Servicos, "Id", "Nome", agendamento.ServicoId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Nome", agendamento.UsuarioId);

            return View(agendamento);
        }

        // POST: Agendamentos/Edit/5 (CORRIGIDO ERROS CS0161)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DataHora,UsuarioId,ProfissionalId,ServicoId")] Agendamento agendamento)
        {
            if (id != agendamento.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(agendamento);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AgendamentoExists(agendamento.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            // Retorna a View com erros de validação
            ViewData["ProfissionalId"] = new SelectList(_context.Profissionais, "Id", "Email", agendamento.ProfissionalId);
            ViewData["ServicoId"] = new SelectList(_context.Servicos, "Id", "Nome", agendamento.ServicoId);
            ViewData["UsuarioId"] = new SelectList(_context.Usuarios, "Id", "Nome", agendamento.UsuarioId);

            return View(agendamento);
        }

        // GET: Agendamentos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var agendamento = await _context.Agendamentos
                .Include(a => a.Profissional)
                .Include(a => a.Servico)
                .Include(a => a.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (agendamento == null) return NotFound();

            return View(agendamento);
        }

        // POST: Agendamentos/Delete/5 (CORRIGIDO ERROS CS0103)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // O nome "agendamento" agora está dentro do escopo e é usado corretamente.
            var agendamento = await _context.Agendamentos.FindAsync(id);

            if (agendamento != null)
            {
                _context.Agendamentos.Remove(agendamento);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AgendamentoExists(int id)
        {
            return _context.Agendamentos.Any(e => e.Id == id);
        }

        // --- ENDPOINTS JSON ---

        [HttpGet]
        public IActionResult GetProfissionaisPorBarbearia(int barbeariaId)
        {
            var profissionais = _context.Profissionais
                                        .Where(p => p.BarbeariaId == barbeariaId)
                                        .Select(p => new { id = p.Id, nome = p.Nome })
                                        .ToList();
            return Json(profissionais);
        }

        [HttpGet]
        public IActionResult GetServicosPorBarbearia(int barbeariaId)
        {
            // CORRIGIDO: Adicionado Distinct() para evitar duplicação de serviços na lista
            var servicos = _context.Servicos
                                   .Where(s => s.BarbeariaId == barbeariaId)
                                   .Select(s => new { id = s.Id, nome = s.Nome })
                                   .Distinct()
                                   .ToList();

            return Json(servicos);
        }

        // NOVO ENDPOINT: Gera e verifica horários disponíveis (30 em 30 min)
        [HttpGet]
        public IActionResult GetHorariosDisponiveis(int profissionalId, string data)
        {
            if (!DateTime.TryParse(data, out DateTime dataSelecionada))
            {
                return BadRequest(new { message = "Data inválida." });
            }

            var horaInicio = new TimeSpan(8, 0, 0);
            var horaFim = new TimeSpan(18, 0, 0);
            var duracaoIntervalo = TimeSpan.FromMinutes(30);

            var agendamentosOcupados = _context.Agendamentos
                .Where(a => a.ProfissionalId == profissionalId &&
                            a.DataHora.Date == dataSelecionada.Date)
                .Select(a => a.DataHora)
                .ToList();

            var listaHorarios = new List<object>();

            for (var hora = horaInicio; hora < horaFim; hora = hora.Add(duracaoIntervalo))
            {
                var dataHoraAtual = dataSelecionada.Date.Add(hora);

                // Impede agendamentos no passado
                if (dataHoraAtual <= DateTime.Now)
                {
                    continue;
                }

                var isOcupado = agendamentosOcupados.Any(dataAgendada => dataAgendada == dataHoraAtual);

                listaHorarios.Add(new
                {
                    horario = hora.ToString(@"hh\:mm"),
                    dataHoraCompleta = dataHoraAtual.ToString("yyyy-MM-ddTHH:mm:ss"),
                    disponivel = !isOcupado
                });
            }

            return Json(listaHorarios);
        }
    }
}