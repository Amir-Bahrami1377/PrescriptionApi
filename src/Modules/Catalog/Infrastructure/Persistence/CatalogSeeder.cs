using Microsoft.EntityFrameworkCore;
using Prescription.Modules.Catalog.Domain;

namespace Prescription.Modules.Catalog.Infrastructure.Persistence;

/// <summary>Seeds the catalogue on startup in every environment, invoked from Program.cs alongside the
/// migration step. Matching on name means a test an admin has since deleted is never resurrected, and
/// one they have renamed or edited is left alone.</summary>
public static class CatalogSeeder
{
    private static readonly (string Name, string Description)[] LabTests =
    [
        ("چکاپ کامل", "بررسی کلی سلامت بدن"),
        ("آزمایش دوره ای دیابت", "تشخیص و کنترل دیابت"),
        ("آزمایش چربی خون", "کلسترول و تری‌گلیسیرید"),
        ("آزمایش خون کامل (CBC)", "شمارش سلول‌های خون"),
        ("آزمایش کم‌خونی و آهن", "بررسی ذخیره آهن بدن"),
        ("آزمایش تیروئید", "کم‌کاری یا پرکاری تیروئید"),
        ("آزمایش عملکرد کبد", "سلامت کبد و کبد چرب"),
        ("آزمایش عملکرد کلیه", "بررسی سلامت کلیه‌ها"),
        ("آزمایش ویتامین D", "شایع‌ترین کمبود ویتامین در ایران"),
        ("آزمایش ویتامین B12", "خستگی و گزگز دست و پا"),
        ("آزمایش اسید اوریک و نقرس", "درد مفاصل و نقرس"),
        ("آزمایش ادرار", "عفونت ادراری و سلامت کلیه"),
        ("کشت ادرار", "شناسایی میکروب عفونت ادراری"),
        ("آزمایش مدفوع", "انگل، کرم و مشکلات گوارشی"),
        ("آزمایش بارداری", "تشخیص حاملگی"),
        ("آزمایش هورمون های زنانه و یائسگی", "اختلال قاعدگی و ناباروری"),
        ("آزمایش پروستات (PSA)", "غربالگری سرطان پروستات"),
        ("آزمایش‌های التهاب", "درد مفاصل و بیماری‌های خودایمنی"),
        ("آزمایش‌های روماتیسم مفصلی", "درد مفاصل و بیماری‌های خودایمنی"),
        ("آزمایش هپاتیت", "هپاتیت B و C"),
        ("آزمایش تب مالت", "رایت و کومبس رایت"),
        ("آزمایش هلیکوباکتر پیلوری", "میکروب معده و زخم گوارشی"),
    ];

    public static async Task SeedLabTestsAsync(CatalogDbContext dbContext, CancellationToken cancellationToken = default)
    {
        // Tracked, and matched one at a time rather than keyed into a dictionary, because nothing
        // stops an admin from creating a second test with a name already in this list.
        var existing = await dbContext.LabTests.ToListAsync(cancellationToken);

        for (var i = 0; i < LabTests.Length; i++)
        {
            var (name, description) = LabTests[i];
            var displayOrder = i + 1;

            var test = existing.FirstOrDefault(t => t.Name == name);
            if (test is not null)
            {
                // Reapplied every startup so the order above stays authoritative: rearranging the
                // array re-orders an existing catalogue too, not just a fresh database.
                test.SetDisplayOrder(displayOrder);
                continue;
            }

            dbContext.LabTests.Add(LabTest.Create(name, description, displayOrder));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
