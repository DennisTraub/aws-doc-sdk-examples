import os
import re
import uuid
import yaml

from dataclasses import dataclass, asdict


@dataclass
class Config:
    AwsRegion: str
    S3BucketName: str
    DynamoDbTableName: str


class ConfigLoader:
    CONFIG_FILE = "config.yaml"
    DEFAULT_REGION = "us-east-1"
    DEFAULT_RESOURCE_NAME = f"bedrock-conversation-demo-{uuid.uuid4().hex[:8]}"

    def __init__(self):
        self.config = self._load_or_create_config()

    def get_region(self):
        return self.config.AwsRegion

    def get_s3_bucket_name(self):
        return self.config.S3BucketName

    def get_dynamo_db_table_name(self):
        return self.config.DynamoDbTableName

    def _load_or_create_config(self):
        # Check if the configuration file exists
        if os.path.exists(self.CONFIG_FILE):
            with open(self.CONFIG_FILE, 'r') as file:
                print("Loading configuration:")
                data = yaml.safe_load(file)
                config = Config(
                    AwsRegion=data['AwsRegion'],
                    S3BucketName=data['S3BucketName'],
                    DynamoDbTableName=data['DynamoDbTableName']
                )
                self._print_config(config)

        else:
            # Prompt the user to create a new configuration
            create_config = input(
                "Configuration file not found. Would you like to create it? (Y/n): "
            ).strip().lower()
            if create_config == 'n':
                print("Configuration not created. Exiting.")
                return None

            # Ask the user for the config values or use defaults
            aws_region = self._get_valid_input("Enter AWS Region", self.DEFAULT_REGION)
            bucket_name = self._get_valid_input("Enter S3 Bucket Name", self.DEFAULT_RESOURCE_NAME)
            table_name = self._get_valid_input("Enter DynamoDB Table Name", self.DEFAULT_RESOURCE_NAME)

            config = Config(
                AwsRegion=aws_region,
                S3BucketName=bucket_name,
                DynamoDbTableName=table_name
            )

            # Save the configuration to a file
            with open(self.CONFIG_FILE, 'w') as file:
                yaml.safe_dump(asdict(config), file, default_flow_style=False)
                print("Saving configuration...")
                self._print_config(config)

        return config

    @staticmethod
    def _get_valid_input(prompt, default_value):
        while True:
            value = input(f"{prompt} (default: {default_value}): ").strip()
            if not value:
                value = default_value
            if re.match("^[a-zA-Z0-9-]+$", value) is not None:
                return value
            else:
                print("Invalid name. Only letters, numbers, and dashes are allowed.")

    @staticmethod
    def _print_config(config):
        for key, value in asdict(config).items():
            print(f"{key}: {value}")
