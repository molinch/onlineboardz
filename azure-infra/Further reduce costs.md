If the VM scale set VM size is less then costs would be drastically lowered.
We could use the VM B series.
Easiest way to apply that is to recreate AKS cluster, since VMSS/Load balancer are impacted.

Something like that would be needed:
```pwsh
$rg = New-AzResourceGroup -Name boardz-rg2 -Location westeurope
$kv = New-AzKeyVault -VaultName boardz-kv2 -ResourceGroupName $rg.ResourceGroupName -Location $rg.Location -Sku Standard
$tm = New-AzTrafficManagerProfile -Name boardz -ResourceGroupName $rg.ResourceGroupName -TrafficRoutingMethod Performance -RelativeDnsName boardz`
        -Ttl 30 -MonitorProtocol HTTPS -MonitorPort 443 -MonitorPath "/"
		
# In order to reduce cost we will disable accelerated networking and change the VMs that get allocated with the VM scale set
# We will use B series which are the cheapest
# First get the two virtual machine scale sets that got created with AKS
$allVmss = Get-AzVMss
foreach ($vmss in $allVmss) {
    # Then disable accelerared networking (B series are not available with it)
    $vmss.VirtualMachineProfile.NetworkProfile.NetworkInterfaceConfigurations[0].EnableAcceleratedNetworking = false

    Stop-AzVmss -ResourceGroupName $vmss.ResourceGroupName -VMScaleSetName $vmss.Name -Force
    Update-AzVmss -ResourceGroupName $rg.ResourceGroupName -VMScaleSetName $vmss.Name -VirtualMachineScaleSet $vmss

    # MANUALLY resize the VMSS to use B1ls VM size, it's the lowest cost

    Start-AzVmss -ResourceGroupName $vmss.ResourceGroupName -VMScaleSetName $vmss.Name
}
```