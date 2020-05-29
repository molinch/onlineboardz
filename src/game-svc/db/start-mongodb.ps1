Set-StrictMode -Version Latest

$ContainerName="game-svc-mongo"

if (-not (docker ps -a -f "name=$ContainerName" --format '{{.Names}}')) {
	echo "**** Creating & starting Game service MongoDB container ****"

	docker volume create --name=game-svc-mongodata
	docker run -d `
		--name $ContainerName `
		-v mongodata:/data/db `
		-d -p 27017:27017 `
		mongo
}
else {
	echo "**** Starting Game service MongoDB container ****"
	docker start $ContainerName
}