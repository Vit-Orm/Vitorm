set -e


#---------------------------------------------------------------------
# args

args_="

export basePath=/root/temp

# "


#---------------------------------------------------------------------
echo '#build-bash__10.Test__#1.InitEnv.sh -> #1 start MySql container'
docker rm dev-mysql -f || true
docker run -d \
--name dev-mysql \
-p 3306:3306 \
-e MYSQL_DATABASE=dev-orm \
-e MYSQL_ROOT_PASSWORD=123456 \
mysql:8.0.26



#---------------------------------------------------------------------
echo '#build-bash__10.Test__#1.InitEnv.sh -> #2 start SqlServer container'
docker rm dev-mssql -f || true
docker run -d \
--name dev-mssql \
-p 1433:1433 \
-e 'ACCEPT_EULA=Y' \
-e 'SA_PASSWORD=Admin0123' \
mcr.microsoft.com/mssql/server:2019-CU12-ubuntu-20.04












#---------------------------------------------------------------------
echo '#build-bash__10.Test__#1.InitEnv.sh -> #8 wait for containers to init'


echo '#build-bash__10.Test__#1.InitEnv.sh -> #8.1 wait for MySql to init' 
docker run -t --rm --link dev-mysql:dev-mysql mysql:8.0.26 timeout 120 sh -c 'until mysql -h dev-mysql -u root -p123456 -e "SELECT 1"; do echo waiting for mysql; sleep 2; done;'


echo '#build-bash__10.Test__#1.InitEnv.sh -> #8.2 wait for SqlServer to init' 
docker run -t --rm --link dev-mssql:dev-mssql mcr.microsoft.com/mssql/server:2019-CU12-ubuntu-20.04 timeout 120 sh -c 'until /opt/mssql-tools/bin/sqlcmd -S "dev-mssql" -U SA -P "Admin0123" -Q "CREATE DATABASE [dev-orm]"; do echo waiting for mysql; sleep 2; done;'


#---------------------------------------------------------------------
echo '#build-bash__10.Test__#1.InitEnv.sh -> #9 init mysql success!'