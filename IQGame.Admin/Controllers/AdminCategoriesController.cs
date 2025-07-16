using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IQGame.Admin.Models;
using IQGame.Application.Interfaces;
using IQGame.Domain.Interfaces;
using IQGame.Infrastructure.Persistence;
using IQGame.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IQGame.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCategoriesController : BaseAdminController
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<AdminCategoriesController> _logger;
        private readonly ISessionService _sessionService;

        public AdminCategoriesController(ICategoryRepository categoryRepository, ILogger<AdminCategoriesController> logger, IQGameDbContext context, ISessionService sessionService)
            : base(context)
        {
            _categoryRepository = categoryRepository;
            _logger = logger;
            _sessionService = sessionService;
        }

        // GET: AdminCategories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.Include(c => c.Group).ToListAsync());
        }

        // GET: AdminCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _categoryRepository.GetByIdAsync(id.Value);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // GET: AdminCategories/Create
        public IActionResult Create()
        {
            ViewData["GroupId"] = new SelectList(_context.Groups.Where(g => g.IsActive), "Id", "Name");
            return View();
        }

        // POST: AdminCategories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["GroupId"] = new SelectList(_context.Groups.Where(g => g.IsActive), "Id", "Name");
                return View(model);
            }

            string imagePath = "/images/categories/placeholder.jpg";

            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                var apiPath = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
                    "IQGame", "wwwroot", "images", "categories");

                Directory.CreateDirectory(apiPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var filePath = Path.Combine(apiPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(stream);
                }

                imagePath = $"/images/categories/{fileName}";
            }

            var category = new Category
            {
                Name = model.Name,
                Description = model.Description,
                GroupId = model.GroupId,
                ImageUrl = imagePath,
                DisableMCQ = model.DisableMCQ
            };

            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: AdminCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _categoryRepository.GetByIdAsync(id.Value);
            if (category == null)
                return NotFound();

            var model = new EditCategoryModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                GroupId = category.GroupId,
                ImageUrl = category.ImageUrl,
                DisableMCQ = category.DisableMCQ
            };

            ViewData["GroupId"] = new SelectList(_context.Groups.Where(g => g.IsActive), "Id", "Name", category.GroupId);
            return View(model);
        }

        // POST: AdminCategories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditCategoryModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var category = await _categoryRepository.GetByIdAsync(id);
                    if (category == null)
                        return NotFound();

                    // Update basic properties
                    category.Name = model.Name;
                    category.Description = model.Description;
                    category.GroupId = model.GroupId;
                    category.DisableMCQ = model.DisableMCQ;

                    // Handle image update
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        var apiPath = Path.Combine(
                            Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
                            "IQGame", "wwwroot", "images", "categories");

                        Directory.CreateDirectory(apiPath);

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                        var filePath = Path.Combine(apiPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(stream);
                        }

                        category.ImageUrl = $"/images/categories/{fileName}";
                    }

                    await _categoryRepository.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating category {id}");
                    throw;
                }
            }

            ViewData["GroupId"] = new SelectList(_context.Groups.Where(g => g.IsActive), "Id", "Name", model.GroupId);
            return View(model);
        }

        // GET: AdminCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var category = await _categoryRepository.GetByIdAsync(id.Value);
            if (category == null)
                return NotFound();

            return View(category);
        }

        // POST: AdminCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category != null)
            {
                try
                {
                    // Delete sessions that contain this category
                    var deletedSessionsCount = await _sessionService.DeleteSessionsContainingCategoryAsync(id);
                    
                    // Delete the category
                    await _categoryRepository.DeleteAsync(category);
                    await _categoryRepository.SaveChangesAsync();

                    if (deletedSessionsCount > 0)
                    {
                        TempData["Message"] = $"Category '{category.Name}' deleted successfully. {deletedSessionsCount} session(s) that contained this category were also deleted.";
                    }
                    else
                    {
                        TempData["Message"] = $"Category '{category.Name}' deleted successfully.";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting category {id}");
                    TempData["Error"] = $"Error deleting category: {ex.Message}";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> BulkDelete(int[] selectedIds)
        {
            if (selectedIds != null && selectedIds.Length > 0)
            {
                var totalDeletedSessions = 0;
                var deletedCategories = new List<string>();

                foreach (var id in selectedIds)
                {
                    var category = await _categoryRepository.GetByIdAsync(id);
                    if (category != null)
                    {
                        try
                        {
                            // Delete sessions that contain this category
                            var deletedSessionsCount = await _sessionService.DeleteSessionsContainingCategoryAsync(id);
                            totalDeletedSessions += deletedSessionsCount;

                            // Delete the category
                            await _categoryRepository.DeleteAsync(category);
                            deletedCategories.Add(category.Name);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Error deleting category {id}");
                            TempData["Error"] = $"Error deleting category {category.Name}: {ex.Message}";
                        }
                    }
                }

                await _categoryRepository.SaveChangesAsync();

                if (deletedCategories.Any())
                {
                    var message = $"Successfully deleted {deletedCategories.Count} category(ies): {string.Join(", ", deletedCategories)}";
                    if (totalDeletedSessions > 0)
                    {
                        message += $". {totalDeletedSessions} session(s) that contained these categories were also deleted.";
                    }
                    TempData["Message"] = message;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: AdminCategories/CleanupUnusedImages
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CleanupUnusedImages()
        {
            try
            {
                // Get all category image paths from database
                var categories = await _categoryRepository.GetAllAsync();
                var dbImagePaths = categories
                    .Where(c => c.ImageUrl != null)
                    .Select(c => c.ImageUrl)
                    .ToList();

                // Convert DB paths to filenames
                var dbImageFiles = dbImagePaths
                    .Select(path => Path.GetFileName(path?.TrimStart('/')))
                    .Where(filename => !string.IsNullOrEmpty(filename))
                    .ToList();

                // Get physical path to images folder
                var imagesPath = Path.Combine(
                    Directory.GetParent(Directory.GetCurrentDirectory())!.FullName,
                    "IQGame", "wwwroot", "images", "categories");

                if (!Directory.Exists(imagesPath))
                {
                    return RedirectToAction(nameof(Index));
                }

                // Get all files in the images folder
                var physicalFiles = Directory.GetFiles(imagesPath)
                    .Select(Path.GetFileName)
                    .ToList();

                // Find files that exist in folder but not in DB
                var unusedFiles = physicalFiles
                    .Where(file => !dbImageFiles.Contains(file))
                    .ToList();

                // Delete unused files
                foreach (var file in unusedFiles)
                {
                    var fullPath = Path.Combine(imagesPath, file);
                    if (System.IO.File.Exists(fullPath))
                    {
                        System.IO.File.Delete(fullPath);
                    }
                }

                TempData["Message"] = $"Successfully deleted {unusedFiles.Count} unused image(s).";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error cleaning up images: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
