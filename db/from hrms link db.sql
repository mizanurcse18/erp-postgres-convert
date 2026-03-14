-- Create a schema for remote tables
CREATE SCHEMA IF NOT EXISTS security_remote;

-- Install the extension if not already installed
CREATE EXTENSION IF NOT EXISTS postgres_fdw;

-- Create a server connection to the security database
CREATE SERVER security_server
FOREIGN DATA WRAPPER postgres_fdw
OPTIONS (host 'localhost', dbname 'security', port '5432');

-- Create a user mapping for the connection
CREATE USER MAPPING FOR current_user
SERVER security_server  -- Use the same server name as defined above
OPTIONS (user 'postgres', password 'root');

-- Import foreign schema (this will create all tables from the remote schema)
IMPORT FOREIGN SCHEMA public
FROM SERVER security_server  -- Use the same server name as defined above
INTO security_remote;


----------------------
SELECT * FROM pg_user_mappings;

--------------------------------
SELECT * FROM pg_namespace WHERE nspname = 'security_remote';



