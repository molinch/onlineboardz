# This script provisions the infrastructure in Azure to run Online Boardz

$rg = New-AzResourceGroup -Name boardz-rg2 -Location westeurope
$kv = New-AzKeyVault -VaultName boardz-kv2 -ResourceGroupName $rg.ResourceGroupName -Location $rg.Location -Sku Standard
$tm = New-AzTrafficManagerProfile -Name boardz -ResourceGroupName $rg.ResourceGroupName -TrafficRoutingMethod Performance -RelativeDnsName boardz`
        -Ttl 30 -MonitorProtocol HTTPS -MonitorPort 443 -MonitorPath "/"

# MANUALLY create keyvault policies for current user (me) to have all rights on the KV


# Create ACR registry
$acr = New-AzContainerRegistry -ResourceGroupName $rg.ResourceGroupName -Name boardz -EnableAdminUser -Sku Basic
# Create SSH keys for AKS
ssh-keygen -o -b 4096 -f aks-ssh
# place the ssh keys in key vault, PowerShell won't import the RSA key so we do it with az cli
az keyvault secret set --name aks-ssh-keys --vault-name $kv.VaultName --file aks-ssh
# since it is free subscription, there is a limit of 4 vCores
# however from Powershell setting MinNodeCount=1/MaxNodeCount=2 won't work it complains about that we need 6 vCores.
# I don't get it as it should be 4 vCores... Anyway let's force it to 1 node, and change it later via UI
$aks = New-AzAks -ResourceGroupName $rg.ResourceGroupName -Name boardz-aks2 -Location $rg.Location -LoadBalancerSku basic -SshKeyPath ./aks-ssh.pub -NodeCount 1 -NodeVmSetType VirtualMachineScaleSets
# MANUALLY change AKS to autoscale with min node count = 1 and max node count = 2
# attach AKS to ACR
az aks update -n $aks.Name -g $rg.ResourceGroupName --attach-acr $acr.Name

# These secrets will need to be set in Azure directly, we initiate them with a dummy value that will be changed MANUALLY throught Portal
$dummySecret = ConvertTo-SecureString 'dummy' -AsPlainText -Force
Set-AzKeyVaultSecret -VaultName $kv.VaultName -Name "MongoPassword" -SecretValue $dummySecret -Tag @{"target"="game-svc"}
Set-AzKeyVaultSecret -VaultName $kv.VaultName -Name "PostgresPassword" -SecretValue $dummySecret -Tag @{"target"="identity-server"}
Set-AzKeyVaultSecret -VaultName $kv.VaultName -Name "Authentication-Google-ClientSecret" -SecretValue $dummySecret -Tag @{"target"="identity-server"}
Set-AzKeyVaultSecret -VaultName $kv.VaultName -Name "Authentication-Google-ClientId" -SecretValue $dummySecret -Tag @{"target"="identity-server"}
Set-AzKeyVaultSecret -VaultName $kv.VaultName -Name "Authentication-Facebook-ClientSecret" -SecretValue $dummySecret -Tag @{"target"="identity-server"}
Set-AzKeyVaultSecret -VaultName $kv.VaultName -Name "Authentication-Facebook-ClientId" -SecretValue $dummySecret -Tag @{"target"="identity-server"}


# use a single app configuration resource since we would have to pay otherwise...
# segregation is done by label
$appConfig = New-AzAppConfigurationStore -ResourceGroupName $rg.ResourceGroupName -Name boardz-appconfig -Location $rg.Location

# currently it's not possible to add referenced Key vault entries in the appconfiguration via PowerShell so let's do it via az cli
foreach ($s in $secrets) { az appconfig kv set-keyvault -n $appConfig.Name --key $s.Name --label $s.Tags.target --secret-identifier $s.Id }

# create our managed identities
$gameSvcIdentity = New-AzUserAssignedIdentity -ResourceGroupName $rg.ResourceGroupName -Name game-svc-mi
$identityServerIdentity = New-AzUserAssignedIdentity -ResourceGroupName $rg.ResourceGroupName -Name identity-server-mi
$identities = $gameSvcIdentity,$identityServerIdentity
# get the virtual machine scale set that got created with AKS, VMSS is created in weird resource group, with a weird name, hence we must search for it
$vmss = Get-AzVMss | where ResourceGroupName -like "*$($rg.ResourceGroupName)*" | select -first 1
foreach ($id in $identities) {
    # allow them to operate on key vault
    az keyvault set-policy -n $kv.VaultName --spn $id.ClientId --secret-permissions get list --certificate-permissions get list

    # allow them to read app configuration, AFAIK this cannot be done in PowerShell yet
    az appconfig identity assign -g $rg.ResourceGroupName -n $appConfig.Name --identities $id.Id
    az role assignment create --assignee-object-id $id.PrincipalId --role "App Configuration Data Reader"

    # assign the managed identities to the virtual machine scale set, so MIs will be on the VMs, and the VMs will receive requests from Pods
    az vmss identity assign -g $vmss.ResourceGroupName -n $vmss.Name --identities $id.Id
}