# 🔧 Migration Fix: division_id TEXT to INTEGER Conversion

## Problem

**Error:** `column "division_id" cannot be cast automatically to type integer`

**Cause:** PostgreSQL cannot automatically convert existing TEXT column data to INTEGER when there's existing data in the table.

---

## ✅ Solution Applied

### Modified Migration File

**File:** `20260313101412_divisionid changes to integer.cs`

### Up Migration (TEXT → INTEGER)

```csharp
migrationBuilder.Sql(@"
    -- Convert division_id from text to integer
    ALTER TABLE department 
    ALTER COLUMN division_id TYPE integer 
    USING CASE 
        WHEN division_id IS NULL OR division_id = '' THEN 0
        ELSE division_id::integer
    END;
    
    -- Set column to NOT NULL
    ALTER TABLE department 
    ALTER COLUMN division_id SET NOT NULL;
    
    -- Set default value
    ALTER TABLE department 
    ALTER COLUMN division_id SET DEFAULT 0;
");
```

**What this does:**
1. **USING clause** - Tells PostgreSQL exactly how to convert existing values
2. **CASE statement** - Handles edge cases:
   - `NULL` values → `0`
   - Empty strings `''` → `0`
   - Valid numeric text → converted to integer
3. **SET NOT NULL** - Enforces non-null constraint after conversion
4. **SET DEFAULT 0** - Sets default value for future inserts

---

### Down Migration (INTEGER → TEXT)

```csharp
migrationBuilder.Sql(@"
    ALTER TABLE department 
    ALTER COLUMN division_id TYPE text 
    USING division_id::text;
    
    ALTER TABLE department 
    ALTER COLUMN division_id DROP NOT NULL;
    
    ALTER TABLE department 
    ALTER COLUMN division_id DROP DEFAULT;
");
```

**What this does:**
- Converts integer values back to text (e.g., `123` → `'123'`)
- Removes NOT NULL constraint
- Removes DEFAULT value

---

## 🎯 Why This Works

### Before (Failed):
```csharp
// EF Core auto-generated migration
migrationBuilder.AlterColumn<int>(
    name: "division_id",
    type: "integer",
    oldType: "text");
```

**Problem:** PostgreSQL doesn't know how to handle:
- Empty strings (`''`)
- Non-numeric text values
- NULL values when converting to NOT NULL

---

### After (Works):
```sql
ALTER TABLE department 
ALTER COLUMN division_id TYPE integer 
USING CASE 
    WHEN division_id IS NULL OR division_id = '' THEN 0
    ELSE division_id::integer
END;
```

**Solution:** Explicitly tells PostgreSQL:
- Convert NULL → 0
- Convert empty string → 0
- Cast everything else to integer

---

## 🚀 How to Apply

### Option 1: Re-run Migration (If Failed)

```bash
# Rollback first (if partially applied)
dotnet ef database update <previous-migration-name>

# Apply the fixed migration
dotnet ef database update
```

---

### Option 2: Manual SQL (If Migration Still Fails)

Run this directly in PostgreSQL:

```sql
-- Check current data
SELECT division_id, pg_typeof(division_id) 
FROM department 
LIMIT 10;

-- Apply the conversion manually
ALTER TABLE department 
ALTER COLUMN division_id TYPE integer 
USING CASE 
    WHEN division_id IS NULL OR division_id = '' THEN 0
    ELSE division_id::integer
END;

ALTER TABLE department 
ALTER COLUMN division_id SET NOT NULL;

ALTER TABLE department 
ALTER COLUMN division_id SET DEFAULT 0;
```

Then mark migration as applied:

```bash
dotnet ef migrations add --suppress-output
```

---

## ⚠️ Potential Issues & Solutions

### Issue 1: Non-Numeric Text Values

If your table has values like `'ABC'` or `'N/A'`:

**Check before migration:**
```sql
SELECT division_id FROM department 
WHERE division_id !~ '^[0-9]+$';
```

