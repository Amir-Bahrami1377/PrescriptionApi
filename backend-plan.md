# پلن اجرایی بک‌اند — سامانه ثبت آزمایش آنلاین

**نام پروژه: Prescription**

این فایل برای Claude Code در VS Code است. هدف: راه‌اندازی کامل بک‌اند با .NET 10 و Vertical Slice Architecture.

## نام‌گذاری (Naming Convention)
- نام Solution: `Prescription.sln`
- Namespace ریشه: `Prescription`
- نام پروژه اصلی API: `Prescription.Api`
- نام ماژول‌ها: `Prescription.Modules.Identity`, `Prescription.Modules.Catalog`, `Prescription.Modules.Orders`, `Prescription.Modules.Consultation`, `Prescription.Modules.Ticketing`, `Prescription.Modules.Notifications`, `Prescription.Modules.Payments`, `Prescription.Modules.FileStorage`
- نام SharedKernel: `Prescription.SharedKernel`
- نام پروژه‌های تست: `Prescription.Modules.Identity.Tests`, `Prescription.Modules.Orders.Tests`, `Prescription.Modules.Payments.Tests`, `Prescription.IntegrationTests`
- نام دیتابیس Postgres: `prescription_db`
- نام Bucket های MinIO: `prescription-test-results`, `prescription-consultation-images`
- نام Container ها در Docker Compose: `prescription-api`, `prescription-postgres`, `prescription-redis`, `prescription-minio`

## استک فنی
- .NET 10, ASP.NET Core Minimal API / Web API
- Vertical Slice Architecture (هر Feature = پوشه مستقل شامل Command/Query + Handler + Endpoint)
- MediatR برای CQRS-lite و دیکوپل کردن Handlerها
- Entity Framework Core + PostgreSQL (دیتابیس اصلی)
- MinIO (S3-compatible) برای ذخیره تصاویر (نتیجه آزمایش، عکس مشاوره) — جدا از Postgres، فقط متادیتا در DB
- Redis برای Cache، OTP Session، Rate Limiting
- FluentValidation برای اعتبارسنجی ورودی‌ها
- Serilog برای Logging
- Hangfire یا BackgroundService برای صف ارسال پیامک ناهمزمان
- Polly برای Retry Policy در فراخوانی سرویس‌های خارجی (ملی‌پیامک، زرین‌پال)
- xUnit + FluentAssertions برای تست

## سرویس‌های خارجی
- **پیامک/OTP**: ملی‌پیامک — پشت اینترفیس `ISmsProvider` / `IOtpProvider`
- **پرداخت**: زرین‌پال — پشت اینترفیس `IPaymentGateway`

## ساختار پیشنهادی Solution
```
src/
  Api/                          -> پروژه اصلی Web API، Program.cs، Middleware، DI Registration
  Modules/
    Identity/
      Features/
        RequestOtp/
        VerifyOtpAndLogin/
        CompleteProfile/         (کد ملی، نام، سن، جنسیت)
      Domain/ (User, Role)
      Infrastructure/ (OtpProvider - MeliPayamak)
    Catalog/
      Features/
        ListTests/
        CreateTest/ (Admin)
        UpdateTest/ (Admin)
      Domain/ (LabTest)
    Orders/
      Features/
        CreateOrder/              (انتخاب آزمایش + آپلود فایل + یادداشت)
        DoctorReviewOrder/         (تایید/رد توسط پزشک)
        AttachPrescriptionReference/ (ثبت ارجاع نسخه از سامانه خارجی)
        MoveToPaymentList/
        InitiatePayment/
        PaymentCallback/
        CompleteOrder/            (تکمیل توسط پزشک)
        GetOrderStatus/           (پیگیری سفارش برای مشتری)
        UploadTestResult/         (بارگذاری جواب آزمایش)
      Domain/
        Order.cs                  (State Machine: Draft, PendingDoctorApproval,
                                    AwaitingPayment, Paid, InProgress, Completed, Rejected)
        OrderStateMachine.cs       (State Pattern)
    Consultation/
      Features/
        RequestConsultation/       (آپلود عکس نتیجه توسط مشتری)
        SubmitDoctorOpinion/       (ثبت نظر تخصصی پزشک)
      Domain/ (ConsultationRequest)
    Ticketing/
      Features/
        CreateTicket/
        ReplyTicket/
        CloseTicket/
        ListMyTickets/
      Domain/ (Ticket, TicketMessage)
    Notifications/
      Features/
        SendSms/ (Background Job)
      Infrastructure/
        MeliPayamakSmsProvider.cs (ISmsProvider)
    Payments/
      Infrastructure/
        ZarinPalPaymentGateway.cs (IPaymentGateway)
    FileStorage/
      Infrastructure/
        MinioFileStorageService.cs (IFileStorageService)
  SharedKernel/
    Behaviors/ (LoggingBehavior, ValidationBehavior - MediatR Pipeline)
    Entities/ (BaseEntity, AuditableEntity)
    Abstractions/ (IRepository<T>, IUnitOfWork)
tests/
  Modules.Identity.Tests/
  Modules.Orders.Tests/
  Modules.Payments.Tests/
  IntegrationTests/
```

