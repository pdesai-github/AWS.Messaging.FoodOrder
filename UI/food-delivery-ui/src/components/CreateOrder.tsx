import axios from "axios"
import { useState } from "react";

export const CreateOrderComponent: React.FC = () => {

    const [order, setOrder] = useState("")

    let orderReq = {
        orderId: "ORD123",
        customerName: "Pradip",
        customerEmail: "pradip@test.com",
        deliveryAddress: "Pune, India",
        items: [
            {
                itemName: "Laptop",
                quantity: 1,
                price: 80000
            },
            {
                itemName: "Mouse",
                quantity: 2,
                price: 500
            }
        ],
        totalAmount: 81000,
        orderDate: new Date().toISOString(),
        messageAttributes: {
            additionalProp1: ["value1"],
            additionalProp2: ["value2"],
            additionalProp3: ["value3"]
        },
        messageGroupId: "order-group"
    };


    const onSubmitOrder = async () => {
        orderReq.customerName = order;
        const res = axios.post("https://localhost:7020/api/foodorder", orderReq, {
            headers: {
                'X-API-Key': 'dev-api-key-12345'  // Add this line
            }
        })
        alert('Order submitted')
    }

    const onUpdateOrder = async (order: string) => {
        setOrder(order)
    }

    return (
        <div>
            <div>
                <h4>Create Order</h4>
                <hr />
            </div>
            <div className="d-flex flex-row">
                <div>
                    <input value={order} onChange={(e) => onUpdateOrder(e.target.value)} type="text" />
                </div>
                <div className="mx-2">
                    <button onClick={onSubmitOrder} className="btn btn-sm btn-primary">Submit Order</button>
                </div>
            </div>
        </div>
    )
}

