# Fix AWS Credentials Error - Step by Step

## Problem
```json
{
    "error": "Failed to process order",
    "details": "The security token included in the request is invalid."
}
```

## Root Cause Found ?
**Region mismatch**: Your SNS topic is in `ap-south-1` but configuration was set to `us-east-1`

---

## ?? Solution Applied

### 1. Fixed Region in Configuration Files
- ? `appsettings.Development.json`: Changed region from `us-east-1` ? `ap-south-1`
- ? `appsettings.json`: Changed region from `us-east-1` ? `ap-south-1`
- ? Added debug logging for AWS SDK errors

---

## ? What You Need To Do Now

### Step 1: Verify AWS Credentials
Open PowerShell/Terminal and run:

```bash
# Verify credentials are valid
aws sts get-caller-identity
```

**Expected output:**
```json
{
    "UserId": "AIDAI...",
    "Account": "509399622377",
    "Arn": "arn:aws:iam::509399622377:user/your-username"
}
```

**If this fails:**
- Run: `aws configure`
- Enter your AWS Access Key ID
- Enter your AWS Secret Access Key
- Enter region: `ap-south-1`
- Enter output format: `json`

---

### Step 2: Verify SNS Topic Exists

```bash
# List SNS topics in ap-south-1 region
aws sns list-topics --region ap-south-1
```

**You should see:**
```
arn:aws:sns:ap-south-1:509399622377:FoodOrderCreated.fifo
```

---

### Step 3: Test SNS Publish Permission

```bash
# Test if you can publish to the topic
aws sns publish \
  --topic-arn arn:aws:sns:ap-south-1:509399622377:FoodOrderCreated.fifo \
  --message "Test message" \
  --region ap-south-1
```

**Expected output:**
```json
{
    "MessageId": "12345-67890-abcde"
}
```

**If you get permission error:**
Ask your AWS admin to add this IAM policy to your user:

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["sns:Publish"],
      "Resource": "arn:aws:sns:ap-south-1:509399622377:FoodOrderCreated.fifo"
    }
  ]
}
```

---

### Step 4: Restart the Application

```bash
# Stop the currently running app (Ctrl+C)

# Clean and rebuild
dotnet clean
dotnet build

# Run the application
dotnet run
```

---

### Step 5: Test the API

#### Using Postman:
1. Open Postman
2. Select **"Create Food Order"** request
3. Click **Send**

#### Using curl:
```bash
curl -X POST http://localhost:5000/api/foodorder \
  -H "X-API-Key: dev-api-key-12345" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "deliveryAddress": "123 Main St",
    "items": [{"itemName": "Pizza", "quantity": 1, "price": 15.99}],
    "totalAmount": 15.99,
    "messageAttributes": {"ShopId": ["shop-1"]}
  }'
```

**Expected response (201 Created):**
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "messageId": "12345-67890-abcde",
  "status": "Order received and published to SNS",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

---

## ?? Configuration Changes Made

### Before ?
```json
{
  "AWS": {
    "Profile": "default",
    "Region": "us-east-1",
    "SNS": {
      "TopicArn": "arn:aws:sns:ap-south-1:509399622377:FoodOrderCreated.fifo"
    }
  }
}
```

### After ?
```json
{
  "AWS": {
    "Profile": "default",
    "Region": "ap-south-1",
    "SNS": {
      "TopicArn": "arn:aws:sns:ap-south-1:509399622377:FoodOrderCreated.fifo"
    }
  },
  "Logging": {
    "LogLevel": {
      "Amazon": "Debug"
    }
  }
}
```

---

## ?? Troubleshooting If Still Not Working

### Check Application Logs
When you run `dotnet run`, look for AWS SDK debug logs:

```
[Amazon.Runtime.Internal.UnityWebRequest] Debug: Making request...
[Amazon.SimpleNotificationService.AmazonSimpleNotificationServiceClient] Debug: Response...
```

### Common Issues:

| Problem | Solution |
|---------|----------|
| "Still getting token error" | Run `aws sts get-caller-identity` to verify credentials |
| "Topic not found" | Verify topic ARN is exactly: `arn:aws:sns:ap-south-1:509399622377:FoodOrderCreated.fifo` |
| "Access Denied / NotAuthorized" | Check IAM permissions - user needs `sns:Publish` |
| "Topic is FIFO" | Make sure you're publishing with correct attributes for FIFO queue |

---

## ?? Additional Resources

- Full troubleshooting guide: `AWS_CREDENTIALS_TROUBLESHOOTING.md`
- API documentation: `POSTMAN_GUIDE.md`
- Postman collection: `Postman_Collection.json`

---

## ? Checklist

- [ ] Ran `aws sts get-caller-identity` successfully
- [ ] Ran `aws sns list-topics --region ap-south-1` and saw the topic
- [ ] Ran `aws sns publish` test successfully
- [ ] Restarted the application
- [ ] Sent test request via Postman or curl
- [ ] Received 201 Created response
- [ ] Order appears in AWS CloudWatch/CloudTrail logs