## نقش‌ها (Roles)
- `Customer`: ثبت آزمایش، پیگیری، بارگذاری جواب، درخواست مشاوره، تیکت
- `Doctor`: بررسی/تایید/رد سفارش، ثبت ارجاع نسخه، تکمیل سفارش، پاسخ به مشاوره
- `Admin`: مدیریت کاتالوگ آزمایش، مدیریت کاربران

## جریان وضعیت سفارش (Order State Machine)
```
Draft
  -> (Customer submits) -> PendingDoctorApproval
PendingDoctorApproval
  -> (Doctor approves) -> AwaitingPayment
  -> (Doctor rejects)  -> Rejected
AwaitingPayment
  -> (Payment verified) -> InProgress
InProgress
  -> (Doctor completes) -> Completed
```
هر انتقال باید توسط State Pattern با Guard Condition مشخص کنترل شود؛ از `RowVersion` (Optimistic Concurrency) در EF Core برای جلوگیری از Race Condition استفاده شود.

## Design Patterns الزامی
1. **Mediator (MediatR)**: هر Feature یک Command/Query + Handler مجزا
2. **Repository + UnitOfWork**: انتزاع دسترسی به داده
3. **Strategy**: `IPaymentGateway` (ZarinPal)، `ISmsProvider` (MeliPayamak) برای امکان تعویض بدون تغییر Core
4. **State**: مدیریت چرخه حیات سفارش
5. **Factory**: ساخت Notification Payload بر اساس نوع رویداد
6. **Decorator/Pipeline Behavior**: Logging و Validation در پایپ‌لاین MediatR

## دیتابیس
- **Postgres** (اصلی): Users, LabTests, Orders, OrderFiles (metadata), Tickets, ConsultationRequests
- **MinIO**: فایل‌های واقعی (تصویر آزمایش، عکس مشاوره) — Bucket جدا برای هرکدام، فقط URL/Key در Postgres ذخیره شود
- **Redis**: OTP session (TTL کوتاه)، Cache کاتالوگ آزمایش، Rate limiting درخواست OTP

## نکات امنیتی
- رمزنگاری کد ملی در سطح ستون دیتابیس (Column-level Encryption)
- Rate Limiting برای درخواست OTP (جلوگیری از سوءاستفاده)
- Idempotency Key در Callback پرداخت زرین‌پال
- Verify اجباری تراکنش سمت سرور (Amount + Authority) پیش از تغییر وضعیت سفارش

## Docker
- `Dockerfile` چند مرحله‌ای (build + runtime) برای Api
- سرویس‌های `docker-compose.yml`: `api`, `postgres`, `redis`, `minio`, `minio-console` (فقط dev)
- Health Check برای همه سرویس‌ها

## ترتیب اجرا برای Claude Code
1. ساخت Solution + پروژه‌های ماژول‌ها طبق ساختار بالا
2. نصب پکیج‌های لازم (MediatR, EFCore.PostgreSQL, FluentValidation, Serilog, Polly, Minio SDK, StackExchange.Redis, Hangfire)
3. پیاده‌سازی SharedKernel (Behaviors, Base Entities, Abstractions)
4. پیاده‌سازی ماژول Identity (OTP با ملی‌پیامک + JWT + تکمیل پروفایل)
5. پیاده‌سازی ماژول Catalog
6. پیاده‌سازی ماژول Orders + State Machine
7. پیاده‌سازی ماژول Payments (ZarinPal)
8. پیاده‌سازی ماژول Consultation
9. پیاده‌سازی ماژول Ticketing + Notifications (صف پیامک)
10. تنظیم Docker Compose و اجرای یکپارچه
11. نوشتن تست‌های Unit/Integration
