set -e


#---------------------------------------------------------------------
# args

args_="

export basePath=/root/temp

# "


#---------------------------------------------------------------------
echo '#build-bash__10.Test_#3.CleanEnv.sh'


echo '#build-bash__10.Test_#3.CleanEnv.sh -> #1 remove MySql'
docker rm orm-mysql -f || true


echo '#build-bash__10.Test_#3.CleanEnv.sh -> #2 remove SqlServer'
docker rm orm-mssql -f || true
