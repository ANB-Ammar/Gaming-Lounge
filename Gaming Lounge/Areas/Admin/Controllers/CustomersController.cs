using Gaming_Lounge.Data;
using Gaming_Lounge.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gaming_Lounge.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class CustomersController(ApplicationDbContext context) : Controller
{
    private const int PageSize = 15;
    private readonly ApplicationDbContext _context = context;

    public async Task<IActionResult> Index(string? searchTerm, int page = 1)
    {
        var normalizedSearch = searchTerm?.Trim() ?? string.Empty;
        var customerRoleId = await _context.Roles
            .Where(role => role.Name == "Customer")
            .Select(role => role.Id)
            .SingleOrDefaultAsync();

        var query = _context.Users
            .AsNoTracking()
            .Where(user => customerRoleId != null && _context.UserRoles.Any(userRole =>
                userRole.UserId == user.Id && userRole.RoleId == customerRoleId));

        if (!string.IsNullOrWhiteSpace(normalizedSearch))
        {
            query = query.Where(user =>
                user.DisplayName.Contains(normalizedSearch)
                || (user.Email != null && user.Email.Contains(normalizedSearch)));
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        page = Math.Clamp(page, 1, Math.Max(totalPages, 1));

        var customers = await query
            .OrderByDescending(user => user.CreatedAtUtc)
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .Select(user => new AdminCustomerViewModel
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                Email = user.Email ?? string.Empty,
                CreatedAtUtc = user.CreatedAtUtc,
                ReservationCount = user.Reservations.Count
            })
            .ToListAsync();

        return View(new AdminCustomersIndexViewModel
        {
            SearchTerm = normalizedSearch,
            Page = page,
            PageSize = PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            Customers = customers
        });
    }
}