-- PostgreSQL Table Creation Script for System Code Generation
-- These tables are required by the make_system_code() function

-- Table: system_code_pattern
-- Stores the pattern templates for generating system codes (IDs) for different tables
CREATE TABLE IF NOT EXISTS system_code_pattern (
    system_code_pattern_id SERIAL PRIMARY KEY,
    company_id VARCHAR(50) NOT NULL,
    table_name VARCHAR(100) NOT NULL,
    id_pattern VARCHAR(250),           -- Pattern for ID field (e.g., '{Year}{Month}{Number}')
    code_pattern VARCHAR(250),         -- Pattern for Code field (e.g., 'INV-{Year}-{Number}')
    period VARCHAR(10) DEFAULT 'Auto', -- Reset period: 'Auto', 'Yearly', 'Monthly', 'Daily'
    start_number SMALLINT DEFAULT 1,   -- Starting number for sequence
    increment SMALLINT DEFAULT 1,      -- Increment value for each new code
    padding SMALLINT DEFAULT 0,        -- Number of digits to pad (0 = no padding)
    padding_char CHAR(1) DEFAULT '0',  -- Character to use for padding
    year_length SMALLINT DEFAULT 2,    -- Length of year in pattern (2 = '24', 4 = '2024')
    master_table VARCHAR(100),         -- Optional master table for global sequencing
    is_active BOOLEAN DEFAULT TRUE,
    created_by INTEGER,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_by INTEGER,
    updated_at TIMESTAMP DEFAULT NOW(),
    
    -- Unique constraint to prevent duplicate patterns for same table/company
    CONSTRAINT uk_system_code_pattern UNIQUE (company_id, table_name)
);

-- Indexes for performance
CREATE INDEX idx_system_code_pattern_company ON system_code_pattern(company_id);
CREATE INDEX idx_system_code_pattern_table ON system_code_pattern(table_name);
CREATE INDEX idx_system_code_pattern_active ON system_code_pattern(is_active);

-- Comments
COMMENT ON TABLE system_code_pattern IS 'Stores code generation patterns for different tables';
COMMENT ON COLUMN system_code_pattern.id_pattern IS 'Template for ID field with placeholders like {Year}, {Month}, {Day}, {Number}, {Prefix}, {Suffix}';
COMMENT ON COLUMN system_code_pattern.code_pattern IS 'Template for Code field with same placeholders';
COMMENT ON COLUMN system_code_pattern.period IS 'When to reset the counter: Auto (never), Yearly, Monthly, Daily';
COMMENT ON COLUMN system_code_pattern.master_table IS 'Optional reference to a master table for centralized numbering';


-- Table: system_code_max
-- Tracks the current maximum number used for each pattern (the actual counter)
CREATE TABLE IF NOT EXISTS system_code_max (
    system_code_max_id SERIAL PRIMARY KEY,
    company_id VARCHAR(50) NOT NULL,
    table_name VARCHAR(100) NOT NULL,
    period VARCHAR(20) NOT NULL,     -- The period value (e.g., '2024', '2024-01', '2024-01-15', or 'Auto')
    max_number INTEGER NOT NULL DEFAULT 0,  -- Current maximum number used
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    
    -- Unique constraint to track one counter per table/period/company
    CONSTRAINT uk_system_code_max UNIQUE (company_id, table_name, period)
);

-- Indexes for performance
CREATE INDEX idx_system_code_max_company ON system_code_max(company_id);
CREATE INDEX idx_system_code_max_table ON system_code_max(table_name);
CREATE INDEX idx_system_code_max_period ON system_code_max(period);
CREATE INDEX idx_system_code_max_lookup ON system_code_max(company_id, table_name, period);

-- Comments
COMMENT ON TABLE system_code_max IS 'Tracks current sequence numbers for code generation';
COMMENT ON COLUMN system_code_max.period IS 'Period identifier: year (2024), year-month (2024-01), date (2024-01-15), or Auto';
COMMENT ON COLUMN system_code_max.max_number IS 'The last used number in the sequence';


-- Sample Data for Testing
-- Uncomment and customize these as needed for your application

/*
-- Example 1: Invoice numbering (yearly reset, 6-digit padding)
INSERT INTO system_code_pattern (company_id, table_name, id_pattern, code_pattern, period, start_number, increment, padding, padding_char, year_length)
VALUES ('1', 'Invoice', 'INV-{Year}-{Number}', 'INV-{Year}-{Number}', 'Yearly', 1, 1, 6, '0', 4)
ON CONFLICT (company_id, table_name) DO NOTHING;

-- Example 2: Purchase Order (monthly reset, 4-digit padding)
INSERT INTO system_code_pattern (company_id, table_name, id_pattern, code_pattern, period, start_number, increment, padding, padding_char, year_length)
VALUES ('1', 'PurchaseOrder', 'PO-{Year}{Month}-{Number}', 'PO-{Year}{Month}-{Number}', 'Monthly', 1, 1, 4, '0', 2)
ON CONFLICT (company_id, table_name) DO NOTHING;

-- Example 3: Employee ID (no reset, 5-digit padding)
INSERT INTO system_code_pattern (company_id, table_name, id_pattern, code_pattern, period, start_number, increment, padding, padding_char, year_length)
VALUES ('1', 'Employee', 'EMP-{Number}', 'EMP-{Number}', 'Auto', 1000, 1, 5, '0', 2)
ON CONFLICT (company_id, table_name) DO NOTHING;

-- Example 4: Attendance (daily reset, 3-digit padding)
INSERT INTO system_code_pattern (company_id, table_name, id_pattern, code_pattern, period, start_number, increment, padding, padding_char, year_length)
VALUES ('1', 'Attendance', 'ATT-{Year}{Month}{Day}-{Number}', 'ATT-{Date}-{Number}', 'Daily', 1, 1, 3, '0', 2)
ON CONFLICT (company_id, table_name) DO NOTHING;
*/


-- Grant permissions (adjust as needed)
-- GRANT ALL PRIVILEGES ON TABLE system_code_pattern TO postgres;
-- GRANT ALL PRIVILEGES ON TABLE system_code_max TO postgres;
-- GRANT USAGE, SELECT ON SEQUENCE system_code_pattern_system_code_pattern_id_seq TO postgres;
-- GRANT USAGE, SELECT ON SEQUENCE system_code_max_system_code_max_id_seq TO postgres;
