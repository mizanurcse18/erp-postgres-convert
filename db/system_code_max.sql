CREATE TABLE system_code_max (
    table_name VARCHAR(100) NOT NULL,
    period VARCHAR(20) NOT NULL,
    company_id VARCHAR(50) NOT NULL,
    max_number INTEGER NOT NULL,
    updated_at TIMESTAMP NOT NULL,
    CONSTRAINT pk_system_code_max PRIMARY KEY (table_name, period, company_id)
);
