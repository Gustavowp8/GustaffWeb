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


        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var verRecados = db.Despesas
                              .Where(s => s.UserId == userId)
                              .OrderByDescending(s => s.DespesasId)
                              .ToList();

            return View(verRecados);
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
    }
}
