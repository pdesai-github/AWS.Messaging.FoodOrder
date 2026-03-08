import { useState } from "react";
import axios from "axios"
import { FoodOrderMessage } from "../models/FoodOrderMessage";

export const DeliveryStoreComponent: React.FC = () => {
    const [orders, setOrders] = useState<FoodOrderMessage[]>([])
    const [errors, setErrors] = useState<string[]>([])

    const getOrders = async (): Promise<void> => {
        try {
            const res = await axios.get("https://localhost:7185/api/deliveryorder/messages")
            const messages: FoodOrderMessage[] = res.data ?? []
            debugger
            setOrders(messages)
        } catch (ex: any) {
            const detail = ex.response?.data ?? ex.message
            const errorMessage = typeof detail === 'object' ? JSON.stringify(detail) : detail
            setErrors([errorMessage])
        }
    }

    const deleteOrder = async (receiptHandle: string): Promise<void> => {
        try {
            await axios.delete("https://localhost:7185/api/deliveryorder/messages", {
                data: JSON.stringify(receiptHandle),
                headers: { 'Content-Type': 'application/json' }
            })
            setOrders(prev => prev.filter(o => o.receiptHandle !== receiptHandle))
            alert("Message deleted successfully")
        } catch (ex: any) {
            const detail = ex.response?.data ?? ex.message
            const errorMessage = typeof detail === 'object' ? JSON.stringify(detail) : detail
            setErrors([errorMessage])
        }
    }

    return <div>
        <h4>Delivery Store</h4>
        <button className="btn btn-sm btn-primary" onClick={getOrders}>Get Orders</button>
        <ul className="list-group mt-2">
            {
                orders.map((order: FoodOrderMessage) => (
                    <li key={order.messageId} className="list-group-item d-flex justify-content-between align-items-center">
                        <span>
                            <strong>Message ID:</strong> {order.messageId} &nbsp;|&nbsp;
                            <strong>Body:</strong> {order.body}
                        </span>
                        <button
                            className="btn btn-sm btn-danger"
                            onClick={() => deleteOrder(order.receiptHandle!)}
                        >Delete</button>
                    </li>
                ))
            }
        </ul>
        <div>
            {
                errors.map((error, index) => (
                    <div key={index} className="alert alert-danger mt-2">{error}</div>
                ))
            }
        </div>
    </div>
}