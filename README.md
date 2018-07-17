# aws-on-hands

This a checklist on learning AWS. Since I'm not working with it at the moment, it should also work as a refresher.
The list is based on this great Reddit post: https://www.reddit.com/r/sysadmin/comments/8inzn5/so_you_want_to_learn_aws_aka_how_do_i_learn_to_be/

1. Account Basics 
    - [x] Create an IAM user for personal use
    - [x] Set up MFA for your root user, turn off all root user API keys
    - [x] Set up Billing Alerts
    - [x] Configure the AWS CLI for your user
2. Web Hosting Basics
    - [x] Deploy a EC2 VM and host a simple static web page
    - [x] Take a snapshot of your VM, delete the VM, and deploy a new one from the snapshot. Basically disk backup + disk restore
3. Auto Scaling
    - [x] Create an AMI from that VM and put it in an autoscaling group so one VM always exists
    - [x] Put a Elastic Load Balancer in front of that ASG
4. External Data
    - [x] Create a DynamoDB table and experiment with loading and retrieving data manually, then do the same via a script on your local machine
    - [x] Refactor your static page so that it reads/updates the AWS DynamoDB table (Hint: EC2 Instance Role)
5. Web Hosting Platform-as-a-Service
    - [x] Deploy an application on ElasticBeanstalk
    - [x] Create a S3 static website
    - [ ] Register a domain. Set Route53 as the Nameservers and use Route53 for DNS. Make www.yourdomain.com go to your Elastic Beanstalk. Make static.yourdomain.com serve data from the S3 bucket
    - [ ] Enable SSL for your Static S3 Website (Hint: CloudFront + ACM)
    - [ ] Enable SSL for your Elastic Beanstalk Website
6. Microservices
    - [ ] Create an API that has POST/GET bindings to update/retrieve data from DynamoDB
7. Serverless
    - [ ] Write a AWS Lambda function to run every night. Implement Least Privilege security for the Lambda Role. (Hint: Lambda using Python 3, Boto3, Amazon SES, scheduled with CloudWatch)
    - [ ] Implement API Gateway to interact with Lambda
8. Continuous Delivery
    - [ ] Explore and implement a Continuous Delivery pipeline
    - [ ] Develop a CI/CD pipeline to automatically update a dev deployment of your infrastructure when new code is published, and then build a workflow to update the production version if approved
9. Misc
    - [ ] Kinesis
    - [ ] KMS
    - [ ] More complex IAM policies
    - [ ] Networking (creating VPC from scratch)
    - [ ] Terraform for provisioning infrastructure
