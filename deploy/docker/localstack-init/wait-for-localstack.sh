set -e

LOCALSTACK_HOST=${LOCALSTACK_HOST:-localstack}
LOCALSTACK_PORT=${LOCALSTACK_PORT:-4566}

echo "Waiting for LocalStack to be ready at $LOCALSTACK_HOST:$LOCALSTACK_PORT..."

# Wait for LocalStack to be ready
until curl -s http://$LOCALSTACK_HOST:$LOCALSTACK_PORT/health > /dev/null; do
  echo "LocalStack is not ready yet - sleeping for 1 second"
  sleep 1
done

echo "LocalStack is up and running!"