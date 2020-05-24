Set-StrictMode -Version Latest
#$ErrorActionPreference = "Stop"

$HostDataPath="c:/identityserver-pgsql-data/" # where the data is stored on the host, this way if the container is dropped data is kept
$AdministratorPassword="C0mPliC4tedP4ssw0rd"
$ContainerName="identityserver-pgsql"

if (-not (docker ps -a -f "name=$ContainerName" --format '{{.Names}}')) {
	echo "**** Creating & starting PostgreSQL container ****"

	docker run -d `
		--name $ContainerName `
		-e POSTGRES_PASSWORD=$AdministratorPassword `
		-e PGDATA=/var/lib/postgresql/data/pgdata `
		-v "$HostDataPath\:/var/lib/postgresql/data" `
		-p 5432:5432 `
		postgres
}
else {
	echo "**** Starting PostgreSQL container ****"
	docker start $ContainerName
}