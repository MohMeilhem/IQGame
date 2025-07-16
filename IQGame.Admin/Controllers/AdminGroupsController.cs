using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class AdminGroupsController : BaseAdminController
    {
        private readonly IGroupRepository _groupRepository;
        private readonly ILogger<AdminGroupsController> _logger;

        public AdminGroupsController(IGroupRepository groupRepository, ILogger<AdminGroupsController> logger, IQGameDbContext context)
            : base(context)
        {
            _groupRepository = groupRepository;
            _logger = logger;
        }

        // GET: AdminGroups
        public async Task<IActionResult> Index()
        {
            var groups = await _groupRepository.GetAllAsync();
            return View(groups);
        }

        // GET: AdminGroups/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var group = await _groupRepository.GetByIdAsync(id.Value);
            if (group == null)
                return NotFound();

            return View(group);
        }

        // GET: AdminGroups/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: AdminGroups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Group group)
        {
            if (ModelState.IsValid)
            {
                // Check if group name already exists
                var existingGroup = await _groupRepository.GetByNameAsync(group.Name);
                if (existingGroup != null)
                {
                    ModelState.AddModelError("Name", "A group with this name already exists.");
                    return View(group);
                }

                await _groupRepository.AddAsync(group);
                await _groupRepository.SaveChangesAsync();
                
                TempData["Message"] = $"Group '{group.Name}' created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(group);
        }

        // GET: AdminGroups/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var group = await _groupRepository.GetByIdAsync(id.Value);
            if (group == null)
                return NotFound();

            return View(group);
        }

        // POST: AdminGroups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Group group)
        {
            if (id != group.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Check if group name already exists (excluding current group)
                    var existingGroup = await _groupRepository.GetByNameAsync(group.Name);
                    if (existingGroup != null && existingGroup.Id != id)
                    {
                        ModelState.AddModelError("Name", "A group with this name already exists.");
                        return View(group);
                    }

                    await _groupRepository.UpdateAsync(group);
                    await _groupRepository.SaveChangesAsync();
                    
                    TempData["Message"] = $"Group '{group.Name}' updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error updating group {id}");
                    ModelState.AddModelError("", "An error occurred while updating the group.");
                }
            }
            return View(group);
        }

        // GET: AdminGroups/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var group = await _groupRepository.GetByIdAsync(id.Value);
            if (group == null)
                return NotFound();

            return View(group);
        }

        // POST: AdminGroups/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var group = await _groupRepository.GetByIdAsync(id);
            if (group != null)
            {
                try
                {
                    // Check if group has categories
                    if (group.Categories != null && group.Categories.Any())
                    {
                        TempData["Error"] = $"Cannot delete group '{group.Name}' because it has {group.Categories.Count} categories assigned to it.";
                        return RedirectToAction(nameof(Index));
                    }

                    await _groupRepository.DeleteAsync(group);
                    await _groupRepository.SaveChangesAsync();
                    
                    TempData["Message"] = $"Group '{group.Name}' deleted successfully.";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error deleting group {id}");
                    TempData["Error"] = "An error occurred while deleting the group.";
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
} 