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
    - [ ] Refactor your static page so that it reads/updates the AWS DynamoDB table (Hint: EC2 Instance Role)
5. Web Hosting Platform-as-a-Service
6. Microservices
7. Serverless
8. Cost Analysis
9. Automation
10. Continuous Delivery
11. Misc
