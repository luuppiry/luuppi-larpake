#! /bin/sh

echo "sleeping 10s..."

sleep 10

psql -c 'CREATE DATABASE larpake_dev;'

export PGDATABASE=larpake_dev

psql -c "CREATE USER api_user_dev WITH PASSWORD '${PG_API_CLIENT_PASSWORD}';" 

psql -c "CREATE USER migrations_user_dev WITH PASSWORD '${PG_API_MIGRATIONS_PASSWORD}';"

psql -c 'GRANT CONNECT ON DATABASE larpake_dev TO api_user_dev;'

psql -c 'GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO api_user_dev;'

psql -c 'GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO api_user_dev;'

psql -c 'GRANT CREATE ON DATABASE larpake_dev TO migrations_user_dev;'

psql -c 'GRANT CREATE, USAGE ON SCHEMA public TO migrations_user_dev;'
