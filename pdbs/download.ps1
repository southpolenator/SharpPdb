[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$webClient = New-Object System.Net.WebClient
$webClient.Credentials = new-object System.Net.NetworkCredential("cidownload", "AP6JaG9ToerxBc7gWP5LcU1CNpb");
$json = $webClient.DownloadString("https://sharpdebug.jfrog.io/sharpdebug/api/storage/generic-local/pdbs/");
$json = ConvertFrom-Json $json
foreach ($child in $json.children)
{
    $file = $child.uri;
    if ($file.StartsWith("/"))
    {
        $file = $file.Substring(1);
    }
    $url = "https://sharpdebug.jfrog.io/sharpdebug/generic-local/pdbs/$file";
    $filename = "$PSScriptRoot\$file";
    Write-Host "$url  =>  $filename"
    $webClient.DownloadFile($url, $filename);
}
