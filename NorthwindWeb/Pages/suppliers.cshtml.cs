using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Packt.Shared; // NorthwindContext

namespace NorthwindWeb.Pages;

public class SuppliersModel : PageModel
{
    private Northwind db;
    public SuppliersModel(Northwind injectedContext)
    {
        db = injectedContext;
    }

    public IEnumerable<Supplier>? Suppliers { get; set; }

    public void OnGet()
    {
        ViewData["Title"] = "Northwind B2B - Suppliers";

        Suppliers = db.Suppliers
            .OrderBy(c => c.Country)
            .ThenBy(c => c.CompanyName);
    }

    [BindProperty]
    public Supplier? Supplier { get; set; }

    public IActionResult OnPost()
    {
        if ((Supplier is not null) && ModelState.IsValid)
        {
            db.Suppliers.Add(Supplier);
            db.SaveChanges();
            return RedirectToPage("/suppliers");
        }
        else
        {
            return Page();
        }
    }
}