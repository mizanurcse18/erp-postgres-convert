# 🔧 System Code Generation Setup Guide

## Problem Solved

**Error:** `Npgsql.PostgresException: '42P01: relation "system_code_pattern" does not exist'`

**Cause:** The `make_system_code()` PostgreSQL function requires two tables that weren't created yet.

---

## ✅ Solution: Create Required Tables

### Step 1: Run the Table Creation Script

Execute the following SQL script in your PostgreSQL database:

```bash
psql -U postgres -d your_database_name -f "d:\SourceCode\Opseek\source\erp-postgress\db\system_code_tables.sql"
```

Or from within pgAdmin/psql:
```sql
\i d:/SourceCode/Opseek/source/erp-postgress/db/system_code_tables.sql
```

This creates two tables:

1. **`system_code_pattern`** - Stores code generation templates
2. **`system_code_max`** - Tracks current sequence numbers

---

## 📋 Table Structures

### Table 1: system_code_pattern

Stores pattern templates for generating automatic codes (IDs) for different business entities.

| Column | Type | Description |
|--------|------|-------------|
| `system_code_pattern_id` | SERIAL | Primary key |
| `company_id` | VARCHAR(50) | Company identifier |
| `table_name` | VARCHAR(100) | Target table (e.g., 'Invoice', 'PurchaseOrder') |
| `id_pattern` | VARCHAR(250) | Pattern for ID field |
| `code_pattern` | VARCHAR(250) | Pattern for Code field |
| `period` | VARCHAR(10) | Reset frequency: 'Auto', 'Yearly', 'Monthly', 'Daily' |
| `start_number` | SMALLINT | Starting number (default: 1) |
| `increment` | SMALLINT | Increment value (default: 1) |
| `padding` | SMALLINT | Digit padding (0 = none) |
| `padding_char` | CHAR(1) | Padding character (default: '0') |
| `year_length` | SMALLINT | Year format: 2='24', 4='2024' |
| `master_table` | VARCHAR(100) | Optional master table for global sequencing |
| `is_active` | BOOLEAN | Active status |
| `created_by`, `updated_by` | INTEGER | Audit fields |
| `created_at`, `updated_at` | TIMESTAMP | Timestamps |

**Placeholders in patterns:**
- `{Year}` - Current year
- `{Month}` - Current month
- `{Day}` - Current day
- `{Number}` - Sequence number
- `{Prefix}` - User-provided prefix
- `{Suffix}` - User-provided suffix

---

### Table 2: system_code_max

Tracks the current maximum sequence number for each pattern.

| Column | Type | Description |
|--------|------|-------------|
| `system_code_max_id` | SERIAL | Primary key |
| `company_id` | VARCHAR(50) | Company identifier |
| `table_name` | VARCHAR(100) | Target table |
| `period` | VARCHAR(20) | Period identifier (e.g., '2024', '2024-01', 'Auto') |
| `max_number` | INTEGER | Current maximum number used |
| `created_at`, `updated_at` | TIMESTAMP | Timestamps |

---

## 🎯 Example Configurations

### Example 1: Invoice Numbering (Yearly Reset)

```sql
INSERT INTO system_code_pattern (company_id, table_name, id_pattern, code_pattern, period, start_number, increment, padding, padding_char, year_length)
VALUES ('1', 'Invoice', 'INV-{Year}-{Number}', 'INV-{Year}-{Number}', 'Yearly', 1, 1, 6, '0', 4);
```

**Result:** `INV-2024-000001`, `INV-2024-000002`, ... resets every year

---

### Example 2: Purchase Order (Monthly Reset)

```sql
INSERT INTO system_code_pattern (company_id, table_name, id_pattern, code_pattern, period, start_number, increment, padding, padding_char, year_length)
VALUES ('1', 'PurchaseOrder', 'PO-{Year}{Month}-{Number}', 'PO-{Year}{Month}-{Number}', 'Monthly', 1, 1, 4, '0', 2);
```

**Result:** `PO-2403-0001`, `PO-2403-0002`, ... resets every month

---

### Example 3: Employee ID (No Reset, Global Sequence)

```sql
INSERT INTO system_code_pattern (company_id, table_name, id_pattern, code_pattern, period, start_number, increment, padding, padding_char, year_length)
VALUES ('1', 'Employee', 'EMP-{Number}', 'EMP-{Number}', 'Auto', 1000, 1, 5, '0', 2);
```

**Result:** `EMP-01000`, `EMP-01001`, ... never resets, starts at 1000

---

### Example 4: Attendance (Daily Reset)

```sql
INSERT INTO system_code_pattern (company_id, table_name, id_pattern, code_pattern, period, start_number, increment, padding, padding_char, year_length)
VALUES ('1', 'Attendance', 'ATT-{Year}{Month}{Day}-{Number}', 'ATT-{Date}-{Number}', 'Daily', 1, 1, 3, '0', 2);
```

