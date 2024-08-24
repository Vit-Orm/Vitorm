set -e


#---------------------------------------------------------------------
# args

args_="

export basePath=/root/temp

# "


#---------------------------------------------------------------------
echo '#build-bash__10.Test_InitEnv.sh'


echo '#build-bash__10.Test_InitEnv.sh -> #1 init mysql'
docker rm dev-mysql -f || true
docker run --rm \
--name dev-mysql \
-p 3306:3306 \
-e MYSQL_DATABASE=dev-orm \
-e MYSQL_ROOT_PASSWORD=123456 \
docker.lith.cloud/library/mysql:8.0.26 &

