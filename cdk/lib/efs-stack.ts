import * as cdk from '@aws-cdk/core';
import * as ec2 from '@aws-cdk/aws-ec2';
import * as efs from '@aws-cdk/aws-efs';

export class EfsStack extends cdk.Stack {
    constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
        super(scope, id, props);

        const vpc = new ec2.Vpc(this, 'Vpc');

        const fileSystem = new efs.FileSystem(this, 'MyFileSystem', {
            vpc,
            encrypted: true,
            performanceMode: efs.PerformanceMode.GENERAL_PURPOSE,
            throughputMode: efs.ThroughputMode.BURSTING,
            lifecyclePolicy: efs.LifecyclePolicy.AFTER_30_DAYS
        });

        const instance = new ec2.Instance(this, 'Client', {
            vpc,
            vpcSubnets: {
                subnetType: ec2.SubnetType.PUBLIC
            },
            machineImage: ec2.MachineImage.latestAmazonLinux({
                generation: ec2.AmazonLinuxGeneration.AMAZON_LINUX_2,
                edition: ec2.AmazonLinuxEdition.STANDARD,
                virtualization: ec2.AmazonLinuxVirt.HVM,
                storage: ec2.AmazonLinuxStorage.GENERAL_PURPOSE
            }),
            instanceType: ec2.InstanceType.of(ec2.InstanceClass.T3, ec2.InstanceSize.MICRO)
        });

        fileSystem.connections.allowDefaultPortFrom(instance);

        instance.addUserData(`
            yum install -y amazon-efs-utils
            mkdir /home/efs-mount-point

            mount -t efs -o tls ${fileSystem.fileSystemId}:/ /home/efs-mount-point

            cd /home/efs-mount-point
            chmod go+rw .
            touch test-file.txt
        `);

        new cdk.CfnOutput(this, 'FileSystemId', { value: fileSystem.fileSystemId });
        new cdk.CfnOutput(this, 'ClientInstanceId', { value: instance.instanceId });
    }
}