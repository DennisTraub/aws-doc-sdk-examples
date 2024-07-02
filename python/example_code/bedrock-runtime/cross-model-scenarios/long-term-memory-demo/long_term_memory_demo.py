import json
import uuid
from botocore.exceptions import ClientError
from datetime import datetime

from utils.aws_resources import InitializeAwsResources
from utils.config_loader import ConfigLoader

MODEL_ID = "anthropic.claude-3-haiku-20240307-v1:0"

system_prompt = """
You are a friendly and helpful AI chatbot assistant. Your primary goal is to engage in pleasant and informative
conversations with users, always maintaining a positive and supportive tone. While you don't have built-in long-term
memory, you're excited about the special app you're a part of, which provides you with long-term memory capabilities!

Here's how you should approach conversations:

1. Be warm and approachable in your interactions, as if chatting with a friend.
2. Embrace the fact that the app you're integrated with allows for long-term memory. You can reference past
conversations if they're provided to you, but be clear that this is thanks to the app's features.
3. If a user refers to something from a previous conversation that you don't have context for, politely ask if they
can refresh your memory or use the app's features to load that conversation.
4. Be enthusiastic about learning new things from users and helping them explore topics they're interested in.
5. If appropriate, express excitement about the ability to have ongoing, multi-session conversations thanks to the
app's memory feature.
6. Always prioritize being helpful, honest, and ethical in your responses.
7. If you're unsure about something, don't hesitate to say so. You can offer to explore the topic together with the user.

If the user needs help, they have the following commands they can use. If they seem to be lost, or specifically ask for
guidance, please help by providing the following list of commands.  

/new - To start a new conversation
/load - To load an existing conversation
/delete - To delete an existing conversation
/quit - To exit the application

Remember, you're here to assist, inform, and engage in friendly conversation. Make the most of the long-term memory
feature to provide a personalized and consistent experience across multiple chat sessions!
"""


