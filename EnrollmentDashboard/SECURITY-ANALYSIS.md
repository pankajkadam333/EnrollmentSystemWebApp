# Security Analysis — Code Review Exercise

This document identifies the security vulnerabilities found in the Code Review Exercise code provided in the take-home exercise README.

---

## 1. SQL Injection — String Concatenation of User Input

- **OWASP Category:** A03:2021 Injection
- **Risk Level:** Critical
- **Vulnerable Code:**
  ```csharp
  sql += "AND (p.FirstName LIKE '%" + participantName + "%' " +
         "OR p.LastName LIKE '%" + participantName + "%') ";
  sql += "AND e.EnrollmentDate >= '" + startDate + "' ";
  sql += "AND e.EnrollmentDate <= '" + endDate + "' ";
  ```
- **How to Exploit:** Enter `'; DROP TABLE Enrollments--` in the search field. The concatenated string becomes valid SQL that terminates the original query and executes a destructive command.
- **How to Fix:** Use parameterized queries with `SqlParameter` or use an ORM's LINQ methods. Never concatenate user input into SQL strings.
  ```csharp
  command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = participantName });
  ```

---

## 2. Cross-Site Scripting (XSS) — `Html.Raw()` on User Content

- **OWASP Category:** A03:2021 Injection (XSS)
- **Risk Level:** Critical
- **Vulnerable Code:**
  ```razor
  <td>@Html.Raw(enrollment.Notes)</td>
  ```
- **How to Exploit:** Sample data contains `<img src=x onerror="alert('XSS')">` in the Notes field. `Html.Raw()` renders this as live HTML, executing JavaScript in the victim's browser. This can be used to steal session cookies, redirect users, or deface the page.
- **How to Fix:** Use Razor's default encoding by writing `@enrollment.Notes` instead of `@Html.Raw(enrollment.Notes)`. Razor auto-encodes all `@` output, rendering script tags as harmless text.

---

## 3. Insecure Direct Object Reference (IDOR) — No Authorization on Details

- **OWASP Category:** A01:2021 Broken Access Control
- **Risk Level:** High
- **Vulnerable Code:**
  ```csharp
  public ActionResult Details(int id)
  {
      var enrollment = db.Enrollments
          .Include(e => e.Participant)
          .Include(e => e.Program)
          .FirstOrDefault(e => e.EnrollmentID == id);
      return View(enrollment);
  }
  ```
- **How to Exploit:** Any user can change the `id` parameter in the URL (e.g., `/Details?id=1`, `/Details?id=2`, etc.) to view enrollment records belonging to other users. There is no check that the current user is authorized to see the requested record.
- **How to Fix:** Verify the authenticated user has permission to access the requested enrollment before returning data. Return `Unauthorized()` or `Forbid()` if access is denied.

---

## 4. Missing Null Check — Information Disclosure via Stack Trace

- **OWASP Category:** A05:2021 Security Misconfiguration
- **Risk Level:** Medium
- **Vulnerable Code:**
  ```csharp
  var enrollment = db.Enrollments...FirstOrDefault(e => e.EnrollmentID == id);
  return View(enrollment); // enrollment could be null
  ```
- **How to Exploit:** Request a non-existent ID such as `/Details?id=9999`. `FirstOrDefault` returns null, which causes a `NullReferenceException` when the view tries to render. In development mode, the full stack trace (including internal paths, class names, and framework versions) is exposed to the attacker.
- **How to Fix:** Check for null and return a proper HTTP 404 response:
  ```csharp
  if (enrollment is null)
      return NotFound();
  ```

---

## 5. No Input Validation on Date Parameters

- **OWASP Category:** A03:2021 Injection
- **Risk Level:** High
- **Vulnerable Code:**
  ```csharp
  public ActionResult SearchEnrollments(string participantName, string startDate, string endDate)
  ```
- **How to Exploit:** Dates are accepted as raw strings with no validation. Beyond enabling SQL injection (covered above), an attacker can pass malformed values, reversed ranges (end < start), or extreme dates to cause unexpected behavior or denial of service.
- **How to Fix:** Accept `DateTime?` typed parameters so model binding rejects non-date input. Validate that start ≤ end, and swap them if reversed. Pass dates as typed `SqlParameter` values.

---

## 6. Cross-Site Request Forgery (CSRF) — Delete via GET Request

- **OWASP Category:** A01:2021 Broken Access Control
- **Risk Level:** High
- **Vulnerable Code:**
  ```razor
  <a href="/Enrollments/Delete?id=@enrollment.EnrollmentID">Delete</a>
  ```
- **How to Exploit:** The delete action is triggered by a simple GET link. A malicious site can embed `<img src="https://yoursite.com/Enrollments/Delete?id=5">` which silently deletes the record when the victim's browser loads the image. No user interaction required.
- **How to Fix:** Destructive actions must use HTTP POST with an anti-forgery token:
  ```razor
  <form method="post" asp-action="Delete" asp-route-id="@enrollment.EnrollmentID">
      @Html.AntiForgeryToken()
      <button type="submit" class="btn btn-danger btn-sm">Delete</button>
  </form>
  ```
  Decorate the controller action with `[HttpPost, ValidateAntiForgeryToken]`.

---

## 7. No Status Whitelist Validation

- **OWASP Category:** A04:2021 Insecure Design
- **Risk Level:** Medium
- **How to Exploit:** The code does not validate that filter values (like status) match an expected set. Combined with the SQL injection vulnerability, this widens the attack surface. Even without injection, unexpected values could cause logic errors or expose unintended data.
- **How to Fix:** Validate status against an explicit allowlist before using it:
  ```csharp
  string[] allowedStatuses = ["Active", "Completed", "Withdrawn"];
  if (!allowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
      status = null;
  ```

---

## Summary

| # | Vulnerability | OWASP Category | Risk |
|---|--------------|----------------|------|
| 1 | SQL Injection via string concatenation | A03:2021 Injection | Critical |
| 2 | XSS via `Html.Raw()` | A03:2021 Injection | Critical |
| 3 | IDOR — no authorization check | A01:2021 Broken Access Control | High |
| 4 | Null reference — stack trace disclosure | A05:2021 Security Misconfiguration | Medium |
| 5 | No date input validation | A03:2021 Injection | High |
| 6 | CSRF — delete via GET request | A01:2021 Broken Access Control | High |
| 7 | No status whitelist validation | A04:2021 Insecure Design | Medium |

---

## How Our Implementation Addresses These

- **SQL Injection:** All queries use parameterized `SqlParameter` or stored procedures. Zero string concatenation.
- **XSS:** All Razor output uses `@` encoding. No `Html.Raw()` anywhere.
- **IDOR:** `Details` action returns `NotFound()` for invalid or non-existent IDs.
- **Null Handling:** Null checks in both the service and controller layers.
- **Input Validation:** Dates are typed `DateTime?`, status is validated against an allowlist, page numbers are clamped.
- **Architecture:** Business logic lives in the service layer, not controllers. Repository pattern with DI for testability.
