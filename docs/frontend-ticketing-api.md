# قرارداد API تیکت پشتیبانی برای فرانت‌اند

تمام endpointها زیر پیشوند `/api` هستند و به‌جز موارد احراز هویت، هدر زیر را نیاز دارند:

```http
Authorization: Bearer <access-token>
```

بدنه‌ها و پاسخ‌ها JSON با نام‌گذاری `camelCase` هستند. تمام زمان‌ها UTC و در قالب ISO 8601 برمی‌گردند.

## نقش‌ها و گردش وضعیت

نقش پشتیبانی در این API فقط `Admin` است؛ نقش `Doctor` به تیکت‌ها دسترسی ندارد.

وضعیت‌های تیکت:

- `Open`: تیکت باز است و Customer یا Admin می‌توانند پیام بفرستند.
- `PendingClosure`: Admin تیکت را در صف بستن قرار داده است. مقدار `autoCloseAtUtc` زمان پایان فرصت ۶ساعته را مشخص می‌کند.
- `Closed`: تیکت بسته است و تا وقتی Customer آن را دوباره باز نکند، امکان ارسال پیام وجود ندارد.

گردش وضعیت:

```text
Create ──> Open
Open ── Admin queues closure ──> PendingClosure
PendingClosure ── Customer replies ──> Open
PendingClosure ── 6 hours without customer reply ──> Closed
Open/PendingClosure ── Customer closes ──> Closed
Closed ── Customer reopens ──> Open
```

Job سرور هر دقیقه deadlineها را بررسی می‌کند؛ بنابراین UI باید `autoCloseAtUtc` را نمایش دهد و پس از رسیدن زمان، داده را refresh کند.

## TypeScript types پیشنهادی

```ts
export type TicketStatus = "Open" | "PendingClosure" | "Closed";

export interface TicketSummary {
  id: string;
  subject: string;
  status: TicketStatus;
  messageCount: number;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  autoCloseAtUtc: string | null;
  closedAtUtc: string | null;
}

export interface AdminTicketSummary extends TicketSummary {
  customerId: string;
}

export interface TicketMessage {
  senderId: string;
  senderRole: "Customer" | "Admin";
  body: string;
  createdAtUtc: string;
}

export interface TicketDetail {
  id: string;
  customerId: string;
  subject: string;
  status: TicketStatus;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  queuedForClosureAtUtc: string | null;
  autoCloseAtUtc: string | null;
  closedAtUtc: string | null;
  messages: TicketMessage[];
}
```

## Endpointهای Customer

### ایجاد تیکت

`POST /api/tickets`

```json
{
  "subject": "مشکل ورود",
  "message": "نمی‌توانم وارد پنل شوم."
}
```

پاسخ `201 Created`:

```json
{
  "ticketId": "57b4db93-e677-41ab-b3c1-1e42cb21f8c7"
}
```

هدر `Location` نیز مسیر جزئیات تیکت را دارد. پس از ثبت موفق، SMS تأیید ثبت برای شمارهٔ Customer در صف ارسال قرار می‌گیرد.

محدودیت‌ها:

- `subject`: اجباری، حداکثر ۳۰۰ کاراکتر
- `message`: اجباری، حداکثر ۴۰۰۰ کاراکتر

### لیست تیکت‌های کاربر

`GET /api/tickets/mine`

پاسخ `200 OK`: آرایهٔ `TicketSummary`، مرتب‌شده بر اساس آخرین تغییر به‌صورت نزولی.

```json
[
  {
    "id": "57b4db93-e677-41ab-b3c1-1e42cb21f8c7",
    "subject": "مشکل ورود",
    "status": "Open",
    "messageCount": 2,
    "createdAtUtc": "2026-08-14T12:00:00+00:00",
    "updatedAtUtc": "2026-08-14T12:10:00+00:00",
    "autoCloseAtUtc": null,
    "closedAtUtc": null
  }
]
```

### جزئیات و پیام‌های تیکت

`GET /api/tickets/{ticketId}`

برای Customer فقط تیکت متعلق به خودش قابل مشاهده است. Admin می‌تواند هر تیکت را از همین endpoint ببیند.

پاسخ `200 OK`: یک `TicketDetail`. پیام‌ها از قدیمی به جدید مرتب شده‌اند.

```json
{
  "id": "57b4db93-e677-41ab-b3c1-1e42cb21f8c7",
  "customerId": "9f909680-2f3d-4210-a4e2-20fdcf89e7ff",
  "subject": "مشکل ورود",
  "status": "PendingClosure",
  "createdAtUtc": "2026-08-14T12:00:00+00:00",
  "updatedAtUtc": "2026-08-14T12:15:00+00:00",
  "queuedForClosureAtUtc": "2026-08-14T12:15:00+00:00",
  "autoCloseAtUtc": "2026-08-14T18:15:00+00:00",
  "closedAtUtc": null,
  "messages": [
    {
      "senderId": "9f909680-2f3d-4210-a4e2-20fdcf89e7ff",
      "senderRole": "Customer",
      "body": "نمی‌توانم وارد پنل شوم.",
      "createdAtUtc": "2026-08-14T12:00:00+00:00"
    }
  ]
}
```

