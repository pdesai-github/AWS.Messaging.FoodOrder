# Postman Setup Guide - FoodOrder Producer API

## ?? Import Collection

### Option 1: Import from File
1. Open **Postman**
2. Click **File** ? **Import**
3. Select **Postman_Collection.json** from the FoodOrder.Producer folder
4. Click **Import**

### Option 2: Import from Link
1. In Postman, click **Import**
2. Choose **Link** tab
3. Paste the collection file path or content

---

## ?? Setup Environment Variables

### Create Environment (Optional but Recommended)

1. Click **Environments** (left sidebar)
2. Click **Create New Environment**
3. Name it: `FoodOrder-Dev`
4. Add these variables:

| Variable | Initial Value | Type |
|----------|---------------|------|
| `api_url` | `http://localhost:5000` | string |
| `api_key` | `dev-api-key-12345` | string |

5. Click **Save**

### Use Environment
- Select environment from dropdown (top-right, next to Send button)
- Choose `FoodOrder-Dev`

---

## ?? Available Requests

### 1. **Create Food Order** (Main Request)
- **Method**: POST
- **URL**: `{{api_url}}/api/foodorder`
- **Auth**: API Key in `X-API-Key` header
- **Body**: Complete order with items, customer info, and metadata
- **Expected Response**: `201 Created`

```json
{
  "customerName": "John Doe",
  "customerEmail": "john.doe@example.com",
  "deliveryAddress": "123 Main Street, New York, NY 10001",
  "items": [
    {
      "itemName": "Margherita Pizza",
      "quantity": 2,
      "price": 15.99
    }
  ],
  "totalAmount": 45.96,
  "messageAttributes": {
    "ShopId": ["shop-1"]
  }
}
```

### 2. **Create Order - Minimal**
- Simplest order format
- No message attributes
- Tests basic functionality

### 3. **Create Order - With Multiple Shop IDs**
- Multiple message attributes
- Multiple shop IDs in metadata
- Tests attribute handling

### 4. **Error Test Cases**
- Missing customer name (400)
- Missing API key (401)
- Wrong API key (401)

---

## ?? Quick Start

1. **Start the API**:
```bash
cd FoodOrder.Producer
dotnet run
```

2. **Open Postman** and import the collection

3. **Select "Create Food Order"** request

4. **Click Send** ? See `201 Created` response with:
```json
{
  "orderId": "550e8400-e29b-41d4-a716-446655440000",
  "messageId": "12345-67890-abcde",
  "status": "Order received and published to SNS",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

---

## ? Built-in Tests

The "Create Food Order" request includes automatic tests:
- ? Status code is 201 Created
- ? Response has required fields (orderId, messageId, status, createdAt)
- ? Status message is correct
- ? Saves orderId to environment for later use

**To view test results**:
1. Click **Send**
2. Click **Tests** tab in response
3. See pass/fail for each assertion

---

## ?? Authentication Details

### Header Format
```
X-API-Key: dev-api-key-12345
```

### Where to Configure
- **Development**: `appsettings.Development.json` ? `Authentication:ApiKey`
- **Update Postman**: Change `api_key` variable or header value

### Change API Key for Different Environments
1. Create multiple Postman Environments
2. Each environment has different `api_key` value
3. Switch environments as needed

---

## ?? Common Response Scenarios

### ? Success (201)
```json
{
  "orderId": "uuid-string",
  "messageId": "sns-message-id",
  "status": "Order received and published to SNS",
  "createdAt": "2024-01-15T10:30:00Z"
}
```

### ? Missing API Key (401)
```json
{
  "error": "Missing API Key"
}
```

### ? Invalid API Key (401)
```json
{
  "error": "Invalid API Key"
}
```

### ? Missing Customer Name (400)
```
"Customer name is required"
```

### ? Empty Items (400)
```
"Order must contain at least one item"
```

---

## ??? Troubleshooting

### "Connection refused" error
- Verify API is running: `dotnet run`
- Check URL: `http://localhost:5000` (not HTTPS for development)
- Check port 5000 is available

### "Invalid API Key" error
- Verify header name: `X-API-Key` (case-sensitive)
- Check value matches `appsettings.Development.json`
- Default: `dev-api-key-12345`

### "AWS SNS error" in response
- Verify AWS credentials are configured
- Run: `aws sts get-caller-identity`
- Check SNS Topic ARN in `appsettings.Development.json`

### "Order request cannot be null"
- Verify body is valid JSON
- Check Content-Type header is `application/json`
- Don't send empty body

---

## ?? Custom Request Example

Create your own request:

```
POST http://localhost:5000/api/foodorder

Headers:
X-API-Key: dev-api-key-12345
Content-Type: application/json

Body (raw JSON):
{
  "customerName": "Your Name",
  "customerEmail": "your@email.com",
  "deliveryAddress": "Your Address",
  "items": [
    {
      "itemName": "Item Name",
      "quantity": 1,
      "price": 9.99
    }
  ],
  "totalAmount": 9.99
}
```

---

## ?? Variables in Requests

Use `{{variable_name}}` syntax:

- `{{api_url}}` ? `http://localhost:5000`
- `{{api_key}}` ? `dev-api-key-12345`
- `{{last_order_id}}` ? Auto-saved from previous response

---

## ?? Pro Tips

1. **Save Responses**: Click **Save Response** to compare different test runs
2. **Pre-request Scripts**: Automatically generate OrderId or timestamps
3. **Environment Switching**: Test against Dev/Staging/Prod easily
4. **Collections Sharing**: Share Postman collection with team
5. **Documentation**: Collection auto-generates from descriptions

---

## ?? Next Steps

After successful order creation:
1. Check **FoodOrder.Consumer.Shop** service (subscribes to orders)
2. Check **FoodOrder.Consumer.Delivery** service (processes deliveries)
3. Monitor **AWS CloudWatch** for SNS metrics
4. Enable **Postman Monitoring** for continuous API testing

