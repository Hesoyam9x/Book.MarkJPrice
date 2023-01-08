using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NorthwindMvc.Models;
using Microsoft.EntityFrameworkCore;
using Packt.Shared;

namespace NorthwindMvc.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IHttpClientFactory clientFactory;
    private Northwind db;

    public HomeController(
        ILogger<HomeController> logger, 
        Northwind injectedContext,
        IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        db = injectedContext;
        clientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        HomeIndexViewModel model = new()
        {
            VisitorCount = (new Random()).Next(1, 1001),
            Categories = await db.Categories.ToListAsync(),
            Products = await db.Products.ToListAsync()
        };

        return View(model);
    }

    public async Task<IActionResult> ProductDetail(int? id)
    {
        if (!id.HasValue)
        {
            return NotFound("You must pass a product ID in the route, for example, /Home/ProductDetail/21");
        }
        
        Product? model = await db.Products.SingleOrDefaultAsync(p => p.ProductId == id);
        if (model == null)
        {
            return NotFound($"Product with ID of {id} not found.");
        }

        return View(model);
    }

    public IActionResult ModelBinding()
    {
        return View(); // страница с формой для отправки
    }
    [HttpPost]
    public IActionResult ModelBinding(Thing thing)
    {
        // return View(thing); // отображение модели, полученной через привязку
        HomeModelBindingViewModel model = new()
        {
            Thing = thing,
            HasErrors = !ModelState.IsValid,
            ValidationErrors = ModelState.Values
                .SelectMany(state => state.Errors)
                .Select(error => error.ErrorMessage)
        };
        return View(model);
    }

    public IActionResult ProductsThatCostMoreThan(decimal? price)
    {
        if (!price.HasValue)
        {
            return NotFound("You must pass a product price in the query string, for example, /Home/ProductsThatCostMoreThan?price=50");
        }

        IEnumerable<Product> model = db.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .Where(p => p.UnitPrice > price);

        if (model.Count() == 0)
        {
            return NotFound($"No products cost more than {price:C}.");
        }

        ViewData["MaxPrice"] = price.Value.ToString("C");
        return View(model); // передача модели представлению
    }

    public async Task<IActionResult> Customers(string country)
    {
        string uri;

        if (string.IsNullOrEmpty(country))
        {
            ViewData["Title"] = "All Customers Worldwide";
            uri = "api/customers";
        }
        else
        {
            ViewData["Title"] = $"Customers in {country}";
            uri = $"api/customers/?country={country}";
        }

        HttpClient client = clientFactory.CreateClient(
            name: "NorthwindApi");
            
        HttpRequestMessage request = new(
            method: HttpMethod.Get, requestUri: uri);

        HttpResponseMessage response = await client.SendAsync(request);

        IEnumerable<Customer>? model = await response.Content
            .ReadFromJsonAsync<IEnumerable<Customer>>();

        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