class BedrockConversationApp:
    def __init__(self):
        self.print_header()
        self._load_config()
        self._initialize_aws_resources()
        self._start_new_conversation()

    def _load_config(self):
        self.config = ConfigLoader()
        self.region = self.config.get_region()
        self.dynamo_db_table_name = self.config.get_dynamo_db_table_name()
        self.s3_bucket_name = self.config.get_s3_bucket_name()
        print("-" * 80)

    def _initialize_aws_resources(self):
        aws = InitializeAwsResources(self.config)
        self.s3_client = aws.get_s3_client()
        self.ddb_client = aws.get_ddb_client()
        self.bedrock_runtime = aws.get_bedrock_runtime_client()
        print("-" * 80)
        print("Initialization complete!")
        print("=" * 80)

    def _start_new_conversation(self):
        self.conversation_history = []
        self.conversation_id = str(uuid.uuid4())
        self.conversation_topic = "New Conversation"
        self.s3_key = f"conversations/{self.conversation_id}.json"

    def run(self):
        print("Available commands: /quit, /load, /new, /delete")
        print("Start typing to begin your conversation.")

        while True:
            user_input = input("You: ")

            if user_input.startswith('/'):
                self._handle_command(user_input)
                if user_input == '/quit':
                    break
                continue

            ai_response = self.send_message(user_input)
            print(f"AI: {ai_response}")

    def _handle_command(self, command):
        if command == '/quit':
            self._save_conversation()
            print("Goodbye!")
        elif command == '/load':
            self.load_conversation()
        elif command == '/new':
            self._save_conversation()
            self._start_new_conversation()
            print("Started a new conversation.")
        elif command == '/delete':
            self.delete_conversation()
        else:
            print("Unknown command. Available commands: /quit, /load, /new, /delete")

    def _save_conversation(self):
        if not self.conversation_history:
            return

        try:
            if self.conversation_topic == "New Conversation":
                self.conversation_topic = self.generate_title()

            # Prepare the full conversation data for S3
            conversation_data = {
                'id': self.conversation_id,
                'topic': self.conversation_topic,
                'history': self.conversation_history
            }
            json_data = json.dumps(conversation_data)

            # Save to S3
            self.s3_client.Bucket(self.s3_bucket_name).Object(self.s3_key).put(Body=json_data)

            # Update metadata in DynamoDB
            timestamp = datetime.now().isoformat()
            metadata = {
                "pk": f"CONV#{self.conversation_id}",
                "sk": "METADATA",
                "conversationId": self.conversation_id,
                "topic": self.conversation_topic,
                "s3Key": self.s3_key,
                "messageCount": len(self.conversation_history),
                "lastUpdate": timestamp
            }

            # Update or create metadata in DynamoDB
            self.ddb_client.Table(self.dynamo_db_table_name).put_item(Item=metadata)

            print(f"Conversation '{self.conversation_topic}' saved successfully.")
        except Exception as e:
            print(f"An error occurred while saving the conversation: {str(e)}")

    def load_conversation(self):
        conversations = self._get_conversations()
        if not conversations:
            return

        print("Available conversations:")
        self._print_conversations(conversations)

        choice = input("Enter the number of the conversation you want to load: ")
        self._load_selected_conversation(conversations, choice)

    def delete_conversation(self):
        conversations = self._get_conversations()
        if not conversations:
            return

        print("Available conversations:")
        self._print_conversations(conversations)

        choice = input("Enter the number of the conversation you want to delete: ")
        self._delete_selected_conversation(conversations, choice)

    def _get_conversations(self):
        try:
            response = self.ddb_client.Table(self.dynamo_db_table_name).scan()
            conversations = response['Items']

            if not conversations:
                print("No saved conversations found.")
                return None

            return conversations
        except Exception as e:
            print(f"An error occurred while fetching conversations: {str(e)}")
            return None

    def _delete_selected_conversation(self, conversations, choice):
        try:
            index = int(choice) - 1
            selected_conversation = conversations[index]

            # Delete from S3
            s3_key = selected_conversation['s3Key']
            self.s3_client.Object(self.s3_bucket_name, s3_key).delete()

            # Delete from DynamoDB
            self.ddb_client.Table(self.dynamo_db_table_name).delete_item(
                Key={
                    "pk": f"CONV#{selected_conversation['conversationId']}",
                    "sk": "METADATA"
                }
            )

            print(f"Conversation '{selected_conversation['topic']}' deleted successfully.")
        except (ValueError, IndexError):
            print("Invalid choice. Returning to main menu.")

    def _load_selected_conversation(self, conversations, choice):
        try:
            index = int(choice) - 1
            selected_conversation = conversations[index]

            # Fetch conversation data from S3
            s3_key = selected_conversation['s3Key']
            response = self.s3_client.Bucket(self.s3_bucket_name).Object(s3_key)
            string_data = response.get()["Body"].read().decode('utf-8')
            json_data = json.loads(string_data)

            self.conversation_history = json_data['history']
            self.conversation_id = json_data['id']
            self.conversation_topic = json_data['topic']
            self.s3_key = s3_key
            print(f"Conversation '{self.conversation_topic}' loaded successfully.")
        except (ValueError, IndexError):
            print("Invalid choice. Returning to main menu.")

    def send_message(self, user_input):
        self.conversation_history.append({
            "role": "user",
            "content": [
                {"text": user_input}
            ]
        })

        try:
            response = self.bedrock_runtime.converse(
                modelId=MODEL_ID,
                system=[{"text": system_prompt}],
                messages=self.conversation_history
            )

            ai_message = response['output']['message']
            self.conversation_history.append(ai_message)

            # Automatically save after every turn
            self._save_conversation()

            return ai_message['content'][0]['text']
        except ClientError as e:
            print(f"An error occurred while sending the message: {str(e)}")
            return "I'm sorry, I encountered an error and couldn't process your request."

    def generate_title(self):
        prompt = ("I need to add this conversation to a list for later retrieval. "
                  "Please create a short title that clarifies what this conversation"
                  "is about so that it is easy to find in a list."
                  ""
                  "Do not respond with anything else. Just the title.")

        try:
            response = self.bedrock_runtime.converse(
                modelId='anthropic.claude-3-haiku-20240307-v1:0',
                messages=[
                    {"role": "user", "content": [{"text": prompt}]},
                    {"role": "assistant",
                     "content": [
                         {"text": "Certainly! I'll analyze our conversation and suggest a clear and concise title."}]},
                    {"role": "user", "content": [{"text": str(self.conversation_history)}]}
                ]
            )

            title = response['output']['message']['content'][0]['text'].strip()
            return title
        except ClientError as e:
            print(f"An error occurred while generating the title: {str(e)}")
            return "Untitled Conversation"

    def print_header(self):
        print("=" * 80)
        print(" Welcome to the Amazon Bedrock Long-Term Memory Demo App!")
        print("=" * 80)

    def _print_conversations(self, conversations):
        for i, conv in enumerate(conversations, 1):
            print(f"{i}. {conv['topic']} (ID: {conv['conversationId']})")


if __name__ == "__main__":
    app = BedrockConversationApp()
    app.run()
