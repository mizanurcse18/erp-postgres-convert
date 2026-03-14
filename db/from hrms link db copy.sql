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


-- Create a schema for remote tables
CREATE SCHEMA IF NOT EXISTS approval_remote;

-- Install the extension if not already installed

-- Create a server connection to the security database
CREATE SERVER approval_server
FOREIGN DATA WRAPPER postgres_fdw
OPTIONS (host 'localhost', dbname 'approval', port '5432');

-- Create a user mapping for the connection
CREATE USER MAPPING FOR current_user
SERVER approval_server  -- Use the same server name as defined above
OPTIONS (user 'postgres', password 'root');

-- Import foreign schema (this will create all tables from the remote schema)
IMPORT FOREIGN SCHEMA public
FROM SERVER approval_server  -- Use the same server name as defined above
INTO approval_remote;


---------------to drop server ----
-- Drop the schema and all its contents
DROP SCHEMA IF EXISTS approval_remote CASCADE;

-- Drop the server and all dependent objects
DROP SERVER IF EXISTS approval_server CASCADE;



----------------------

-- Drop the schema if it exists
DROP SCHEMA IF EXISTS approval_remote CASCADE;

-- Drop the server if it exists
DROP SERVER IF EXISTS approval_server CASCADE;

-- Drop user mappings if they exist
DROP USER MAPPING IF EXISTS FOR current_user SERVER approval_server;
DROP USER MAPPING IF EXISTS FOR postgres SERVER approval_server;





