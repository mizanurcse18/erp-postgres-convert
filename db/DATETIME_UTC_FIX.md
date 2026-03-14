# 🔧 Complete DateTime UTC Fix for PostgreSQL

## Problem Summary

**Error:** `Cannot write DateTime with Kind=Local to PostgreSQL type 'timestamp with time zone', only UTC is supported`

**Root Cause:** Entities with DateTime properties may have Local or Unspecified Kind values when created/modified.

---

## ✅ Fixes Applied

### Fix 1: BaseDbContext - Global DateTime Configuration

**Location:** `BaseDbContext.cs` - `OnModelCreating()` method

```csharp
// Configure all DateTime properties to use UTC for PostgreSQL compatibility
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
{
    foreach (var property in entityType.GetProperties())
    {
        if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
        {
            property.SetColumnType("timestamp with time zone");
        }
    }
}
```

**What this does:**
- Tells EF Core to map ALL DateTime properties to PostgreSQL's `timestamp with time zone`
- Ensures consistent column types across the database
- **Does NOT automatically convert Local to UTC** - you still need to provide UTC values

---

### Fix 2: ManagerBase - SetAuditFields UTC Conversion

**Location:** `ManagerBase.cs` - Lines 159, 163

```csharp
// BEFORE
SetAuditFields(model, DateTime.Now);  // ❌ Local time

// AFTER
SetAuditFields(model, DateTime.UtcNow);  // ✅ UTC time
```

**What this fixes:**
- All audit fields (`CreatedDate`, `UpdatedDate`) now use UTC
- Prevents errors when saving entities with audit information

---

### Fix 3: AuditEntry - Already Using UTC

**Location:** `AuditEntry.cs` - Lines 37, 44

```csharp
AuditDate = DateTime.UtcNow,      // ✅ Already correct
CreatedDate = DateTime.UtcNow,    // ✅ Already correct
```

No changes needed here - already using UTC!

---

## ⚠️ Remaining Issues

### Problem: Entity Properties May Still Have Local DateTime

When you create a new entity like this:

```csharp
var user = new User
{
    UserName = "john",
    CreatedDate = DateTime.Now  // ❌ This will cause an error!
};
```

Or even worse, when it's not initialized:

```csharp
var user = new User
{
    UserName = "john"
    // CreatedDate defaults to default(DateTime) = DateTime.MinValue with Kind=Unspecified
};
```

**This will fail on SaveChanges()!**

---

## 🎯 Complete Solution

### Option 1: Always Use DateTime.UtcNow in Your Code (RECOMMENDED)

**Rule:** NEVER use `DateTime.Now` anywhere in your codebase.

```csharp
// ❌ WRONG - Will cause PostgreSQL error
var entity = new MyEntity 
{ 
    CreatedDate = DateTime.Now 
};

// ✅ CORRECT - Works with PostgreSQL
var entity = new MyEntity 
{ 
    CreatedDate = DateTime.UtcNow 
};
```

**Search and replace all occurrences:**
- `DateTime.Now` → `DateTime.UtcNow`
- Search in: All `.cs` files in `erpservices/`

---

### Option 2: Add Value Converters (AUTOMATIC CONVERSION)

Add this to `BaseDbContext.cs` to automatically convert Local to UTC:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    
    // Add automatic conversion for all DateTime properties
    foreach (var entityType in modelBuilder.Model.GetEntityTypes())
    {
        foreach (var property in entityType.GetProperties())
        {
            if (property.ClrType == typeof(DateTime))
            {
                var converter = new ValueConverter<DateTime, DateTime>(
                    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                    v => v);
                property.SetValueConverter(converter);
            }
            else if (property.ClrType == typeof(DateTime?))
            {
                var converter = new ValueConverter<DateTime?, DateTime?>(
                    v => v.HasValue && v.Value.Kind == DateTimeKind.Utc ? v : v?.ToUniversalTime(),
                    v => v);
                property.SetValueConverter(converter);
            }
        }
    }
}
```

**What this does:**
- Automatically converts ANY DateTime value to UTC before saving
- Works even if you forget to use `DateTime.UtcNow`
- **Recommended as a safety net**

---

### Option 3: Initialize Entity Properties with Defaults

Update your entity classes to initialize DateTime properties:

```csharp
public class MyEntity : Auditable
{
    public int Id { get; set; }
    
    // Initialize with UTC by default
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedDate { get; set; }
    
    // Or use a backing field with automatic conversion
    private DateTime _createdDate;
    public DateTime CreatedDate 
    { 
        get => _createdDate;
        set => _createdDate = value.Kind == DateTimeKind.Utc ? value : value.ToUniversalTime();
    }
}
```

**Downside:** Requires modifying every entity class.

---

## 📋 Checklist: Find and Fix All DateTime.Now

### Files Already Fixed ✅

1. ✅ `ManagerBase.cs` - Line 87 - `GenerateSystemCode()`
2. ✅ `ManagerBase.cs` - Line 159 - `SetAuditFields()`
3. ✅ `ManagerBase.cs` - Line 163 - `SetAuditFieldsOracle()`
4. ✅ `AuditEntry.cs` - Lines 37, 44 - Already using UTC

### Files to Check ⚠️

Search your entire codebase for these patterns:

```bash
# PowerShell command to find all DateTime.Now usage
Get-ChildItem -Path "erpservices" -Recurse -Filter "*.cs" | 
    Select-String -Pattern "DateTime\.Now" | 
    Select-Object Filename, LineNumber, Line
