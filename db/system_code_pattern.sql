CREATE TABLE system_code_pattern (
    table_name VARCHAR(100) NOT NULL,
    company_id VARCHAR(50) NOT NULL,
    period VARCHAR(10) NOT NULL,
    id_pattern VARCHAR(250),
    code_pattern VARCHAR(250),
    start_number SMALLINT NOT NULL,
    increment SMALLINT NOT NULL,
    padding SMALLINT,
    padding_char CHAR(1),
    year_length SMALLINT,
    master_table VARCHAR(100),
    CONSTRAINT pk_system_code_pattern PRIMARY KEY (table_name, company_id)
);
