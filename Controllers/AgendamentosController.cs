using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using iBarber.Models;
using System.Collections.Generic; // Necessário para List<T>

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
                    .ThenInclude(p => p.Barbearia)
                .Include(a => a.Servico) // Assume que Agendamento tem um ServicoId (mestre)
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

        // GET: Agendamentos/Create 
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

        // POST: Agendamentos/Create (Agora recebe a lista de serviços selecionados)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,DataHora,ProfissionalId")] Agendamento agendamento,
            string ServicosSelecionados)
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

            // Processa os IDs de serviços selecionados (ex: "1,5,7")
            var servicoIds = ServicosSelecionados?.Split(',')
                                                  .Where(s => int.TryParse(s.Trim(), out _))
                                                  .Select(int.Parse)
                                                  .ToList() ?? new List<int>();

            if (!servicoIds.Any())
            {
                ModelState.AddModelError(string.Empty, "Selecione pelo menos um serviço.");
            }

            if (ModelState.IsValid)
            {
                // ** LÓGICA DE SALVAMENTO DE MÚLTIPLOS SERVIÇOS **
                // Simplificação: Cria um Agendamento para cada serviço, sequencialmente.
                // Isso garante que os slots de 30 min sejam ocupados no banco.

                var dataHoraInicio = agendamento.DataHora;
                var duracaoServico = TimeSpan.FromMinutes(30);

                foreach (var servicoId in servicoIds)
                {
                    // Cria uma NOVA instância de agendamento para cada serviço
                    var novoAgendamento = new Agendamento
                    {
                        DataHora = dataHoraInicio,
                        ProfissionalId = agendamento.ProfissionalId,
                        UsuarioId = agendamento.UsuarioId,
                        ServicoId = servicoId // Salva o ID do serviço individual
                    };

                    _context.Add(novoAgendamento);

                    // Avança o horário de início para o próximo serviço (30 minutos)
                    dataHoraInicio = dataHoraInicio.Add(duracaoServico);
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Se houver erro, recarrega o ViewData
            var usuarioLogado = _context.Usuarios.FirstOrDefault(u => u.Id == agendamento.UsuarioId);
            if (usuarioLogado != null)
            {
                ViewData["NomeClienteLogado"] = usuarioLogado.Nome;
            }
            ViewData["BarbeariaId"] = new SelectList(_context.Barbearias, "Id", "Nome");

            return View(agendamento);
        }

        // ... (Edit, Delete, Details - Mantidos do código anterior, com correções) ...
        public async Task<IActionResult> Edit(int? id) { /* ... */ return View(); }
        [HttpPost] public async Task<IActionResult> Edit(int id, [Bind("Id,DataHora,UsuarioId,ProfissionalId,ServicoId")] Agendamento agendamento) { /* ... */ return View(); }
        public async Task<IActionResult> Delete(int? id) { /* ... */ return View(); }
        [HttpPost, ActionName("Delete")] public async Task<IActionResult> DeleteConfirmed(int id) { /* ... */ return RedirectToAction(nameof(Index)); }
        private bool AgendamentoExists(int id) { return _context.Agendamentos.Any(e => e.Id == id); }

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

        // NOVO ENDPOINT: Retorna serviços com detalhes (Nome e Preço)
        [HttpGet]
        public IActionResult GetServicosDetalhesPorBarbearia(int barbeariaId)
        {
            var servicos = _context.Servicos
                           .Where(s => s.BarbeariaId == barbeariaId)
                           .Select(s => new {
                               id = s.Id,
                               nome = s.Nome,
                               preco = s.Preco // Assume a propriedade 'Preco'
                           })
                           .Distinct()
                           .ToList();

            return Json(servicos);
        }

        // NOVO ENDPOINT: Gera e verifica horários disponíveis (com duração variável)
        [HttpGet]
        public IActionResult GetHorariosDisponiveis(int profissionalId, string data, int duracaoMinutos)
        {
            if (!DateTime.TryParse(data, out DateTime dataSelecionada))
            {
                return BadRequest(new { message = "Data inválida." });
            }

            if (duracaoMinutos <= 0)
            {
                return Json(new List<object>());
            }

            var horaInicio = new TimeSpan(8, 0, 0);
            var horaFim = new TimeSpan(18, 0, 0);
            var duracaoIntervalo = TimeSpan.FromMinutes(30);
            var duracaoAgendamento = TimeSpan.FromMinutes(duracaoMinutos); // Duração total requerida

            // Buscar todos os blocos ocupados (considerando 30 minutos por agendamento salvo)
            var agendamentosOcupados = _context.Agendamentos
                .Where(a => a.ProfissionalId == profissionalId &&
                            a.DataHora.Date == dataSelecionada.Date)
                .OrderBy(a => a.DataHora)
                .Select(a => new {
                    Inicio = a.DataHora,
                    // Assumimos que cada Agendamento salvo no BD dura 30 minutos
                    Fim = a.DataHora.Add(TimeSpan.FromMinutes(30))
                })
                .ToList();

            var listaHorarios = new List<object>();

            // Gera a lista de horários de início válidos
            for (var hora = horaInicio; hora.Add(duracaoAgendamento) <= horaFim; hora = hora.Add(duracaoIntervalo))
            {
                var blocoInicio = dataSelecionada.Date.Add(hora);
                var blocoFim = blocoInicio.Add(duracaoAgendamento);

                // Impede agendamentos no passado
                if (blocoInicio <= DateTime.Now)
                {
                    continue;
                }

                bool isOcupado = false;

                // Verifica se o bloco de tempo (blocoInicio a blocoFim) se sobrepõe a qualquer agendamento existente
                foreach (var agendamento in agendamentosOcupados)
                {
                    // Lógica de sobreposição: A < D e C > B
                    if (blocoInicio < agendamento.Fim && blocoFim > agendamento.Inicio)
                    {
                        isOcupado = true;
                        break;
                    }
                }
                // Adiciona o horário à lista com a disponibilidade
                listaHorarios.Add(new
                {
                    horario = hora.ToString(@"hh\:mm"),
                    dataHoraCompleta = blocoInicio.ToString("yyyy-MM-ddTHH:mm:ss"),
                    disponivel = !isOcupado
                });
            }

            return Json(listaHorarios);
        }
    }
}