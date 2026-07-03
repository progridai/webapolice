#!/usr/bin/env bash
set -euo pipefail

echo "Starting database initialization script..."

# Create ERP user if not exists
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
  DO \$$
  BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = '$ERP_DB_USER') THEN
      CREATE ROLE "$ERP_DB_USER" WITH LOGIN PASSWORD '$ERP_DB_PASSWORD';
    END IF;
  END
  \$$;
EOSQL

# Create ERP database if not exists
if ! psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -tAc "SELECT 1 FROM pg_database WHERE datname='$ERP_DB_NAME'" | grep -q 1; then
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -c "CREATE DATABASE \"$ERP_DB_NAME\" OWNER \"$ERP_DB_USER\""
fi

# Ensure correct owner and permissions for ERP database
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
  ALTER DATABASE "$ERP_DB_NAME" OWNER TO "$ERP_DB_USER";
  GRANT ALL PRIVILEGES ON DATABASE "$ERP_DB_NAME" TO "$ERP_DB_USER";
  REVOKE ALL PRIVILEGES ON DATABASE "$ERP_DB_NAME" FROM PUBLIC;
  GRANT CONNECT ON DATABASE "$ERP_DB_NAME" TO "$ERP_DB_USER";
EOSQL

# Create Keycloak user if not exists
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
  DO \$$
  BEGIN
    IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = '$KEYCLOAK_DB_USER') THEN
      CREATE ROLE "$KEYCLOAK_DB_USER" WITH LOGIN PASSWORD '$KEYCLOAK_DB_PASSWORD';
    END IF;
  END
  \$$;
EOSQL

# Create Keycloak database if not exists
if ! psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -tAc "SELECT 1 FROM pg_database WHERE datname='$KEYCLOAK_DB_NAME'" | grep -q 1; then
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" -c "CREATE DATABASE \"$KEYCLOAK_DB_NAME\" OWNER \"$KEYCLOAK_DB_USER\""
fi

# Ensure correct owner and privileges for Keycloak database
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
  ALTER DATABASE "$KEYCLOAK_DB_NAME" OWNER TO "$KEYCLOAK_DB_USER";
  GRANT ALL PRIVILEGES ON DATABASE "$KEYCLOAK_DB_NAME" TO "$KEYCLOAK_DB_USER";
  REVOKE ALL PRIVILEGES ON DATABASE "$KEYCLOAK_DB_NAME" FROM PUBLIC;
  GRANT CONNECT ON DATABASE "$KEYCLOAK_DB_NAME" TO "$KEYCLOAK_DB_USER";
EOSQL

echo "Database initialization script completed successfully."
