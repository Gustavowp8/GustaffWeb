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
                    model.UserId = userId; // Define o ID do usuário logado
                    db.Despesas.Add(model);
                    await db.SaveChangesAsync();

                    TempData["mensagem"] = MensagemModel.Serializar("Nova despesa adicionada");

                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Usuário não encontrado.");
                    Debug.WriteLine("Erro: Usuário não encontrado.");
                }
                Debug.WriteLine("Erro: ModelState inválido.");
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
    }
}
