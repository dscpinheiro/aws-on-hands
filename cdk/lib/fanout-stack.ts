import * as cdk from '@aws-cdk/core';
import * as kms from '@aws-cdk/aws-kms';
import * as sns from '@aws-cdk/aws-sns';
import * as subs from '@aws-cdk/aws-sns-subscriptions';
import * as sqs from '@aws-cdk/aws-sqs';

export class FanoutStack extends cdk.Stack {
    constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
        super(scope, id, props);

        const emailParam = new cdk.CfnParameter(this, 'Email', {
            description: 'Email to include in the topic subscription list'
        });

        const snsKey = kms.Alias.fromAliasName(this, 'SnsKey', 'alias/aws/sns');
        const topic = new sns.Topic(this, 'FanoutTopic', {
            displayName: 'Sample topic that sends messages to two SQS queues',
            topicName: 'FanoutTopic',
            masterKey: snsKey
        });

        const queue1 = new sqs.Queue(this, 'Queue1', {
            deliveryDelay: cdk.Duration.seconds(3),
            retentionPeriod: cdk.Duration.days(7),
            visibilityTimeout: cdk.Duration.minutes(10)
        });

        const queue2 = new sqs.Queue(this, 'Queue2', {
            maxMessageSizeBytes: cdk.Size.mebibytes(256).toKibibytes(),
            receiveMessageWaitTime: cdk.Duration.seconds(20),
            visibilityTimeout: cdk.Duration.minutes(5)
        });

        topic.addSubscription(new subs.SqsSubscription(queue1));
        topic.addSubscription(new subs.SqsSubscription(queue2));
        topic.addSubscription(new subs.EmailSubscription(emailParam.valueAsString));

        new cdk.CfnOutput(this, 'TopicArn', { value: topic.topicArn });
        new cdk.CfnOutput(this, 'Queue1Url', { value: queue1.queueUrl });
        new cdk.CfnOutput(this, 'Queue2Url', { value: queue2.queueUrl });
    }
}