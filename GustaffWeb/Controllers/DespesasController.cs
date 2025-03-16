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
                // Obtenha as despesas pagas do banco de dados associadas ao usuário
                var despesasMensais = db.Despesas
                    .Where(d => d.UserId == userId && d.Pago == true) // Filtra apenas despesas pagas
                    .GroupBy(d => d.Data.Month) // Agrupa por mês
                    .Select(g => new
                    {
                        Mes = g.Key,
                        Total = g.Sum(d => d.Preco) // Soma os preços das despesas pagas em cada grupo
                    })
                    .ToList();

                // Cria um array com 12 posições para preencher todos os meses
                var totaisMensais = new double[12];
                foreach (var item in despesasMensais)
                {
                    totaisMensais[item.Mes - 1] = item.Total; // Preenche o array com os totais por mês
                }

                // Envia os dados para a view
                ViewBag.GastosMensais = totaisMensais;
            }

            return View();
        }
    }
}
