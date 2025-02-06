# Postgres Databases

## Database Configuration

### Connection strings

Larpake uses two connection strings for it's database actions. Two different connection strings are used to achieve higher security. For example normal larpake server should not have access to run any commands like create or drop tables. The other connection string is used by database migrations service.

### Databases

Larpake project uses one postgres database with multiple tables. Creation of the database and its users must be made by a database admin. This document contains intructions to how to complete required admin work.

## Connecting to server

Open new connection to postgres server. If database is locally hosted, you should be able to connect usin psql command line. Note that `psql.exe` might not be added to system path environment variable by default. To connect to locally hosted postgres server run command below.

```
psql -h localhost -U postgres
```

-   option `-h` means host.
-   option `-U` means user. `postgres` is default admin user with all privileges.
-   application will ask for password, that should be `postgres` by default.

<br/>

## Creating new database

PostgreSQL server instance can have multiple databases. Larpake Server uses one database, so let's create one. I use `larpake_dev` for my development database name, but you can use name you want.

```sql
CREATE DATABASE larpake_dev;
```

Now you can connect to created database by using command below. Note that my database name `larpake_dev` is used after this point, so you might need to change your commands to match the name.

```sql
\c larpake_dev
```

<br/>

## Creating Database User/Role

New PostgreSQL user is created with command below.

```sql
CREATE USER <user_name>;
```

### Larpake server User Permissions

-   This connection string is used to access all tables and their sequence tables
-   Any name can be chosen for created user, I use `server_user`

Create User

```sql
CREATE USER server_user;
```

Give Permissions To Connect To Database

```sql
GRANT CONNECT ON DATABASE larpake_server TO server_user;
```

### Admin/Migrations User Permissions

-   This connection string should have access to create/edit tables and stored procedures
-   Any name can be chosen for created user, I use `migrations_service`

### Create user

```
CREATE USER
```
