# Wait for LocalStack to be ready
echo "Waiting for LocalStack to be ready..."
/wait-for-localstack.sh

# Create resources
echo "Creating AWS resources in LocalStack..."
/create-resources.sh

echo "LocalStack initialization completed."