using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Interfaces;
using Shop.Application.ViewModels;
using Shop.Presentation.Services;

namespace Shop.Presentation;

/// <summary>صفحات ایستا: درباره ما، تماس با ما، سوالات متداول، قوانین.</summary>
public class PagesController(
    ISettingsService settings,
    ISiteMessageService messages,
    INewsLetterService newsLetter) : Controller
{
    [HttpGet("/about")]
    public async Task<IActionResult> About(CancellationToken ct)
    {
        var config = await settings.GetAsync(ct);
        return View(config);
    }

    [HttpGet("/contact")]
    public async Task<IActionResult> Contact(CancellationToken ct)
    {
        var config = await settings.GetAsync(ct);
        return View(new ContactPageViewModel { Settings = config });
    }

    [HttpPost("/contact")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Contact(ContactPageViewModel page, CancellationToken ct)
    {
        page.Settings = await settings.GetAsync(ct);

        if (!ModelState.IsValid)
            return View(page);

        var m = page.Message;
        await messages.AddAsync(m.FullName, m.Email, m.Subject, m.Body, ct);

        TempData["Success"] = "پیام شما با موفقیت ارسال شد. به‌زودی پاسخ می‌دهیم.";
        return RedirectToAction(nameof(Contact));
    }

    [HttpGet("/faq")]
    public IActionResult Faq() => View();

    [HttpGet("/terms")]
    public IActionResult Terms() => View();

    [HttpPost("/api/newsletter")]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Subscribe([FromBody] NewsLetterRequest request, CancellationToken ct)
    {
        var (success, message) = await newsLetter.SubscribeAsync(request.Email, ct);
        return Json(new { success, message });
    }
}

public class NewsLetterRequest
{
    public string Email { get; set; } = default!;
}

public class ContactPageViewModel
{
    public Domain.SiteSettings Settings { get; set; } = default!;
    public ContactMessageViewModel Message { get; set; } = new();
}
