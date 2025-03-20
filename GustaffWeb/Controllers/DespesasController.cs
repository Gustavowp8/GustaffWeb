using GustaffWeb.Data;
using GustaffWeb.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace GustaffWeb.Controllers
{
    [Authorize]
    public class DespesasController : Controller
    {
        private readonly Context db;
        private readonly UserManager<IdentityUser> _userManager;
        private string userId;

        public DespesasController(Context db, UserManager<IdentityUser> userManager)
        {
            this.db = db;
            this._userManager = userManager;
        }


        public IActionResult Index(int? mes)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var despesas = db.Despesas.Where(s => s.UserId == userId).ToList();

            int mesSelecionado = mes.HasValue ? mes.Value : DateTime.Now.Month;
            ViewBag.MesSelecionado = mesSelecionado;

            if (mes.HasValue)
            {
                despesas = despesas.Where(d => d.Vencimento.Month == mes.Value).ToList(); 
            }
            else
            {
                despesas = despesas.Where(d => d.Vencimento.Month == DateTime.Now.Month).ToList();
            }

            // Calcular totais
            var totalDespesasPagas = despesas.Where(d => d.Pago).Sum(d => d.Preco);
            var totalDespesasNaoPagas = despesas.Where(d => !d.Pago).Sum(d => d.Preco);

            // Passar totais para a view
            ViewBag.TotalDespesasPagas = totalDespesasPagas;
            ViewBag.TotalDespesasNaoPagas = totalDespesasNaoPagas;

            return View(despesas);
        }

        [HttpPost]
        public async Task<IActionResult> AdicionarGastos(DespesasModel model)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (userId != null)
                {
                    model.UserId = userId;

                    // Salvando a despesa atual
                    db.Despesas.Add(model);

                    // Verificando se é uma despesa fixa e número de parcelas foi definido
                    if (model.Fixo && model.NumeroParcelas > 1)
                    {
                        for (int i = 1; i < model.NumeroParcelas; i++)
                        {
                            // Criar uma nova instância de despesa para cada mês adicional
                            var novaDespesa = new DespesasModel
                            {
                                Titulo = model.Titulo,
                                Preco = model.Preco,
                                Fixo = model.Fixo,
                                Pago = false,
                                NumeroParcelas = model.NumeroParcelas,
                                Data = DateTime.Now,
                                Vencimento = model.Vencimento.AddMonths(i),
                                UserId = userId
                            };

                            db.Despesas.Add(novaDespesa);
                        }
                    }

                    await db.SaveChangesAsync();

                    TempData["mensagem"] = MensagemModel.Serializar("Nova despesa adicionada com sucesso!");

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                    Debug.WriteLine("Erro: Usuário não encontrado.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erro ao salvar as alterações: {ex}");
                TempData["mensagem"] = MensagemModel.Serializar("Houve um erro: " + ex.Message, TipoMensagem.Erro);
                return RedirectToAction("Index");
            }

            TempData["mensagem"] = MensagemModel.Serializar("Houve um erro", TipoMensagem.Erro);
            return RedirectToAction("Index");
        }

        //Ver detalher da despesa
        public IActionResult Ver(int id)
        {
            var Des = db.Despesas.Where(g => g.DespesasId == id).FirstOrDefault();
            if (Des == null)
            {
                return NotFound();
            }

            return View(Des);
        }

        [HttpPost]
        public IActionResult AtualizarStatus(int id)
        {
            var despesa = db.Despesas.Where(g => g.DespesasId == id).FirstOrDefault();
            if (despesa == null)
            {
                return NotFound();
            }

            despesa.Pago = true;
            db.SaveChanges();

            TempData["mensagem"] = MensagemModel.Serializar("Divida paga!");
            return RedirectToAction("Index", new { id = despesa.DespesasId });
        }

        [HttpPost]
        public IActionResult Apagar(int id)
        {
            db.Despesas.Remove(db.Despesas.Where(g => g.DespesasId == id).FirstOrDefault());
            db.SaveChanges();

            TempData["mensagem"] = MensagemModel.Serializar("O registro foi apagado!", TipoMensagem.Erro);

            return RedirectToAction("Index");
        }

        //Graficos
        public IActionResult Graficos()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId != null)
            {
                // Obtenha as despesas do banco de dados associadas ao usuário
                var despesasMensais = db.Despesas
                    .Where(d => d.UserId == userId) // Filtra despesas do usuário
                    .GroupBy(d => new { d.Data.Year, d.Data.Month }) // Agrupa por ano e mês
                    .Select(g => new
                    {
                        Ano = g.Key.Year,
                        Mes = g.Key.Month,
                        TotalPagas = g.Count(d => d.Pago == true), // Conta o número de despesas pagas em cada grupo
                        TotalNaoPagas = g.Count(d => d.Pago == false) // Conta o número de despesas não pagas em cada grupo
                    })
                    .ToList();

                // Verificação de dados
                foreach (var item in despesasMensais)
                {
                    Console.WriteLine($"Ano: {item.Ano}, Mês: {item.Mes}, Pagas: {item.TotalPagas}, Não Pagas: {item.TotalNaoPagas}");
                }

                // Cria arrays com 12 posições para preencher todos os meses
                var totaisPagasMensais = new int[12];
                var totaisNaoPagasMensais = new int[12];
                foreach (var item in despesasMensais)
                {
                    totaisPagasMensais[item.Mes - 1] += item.TotalPagas; // Preenche o array com os totais de despesas pagas por mês
                    totaisNaoPagasMensais[item.Mes - 1] += item.TotalNaoPagas; // Preenche o array com os totais de despesas não pagas por mês
                }

                // Verificação de arrays preenchidos
                for (int i = 0; i < 12; i++)
                {
                    Console.WriteLine($"Mês: {i + 1}, Pagas: {totaisPagasMensais[i]}, Não Pagas: {totaisNaoPagasMensais[i]}");
                }

                // Cria os labels dos meses
                var labels = new string[] { "Janeiro", "Fevereiro", "Março", "Abril", "Maio", "Junho", "Julho", "Agosto", "Setembro", "Outubro", "Novembro", "Dezembro" };

                // Envia os dados para a view
                ViewBag.Labels = labels;
                ViewBag.DespesasPagasMensais = totaisPagasMensais;
                ViewBag.DespesasNaoPagasMensais = totaisNaoPagasMensais;
            }

            return View();
        }
    }
}
