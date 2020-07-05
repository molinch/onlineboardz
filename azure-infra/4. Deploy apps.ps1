$rg = Get-AzResourceGroup -Name boardz-rg2
$aks = Get-AzAks -ResourceGroupName $rg.ResourceGroupName -Name boardz-aks
$kv = Get-AzKeyVault -ResourceGroupName $rg.ResourceGroupName -Name boardz-kv2


az keyvault secret  download --file boardz.pfx --vault-name $kv.VaultName --encoding base64 --name tls-cert
openssl pkcs12 -in ./boardz.pfx -clcerts -nokeys -out /tmp/boardz-out.crt -passin 'pass:'
openssl pkcs12 -in ./boardz.pfx -nocerts -nodes -out /tmp/boardz-out.rsa -passin 'pass:'

# create tls secret
kubectl create secret tls boardz-tls --key /tmp/boardz-out.rsa --cert /tmp/boardz-out.crt

# make sure you are in the folder src/helm-charts
helm install boardz-apps boardz-apps

rm /tmp/boardz-out.*