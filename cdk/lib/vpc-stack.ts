import * as cdk from '@aws-cdk/core';
import * as ec2 from '@aws-cdk/aws-ec2';

export class VpcStack extends cdk.Stack {
    constructor(scope: cdk.Construct, id: string, props?: cdk.StackProps) {
        super(scope, id, props);

        const vpc = new ec2.Vpc(this, 'Vpc', {
            cidr: '10.200.0.0/16',

            enableDnsHostnames: true,
            enableDnsSupport: true,
            defaultInstanceTenancy: ec2.DefaultInstanceTenancy.DEFAULT,

            maxAzs: 2,

            subnetConfiguration: [
                {
                    subnetType: ec2.SubnetType.PUBLIC,
                    name: 'Public',
                    cidrMask: 22
                },
                {
                    subnetType: ec2.SubnetType.PRIVATE,
                    name: 'Private',
                    cidrMask: 20
                }
            ],

            gatewayEndpoints: {
                S3: { service: ec2.GatewayVpcEndpointAwsService.S3 },
                DDB: { service: ec2.GatewayVpcEndpointAwsService.DYNAMODB }
            },

            vpnGateway: true
        });

        vpc.addFlowLog('FlowLogCW', {
            trafficType: ec2.FlowLogTrafficType.REJECT,
            destination: ec2.FlowLogDestination.toCloudWatchLogs()
        });

        vpc.addInterfaceEndpoint('CloudFormationEndpoint', {
            service: ec2.InterfaceVpcEndpointAwsService.CLOUDFORMATION,
            subnets: {
                subnetType: ec2.SubnetType.PRIVATE
            }
        });

        const loadBalancerSG = new ec2.SecurityGroup(this, 'LoadBalancerSG', {
            vpc,
            description: 'Enable access via HTTP and HTTPS',
            allowAllOutbound: true
        });

        loadBalancerSG.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(80), 'HTTP access');
        loadBalancerSG.addIngressRule(ec2.Peer.anyIpv4(), ec2.Port.tcp(443), 'HTTPS access');

        new cdk.CfnOutput(this, 'VpcId', { value: vpc.vpcId });
        new cdk.CfnOutput(this, 'SecurityGroupId', { value: loadBalancerSG.securityGroupId });
    }
}
