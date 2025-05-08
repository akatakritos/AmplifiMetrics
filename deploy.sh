set -euxo pipefail

IMAGE=amplifimetrics
TAG=latest
ARCHIVE="${IMAGE}_$TAG.tar.gz"

docker build -t "$IMAGE:$TAG" -f AmplifiMetrics/Dockerfile .
docker image save "$IMAGE:$TAG" | gzip > "$ARCHIVE"
scp "$ARCHIVE" matt@beelink-01:~/docker/

ssh matt@beelink-01 -t "cd ~/docker; docker load --input $ARCHIVE"
ssh matt@beelink-01 -t "cd ~/docker/monitoring; docker compose up -d --force-recreate amplifimetrics"
