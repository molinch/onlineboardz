$rg = Get-AzResourceGroup -Name boardz-rg2
$aks = Get-AzAks -ResourceGroupName $rg.ResourceGroupName -Name boardz-aks
$kv = Get-AzKeyVault -ResourceGroupName $rg.ResourceGroupName -Name boardz-kv2


# Connect to AKS
sudo az aks install-cli
az aks get-credentials --resource-group $rg.ResourceGroupName --name $aks.Name
# Deploy PostgreSQL chart template from Bitnami, read: https://engineering.bitnami.com/articles/create-a-production-ready-postgresql-cluster-bitnami-kubernetes-and-helm.html
helm repo add bitnami https://charts.bitnami.com/bitnami
curl -O https://raw.githubusercontent.com/bitnami/charts/master/bitnami/postgresql/values-production.yaml
$pgPassword = Get-AzKeyVaultSecret -VaultName $kv.VaultName -Name PostgresPassword
$pgReplicationPassword = Get-AzKeyVaultSecret -VaultName $kv.VaultName -Name PostgresReplicationPassword
# Use simple values for the setup
helm install identity-server-pgsql bitnami/postgresql -f values-production.yaml `
  --set postgresqlPassword="$($pgPassword.SecretValueText)" --set replication.password="$($pgReplicationPassword.SecretValueText)" `
  --set replication.enabled=false --set replication.slaveReplicas=0 --set metrics.enabled=false
<#
# PostgreSQL can be accessed via port 5432 on the following DNS name from within your cluster:
identity-server-pgsql-postgresql.default.svc.cluster.local - Read/Write connection

# To get the password for "postgres" run:
export POSTGRES_PASSWORD=$(kubectl get secret --namespace default identity-server-pgsql-postgresql -o jsonpath="{.data.postgresql-password}" | base64 --decode)

# To connect to your database run the following command:
kubectl run identity-server-pgsql-postgresql-client --rm --tty -i --restart='Never' --namespace default --image docker.io/bitnami/postgresql:11.8.0-debian-10-r51 --env="PGPASSWORD=$POSTGRES_PASSWORD" --command -- psql --host identity-server-pgsql-postgresql -U postgres -d postgres -p 5432

# To connect to your database from outside the cluster execute the following commands:
kubectl port-forward --namespace default svc/identity-server-pgsql-postgresql 5432:5432 &
    PGPASSWORD="$POSTGRES_PASSWORD" psql --host 127.0.0.1 -U postgres -d postgres -p 5432
#>


# Deploy PgAdmin4 from Runix
helm repo add runix https://helm.runix.net/
$pgAdminPassword = Get-AzKeyVaultSecret -VaultName $kv.VaultName -Name identity-server-pgadmin4-password
helm install identity-server-pgadmin4 runix/pgadmin4 --set env.email="fabien.molinet@gmail.com"  --set env.password="$($pgAdminPassword.SecretValueText)"

<#
# Can access it using port forwarding, and browsing on http://localhost:8080/
export POD_NAME=$(kubectl get pods --namespace default -l "app.kubernetes.io/name=pgadmin4,app.kubernetes.io/instance=identity-server-pgadmin4" -o jsonpath="{.items[0].metadata.name}")
echo "Visit http://127.0.0.1:8080 to use your application"
kubectl port-forward $POD_NAME 8080:80
#>