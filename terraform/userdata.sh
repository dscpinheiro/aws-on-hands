#!/bin/bash -xe
sudo yum update -y
sudo yum -y install httpd
sudo service httpd start
sudo systemctl start httpd
sudo systemctl enable httpd

echo "Hello from Terraform!" > /var/www/html/index.html