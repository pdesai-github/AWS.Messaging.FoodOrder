# FoodOrder.Producer - Setup Guide

## Quick Start with Local AWS Profile

This project uses AWS SNS to publish food orders. Authentication is handled via your local AWS credentials.

### Prerequisites

1. **AWS Credentials Configured** on your machine
   - Typically stored in `~/.aws/credentials` or `~/.aws/config`
   - Or set environment variables:
     ```bash
     export AWS_ACCESS_KEY_ID=your-access-key
     export AWS_SECRET_ACCESS_KEY=your-secret-key
     export AWS_DEFAULT_REGION=us-east-1
     ```

2. **AWS CLI Profile** (if using named profiles)
   ```bash
   # View configured profiles
   aws configure list-profiles
   
   # Create a profile
   aws configure --profile your-profile-name
   ```

### Configuration

#### appsettings.Development.json
```json
{
  "Authentication": {
    "ApiKey": "dev-api-key-12345"
  },
  "AWS": {
    "Profile": "default",
    "Region": "us-east-1",
    "SNS": {
      "TopicArn": "arn:aws:sns:us-east-1:123456789012:food-order-topic"
    }
  }
}
```

Replace:
- `TopicArn`: Your actual SNS topic ARN
- `Profile`: Your AWS profile name (default: "default")
- `Region`: Your AWS region

### API Usage

#### 1. Create a Food Order

**Request:**
```bash
curl -X POST http://localhost:5000/api/foodorder \
  -H "X-API-Key: dev-api-key-12345" \
  -H "Content-Type: application/json" \
  -d '{
    "customerName": "John Doe",
    "customerEmail": "john@example.com",
    "deliveryAddress": "123 Main Street, New York",
    "items": [
      {
        "itemName": "Margherita Pizza",
        "quantity": 2,
        "price": 15.99
      },
      {
        "itemName": "Caesar Salad",
        "quantity": 1,
        "price": 8.99
      }
    ],
    "totalAmount": 40.97,
    "messageAttributes": {
      "ShopId": ["shop-1"]
    }
  }'
```

**Response (201 Created):**
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "messageId": "12345-67890-abcde",
  "status": "Order received and published to SNS",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

#### 2. Required Headers
- `X-API-Key`: API key from appsettings (required for API authentication)
- `Content-Type: application/json` (required for POST)

### Authentication Flow

```
HTTP Request
    ?
API Key Validation (ApiKeyAuthAttribute)
    ?
AWS Profile Credentials (from ~/.aws/credentials or env vars)
    ?
AWS SNS Authentication (automatic via AWSSDK)
    ?
Message Published to SNS Topic
```

### Troubleshooting

#### "AWS SNS Topic ARN is not configured"
- Check `appsettings.json` has valid `AWS:SNS:TopicArn`
- Verify topic exists in your AWS account

#### "Invalid API Key"
- Check header: `X-API-Key: dev-api-key-12345`
- Verify key matches `Authentication:ApiKey` in appsettings

#### AWS Credentials Error
```bash
# Verify AWS credentials are configured
aws sts get-caller-identity

# Set credentials if needed
aws configure
```

#### Cannot Find SNS Topic
```bash
# List SNS topics in your region
aws sns list-topics --region us-east-1
```

### Message Attributes

Orders can include metadata attributes (like `ShopId`) which are sent to SNS:
```json
"messageAttributes": {
  "ShopId": ["shop-1"],
  "DeliveryZone": ["zone-north"]
}
```

These appear as SNS message attributes for filtering in subscribers.

### Development

The application uses:
- **.NET 8** - Latest framework
- **AWS SDK for .NET** - SNS integration
- **Swagger/OpenAPI** - API documentation (http://localhost:5000/swagger)

Run the application:
```bash
dotnet run
```

### Production Deployment

For production, consider:
1. Use IAM roles instead of local credentials
2. Store `Authentication:ApiKey` in AWS Secrets Manager
3. Update `TopicArn` via environment variables or parameter store
4. Enable logging for audit trails
5. Use HTTPS only (already configured)
