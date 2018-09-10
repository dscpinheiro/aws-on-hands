#!/bin/bash -xe
sudo yum update -y
sudo yum -y install httpd
sudo service httpd start
sudo chkconfig httpd on

echo "Hello from Terraform!" > /var/www/html/index.html