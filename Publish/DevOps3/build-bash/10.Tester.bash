set -e


#---------------------------------------------------------------------
# args
args_="

export basePath=/root/temp/svn

# "

#----------------------------------------------
# basePath
if [ -z "$basePath" ]; then basePath=$PWD/../../..; fi


#----------------------------------------------
echo "#10.Tester.bash -> find test projects and run test"

export devOpsPath="$PWD/.."

docker run -i --rm \
--env LANG=C.UTF-8 \
-v $NUGET_PATH:/root/.nuget \
-v "$basePath":/root/code \
-v "$basePath":"$basePath" \
serset/dotnet:sdk-6.0 \
bash -c "
set -e

if grep '<test>' -r --include *.csproj /root/code; then
	echo '#10.Tester.bash -> got projects need to test'
else
	echo '#10.Tester.bash -> skip for no project needs to test'
	exit 0
fi


export basePath=/root/code

echo '#10.Tester.bash -> #2 run test...'
cd \$basePath
for file in \$(grep -a '<test>' . -rl --include *.csproj)
do
	cd \$basePath

	echo '#10.Tester.bash -> #2 run test:'
	echo run test for project \"\$file\"

	#publish
	cd \$(dirname \"\$file\")
	dotnet test

done


"



echo '#10.Tester.bash -> success!'





