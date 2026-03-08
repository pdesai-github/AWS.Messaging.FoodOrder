namespace FoodOrder.Shared
{
    public class FoodOrderMessage
    {
        private string? _receiptHandle;
        public string? ReceiptHandle
        {
            get { return this._receiptHandle; }
            set { this._receiptHandle = value; }
        }

        private string? _messageId;
        public string? MessageId
        {
            get { return this._messageId; }
            set { this._messageId = value; }
        }

        private Dictionary<string, string> _messageAttributes;
        public Dictionary<string, string> MessageAttributes
        {
            get { return this._messageAttributes; }
            set { this._messageAttributes = value; }
        }

        private string _body;
        public string Body
        {
            get { return this._body; }
            set { this._body = value; }
        }

    }
}
