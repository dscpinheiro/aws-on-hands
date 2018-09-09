provider "aws" {
  region = "us-east-1"
}

resource "aws_instance" "sample_instance" {
  ami = "ami-b374d5a5"
  instance_type = "t2.micro"
}

resource "aws_eip" "elastic_ip" {
  instance = "${aws_instance.sample_instance.id}"
}
