FROM amazon/aws-cli:latest

# Install needed packages
RUN yum install -y jq python3 python3-pip

# Install boto3 for Python AWS SDK interactions
RUN pip3 install boto3 awscli-local

# Copy initialization scripts
COPY init.sh /init.sh
COPY create-resources.sh /create-resources.sh
COPY wait-for-localstack.sh /wait-for-localstack.sh

# Make scripts executable
RUN chmod +x /init.sh /create-resources.sh /wait-for-localstack.sh

# Set the entrypoint
ENTRYPOINT ["/init.sh"]