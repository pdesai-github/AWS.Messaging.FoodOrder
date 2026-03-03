# AWS Credentials Troubleshooting Guide

## Error: "The security token included in the request is invalid"

This error typically means one of the following:

### ? Common Causes

1. **AWS credentials are expired or invalid**
2. **Region mismatch between appsettings and SNS topic**
3. **AWS credentials not configured on your machine**
4. **IAM permissions insufficient for SNS:Publish**
5. **AWS credentials file corrupted or misconfigured**

---

## ? Step 1: Verify AWS Credentials Are Configured

### Check if credentials exist:

**Windows (PowerShell):**
```powershell
# Check if credentials file exists
Test-Path $env:USERPROFILE\.aws\credentials
Test-Path $env:USERPROFILE\.aws\config

# View configured profiles
aws configure list-profiles
```

**macOS/Linux:**
```bash
# Check if credentials file exists
ls -la ~/.aws/credentials
ls -la ~/.aws/config

# View configured profiles
aws configure list-profiles
```

### If no credentials exist, configure them:

```bash
# Interactive setup
aws configure

# You'll be prompted for:
# AWS Access Key ID: [your access key]
# AWS Secret Access Key: [your secret key]
# Default region name: ap-south-1  ? USE THIS (matches your SNS topic)
# Default output format: json
```

---

## ? Step 2: Verify Credentials Are Valid

```bash
# Test your AWS credentials
aws sts get-caller-identity

# Should output:
# {
#   "UserId": "AIDAI...",
#   "Account": "509399622377",
#   "Arn": "arn:aws:iam::509399622377:user/your-username"
# }
```

**If this fails**: Your credentials are invalid/expired. Get new AWS credentials from AWS Console.

---

## ? Step 3: Fix Region Mismatch

### Your SNS topic is in: `ap-south-1`
### But appsettings.Development.json has: `us-east-1`

**FIX: Update appsettings.Development.json**

Replace the AWS region to match your SNS topic region:

```json
{
  "Authentication": {
    "ApiKey": "dev-api-key-12345"
  },
  "AWS": {
    "Profile": "default",
    "Region": "ap-south-1",
    "SNS": {
      "TopicArn": "arn:aws:sns:ap-south-1:509399622377:FoodOrderCreated.fifo"
    }
  }
}
```

---

## ? Step 4: Verify SNS Topic Exists and is Accessible

```bash
# List SNS topics in ap-south-1 region
aws sns list-topics --region ap-south-1

# Should show your FoodOrderCreated.fifo topic
```

If topic doesn't appear, either:
- Topic was deleted
- You're looking at wrong region
- You don't have SNS permissions

---

## ? Step 5: Check IAM Permissions

Your AWS user needs these SNS permissions:

```bash
# Test publish permission
aws sns publish \
  --topic-arn arn:aws:sns:ap-south-1:509399622377:FoodOrderCreated.fifo \
  --message "Test message" \
  --region ap-south-1
```

If you get a permission error, you need IAM policy:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "sns:Publish"
      ],
      "Resource": "arn:aws:sns:ap-south-1:509399622377:FoodOrderCreated.fifo"
    }
  ]
}
```

Ask your AWS admin to attach this policy to your user.

---

## ? Step 6: Reload/Restart Application

After fixing credentials or region:

1. **Stop the running application** (Ctrl+C in terminal)
2. **Build the project**:
   ```bash
   dotnet clean
   dotnet build
   ```
3. **Run the application**:
   ```bash
   dotnet run
   ```

---

## ? Step 7: Test the API Again

```bash
# Test with curl
curl -X POST http://localhost:5000/api/foodorder \
  -H "X-API-Key: dev-api-key-12345" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "deliveryAddress": "123 Main St",
    "items": [{"itemName": "Pizza", "quantity": 1, "price": 15.99}],
    "totalAmount": 15.99
  }'
```

Or use Postman with "Create Food Order" request.

---

## ?? Debug: Enable Verbose AWS Logging

Add this to `appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Amazon": "Debug"
    }
  }
}
```

This will show detailed AWS SDK logs in console.

---

## ?? Quick Checklist

- [ ] AWS credentials configured (`aws configure`)
- [ ] Credentials are valid (`aws sts get-caller-identity` works)
- [ ] Region matches SNS topic region: `ap-south-1`
- [ ] SNS topic exists (`aws sns list-topics --region ap-south-1`)
- [ ] IAM user has `sns:Publish` permission
- [ ] Application restarted after config changes
- [ ] Postman header has correct API key: `X-API-Key: dev-api-key-12345`

---

## ?? Still Not Working?

### Get more debug info:

1. **Check application logs** - look for AWS error details
2. **Test AWS directly**:
   ```bash
   aws sns publish \
     --topic-arn arn:aws:sns:ap-south-1:509399622377:FoodOrderCreated.fifo \
     --message '{"test": "message"}' \
     --region ap-south-1
   ```
3. **Verify Topic ARN format** - FIFO topics need `.fifo` suffix
4. **Check Topic Access Policy** - ensure topic allows your AWS user to publish

---

## Common Error Messages

| Error | Cause | Solution |
|-------|-------|----------|
| "security token included in the request is invalid" | Bad/expired credentials | Run `aws configure` with new credentials |
| "NotAuthorizedException" | No SNS:Publish permission | Add IAM policy |
| "TopicNotFound" | SNS topic doesn't exist | Check topic ARN |
| "InvalidParameter" | Invalid topic ARN format | Verify ARN is correct |
| "User: ... is not authorized to perform: sns:Publish" | IAM permission denied | Add sns:Publish to user policy |