### ارسال پاسخ

`POST /api/tickets/{ticketId}/reply`

قابل استفاده توسط Customer مالک تیکت و Admin:

```json
{
  "message": "مشکل هنوز برطرف نشده است."
}
```

پاسخ موفق: `204 No Content`

- پاسخ Customer به تیکت `PendingClosure`، countdown را لغو و وضعیت را `Open` می‌کند.
- پاسخ Admin به تیکت `PendingClosure`، deadline فعلی را تغییر نمی‌دهد.
- پاسخ Admin باعث صف‌شدن SMS اطلاع‌رسانی برای Customer می‌شود.
- روی تیکت `Closed` پاسخ با `409 Conflict` رد می‌شود؛ Customer باید ابتدا تیکت را reopen کند.

محدودیت `message`: اجباری، حداکثر ۴۰۰۰ کاراکتر.

### بستن تیکت توسط Customer

`POST /api/tickets/{ticketId}/close`

بدون body. پاسخ موفق: `204 No Content`

Customer می‌تواند تیکت `Open` یا `PendingClosure` متعلق به خودش را فوراً ببندد.

### بازکردن مجدد توسط Customer

`POST /api/tickets/{ticketId}/reopen`

بدون body. پاسخ موفق: `204 No Content`

فقط تیکت `Closed` متعلق به Customer باز می‌شود. اجرای آن روی وضعیت دیگر `409 Conflict` می‌دهد.

## Endpointهای Admin

### لیست همهٔ تیکت‌ها

`GET /api/admin/tickets`

فیلتر اختیاری:

```http
GET /api/admin/tickets?status=PendingClosure
```

مقادیر معتبر `status`: `Open`، `PendingClosure` و `Closed`.

پاسخ `200 OK`: آرایهٔ `AdminTicketSummary` مرتب‌شده بر اساس آخرین تغییر به‌صورت نزولی. تفاوت آن با لیست Customer وجود `customerId` است.

### مشاهده و پاسخ

- جزئیات: `GET /api/tickets/{ticketId}`
- پاسخ: `POST /api/tickets/{ticketId}/reply`

قرارداد این دو endpoint در بخش Customer آمده است. Admin محدود به مالکیت تیکت نیست.

### قراردادن در صف بستن

`POST /api/admin/tickets/{ticketId}/queue-closure`

بدون body. پاسخ موفق: `204 No Content`

نتیجه:

- وضعیت به `PendingClosure` تغییر می‌کند.
- `queuedForClosureAtUtc` زمان فعلی سرور است.
- `autoCloseAtUtc` دقیقاً ۶ ساعت بعد تنظیم می‌شود.
- اگر Customer تا deadline پاسخ دهد، تیکت دوباره `Open` می‌شود.
- اگر پاسخ ندهد، Job سرور آن را خودکار `Closed` می‌کند.
- تکرار عملیات روی تیکت PendingClosure یا اجرای آن روی تیکت Closed، پاسخ `409 Conflict` دارد.

## خطاها

خطاها با ساختار استاندارد `ProblemDetails` برمی‌گردند:

```json
{
  "title": "تعارض وضعیت",
  "status": 409,
  "detail": "متن قابل نمایش خطا",
  "instance": "/api/tickets/..."
}
```

- `400`: body یا query نامعتبر
- `401`: token وجود ندارد یا نامعتبر است
- `403`: نقش مجاز نیست یا Customer مالک تیکت نیست
- `404`: تیکت پیدا نشد
- `409`: عملیات با وضعیت فعلی تیکت سازگار نیست

## پیشنهاد رفتار UI

- در صفحهٔ لیست، badge وضعیت را مستقیماً از `status` بسازید.
- در `PendingClosure` یک هشدار و زمان `autoCloseAtUtc` نمایش دهید.
- دکمهٔ Reply در `Closed` غیرفعال باشد؛ برای Customer دکمهٔ Reopen نمایش داده شود.
- دکمهٔ Close و Reopen فقط در پنل Customer نمایش داده شود.
- دکمهٔ Queue closure فقط در پنل Admin و فقط برای وضعیت `Open` نمایش داده شود.
- بعد از هر mutation، جزئیات و لیست مربوطه refetch شود؛ وضعیت بسته‌شدن خودکار صرفاً با countdown محلی قطعی فرض نشود.
