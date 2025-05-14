# Set environment variables
export AWS_ACCESS_KEY_ID="test"
export AWS_SECRET_ACCESS_KEY="test"
export AWS_DEFAULT_REGION="us-east-1"
export ENDPOINT_URL="http://localstack:4566"

# Create DynamoDB tables
echo "Creating DynamoDB tables..."

# Transactions table
aws --endpoint-url=$ENDPOINT_URL dynamodb create-table \
  --table-name Transactions \
  --attribute-definitions \
    AttributeName=Id,AttributeType=S \
    AttributeName=UserId,AttributeType=S \
    AttributeName=Status,AttributeType=S \
  --key-schema AttributeName=Id,KeyType=HASH \
  --global-secondary-indexes \
    "[
      {
        \"IndexName\": \"UserIdIndex\",
        \"KeySchema\": [{\"AttributeName\":\"UserId\",\"KeyType\":\"HASH\"}],
        \"Projection\": {\"ProjectionType\":\"ALL\"},
        \"ProvisionedThroughput\": {\"ReadCapacityUnits\":5,\"WriteCapacityUnits\":5}
      },
      {
        \"IndexName\": \"StatusIndex\",
        \"KeySchema\": [{\"AttributeName\":\"Status\",\"KeyType\":\"HASH\"}],
        \"Projection\": {\"ProjectionType\":\"ALL\"},
        \"ProvisionedThroughput\": {\"ReadCapacityUnits\":5,\"WriteCapacityUnits\":5}
      }
    ]" \
  --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5

# PaymentMethods table
aws --endpoint-url=$ENDPOINT_URL dynamodb create-table \
  --table-name PaymentMethods \
  --attribute-definitions \
    AttributeName=Id,AttributeType=S \
    AttributeName=UserId,AttributeType=S \
  --key-schema AttributeName=Id,KeyType=HASH \
  --global-secondary-indexes \
    "[
      {
        \"IndexName\": \"UserIdIndex\",
        \"KeySchema\": [{\"AttributeName\":\"UserId\",\"KeyType\":\"HASH\"}],
        \"Projection\": {\"ProjectionType\":\"ALL\"},
        \"ProvisionedThroughput\": {\"ReadCapacityUnits\":5,\"WriteCapacityUnits\":5}
      }
    ]" \
  --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5

# Create S3 bucket
echo "Creating S3 bucket..."
aws --endpoint-url=$ENDPOINT_URL s3api create-bucket --bucket payment-processor-files

# Create SQS queues
echo "Creating SQS queues..."
aws --endpoint-url=$ENDPOINT_URL sqs create-queue --queue-name payment-notifications
aws --endpoint-url=$ENDPOINT_URL sqs create-queue --queue-name payment-failures

# Create SNS topics and subscriptions
echo "Creating SNS topics and subscriptions..."
NOTIFICATION_TOPIC_ARN=$(aws --endpoint-url=$ENDPOINT_URL sns create-topic --name payment-notifications --output json | jq -r '.TopicArn')
FAILURE_TOPIC_ARN=$(aws --endpoint-url=$ENDPOINT_URL sns create-topic --name payment-failures --output json | jq -r '.TopicArn')

# Subscribe SQS to SNS
aws --endpoint-url=$ENDPOINT_URL sns subscribe \
  --topic-arn $NOTIFICATION_TOPIC_ARN \
  --protocol sqs \
  --notification-endpoint http://localstack:4566/000000000000/payment-notifications

aws --endpoint-url=$ENDPOINT_URL sns subscribe \
  --topic-arn $FAILURE_TOPIC_ARN \
  --protocol sqs \
  --notification-endpoint http://localstack:4566/000000000000/payment-failures

# Set up SES identity for email sending
echo "Setting up SES email identity..."
aws --endpoint-url=$ENDPOINT_URL ses verify-email-identity --email-address no-reply@paymentprocessor.com

echo "AWS resources created successfully in LocalStack!"