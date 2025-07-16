using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using IQGame.Infrastructure.Persistence;
using IQGame.Shared.Models;
using IQGame.Admin.Models;
using IQGame.Admin.Services;
using Microsoft.AspNetCore.Authorization;

namespace IQGame.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminQuestionsController : BaseAdminController
    {
        private readonly IImageSearchService _imageService;

        public AdminQuestionsController(IQGameDbContext context, IImageSearchService imageService)
            : base(context)
        {
            _imageService = imageService;
        }

        public async Task<IActionResult> Index(int? categoryId, int? difficulty)
        {
            var questionsQuery = _context.Questions
                .Include(q => q.Category)
                .Include(q => q.Answers) // Include answers for correct answer display
                .AsQueryable();

            if (categoryId.HasValue)
                questionsQuery = questionsQuery.Where(q => q.CategoryId == categoryId.Value);

            if (difficulty.HasValue)
                questionsQuery = questionsQuery.Where(q => q.Difficulty == difficulty.Value);

            ViewData["Categories"] = new SelectList(_context.Categories, "Id", "Name");
            ViewData["SelectedCategory"] = categoryId;
            ViewData["SelectedDifficulty"] = difficulty;

            return View(await questionsQuery.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        public IActionResult Create()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Question question, IFormFile ImageFile, int? returnCategoryId, int? returnDifficulty)
        {
            // Assign points based on difficulty
            question.Points = question.Difficulty == 1 ? 250 : question.Difficulty == 2 ? 500 : question.Difficulty == 3 ? 750 : 0;
            string imagePath = "/images/defaults/question-placeholder.png";

            if (ImageFile != null && ImageFile.Length > 0)
            {
                var apiPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "IQGame", "wwwroot", "images", "questions");
                Console.WriteLine($"[Create] Image Save Path: {apiPath}");
                Directory.CreateDirectory(apiPath);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                var filePath = Path.Combine(apiPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                imagePath = $"/images/questions/{fileName}";
            }

            question.ImageUrl = imagePath;

            _context.Add(question);
            await _context.SaveChangesAsync();
            
            // Redirect back to Index with preserved filters
            var redirectParams = new Dictionary<string, object>();
            if (returnCategoryId.HasValue)
                redirectParams["categoryId"] = returnCategoryId.Value;
            if (returnDifficulty.HasValue)
                redirectParams["difficulty"] = returnDifficulty.Value;
            
            return RedirectToAction(nameof(Index), redirectParams);
        }

        [HttpGet]
        public IActionResult BulkCreate()
        {
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name");
            return View(new BulkCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkCreate(BulkCreateViewModel model, int? returnCategoryId, int? returnDifficulty)
        {
            var categoryId = model.CategoryId;
            var bulkInput = model.BulkInput;

            if (string.IsNullOrWhiteSpace(bulkInput))
            {
                ModelState.AddModelError("", "Input is empty.");
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", categoryId);
                return View(model);
            }

            var lines = bulkInput.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            if (lines.Length % 5 != 0)
            {
                ModelState.AddModelError("", "Each question must have 5 lines: question, 4 answers, correct answer, question image search (or 'none'), answer image search.");
                ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", categoryId);
                return View(model);
            }

            for (int i = 0; i < lines.Length; i += 5)
            {
                string questionText = lines[i].Trim();
                string[] answers = lines[i + 1].Split('|', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim()).ToArray();
                string correctAnswerText = lines[i + 2].Trim();
                string questionSearch = lines[i + 3].Trim();
                string answerSearch = lines[i + 4].Trim();

                if (answers.Length != 4)
                    continue;

                // Handle question image - use default if "none" or empty
                string questionImageUrl = "/images/defaults/question-placeholder.png";
                if (!string.IsNullOrWhiteSpace(questionSearch) && 
                    !questionSearch.Equals("none", StringComparison.OrdinalIgnoreCase) &&
                    !questionSearch.Equals("null", StringComparison.OrdinalIgnoreCase) &&
                    !questionSearch.Equals("-", StringComparison.OrdinalIgnoreCase))
                {
                    questionImageUrl = await _imageService.SearchAndDownloadImageAsync(questionSearch);
                }

                var question = new Question
                {
                    Text = questionText,
                    Difficulty = model.Difficulty,
                    CategoryId = categoryId,
                    ImageUrl = questionImageUrl,
                    Points = model.Difficulty == 1 ? 250 : model.Difficulty == 2 ? 500 : model.Difficulty == 3 ? 750 : 0
                };

                _context.Questions.Add(question);
                await _context.SaveChangesAsync();

                foreach (var answerText in answers)
                {
                    bool isCorrect = answerText.Equals(correctAnswerText, StringComparison.OrdinalIgnoreCase);

                    string answerImageUrl = "/images/defaults/answer-placeholder.png";
                    if (isCorrect)
                    {
                        // Check if answer search is "none" or similar keywords
                        if (!string.IsNullOrWhiteSpace(answerSearch) && 
                            !answerSearch.Equals("none", StringComparison.OrdinalIgnoreCase) &&
                            !answerSearch.Equals("null", StringComparison.OrdinalIgnoreCase) &&
                            !answerSearch.Equals("-", StringComparison.OrdinalIgnoreCase))
                        {
                            // Search for answer image if not "none"
                            var searchedImageUrl = await _imageService.SearchAndDownloadImageAsync(answerSearch, isAnswer: true);
                            
                            // If the search returned the question default, use answer default instead
                            if (searchedImageUrl == "/images/defaults/question-placeholder.png")
                            {
                                answerImageUrl = "/images/defaults/answer-placeholder.png";
                            }
                            else
                            {
                                answerImageUrl = searchedImageUrl;
                            }
                        }
                        // If "none" or empty, keep the default answer image
                    }

                    var answer = new Answer
                    {
                        Text = answerText,
                        IsCorrect = isCorrect,
                        QuestionId = question.Id,
                        ImageUrl = answerImageUrl
                    };

                    _context.Answers.Add(answer);
                }

                await _context.SaveChangesAsync();
            }

            // Redirect back to Index with preserved filters
            var redirectParams = new Dictionary<string, object>();
            if (returnCategoryId.HasValue)
                redirectParams["categoryId"] = returnCategoryId.Value;
            if (returnDifficulty.HasValue)
                redirectParams["difficulty"] = returnDifficulty.Value;
            
            return RedirectToAction(nameof(Index), redirectParams);
        }

        public async Task<IActionResult> Edit(int? id, int? categoryId, int? difficulty)
        {
            if (id == null)
                return NotFound();

            var question = await _context.Questions
                .Include(q => q.Answers)
                .FirstOrDefaultAsync(q => q.Id == id);
            if (question == null)
                return NotFound();

            var model = new EditQuestionModel
            {
                Id = question.Id,
                Text = question.Text,
                Difficulty = question.Difficulty,
                CategoryId = question.CategoryId,
                ImageUrl = question.ImageUrl,
                Answers = question.Answers
                    ?.OrderByDescending(a => a.IsCorrect)
                    .Select(a => new EditAnswerModel
                    {
                        Id = a.Id,
                        Text = a.Text,
                        IsCorrect = a.IsCorrect,
                        ImageUrl = a.ImageUrl
                    }).ToList() ?? new List<EditAnswerModel>()
            };

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", question.CategoryId);
            ViewData["ReturnCategoryId"] = categoryId;
            ViewData["ReturnDifficulty"] = difficulty;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditQuestionModel model, IFormFile ImageFile, int? returnCategoryId, int? returnDifficulty)
        {
            ModelState.Remove("ImageFile");
            Console.WriteLine($"[Edit] Starting edit for question {id}");
            Console.WriteLine($"[Edit] ImageFile is null: {ImageFile == null}");
            if (ImageFile != null)
            {
                Console.WriteLine($"[Edit] ImageFile length: {ImageFile.Length}");
                Console.WriteLine($"[Edit] ImageFile name: {ImageFile.FileName}");
            }

            Console.WriteLine($"[Edit] Model.Id: {model.Id}");
            Console.WriteLine($"[Edit] Model.Text: '{model.Text}'");
            Console.WriteLine($"[Edit] Model.Difficulty: {model.Difficulty}");
            Console.WriteLine($"[Edit] Model.CategoryId: {model.CategoryId}");

            if (id != model.Id)
                return NotFound();

            var existing = await _context.Questions.FindAsync(id);
            if (existing == null)
                return NotFound();

            Console.WriteLine($"[Edit] Existing question found: {existing.Text}");
            Console.WriteLine($"[Edit] Existing image URL: {existing.ImageUrl}");

            if (ModelState.IsValid)
            {
                Console.WriteLine($"[Edit] ModelState is valid, proceeding with update");
                existing.Text = model.Text;
                existing.Difficulty = model.Difficulty;
                existing.CategoryId = model.CategoryId;
                // Assign points based on difficulty
                existing.Points = model.Difficulty == 1 ? 250 : model.Difficulty == 2 ? 500 : model.Difficulty == 3 ? 750 : 1000;

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    Console.WriteLine($"[Edit] Processing new image upload");
                    var apiPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "IQGame", "wwwroot", "images", "questions");
                    Console.WriteLine($"[Edit] Image Save Path: {apiPath}");
                    
                    // Check if directory exists
                    Console.WriteLine($"[Edit] Directory exists: {Directory.Exists(apiPath)}");
                    
                    Directory.CreateDirectory(apiPath);

                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageFile.FileName);
                    var filePath = Path.Combine(apiPath, fileName);
                    Console.WriteLine($"[Edit] Full file path: {filePath}");

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }

                    Console.WriteLine($"[Edit] File saved successfully. File exists: {System.IO.File.Exists(filePath)}");
                    existing.ImageUrl = $"/images/questions/{fileName}";
                    Console.WriteLine($"[Edit] New image URL set: {existing.ImageUrl}");
                }
                else
                {
                    Console.WriteLine($"[Edit] No new image uploaded, keeping existing image");
                }

                // Update only the correct answer's text and image
                if (model.Answers != null && model.Answers.Count > 0)
                {
                    var answerEntities = _context.Answers.Where(a => a.QuestionId == existing.Id).ToList();
                    var correctAnswerModel = model.Answers.FirstOrDefault(a => a.IsCorrect);
                    var correctAnswerEntity = answerEntities.FirstOrDefault(a => a.IsCorrect);
                    var answerImageFiles = Request.Form.Files.Where(f => f.Name == "AnswerImageFiles").ToList();
                    if (correctAnswerModel != null && correctAnswerEntity != null)
                    {
                        correctAnswerEntity.Text = correctAnswerModel.Text;
                        // Handle answer image upload
                        if (answerImageFiles.Count > 0 && answerImageFiles[0] != null && answerImageFiles[0].Length > 0)
                        {
                            var apiPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "IQGame", "wwwroot", "images", "answers");
                            Directory.CreateDirectory(apiPath);
                            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(answerImageFiles[0].FileName);
                            var filePath = Path.Combine(apiPath, fileName);
                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await answerImageFiles[0].CopyToAsync(stream);
                            }
                            correctAnswerEntity.ImageUrl = $"/images/answers/{fileName}";
                        }
                    }
                }

                await _context.SaveChangesAsync();
                Console.WriteLine($"[Edit] Database updated successfully");
                
                // Redirect back to Index with preserved filters
                var redirectParams = new Dictionary<string, object>();
                if (returnCategoryId.HasValue)
                    redirectParams["categoryId"] = returnCategoryId.Value;
                if (returnDifficulty.HasValue)
                    redirectParams["difficulty"] = returnDifficulty.Value;
                
                return RedirectToAction(nameof(Index), redirectParams);
            }
            else
            {
                Console.WriteLine($"[Edit] ModelState is invalid");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    Console.WriteLine($"[Edit] Validation error: {error.ErrorMessage}");
                }
            }

            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Name", model.CategoryId);
            ViewData["ReturnCategoryId"] = returnCategoryId;
            ViewData["ReturnDifficulty"] = returnDifficulty;
            return View(model);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var question = await _context.Questions
                .Include(q => q.Category)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (question == null)
            {
                return NotFound();
            }

            return View(question);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int? returnCategoryId, int? returnDifficulty)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question != null)
            {
                _context.Questions.Remove(question);
            }

            await _context.SaveChangesAsync();
            
            // Redirect back to Index with preserved filters
            var redirectParams = new Dictionary<string, object>();
            if (returnCategoryId.HasValue)
                redirectParams["categoryId"] = returnCategoryId.Value;
            if (returnDifficulty.HasValue)
                redirectParams["difficulty"] = returnDifficulty.Value;
            
            return RedirectToAction(nameof(Index), redirectParams);
        }

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> BulkDelete(int[] selectedIds, int? returnCategoryId, int? returnDifficulty)
        {
            if (selectedIds != null && selectedIds.Length > 0)
            {
                var questionsToDelete = await _context.Questions
                    .Where(q => selectedIds.Contains(q.Id))
                    .ToListAsync();

                _context.Questions.RemoveRange(questionsToDelete);
                await _context.SaveChangesAsync();
            }

            // Redirect back to Index with preserved filters
            var redirectParams = new Dictionary<string, object>();
            if (returnCategoryId.HasValue)
                redirectParams["categoryId"] = returnCategoryId.Value;
            if (returnDifficulty.HasValue)
                redirectParams["difficulty"] = returnDifficulty.Value;
            
            return RedirectToAction(nameof(Index), redirectParams);
        }

        [HttpPost]
        public async Task<IActionResult> FixQuestionPoints()
        {
            try
            {
                // Get all questions with 0 points
                var questionsWithZeroPoints = await _context.Questions
                    .Where(q => q.Points == 0)
                    .ToListAsync();

                int fixedCount = 0;
                foreach (var question in questionsWithZeroPoints)
                {
                    // Assign points based on difficulty
                    question.Points = question.Difficulty == 1 ? 250 : question.Difficulty == 2 ? 500 : question.Difficulty == 3 ? 750 : 0;
                    fixedCount++;
                }

                if (fixedCount > 0)
                {
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Fixed points for {fixedCount} questions.";
                }
                else
                {
                    TempData["InfoMessage"] = "No questions with 0 points found.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error fixing question points: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
