ARTIFACTS_USER='cidownload'
ARTIFACTS_PASSWORD='AP6JaG9ToerxBc7gWP5LcU1CNpb'
ARTIFACTS_URL='https://sharpdebug.jfrog.io/sharpdebug/api/storage/generic-local/pdbs/'

files=$(curl -u$ARTIFACTS_USER:$ARTIFACTS_PASSWORD $ARTIFACTS_URL | grep -Po '(?<="uri" : "/)[^"]*')
for file in $files ; do
    url="https://sharpdebug.jfrog.io/sharpdebug/generic-local/pdbs/$file"
    echo $url '-->' $file
    curl -u$ARTIFACTS_USER:$ARTIFACTS_PASSWORD $url --output $file
done
