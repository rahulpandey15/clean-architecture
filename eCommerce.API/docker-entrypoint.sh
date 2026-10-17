#!/bin/bash
set -e

DB_HOST=${DB_HOST:-db}
DB_PORT=${DB_PORT:-1433}

echo "Waiting for database $DB_HOST:$DB_PORT..."
# wait until TCP port is open
until (echo > /dev/tcp/$DB_HOST/$DB_PORT) >/dev/null 2>&1; do
  printf '.'
  sleep 1
done

echo "Database reachable. Starting application..."

# Run the application
exec dotnet eCommerce.API.dll