```

**Replace ALL occurrences with `DateTime.UtcNow`**

---

## 🔍 Common Places Where DateTime.Now Hides

### 1. Entity Constructors

```csharp
public class User : Auditable
{
    public User()
    {
        CreatedDate = DateTime.Now;  // ❌ BAD
    }
}
```

**Fix:**
```csharp
public User()
{
    CreatedDate = DateTime.UtcNow;  // ✅ GOOD
}
```

---

### 2. Default Values in Methods

```csharp
public void ProcessData(DateTime? date = null)
{
    var processDate = date ?? DateTime.Now;  // ❌ BAD
}
```

**Fix:**
```csharp
public void ProcessData(DateTime? date = null)
{
    var processDate = date ?? DateTime.UtcNow;  // ✅ GOOD
}
```

---

### 3. Logging/Timestamping

```csharp
_logger.LogInformation($"Processed at: {DateTime.Now}");  // ❌ Still OK for logs
```

**Note:** For logging ONLY, `DateTime.Now` is acceptable since it's just for display. The issue is only when storing to database.

---

### 4. Calculations and Comparisons

```csharp
if (entity.ExpiryDate > DateTime.Now)  // ❌ Might cause issues
```

**Fix:**
```csharp
if (entity.ExpiryDate > DateTime.UtcNow)  // ✅ Better
```

---

## 🛡️ Best Practices Going Forward

### 1. Code Review Checklist

- [ ] No `DateTime.Now` in data access layer
- [ ] No `DateTime.Now` in business logic layer  
- [ ] All entity DateTime properties initialized to UTC
- [ ] All DTO DateTime properties use UTC

### 2. Static Analysis Rules

Add this to your coding standards:
> **"All DateTime values stored in database MUST be UTC"**

### 3. Helper Method

Create a utility method:

```csharp
public static class DateTimeHelper
{
    /// <summary>
    /// Gets current UTC time. Use this instead of DateTime.Now or DateTime.UtcNow
    /// to ensure consistency and allow easy mocking in tests.
    /// </summary>
    public static DateTime Now => DateTime.UtcNow;
}

// Usage:
entity.CreatedDate = DateTimeHelper.Now;  // Always UTC
```

---

## 🧪 Testing Your Fix

### Test 1: Create and Save Entity

```csharp
var user = new User
{
    UserName = "test_user",
    CreatedDate = DateTime.UtcNow,  // Explicitly UTC
    UpdatedDate = DateTime.UtcNow
};

_context.Users.Add(user);
await _context.SaveChangesAsync();

Console.WriteLine("✅ Success! DateTime.UTC works correctly");
```

---

### Test 2: Try with Local Time (Should Fail or Auto-Convert)

```csharp
var user = new User
{
    UserName = "test_user2",
    CreatedDate = DateTime.Now  // Local time
};

_context.Users.Add(user);
await _context.SaveChangesAsync();  // Will throw without converters
```

**Expected:**
- Without converters: Throws exception ❌
- With converters: Auto-converts to UTC ✅

---

### Test 3: Query Existing Data

```csharp
var users = await _context.Users.ToListAsync();

foreach (var user in users)
{
    Console.WriteLine($"User: {user.UserName}, Created: {user.CreatedDate} (Kind: {user.CreatedDate.Kind})");
    // All should show Kind=Utc
}
```

---

## 📊 Summary of Changes

| Component | Change Made | Status |
|-----------|-------------|--------|
| **BaseDbContext** | Added global DateTime UTC configuration | ✅ Done |
| **ManagerBase** | Changed `SetAuditFields` to use UTC | ✅ Done |
| **AuditEntry** | Already using UTC | ✅ Verified |
| **Entity Classes** | Need manual review | ⚠️ Check yours |
| **Service Code** | Replace all `DateTime.Now` | ⚠️ Your task |

---

## 🎯 Next Steps

1. **Run the search** to find all `DateTime.Now` in your codebase
2. **Replace** them with `DateTime.UtcNow`
3. **Test** your application thoroughly
4. **Optional:** Add value converters as a safety net
5. **Document** this in your coding standards

---

## 📞 Quick Reference

### DO ✅
```csharp
entity.CreatedDate = DateTime.UtcNow;
var now = DateTime.UtcNow;
audit.AuditDate = DateTime.UtcNow;
```

### DON'T ❌
```csharp
entity.CreatedDate = DateTime.Now;
var now = DateTime.Now;
audit.AuditDate = DateTime.Now;
```

---

**Created:** 2026-03-06  
**Status:** Partial fix applied - Manual code review required  
**Priority:** HIGH - Replace all remaining DateTime.Now calls
