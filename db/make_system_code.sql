CREATE OR REPLACE FUNCTION make_system_code(
   p_table_name VARCHAR(100),
    p_company_id VARCHAR(50),
    p_date VARCHAR(20),
    p_add_number SMALLINT DEFAULT 1,
    p_client_date TIMESTAMP DEFAULT NOW(),
    p_prefix VARCHAR(50) DEFAULT '',
    p_suffix VARCHAR(50) DEFAULT '',
   p_group_name VARCHAR(100) DEFAULT ''
)
RETURNS TEXT AS $$
DECLARE
    v_id_pattern VARCHAR(250);
    v_code_pattern VARCHAR(250);
    v_period VARCHAR(10);
    v_start_number SMALLINT;
    v_increment_val SMALLINT;
    v_padding SMALLINT;
    v_padding_char CHAR(1);
    v_year_length SMALLINT;
    v_master_table VARCHAR(100);
    v_date_period VARCHAR(20);
    v_max_number INT;
    v_master_max_number INT;
    v_system_id VARCHAR(250);
    v_system_code VARCHAR(250);
    v_row_count INT;
BEGIN
    IF (p_group_name IS NULL OR p_group_name = '') THEN
        p_group_name := p_table_name;
    END IF;

    -- Get pattern configuration
    SELECT
        scp.id_pattern,
        scp.code_pattern,
        scp.period,
        scp.start_number,
        scp.increment,
        scp.padding,
        scp.padding_char,
        scp.year_length,
        scp.master_table
    INTO
        v_id_pattern,
        v_code_pattern,
        v_period,
        v_start_number,
        v_increment_val,
        v_padding,
        v_padding_char,
        v_year_length,
        v_master_table
    FROM system_code_pattern scp
    WHERE scp.table_name = p_group_name AND scp.company_id = p_company_id;

    -- Set defaults if NULL
    IF v_id_pattern IS NULL THEN v_id_pattern := ''; END IF;
    IF v_code_pattern IS NULL THEN v_code_pattern := ''; END IF;
    IF v_period IS NULL THEN v_period := 'Auto'; END IF;
    IF v_start_number IS NULL THEN v_start_number := 1; END IF;
    IF v_increment_val IS NULL THEN v_increment_val := 1; END IF;
    IF v_padding IS NULL THEN v_padding := 0; END IF;
    IF v_padding_char IS NULL THEN v_padding_char := '0'; END IF;
    IF v_year_length IS NULL THEN v_year_length := 2; END IF;
    IF v_master_table IS NULL THEN v_master_table := ''; END IF;

    -- Set date period
    IF v_period = 'Auto' THEN v_date_period := 'Auto'; END IF;
    IF v_period = 'Yearly' THEN v_date_period := EXTRACT(YEAR FROM p_client_date::DATE); END IF;
    IF v_period = 'Monthly' THEN
        v_date_period := EXTRACT(YEAR FROM p_client_date::DATE) || '-' ||
                      TO_CHAR(p_client_date::DATE, 'Month');
    END IF;
    IF v_period = 'Daily' THEN v_date_period := p_date; END IF;

    -- Update or insert max number
    UPDATE system_code_max
    SET
        max_number = max_number + (p_add_number * v_increment_val),
        updated_at = p_client_date
    WHERE
        table_name = p_table_name AND
        period = v_date_period AND
        company_id = p_company_id;

    GET DIAGNOSTICS v_row_count = ROW_COUNT;

    IF v_row_count = 0 THEN
        INSERT INTO system_code_max(
            table_name, period, company_id, max_number, updated_at
        ) VALUES (
            p_table_name, v_date_period, p_company_id,
            v_start_number + ((p_add_number - 1) * v_increment_val),
            p_client_date
        );
    END IF;

    -- Get current max number
    SELECT max_number INTO v_max_number
    FROM system_code_max
    WHERE
        table_name = p_table_name AND
        period = v_date_period AND
        company_id = p_company_id;

    -- Build system ID
    v_system_id := REPLACE(v_id_pattern, '{Year}', RIGHT(EXTRACT(YEAR FROM p_client_date::DATE)::TEXT, v_year_length));
    v_system_id := REPLACE(v_system_id, '{Month}', LPAD(EXTRACT(MONTH FROM p_client_date::DATE)::TEXT, 2, '0'));
    v_system_id := REPLACE(v_system_id, '{Day}', LPAD(EXTRACT(DAY FROM p_client_date::DATE)::TEXT, 2, '0'));
    v_system_id := REPLACE(v_system_id, '{Prefix}', p_prefix);
    v_system_id := REPLACE(v_system_id, '{Suffix}', p_suffix);

    IF v_padding > 0 AND v_padding > LENGTH(v_max_number::TEXT) THEN
        v_system_id := REPLACE(v_system_id, '{Number}', LPAD(v_max_number::TEXT, v_padding, v_padding_char));
    ELSE
        v_system_id := REPLACE(v_system_id, '{Number}', v_max_number::TEXT);
    END IF;

    -- Build system code
    v_system_code := REPLACE(v_code_pattern, '{Year}', RIGHT(EXTRACT(YEAR FROM p_client_date::DATE)::TEXT, v_year_length));
    v_system_code := REPLACE(v_system_code, '{Month}', LPAD(EXTRACT(MONTH FROM p_client_date::DATE)::TEXT, 2, '0'));
    v_system_code := REPLACE(v_system_code, '{Day}', LPAD(EXTRACT(DAY FROM p_client_date::DATE)::TEXT, 2, '0'));
    v_system_code := REPLACE(v_system_code, '{Prefix}', p_prefix);
    v_system_code := REPLACE(v_system_code, '{Suffix}', p_suffix);

    IF v_padding > 0 AND v_padding > LENGTH(v_max_number::TEXT) THEN
        v_system_code := REPLACE(v_system_code, '{Number}', LPAD(v_max_number::TEXT, v_padding, v_padding_char));
    ELSE
        v_system_code := REPLACE(v_system_code, '{Number}', v_max_number::TEXT);
    END IF;

    -- Handle master table if specified
    v_master_max_number := v_max_number;
    IF v_master_table <> '' THEN
        UPDATE system_code_max
        SET
            max_number = max_number + (p_add_number * v_increment_val),
            updated_at = p_client_date
        WHERE
            table_name = v_master_table AND
            period = 'Auto' AND
            company_id = p_company_id;

        GET DIAGNOSTICS v_row_count = ROW_COUNT;

        IF v_row_count = 0 THEN
            INSERT INTO system_code_max(
                table_name, period, company_id, max_number, updated_at
            ) VALUES (
                v_master_table, 'Auto', p_company_id,
                v_start_number + ((p_add_number - 1) * v_increment_val),
                p_client_date
            );
        END IF;

        SELECT max_number INTO v_master_max_number
        FROM system_code_max
        WHERE
            table_name = v_master_table AND
            period = 'Auto' AND
            company_id = p_company_id;
    END IF;

    -- Return the result
    RETURN CONCAT(v_max_number::TEXT, '%', v_system_id, '%', v_system_code, '%', v_master_max_number::TEXT);
END;
$$ LANGUAGE plpgsql;
