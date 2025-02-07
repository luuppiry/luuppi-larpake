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
CREATE USER <user_name> WITH PASSWORD '<password>';
```

### Larpake server User Permissions

-   This connection string is used to access all tables and their sequence tables
-   Any name can be chosen for created user, I use `api_user`
-   In production environment stronger passwords should be used

Create User

```sql
CREATE USER api_user WITH PASSWORD 'api_pwd';
```

Give Permissions To Connect To Database

```sql
GRANT CONNECT ON DATABASE larpake_dev TO api_user;
```

Give Permissions To All Tables

```sql
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO api_user;
```

Give Permissions To All Sequences

```sql
GRANT USAGE, SELECT ON ALL SEQUENCES IN SCHEMA public TO api_user;
```

### Admin/Migrations User Permissions

-   This connection string should have access to create/edit tables and stored procedures
-   Any name can be chosen for created user, I use `migrations_user`
-   In production environment stronger passwords should be used

Create user

```sql
CREATE USER migrations_user WITH PASSWORD 'migrations_pwd';
```

Give Permissions To Manage Database

```sql
GRANT CREATE ON DATABASE larpake_dev TO migrations_user;
```

Give permissions

```sql
GRANT CREATE, USAGE ON SCHEMA public TO migrations_user;
```

## Create Connection Strings

Required permissions should now be set. Connections strings consist of server information and user information.

Connection string template:

```
Host=<server_ip>; Port=<port>; Database=<db_name>; Username=<username>; Password=<password>
```

For Example Api Client Connection Would Be

```
Host=localhost; Port=5432; Database=larpake_dev; Username=api_user; Password='api_pwd';
```

For Example Migrations Client Connection Would Be

```
Host=localhost; Port=5432; Database=larpake_dev; Username=migrations_user; Password='migrations_pwd';
```

### Using Connection Strings

In development environment you can put your connections strings into `appsettings.Development.json` file. This file should be stored in the same folder as appsettings.json files, which every runnable C# project has. DO NOT PUT your connections strings into `appsettings.json`, USE ONLY `appsettings.Development.json` files which are not tracked by source control.

appsettings.Development.json

```json
{
    "ConnectionStrings": {
        "Default": "<your-connection-string-here>"
    }
}
```

-   This is minimal example of appsettings file.
-   Note that all values that are filled with `"--value-here--` should be filled in your `appsettings.Development.json`.
-   If you cannot find your `appsettings.Development.json`, you must create on.
-   In production, secrets like connection strings should be stored securely.
