variable "region" {
  default = "us-west-2"
}

variable "amis" {
  type = "map"
  default = {
    "us-west-2" = "ami-6cd6f714"
  }
}