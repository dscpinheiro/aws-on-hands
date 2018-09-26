sam local generate-event sns notification > testNotification.json

sam local invoke "SnsMessageFn" --event testNotification.json

sam package --template-file template.yaml --output-template sns-function.yaml --s3-bucket MY_BUCKET

sam deploy --template-file sns-function.yaml --stack-name SamSNS --capabilities CAPABILITY_IAM --region us-west-2