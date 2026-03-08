export interface FoodOrderMessage {
    receiptHandle?: string;
    messageId?: string;
    messageAttributes?: Record<string, string>;
    body: string;
}
