using Microsoft.AspNetCore.Mvc.RazorPages; // PageModel
using Packt.Shared; // Employee, NorthwindContext

namespace PacktFeatures.Pages;

public class EmployeesPageModel : PageModel
{
    private Northwind db;

    public EmployeesPageModel(Northwind injectedContext)
    {
        db = injectedContext;
    }

    public Employee[] Employees { get; set; } = null!;
    public void OnGet()
    {
        ViewData["Title"] = "Northwind B2B - Employees";
        
        Employees = db.Employees.OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName).ToArray();
    }
}