**Result:** `ATT-20240306-001`, `ATT-20240306-002`, ... resets every day

---

## 🚀 How to Use in C# Code

The function is already called correctly in `ManagerBase.cs`:

```csharp
public UniqueCode GenerateSystemCode(string tableName, string companyId, 
    short addNumber = 1, string prefix = "", string suffix = "")
{
    var context = (DbUtility)AppContexts.GetInstance(typeof(DbUtility));
    
    var result = context.ExecuteScalar(
        "SELECT make_system_code(@0::varchar, @1::varchar, @2::varchar, @3::smallint, @4::timestamp, @5::varchar, @6::varchar)", 
        tableName, companyId,
        DateTime.Today.ToString("yyyy-MM-dd"), 
        addNumber, DateTime.UtcNow, 
        prefix, suffix);
    
    // Parse result: "max_number%system_id%system_code%master_max_number"
    var dataArray = result.ToString().Split('%');
    // ... return UniqueCode object
}
```

---

## 🔍 Testing the Setup

### Test 1: Verify Tables Exist

```sql
-- Check if tables exist
SELECT tablename FROM pg_tables 
WHERE tablename IN ('system_code_pattern', 'system_code_max');
```

**Expected:** Both tables should appear in results.

---

### Test 2: Insert Sample Pattern

```sql
-- Insert a test pattern
INSERT INTO system_code_pattern (company_id, table_name, id_pattern, code_pattern, period, start_number, padding, padding_char, year_length)
VALUES ('TEST', 'TestTable', 'TEST-{Year}-{Number}', 'TEST-{Year}-{Number}', 'Yearly', 1, 5, '0', 4)
ON CONFLICT (company_id, table_name) DO NOTHING;
```

---

### Test 3: Call the Function

```sql
-- Test the function
SELECT make_system_code(
    'TestTable',                    -- p_table_name
    'TEST',                         -- p_company_id
    '2024-03-06',                   -- p_date
    1,                              -- p_add_number
    NOW(),                          -- p_client_date
    '',                             -- p_prefix
    ''                              -- p_suffix
) as result;
```

**Expected Result:** `1%TEST-2024-00001%TEST-2024-00001%1`

---

### Test 4: Verify Counter Incremented

```sql
-- Check the counter
SELECT * FROM system_code_max 
WHERE company_id = 'TEST' AND table_name = 'TestTable';
```

**Expected:** One row with `max_number= 1`

---

### Test 5: Call Again to See Increment

```sql
SELECT make_system_code('TestTable', 'TEST', '2024-03-06', 1, NOW(), '', '');
```

**Expected Result:** `2%TEST-2024-00002%TEST-2024-00002%2`

---

## ⚠️ Troubleshooting

### Error: "relation does not exist"
**Solution:** Run the `system_code_tables.sql` script to create the tables.

---

### Error: "duplicate key value violates unique constraint"
**Cause:** Trying to insert a pattern that already exists.

**Solution:** Use `ON CONFLICT DO NOTHING` or update existing:
```sql
INSERT INTO system_code_pattern (...) VALUES (...)
ON CONFLICT (company_id, table_name) 
DO UPDATE SET 
    id_pattern = EXCLUDED.id_pattern,
    code_pattern = EXCLUDED.code_pattern;
```

---

### Error: "function does not exist"
**Solution:** Make sure you've run the `make_system_code.sql` script first.

---

### Function Returns Wrong Format
**Check:** The function returns: `max_number%system_id%system_code%master_max_number`

In C#, parse it like this:
```csharp
var parts = result.ToString().Split('%');
var maxNumber = Convert.ToInt32(parts[0]);
var systemId = parts[1];
var systemCode = parts[2];
var masterMaxNumber = Convert.ToInt32(parts[3]);
```

---

## 📝 Migration Checklist

- [ ] Execute `system_code_tables.sql` in PostgreSQL
- [ ] Verify both tables created successfully
- [ ] Insert sample patterns for your business entities
- [ ] Test the `make_system_code()` function manually
- [ ] Test the C# `GenerateSystemCode()` method
- [ ] Configure patterns for all tables that need auto-numbering
- [ ] Set up appropriate periods (Yearly/Monthly/Daily/Auto)
- [ ] Document your numbering schemes

---

## 🎯 Next Steps

After creating the tables:

1. **Configure your patterns** - Insert records into `system_code_pattern` for each entity type
2. **Test thoroughly** - Use the test queries above
3. **Integrate with your application** - Call `GenerateSystemCode()` when creating new records
4. **Monitor sequences** - Query `system_code_max` to see current counters

---

## 📞 Support

If you encounter issues:

1. Check PostgreSQL logs for detailed error messages
2. Verify table existence: `\dt` in psql
3. Check function exists: `\df make_system_code`
4. Review permissions on both tables

---

**Created:** 2026-03-06  
**Status:** Ready for execution  
**Dependencies:** Requires `make_system_code()` function already created
