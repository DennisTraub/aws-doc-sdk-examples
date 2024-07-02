import boto3
from abc import ABC, abstractmethod
from botocore.exceptions import ClientError
from dataclasses import dataclass
from enum import Enum, auto


class ResourceStatus(Enum):
    INITIALIZING = auto()
    EXISTS = auto()
    NOT_CREATED = auto()
    CREATION_REQUESTED = auto()
    CREATION_DECLINED = auto()
    CREATED = auto()
    ERROR = auto()


@dataclass
class ErrorCodes:
    not_found: str
    access_denied: str


class Resource(ABC):
    def __init__(self, config):
        self.config = config
        self.client = self._create_client()
        self.error_codes = self._get_error_codes()
        self.name = self._get_resource_name()
        self.description = f"{self.__class__.__name__} '{self.name}'"
        self.status = self._get_or_create()

    def get_client(self):
        if self.client is None:
            self.client = self._create_client()
        return self.client

    def _get_or_create(self) -> ResourceStatus:
        try:
            if self._try_access_resource():
                return ResourceStatus.EXISTS
        except ClientError as e:
            error_code = e.response["Error"]["Code"]
            if error_code == self.error_codes.not_found:
                return self._handle_not_created()
            if error_code == self.error_codes.access_denied:
                print(f"Access denied to {self.description}.")
                return ResourceStatus.ERROR
            print(f"Error: {e}")
            return ResourceStatus.ERROR

    def _handle_not_created(self) -> ResourceStatus:
        create = input(
            f"- {self.description} does not exist. Would you like to create it? (Y/n): "
        ).strip().lower()
        if create != 'n':
            return self._create_resource()
        print(f"- {self.description} not created.")
        return ResourceStatus.CREATION_DECLINED

    @abstractmethod
    def _create_client(self):
        pass

    @abstractmethod
    def _get_error_codes(self) -> ErrorCodes:
        pass

    @abstractmethod
    def _try_access_resource(self) -> bool:
        pass

    @abstractmethod
    def _create_resource(self) -> ResourceStatus:
        pass

    @abstractmethod
    def _get_resource_name(self) -> str:
        pass


class Bucket(Resource):
    def _create_client(self):
        return boto3.resource("s3", region_name=self.config.get_region())

    def _get_error_codes(self) -> ErrorCodes:
        return ErrorCodes(not_found="404", access_denied="403")

    def _create_resource(self) -> ResourceStatus:
        try:
            self.client.create_bucket(Bucket=self.name)
            print(f"- {self.description} created.")
            return ResourceStatus.CREATED
        except ClientError as e:
            print(f"Error: {e}")
            return ResourceStatus.ERROR

    def _try_access_resource(self) -> bool:
        print("- Checking S3 bucket...")
        self.client.meta.client.head_bucket(Bucket=self.name)
        print(f"- {self.description} exists and is accessible.")
        return True

    def _get_resource_name(self) -> str:
        return self.config.get_s3_bucket_name()


class Table(Resource):
    def _create_client(self):
        return boto3.resource("dynamodb", region_name=self.config.get_region())

    def _get_error_codes(self) -> ErrorCodes:
        return ErrorCodes(
            not_found="ResourceNotFoundException",
            access_denied="AccessDeniedException"
        )

    def _try_access_resource(self) -> bool:
        print("- Checking DynamoDB table...")
        self.client.Table(self.name).load()
        print(f"- {self.description} exists and is accessible.")
        return True

    def _create_resource(self) -> ResourceStatus:
        try:
            self.client.create_table(
                TableName=self.name,
                KeySchema=[
                    {"AttributeName": "pk", "KeyType": "HASH"},
                    {"AttributeName": "sk", "KeyType": "RANGE"}
                ],
                AttributeDefinitions=[
                    {"AttributeName": "pk", "AttributeType": "S"},
                    {"AttributeName": "sk", "AttributeType": "S"}
                ],
                BillingMode='PAY_PER_REQUEST'
            )
            print(f"- {self.description} created.")
            return ResourceStatus.CREATED
        except ClientError as e:
            print(f"Error: {e}")
            return ResourceStatus.ERROR

    def _get_resource_name(self) -> str:
        return self.config.get_dynamo_db_table_name()


class InitializeAwsResources:
    def __init__(self, config):
        print("Looking for required AWS Resources:")
        self.s3_client = Bucket(config).get_client()
        self.ddb_client = Table(config).get_client()
        self.bedrock_runtime_client = boto3.client("bedrock-runtime", region_name=config.get_region())

    def get_s3_client(self):
        return self.s3_client

    def get_ddb_client(self):
        return self.ddb_client

    def get_bedrock_runtime_client(self):
        return self.bedrock_runtime_client
