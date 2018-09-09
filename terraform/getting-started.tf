provider "aws" {
  region = "${var.region}"
}

resource "aws_instance" "sample_instance" {
  ami = "${lookup(var.amis, var.region)}"
  instance_type = "t2.micro"
}

resource "aws_eip" "elastic_ip" {
  instance = "${aws_instance.sample_instance.id}"
}

output "ip" {
  value = "${aws_eip.elastic_ip.public_ip}"
}
