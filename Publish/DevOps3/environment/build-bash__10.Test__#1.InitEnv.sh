set -e


#---------------------------------------------------------------------
# args

args_="

export basePath=/root/temp

# "


#---------------------------------------------------------------------
echo '#build-bash__10.Test__#1.InitEnv.sh -> #1 start MySql container'
docker rm vitorm-mysql -f || true
docker run -d \
--name vitorm-mysql \
-p 3306:3306 \
-e MYSQL_DATABASE=db_orm \
-e MYSQL_ROOT_PASSWORD=123456 \
mysql:8.0.26



#---------------------------------------------------------------------
echo '#build-bash__10.Test__#1.InitEnv.sh -> #2 start SqlServer container'
docker rm vitorm-sqlserver -f || true
docker run -d \
--name vitorm-sqlserver \
-p 1433:1433 \
-e 'ACCEPT_EULA=Y' \
-e 'SA_PASSWORD=Admin0123' \
mcr.microsoft.com/mssql/server:2019-CU12-ubuntu-20.04












#---------------------------------------------------------------------
echo '#build-bash__10.Test__#1.InitEnv.sh -> #8 wait for containers to init'


echo '#build-bash__10.Test__#1.InitEnv.sh -> #8.1 wait for MySql to init' 
docker run -t --rm --link vitorm-mysql mysql:8.0.26 timeout 120 sh -c 'until mysql -h vitorm-mysql -u root -p123456 -e "SELECT 1"; do echo waiting for MySql; sleep 2; done;  mysql -h vitorm-mysql --database=db_orm -u root -p123456 -e "create database if not exists db_orm2;create schema if not exists orm;";    '


echo '#build-bash__10.Test__#1.InitEnv.sh -> #8.2 wait for SqlServer to init' 
docker run -t --rm --link vitorm-sqlserver mcr.microsoft.com/mssql/server:2019-CU12-ubuntu-20.04 timeout 120 sh -c 'until /opt/mssql-tools/bin/sqlcmd -S "vitorm-sqlserver" -U SA -P "Admin0123" -Q "SELECT 1"; do echo waiting for SqlServer; sleep 2; done;  /opt/mssql-tools/bin/sqlcmd -S "vitorm-sqlserver" -U SA -P "Admin0123" -Q "CREATE DATABASE db_orm";  /opt/mssql-tools/bin/sqlcmd -S "vitorm-sqlserver" -d "db_orm" -U SA -P "Admin0123" -Q "CREATE Schema orm";  /opt/mssql-tools/bin/sqlcmd -S "vitorm-sqlserver" -U SA -P "Admin0123" -Q "CREATE DATABASE db_orm2";    '


#---------------------------------------------------------------------
echo '#build-bash__10.Test__#1.InitEnv.sh -> #9 init test environment success!'