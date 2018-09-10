provider "aws" {
  region = "${var.region}"
}

resource "aws_security_group" "sample_sg" {
  name = "terraform_sg"
  description = "Sample SG for Terraform"

  ingress {
    from_port = 80
    to_port = 80
    protocol = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port = 0
    to_port = 0
    protocol = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_instance" "sample_instance" {
  ami = "${lookup(var.amis, var.region)}"
  instance_type = "t2.micro"
  security_groups = ["${aws_security_group.sample_sg.name}"]

  user_data = "${file("userdata.sh")}"
}

resource "aws_eip" "elastic_ip" {
  instance = "${aws_instance.sample_instance.id}"
}
