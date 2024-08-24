set -e


#---------------------------------------------------------------------
# args

args_="

export basePath=/root/temp

# "


#---------------------------------------------------------------------
echo '#build-bash__10.Test__#1.InitEnv.sh -> #1 init mysql'


echo '#build-bash__10.Test__#1.InitEnv.sh -> #1.0 start mysql container'
docker rm dev-mysql -f || true
docker run -d \
--name dev-mysql \
-p 3306:3306 \
-e MYSQL_DATABASE=dev-orm \
-e MYSQL_ROOT_PASSWORD=123456 \
mysql:8.0.26

echo '#build-bash__10.Test__#1.InitEnv.sh -> #1.2 wait for mysql to init' 
docker run -t --rm --link dev-mysql:dev-mysql mysql:8.0.26 timeout 120 sh -c 'until mysql -h dev-mysql -u root -p123456 -e "SELECT 1"; do echo waiting for mysql; sleep 2; done;'

echo '#build-bash__10.Test__#1.InitEnv.sh -> #1.3 init mysql success!'