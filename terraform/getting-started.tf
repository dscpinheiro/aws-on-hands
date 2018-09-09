provider "aws" {
  region = "${var.region}"
}

resource "aws_instance" "sample_instance" {
  ami = "${lookup(var.amis, var.region)}"
  instance_type = "t2.micro"

  provisioner "remote-exec" {
    inline = [
      "sudo yum update -y",
      "sudo yum -y install httpd",
      "sudo service httpd start",
      "sudo chkconfig httpd on"
    ]
  }
}

resource "aws_eip" "elastic_ip" {
  instance = "${aws_instance.sample_instance.id}"
}
