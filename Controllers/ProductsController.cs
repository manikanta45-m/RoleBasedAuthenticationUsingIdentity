using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RoleBasedAuthenticationUsingIdentity.Models;
using OfficeOpenXml;
using ExcelDataReader;
using System.Text;

namespace RoleBasedAuthenticationUsingIdentity.Controllers
{

    public class ProductsController : Controller
    {
        private readonly UserContext _context;

        public ProductsController(UserContext context)
        {
            _context = context;
        }
        // GET: Products
        public async Task<IActionResult> Index()
        {
            return _context.Product != null ?
                        View(await _context.Product.ToListAsync()) :
                        Problem("Entity set 'UserContext.Product'  is null.");
        }
        public async Task<IActionResult> Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(String UserName, string password)
        {
            if (UserName != null && password != null)
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.UserName == UserName && u.Password == password);
                ClaimsIdentity identity = null;
                bool isAuthenticated = false;
                if (user != null)
                {
                    identity = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.UserName),
                        new Claim(ClaimTypes.Role, user.Role)
                    },
                    CookieAuthenticationDefaults.AuthenticationScheme);
                    isAuthenticated = true;
                }
                else
                {
                    return View("Invalid Credentials");
                }
                if (isAuthenticated)
                {
                    var principal = new ClaimsPrincipal(identity);
                    var login = HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return RedirectToAction("Index", "Products");
                }
            }
            else
            {
                return View("Email & Password are not provided.");
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [Authorize(Roles = "Admin")]
        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ProductId,ProductName,Price,ProductType")] Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        [Authorize(Roles = "Admin")]
        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,ProductName,Price,ProductType")] Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }
        [Authorize(Roles = "Admin")]
        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Product == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
        [Authorize(Roles = "Admin")]
        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Product == null)
            {
                return Problem("Entity set 'UserContext.Product'  is null.");
            }
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        [HttpGet]
        public IActionResult UploadExcel()
        {
            return View();
        }

        
        [HttpPost]
        public async Task<IActionResult> UploadExcel(IFormFile file)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            //var products = new List<ProductTables>();
            if (file != null || file.Length > 0)
            {
                var uploadsfolder = $"{Directory.GetCurrentDirectory()}\\wwwroot\\uploads\\";
                var filePath = Path.Combine(uploadsfolder, file.FileName);
                using(var stream = new FileStream(filePath,FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }
                using (var stream = System.IO.File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        do
                        {
                            while (reader.Read())
                            {
                                ProductTables product = new ProductTables
                                {
                                    ProductName = reader.GetValue(1).ToString(),
                                    Price = reader.GetValue(2).ToString(),
                                    ProductType = reader.GetValue(3).ToString(),
                                    Quantity = reader.GetValue(4).ToString(),
                                };
                                _context.Add(product);
                                await _context.SaveChangesAsync();

                            }
                        } while (reader.NextResult());
                    }
                }          
               

                return RedirectToAction("ProductDetails");
            }
            return View();
        }
        public async Task<IActionResult> ProductDetails()
        {
            return View(await _context.ProductTables.ToListAsync());
        }




            //public async Task<IActionResult> ExportData(string format)
            //{
            //    var product = await _context.ProductTable.ToListAsync();
            //    switch(format.ToLower()) 
            //    {
            //        case "excel":
            //            return ExportToExcel(ProductTable);
            //        case "pdf":
            //            return ExportToPdf(ProductTable);
            //        case "csv":
            //            return ExportToCsv(ProductTable);
            //        default:
            //            return BadRequest("Invalid export format");
            //    }
            //}

            //private IActionResult ExportToExcel(List<ProductTable> products) 
            //{
            //    using (var package = new ExcelPackage())
            //    {
            //        var worksheet = package.Workbook.Worksheets.Add("Products");

            //        worksheet.Cells.LoadFromCollection(products, true);

            //        var stream = new MemoryStream(package.GetAsByteArray());

            //        return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Products.xlsx");
            //    }
            //}


            private bool ProductExists(int id)
            {
                return (_context.Product?.Any(e => e.ProductId == id)).GetValueOrDefault();
            }
        }
    }
