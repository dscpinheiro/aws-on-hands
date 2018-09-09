output "instance_id" {
  value = "${aws_instance.sample_instance.id}"
}

output "public_ip" {
  value = "${aws_eip.elastic_ip.public_ip}"
}
