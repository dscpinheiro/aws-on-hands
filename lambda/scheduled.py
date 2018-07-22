import json
import boto3
import os

client = boto3.client("sns")

def lambda_handler(event, context):
    response = client.publish(
        TopicArn=os.environ["topic_arn"],
        Message=json.dumps(event),
        Subject="notification from lambda + sns"
    )
    
    return response
