# Let's Encrypt

Using https://github.com/rmbolger/Posh-ACME

```pwsh
# Create the certificate
$certNames = 'boardz.fabien-molinet.fr', 'boardz-identity.fabien-molinet.fr'
$email = 'fabien.molinet@gmail.com'
New-PACertificate $certNames -AcceptTOS -Contact $email
$cert = Get-PACertificate
$cert | Format-List

# Import it into KV
Import-AzKeyVaultCertificate -VaultName boardz-kv2 -Name tls-cert -FilePath $cert.PfxFile -Password $cert.PfxPass -Tag @{"target"="all"}
```


The certificate duration is 90 days, renewal should be submitted afterwards
Submit-Renewal