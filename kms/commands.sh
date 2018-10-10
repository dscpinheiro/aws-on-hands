#!/bin/bash -xe

echo "This is a secret" > secret.txt

export KMS_KEY_ID=00000000-0000-0000-0000-000000000000

aws kms encrypt --key-id $KMS_KEY_ID --plaintext fileb://secret.txt --output text --query CiphertextBlob | base64 --decode > encryptedsecret.txt
aws kms decrypt --ciphertext-blob fileb://encryptedsecret.txt --output text --query Plaintext | base64 --decode > decryptedsecret.txt
aws kms re-encrypt --destination-key-id $KMS_KEY_ID --ciphertext-blob fileb://encryptedsecret.txt | base64 > newencryption.txt
aws kms enable-key-rotation --key-id $KMS_KEY_ID