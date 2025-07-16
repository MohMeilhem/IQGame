using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IQGame.Domain.Interfaces;
using IQGame.Infrastructure.Persistence;
using IQGame.Infrastructure.Repositories;
using IQGame.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IQGame.Admin.Models;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace IQGame.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminAnswersController : BaseAdminController
    {
        public AdminAnswersController(IQGameDbContext context)
            : base(context)
        {
        }

        // GET: AdminAnswers
        public async Task<IActionResult> Index(int? questionId, bool? isCorrect, int? categoryId, int? difficulty)
        {
            var answersQuery = _context.Answers
                .Include(a => a.Question)
                    .ThenInclude(q => q.Category)
                .AsQueryable();

            if (questionId.HasValue)
                answersQuery = answersQuery.Where(a => a.QuestionId == questionId.Value);

            if (isCorrect.HasValue)
                answersQuery = answersQuery.Where(a => a.IsCorrect == isCorrect.Value);
            else
                // Default to showing only correct answers if no correctness filter is applied
                answersQuery = answersQuery.Where(a => a.IsCorrect == true);

            if (categoryId.HasValue)
                answersQuery = answersQuery.Where(a => a.Question.CategoryId == categoryId.Value);

            if (difficulty.HasValue)
                answersQuery = answersQuery.Where(a => a.Question.Difficulty == difficulty.Value);

            ViewData["Questions"] = new SelectList(_context.Questions
                .Include(q => q.Category)
                .Select(q => new { 
                    Id = q.Id, 
                    Text = $"{q.Text} ({q.Category.Name})" 
                }), "Id", "Text");
            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["SelectedQuestionId"] = questionId;
            ViewData["SelectedCorrectness"] = isCorrect?.ToString().ToLower() ?? "true";
            ViewData["SelectedCategoryId"] = categoryId;
            ViewData["SelectedDifficulty"] = difficulty;

            return View(await answersQuery.ToListAsync());
        }

        // GET: AdminAnswers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var answer = await _context.Answers
                .Include(a => a.Question)
                    .ThenInclude(q => q.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (answer == null)
                return NotFound();

            return View(answer);
        }

        // GET: AdminAnswers/Create
        public IActionResult Create()
        {
            var questionsWithCategories = _context.Questions
                .Include(q => q.Category)
                .Select(q => new { 
                    Id = q.Id, 
                    Text = $"{q.Text} ({q.Category.Name})" 
                })
                .ToList();

            ViewData["QuestionId"] = new SelectList(questionsWithCategories, "Id", "Text");
            return View();
        }

        // POST: AdminAnswers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Answer answer, IFormFile ImageFile, int? returnQuestionId, bool? returnCorrectness, int? returnCategoryId)
        {
            string imagePath = "/images/defaults/answer-placeholder.png";

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var apiPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "IQGame", "wwwroot", "images", "answers");
                Directory.CreateDirectory(apiPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(apiPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                imagePath = $"/images/answers/{fileName}";
            }

            answer.ImageUrl = imagePath;

            _context.Answers.Add(answer);
            await _context.SaveChangesAsync();

            // Redirect back to Index with preserved filters
            var redirectParams = new Dictionary<string, object>();
            if (returnQuestionId.HasValue)
                redirectParams["questionId"] = returnQuestionId.Value;
            if (returnCorrectness.HasValue)
                redirectParams["isCorrect"] = returnCorrectness.Value;
            if (returnCategoryId.HasValue)
                redirectParams["categoryId"] = returnCategoryId.Value;
            
            return RedirectToAction(nameof(Index), redirectParams);
        }

        private void PopulateQuestionViewData(EditAnswerModel model, int? returnQuestionId, bool? returnCorrectness, int? returnCategoryId)
        {
            var questionsWithCategories = _context.Questions
                .Include(q => q.Category)
                .Select(q => new {
                    Id = q.Id,
                    Text = q.Text + " (" + q.Category.Name + ") [" + (q.Difficulty == 1 ? "Easy" : q.Difficulty == 2 ? "Medium" : "Hard") + "]"
                })
                .ToList();

            ViewData["QuestionId"] = new SelectList(questionsWithCategories, "Id", "Text", model.QuestionId);
            ViewData["ReturnQuestionId"] = returnQuestionId;
            ViewData["ReturnCorrectness"] = returnCorrectness;
            ViewData["ReturnCategoryId"] = returnCategoryId;
        }

        // GET: AdminAnswers/Edit/5
        public async Task<IActionResult> Edit(int? id, int? questionId, bool? isCorrect, int? categoryId)
        {
            if (id == null)
                return NotFound();

            var answer = await _context.Answers.FindAsync(id);
            if (answer == null)
                return NotFound();

            var model = new EditAnswerModel
            {
                Id = answer.Id,
                Text = answer.Text,
                IsCorrect = answer.IsCorrect,
                QuestionId = answer.QuestionId
            };

            PopulateQuestionViewData(model, questionId, isCorrect, categoryId);
            return View(model);
        }

        // POST: AdminAnswers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditAnswerModel model, IFormFile ImageFile, int? returnQuestionId, bool? returnCorrectness, int? returnCategoryId)
        {
            ModelState.Remove("ImageFile");
            Console.WriteLine($"[Edit Answer] Starting edit for answer {id}");
            Console.WriteLine($"[Edit Answer] Model.Id: {model.Id}");
            Console.WriteLine($"[Edit Answer] Model.Text: '{model.Text}'");
            Console.WriteLine($"[Edit Answer] Model.IsCorrect: {model.IsCorrect}");
            Console.WriteLine($"[Edit Answer] Model.QuestionId: {model.QuestionId}");
            Console.WriteLine($"[Edit Answer] ImageFile is null: {ImageFile == null}");
            if (ImageFile != null)
            {
                Console.WriteLine($"[Edit Answer] ImageFile length: {ImageFile.Length}");
                Console.WriteLine($"[Edit Answer] ImageFile name: {ImageFile.FileName}");
            }

            // Log model state for debugging
            Console.WriteLine($"[Edit Answer] ModelState.IsValid: {ModelState.IsValid}");
            Console.WriteLine($"[Edit Answer] ModelState error count: {ModelState.ErrorCount}");
            foreach (var key in ModelState.Keys)
            {
                var state = ModelState[key];
                if (state.ValidationState != ModelValidationState.Valid)
                {
                    Console.WriteLine($"[Edit Answer] ModelState error for {key}: {string.Join(", ", state.Errors.Select(e => e.ErrorMessage))}");
                }
            }

            // Log request data for debugging
            Console.WriteLine($"[Edit Answer] Request.Form keys: {string.Join(", ", Request.Form.Keys)}");
            foreach (var key in Request.Form.Keys)
            {
                Console.WriteLine($"[Edit Answer] Form[{key}]: {Request.Form[key]}");
            }

            // Try to manually bind QuestionId if it's not bound correctly
            if (!model.QuestionId.HasValue && Request.Form.ContainsKey("QuestionId"))
            {
                var questionIdValue = Request.Form["QuestionId"].ToString();
                Console.WriteLine($"[Edit Answer] Manual binding - QuestionId from form: {questionIdValue}");
                if (int.TryParse(questionIdValue, out int parsedQuestionId))
                {
                    model.QuestionId = parsedQuestionId;
                    Console.WriteLine($"[Edit Answer] Manual binding - QuestionId parsed successfully: {model.QuestionId}");
                }
                else
                {
                    Console.WriteLine($"[Edit Answer] Manual binding - Failed to parse QuestionId: {questionIdValue}");
                }
            }

            // Validate QuestionId exists in database
            if (model.QuestionId.HasValue)
            {
                var questionExists = await _context.Questions.AnyAsync(q => q.Id == model.QuestionId.Value);
                Console.WriteLine($"[Edit Answer] QuestionId {model.QuestionId.Value} exists in database: {questionExists}");
                if (!questionExists)
                {
                    ModelState.AddModelError("QuestionId", "The selected question does not exist.");
                }
            }

            if (id != model.Id)
                return NotFound();

            var existing = await _context.Answers.FindAsync(id);
            if (existing == null)
                return NotFound();

            Console.WriteLine($"[Edit Answer] Existing answer found: {existing.Text}");
            Console.WriteLine($"[Edit Answer] Existing QuestionId: {existing.QuestionId}");

            if (ModelState.IsValid)
            {
                Console.WriteLine($"[Edit Answer] ModelState is valid, proceeding with update");
                existing.Text = model.Text;
                existing.IsCorrect = model.IsCorrect;
                
                // Ensure QuestionId is not null before assignment
                if (model.QuestionId.HasValue)
                {
                    existing.QuestionId = model.QuestionId.Value;
                }
                else
                {
                    Console.WriteLine($"[Edit Answer] QuestionId is null, adding validation error");
                    ModelState.AddModelError("QuestionId", "Please select a question.");
                    // Re-populate the view data and return the view
                    PopulateQuestionViewData(model, returnQuestionId, returnCorrectness, returnCategoryId);
                    return View(model);
                }

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    Console.WriteLine($"[Edit Answer] Processing new image upload");
                    var apiPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "IQGame", "wwwroot", "images", "answers");
                    Directory.CreateDirectory(apiPath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(apiPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    existing.ImageUrl = $"/images/answers/{fileName}";
                    Console.WriteLine($"[Edit Answer] New image URL set: {existing.ImageUrl}");
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"[Edit Answer] Database updated successfully");
                
                // Redirect back to Index with preserved filters
                var redirectParams = new Dictionary<string, object>();
                if (returnQuestionId.HasValue)
                    redirectParams["questionId"] = returnQuestionId.Value;
                if (returnCorrectness.HasValue)
                    redirectParams["isCorrect"] = returnCorrectness.Value;
                if (returnCategoryId.HasValue)
                    redirectParams["categoryId"] = returnCategoryId.Value;
                
                return RedirectToAction(nameof(Index), redirectParams);
            }
            else
            {
                Console.WriteLine($"[Edit Answer] ModelState is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"[Edit Answer] Validation error: {error.ErrorMessage}");
                }
            }

            PopulateQuestionViewData(model, returnQuestionId, returnCorrectness, returnCategoryId);
            return View(model);
        }

        // GET: AdminAnswers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var answer = await _context.Answers
                .Include(a => a.Question)
                    .ThenInclude(q => q.Category)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (answer == null)
                return NotFound();

            return View(answer);
        }

        // POST: AdminAnswers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? returnQuestionId, bool? returnCorrectness, int? returnCategoryId)
        {
            var answer = await _context.Answers.FindAsync(id);
            if (answer != null)
            {
                _context.Answers.Remove(answer);
            }

            await _context.SaveChangesAsync();
            
            // Redirect back to Index with preserved filters
            var redirectParams = new Dictionary<string, object>();
            if (returnQuestionId.HasValue)
                redirectParams["questionId"] = returnQuestionId.Value;
            if (returnCorrectness.HasValue)
                redirectParams["isCorrect"] = returnCorrectness.Value;
            if (returnCategoryId.HasValue)
                redirectParams["categoryId"] = returnCategoryId.Value;
            
            return RedirectToAction(nameof(Index), redirectParams);
        }

        private bool AnswerExists(int id)
        {
            return _context.Answers.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> BulkDelete(int[] selectedIds, int? returnQuestionId, bool? returnCorrectness, int? returnCategoryId)
        {
            if (selectedIds != null && selectedIds.Length > 0)
            {
                var answers = await _context.Answers
                    .Where(a => selectedIds.Contains(a.Id))
                    .ToListAsync();

                _context.Answers.RemoveRange(answers);
                await _context.SaveChangesAsync();
            }

            // Redirect back to Index with preserved filters
            var redirectParams = new Dictionary<string, object>();
            if (returnQuestionId.HasValue)
                redirectParams["questionId"] = returnQuestionId.Value;
            if (returnCorrectness.HasValue)
                redirectParams["isCorrect"] = returnCorrectness.Value;
            if (returnCategoryId.HasValue)
                redirectParams["categoryId"] = returnCategoryId.Value;
            
            return RedirectToAction(nameof(Index), redirectParams);
        }
    }
}
