variable "region" {
  default = "us-west-2"
}

variable "amis" {
  type = "map"
  default = {
    "us-west-2" = "ami-061e7ebbc234015fe"
  }
}