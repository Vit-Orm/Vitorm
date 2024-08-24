set -e


#---------------------------------------------------------------------
# args
args_="

export basePath=/root/temp/svn

# "

#----------------------------------------------
# basePath
if [ -z "$basePath" ]; then export basePath=$PWD/../../..; fi
export devOpsPath="$PWD/.."

#----------------------------------------------
echo "#10.Test.bash -> #1 init test environment"
bashFile="$devOpsPath/environment/build-bash__10.Test_InitEnv.sh"
if [ -f "$bashFile" ]; then
	echo "#10.Test.bash -> #1 init test environment - Run bash"
	sh "$bashFile"
fi


#----------------------------------------------
echo "#10.Test.bash -> #2 find test projects and run test"

docker run -i --rm \
--net=host \
--env LANG=C.UTF-8 \
-v $NUGET_PATH:/root/.nuget \
-v "$basePath":/root/code \
-v "$basePath":"$basePath" \
serset/dotnet:sdk-6.0 \
bash -c "
set -e

cd /root/code

#2.1 skip if no test projects
if grep '<test>' -r --include *.csproj; then
	echo '#10.Test.bash -> got projects need to test'
else
	echo '#10.Test.bash -> skip for no project needs to test'
	exit 0
fi

#2.2 run test
echo '#10.Test.bash -> #2.2 run test...'
for file in \$(grep -a '<test>' . -rl --include *.csproj)
do
	echo '#10.Test.bash -> #2.2.1 run test:'
	echo run test for project \"\$file\"

	# run test
	cd \$(dirname \"\$file\")
	dotnet test
done


"
#----------------------------------------------
echo "#10.Test.bash -> #3 clean test environment"
bashFile="$devOpsPath/environment/build-bash__10.Test_CleanEnv.sh"
if [ -f "$bashFile" ]; then
	echo "#10.Test.bash -> #1 Clean test environment - Run bash"
	sh "$bashFile"
fi



#----------------------------------------------

echo '#10.Test.bash -> success!'