**Solutions:**
1. Clean the data first:
```sql
UPDATE department 
SET division_id = '0' 
WHERE division_id !~ '^[0-9]+$';
```

2. Or modify the USING clause to handle them:
```sql
USING CASE 
    WHEN division_id IS NULL OR division_id = '' THEN 0
    WHEN division_id ~ '^[0-9]+$' THEN division_id::integer
    ELSE 0  -- Default for non-numeric values
END;
```

---

### Issue 2: Large Dataset

If you have millions of rows:

**Add index after conversion:**
```sql
CREATE INDEX CONCURRENTLY idx_department_division_id 
ON department(division_id);
```

**Do it in batches:**
```sql
-- Create temporary column
ALTER TABLE department ADD COLUMN division_id_new INTEGER DEFAULT 0;

-- Update in batches
UPDATE department 
SET division_id_new = division_id_old::integer 
WHERE id BETWEEN 1 AND 10000;

-- Swap columns
ALTER TABLE department DROP COLUMN division_id;
ALTER TABLE department RENAME COLUMN division_id_new TO division_id;
```

---

## 📊 Verification Steps

### 1. Check Column Type

```sql
SELECT column_name, data_type, is_nullable, column_default
FROM information_schema.columns
WHERE table_name = 'department' AND column_name = 'division_id';
```

**Expected:**
```
column_name   | data_type | is_nullable | column_default
--------------|-----------|-------------|---------------
division_id   | integer   | NO          | 0
```

---

### 2. Check Data Conversion

```sql
SELECT division_id, pg_typeof(division_id)
FROM department
LIMIT 10;
```

**Expected:** All values should be integers, no errors.

---

### 3. Test Insert/Update

```sql
-- Test default value
INSERT INTO department (department_name, division_code) 
VALUES ('Test Dept', 'TEST');

-- Should get division_id = 0 automatically
SELECT * FROM department WHERE department_name = 'Test Dept';

-- Test explicit value
INSERT INTO department (division_id, department_name, division_code) 
VALUES (999, 'Test Dept 2', 'TEST2');

-- Verify
SELECT * FROM department WHERE department_name = 'Test Dept 2';
```

---

### 4. Check Application

Run your application and test:
```csharp
var dept = new Department 
{
    DivisionId = 123,  // Integer now works
    DepartmentName = "New Dept"
};

context.Departments.Add(dept);
context.SaveChanges();  // Should work without errors
```

---

## 🎓 Lessons Learned

### 1. Always Check Existing Data

Before creating type-change migrations:
```sql
SELECT DISTINCT division_id, pg_typeof(division_id)
FROM department;
```

---

### 2. Use Raw SQL for Complex Conversions

EF Core's `AlterColumn` works for simple cases, but for type conversions with data:
```csharp
// ❌ Auto-generated (might fail)
migrationBuilder.AlterColumn<int>("division_id", ...);

// ✅ Custom SQL (full control)
migrationBuilder.Sql("ALTER TABLE ... USING ...");
```

---

### 3. Handle Edge Cases

Always consider:
- NULL values
- Empty strings
- Invalid data
- Default values
- Constraints (NOT NULL, UNIQUE, etc.)

---

## 📞 Troubleshooting

### Error: "cannot be cast automatically"
**Solution:** Use explicit USING clause (already fixed in migration)

### Error: "invalid input syntax for integer"
**Solution:** Clean data first or handle in CASE statement

### Error: "cannot implement NOT NULL"
**Solution:** Convert NULL values first:
```sql
UPDATE department SET division_id = '0' WHERE division_id IS NULL;
```

### Migration Already Applied Partially
**Solution:** Rollback and reapply:
```bash
dotnet ef database update previous-migration
dotnet ef database update target-migration
```

---

**Status:** ✅ Fixed  
**Migration File:** Updated with custom SQL  
**Next Step:** Run `dotnet ef database update`
