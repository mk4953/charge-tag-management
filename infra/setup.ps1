# Creates the Azure resources for the ChargeTags API. Run it once before the first deployment.
# Requires the Azure CLI and "az login".
param(
    [string]$ResourceGroup = "rg-chargetags",
    [string]$Location = "swedencentral",
    [string]$Suffix = (Get-Random -Minimum 1000 -Maximum 9999)
)

$ErrorActionPreference = "Stop"

function Invoke-Az {
    az @args --only-show-errors --output none
    if ($LASTEXITCODE -ne 0) {
        throw "Azure CLI command failed: az $($args -join ' ')"
    }
}

$sqlServer = "sql-chargetags-$Suffix"
$sqlAdmin = "chargetagsadmin"
$plan = "plan-chargetags"
$webApp = "app-chargetags-$Suffix"

# The password is only stored in the web app's connection string, never in the repository
$characters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789".ToCharArray()
$sqlPassword = (-join (1..24 | ForEach-Object { Get-Random -InputObject $characters })) + "aA1"

if ((az group exists --name $ResourceGroup) -eq "false") {
    Invoke-Az group create --name $ResourceGroup --location $Location
}

Write-Host "Creating SQL server $sqlServer..."
Invoke-Az sql server create --name $sqlServer --resource-group $ResourceGroup --location $Location --admin-user $sqlAdmin --admin-password $sqlPassword

# Lets Azure services, such as the web app, reach the database
Invoke-Az sql server firewall-rule create --resource-group $ResourceGroup --server $sqlServer --name AllowAzureServices --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

Write-Host "Creating database on the Azure SQL free offer..."
Invoke-Az sql db create --resource-group $ResourceGroup --server $sqlServer --name ChargeTags --edition GeneralPurpose --compute-model Serverless --family Gen5 --capacity 2 --use-free-limit --free-limit-exhaustion-behavior AutoPause

Write-Host "Creating web app $webApp..."
Invoke-Az appservice plan create --resource-group $ResourceGroup --name $plan --location $Location --sku F1 --is-linux
Invoke-Az webapp create --resource-group $ResourceGroup --plan $plan --name $webApp --runtime "DOTNETCORE:10.0"

$connectionString = "Server=tcp:$sqlServer.database.windows.net,1433;Database=ChargeTags;User ID=$sqlAdmin;Password=$sqlPassword;Encrypt=True;TrustServerCertificate=False;"
Invoke-Az webapp config connection-string set --resource-group $ResourceGroup --name $webApp --connection-string-type SQLAzure --settings "ChargeTags=$connectionString"

Write-Host "Done: https://$webApp.azurewebsites.net/swagger"
