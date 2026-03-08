import axios from "axios"
import { useState } from "react";
import { FoodOrderMessage } from "../models/FoodOrderMessage";

export const CreateOrderComponent: React.FC = () => {

    const [messageBody, setMessageBody] = useState("")
    const [shopGroupId, setShopGroupId] = useState("")

    const onSubmitOrder = async () => {
        const orderReq: FoodOrderMessage = {
            body : messageBody,
            messageAttributes:{
                "ShopId": shopGroupId
            }
        };
        const res = axios.post("https://localhost:7020/api/foodorder", orderReq, {
            headers: {
                'X-API-Key': 'dev-api-key-12345'  // Add this line
            }
        })
        alert('Order submitted')
    }

    const onUpdateOrder = async (order: string) => {
        setMessageBody(order)
    }

    return (
        <div>
            <div>
                <h4>Create Order</h4>
                <hr />
            </div>
            <div className="d-flex flex-row">
                <div>
                    <input placeholder="Customer Name" value={messageBody} onChange={(e) => setMessageBody(e.target.value)} type="text" className="me-2" />
                    <input placeholder="Shop Group ID" value={shopGroupId} onChange={(e) => setShopGroupId(e.target.value)} type="text" />
                </div>
                <div className="mx-2">
                    <button onClick={onSubmitOrder} className="btn btn-sm btn-primary">Submit Order</button>
                </div>
            </div>
        </div>
    )
}

