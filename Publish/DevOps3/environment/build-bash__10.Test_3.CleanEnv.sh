set -e


#---------------------------------------------------------------------
# args

args_="

export basePath=/root/temp

# "


#---------------------------------------------------------------------
echo '#build-bash__10.Test_3.CleanEnv.sh'


echo '#build-bash__10.Test_3.CleanEnv.sh -> #1 remove mysql'
docker rm dev-mysql -f || true
