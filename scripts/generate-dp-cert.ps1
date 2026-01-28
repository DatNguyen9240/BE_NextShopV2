Param(
    [int]$Days = 3650,
    [string]$Password = "ChangeMePlease"
)

Write-Host "Generating self-signed certificate (will export dp_key.pfx)..."
$cert = New-SelfSignedCertificate -Subject "CN=nextshop.local" -KeyExportPolicy Exportable -CertStoreLocation Cert:\CurrentUser\My -NotAfter (Get-Date).AddDays($Days)
$pwd = ConvertTo-SecureString -String $Password -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath .\dp_key.pfx -Password $pwd
Write-Host "Created dp_key.pfx (password: $Password). Move dp_key.pfx to ./secrets and add to docker-compose as a bind mount or use Docker secrets."