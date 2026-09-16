using Microsoft.AspNetCore.Mvc;
using Supabase;
using SistemaPastelaria.Models;

namespace SistemaPastelaria.Controllers
{
    public class CardapioController : Controller
    {
        private readonly Client _supabase;

        public CardapioController(Client supabase)
        {
            _supabase = supabase;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? operarioId = HttpContext.Session.GetInt32("IdOperario");
            if (operarioId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            try
            {
                var response = await _supabase.From<Produto>().Get();
                ViewBag.NomeOperario = HttpContext.Session.GetString("NomeOperario");
                ViewBag.NivelAcesso = HttpContext.Session.GetInt32("NivelAcesso") ?? 1;
                return View(response.Models);
            }
            catch (Exception ex)
            {
                ViewBag.Erro = "Erro ao carregar cardápio: " + ex.Message;
                return View(new List<Produto>());
            }
        }

        [HttpGet]
        public IActionResult Cadastrar()
        {
            int? nivel = HttpContext.Session.GetInt32("NivelAcesso");
            if (nivel == null || nivel < 2)
            {
                return RedirectToAction("Index");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Cadastrar(string nome, string categoria, decimal preco)
        {
            int? nivel = HttpContext.Session.GetInt32("NivelAcesso");
            if (nivel == null || nivel < 2)
            {
                return RedirectToAction("Index");
            }

            try
            {
                var produto = new Produto
                {
                    Nome = nome,
                    Categoria = categoria,
                    Preco = preco
                };

                await _supabase.From<Produto>().Insert(produto);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Erro"] = "Erro ao cadastrar produto: " + ex.Message;
                return RedirectToAction("Cadastrar");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Excluir(int id)
        {
            int? nivel = HttpContext.Session.GetInt32("NivelAcesso");
            if (nivel == null || nivel < 4) // Exclusivo Nível 4 (Dono)
            {
                return RedirectToAction("Index");
            }

            try
            {
                await _supabase.From<Produto>()
                    .Where(x => x.IdProduto == id)
                    .Delete();

                return RedirectToAction("Index");
            }
            catch (Exception)
            {
                TempData["Erro"] = "Não foi possível excluir o produto. Ele está vinculado a pedidos anteriores.";
                return RedirectToAction("Index");
            }
        }
    }
}