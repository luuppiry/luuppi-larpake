#! /bin/sh

echo "sleeping 10s..."

sleep 10

psql -c 'CREATE DATABASE larpake;'

export PGDATABASE=larpake

psql -c "CREATE USER api_user WITH PASSWORD '${PG_API_CLIENT_PASSWORD}';" 

psql -c "CREATE USER migrations_user WITH PASSWORD '${PG_API_MIGRATIONS_PASSWORD}';"

psql -c 'GRANT CONNECT ON DATABASE larpake TO api_user;'

psql -c 'GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO api_user;'

psql -c 'GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO api_user;'

psql -c 'GRANT CREATE ON DATABASE larpake TO migrations_user;'

psql -c 'GRANT CREATE, USAGE ON SCHEMA public TO migrations_user;'
